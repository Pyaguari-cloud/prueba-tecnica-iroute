namespace Commerce.Api.Exceptions;

/// <summary>
/// Error de validación de negocio (archivo vacío, nombre incorrecto,
/// columnas faltantes, fecha inválida, etc.).
/// El controlador la convierte en una respuesta 400 BadRequest.
/// </summary>
public class BusinessValidationException : Exception
{
    public BusinessValidationException(string message) : base(message)
    {
    }

    public BusinessValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}