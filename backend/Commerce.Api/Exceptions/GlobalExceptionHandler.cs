using Microsoft.AspNetCore.Diagnostics;

namespace Commerce.Api.Exceptions;

/// <summary>
/// Manejador global de errores (.NET 8). Atrapa cualquier excepción no
/// controlada de la API y la convierte en una respuesta estándar
/// ProblemDetails (RFC 7807):
///   - <see cref="BusinessValidationException"/> -> 400 BadRequest con el mensaje de validación.
///   - Cualquier otra excepción                  -> 500 con un mensaje genérico
///     (el detalle técnico solo se registra en el log, nunca se expone al cliente).
/// Así los controladores no necesitan bloques try/catch repetidos.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            BusinessValidationException ex =>
                (StatusCodes.Status400BadRequest, "Solicitud inválida", ex.Message),

            _ =>
                (StatusCodes.Status500InternalServerError, "Error interno del servidor",
                 "Ocurrió un error inesperado. Intente nuevamente o contacte al administrador.")
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Error no controlado en {Path}", httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning("Validación fallida en {Path}: {Message}",
                httpContext.Request.Path, exception.Message);
        }

        httpContext.Response.StatusCode = status;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = title,
                Detail = detail
            }
        });
    }
}