using Commerce.Api.Models;

namespace Commerce.Api.Services;

/// <summary>
/// Lógica de negocio de comercios: validación y lectura del archivo CSV,
/// procesamiento por fecha y consulta de la cuarentena.
/// </summary>
public interface ICommerceService
{
    /// <summary>
    /// Valida el archivo commerce_DDMMYYYY.csv, lo lee y registra cada fila
    /// en la tabla commerce.
    /// </summary>
    /// <exception cref="Exceptions.BusinessValidationException">
    /// Si el archivo es nulo, está vacío, tiene un nombre inválido o un formato incorrecto.
    /// </exception>
    Task<UploadResult> UploadAsync(IFormFile? file, CancellationToken ct = default);

    /// <summary>
    /// Valida los registros de la fecha indicada y mueve los inválidos a cuarentena.
    /// </summary>
    Task<ProcessResult> ProcessAsync(DateTime processDate, CancellationToken ct = default);

    /// <summary>
    /// Lista los registros en cuarentena. La fecha es un filtro opcional.
    /// </summary>
    Task<IEnumerable<QuarantineRecord>> GetQuarantineAsync(DateTime? processDate = null, CancellationToken ct = default);
}