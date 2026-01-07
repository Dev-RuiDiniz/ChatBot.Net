using Microsoft.AspNetCore.Mvc;
using OmniChat.Infrastructure.Repositories;
using OmniChat.Infrastructure.Security;
using OmniChat.Shared.DTOs;

namespace OmniChat.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserRepository _userRepo;
    private readonly AuthService _authService;

    public AuthController(UserRepository userRepo, AuthService authService)
    {
        _userRepo = userRepo;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        // 1. Buscar usuário (pelo email/username)
        var user = await _userRepo.GetByEmailAsync(login.Username);
        if (user == null) return Unauthorized("Usuário não encontrado.");

        // 2. Verificar Senha
        if (!_authService.VerifyPassword(login.Password, user.PasswordHash))
            return Unauthorized("Senha incorreta.");

        // 3. Gerar Token
        var token = _authService.GenerateJwtToken(user, user.Role.ToString(), user.OrganizationId);

        return Ok(new LoginResultDto 
        { 
            Token = token, 
            Role = user.Role.ToString(),
            Expiration = DateTime.UtcNow.AddHours(8)
        });
    }
}