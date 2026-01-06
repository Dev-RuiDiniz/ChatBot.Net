using Microsoft.EntityFrameworkCore;
using OmniChat.Application.Services;
using OmniChat.Domain.Interfaces;
using OmniChat.Infrastructure.AI;
using OmniChat.Infrastructure.Channels;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Resilience;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração de Banco de Dados (EF Core + SQL Server/Postgres)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Configuração de Resiliência e HttpClients
builder.Services.AddHttpClient<OpenAIService>()
    .AddPolicyHandler(ResiliencePolicies.GetRetryPolicy());

builder.Services.AddHttpClient<GeminiService>()
    .AddPolicyHandler(ResiliencePolicies.GetRetryPolicy());

builder.Services.AddHttpClient<WhatsAppChannel>()
    .AddPolicyHandler(ResiliencePolicies.GetRetryPolicy());

builder.Services.AddHttpClient<TelegramChannel>()
    .AddPolicyHandler(ResiliencePolicies.GetRetryPolicy());

// 3. Injeção de Dependência dos Serviços Core
builder.Services.AddScoped<IMcpRepository, McpRepository>();
builder.Services.AddScoped<McpService>();
builder.Services.AddScoped<IPlanEnforcementService, PlanEnforcementService>();
builder.Services.AddScoped<SecureChatOrchestrator>();

// 4. Factory e Estratégias
builder.Services.AddScoped<IAIFactory, AiFactory>();

// Registro dos Canais como uma coleção para o Orquestrador escolher ou usar todos
builder.Services.AddScoped<IMessagingChannel, WhatsAppChannel>();
builder.Services.AddScoped<IMessagingChannel, TelegramChannel>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Pipeline de Execução ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Migração automática ao iniciar (Cuidado em produção, usar apenas se tiver controle)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();