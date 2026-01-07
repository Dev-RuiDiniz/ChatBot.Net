using Moq;
using OmniChat.Application.Services;
using OmniChat.Domain.Entities;
using OmniChat.Infrastructure.Repositories;

namespace OmniChat.Tests.Services;

public class PlanEnforcementServiceTests
{
    private readonly Mock<UserRepository> _userRepoMock;
    private readonly Mock<PlanRepository> _planRepoMock;
    private readonly PlanEnforcementService _service;

    public PlanEnforcementServiceTests()
    {
        // Setup dos Mocks (Simuladores de Banco de Dados)
        _userRepoMock = new Mock<UserRepository>(null!); // Null pois não usaremos o MongoDbContext real aqui
        _planRepoMock = new Mock<PlanRepository>(null!);
        
        _service = new PlanEnforcementService(_userRepoMock.Object, _planRepoMock.Object);
    }

    [Fact]
    public async Task CanUserSendMessage_ShouldReturnFalse_WhenLimitExceeded()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        var user = new User("5511999999999") 
        { 
            Id = userId,
            Subscription = new UserSubscription 
            { 
                PlanId = planId, 
                MessagesUsedThisMonth = 100, // Já usou 100
                LastUsageReset = DateTime.UtcNow 
            }
        };

        var plan = new Plan 
        { 
            Id = planId, 
            MonthlyMessageLimit = 100 // Limite é 100
        };

        // Configura o comportamento dos Mocks
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _planRepoMock.Setup(r => r.GetByIdAsync(planId)).ReturnsAsync(plan);

        // Act
        var result = await _service.CanUserSendMessageAsync(userId);

        // Assert
        Assert.False(result); // Deve bloquear
    }

    [Fact]
    public async Task CanUserSendMessage_ShouldReturnTrue_WhenLimitNotReached()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User("...") { 
            Subscription = new UserSubscription { MessagesUsedThisMonth = 50 } 
        };
        var plan = new Plan { MonthlyMessageLimit = 100 };

        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(user);
        _planRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(plan);

        // Act
        var result = await _service.CanUserSendMessageAsync(userId);

        // Assert
        Assert.True(result);
    }
}