using System.Net;
using System.Text.Json;

namespace OmniChat.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new ErrorDetails
        {
            Success = false,
            Message = exception.Message
        };

        switch (exception)
        {
            case ApplicationException e: // Erros de regra de negócio customizados
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            case KeyNotFoundException e:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
            case UnauthorizedAccessException e:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;
            default:
                // Erros não tratados (Bugs, falhas de DB)
                _logger.LogError(exception, "Erro crítico não tratado.");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "Ocorreu um erro interno no servidor. Contate o suporte.";
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(result);
    }
}

// DTO Simples para erro
public class ErrorDetails
{
    public bool Success { get; set; }
    public string Message { get; set; }
}