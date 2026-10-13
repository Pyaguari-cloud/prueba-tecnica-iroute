using Commerce.Api.Models;

namespace Commerce.Api.Repositories;

/// <summary>
/// Acceso a datos de comercios. Cada método invoca un procedimiento
/// almacenado de la base de datos CommerceDB.
/// </summary>
public interface ICommerceRepository
{
    /// <summary>
    /// Inserta los registros invocando sp_create_commerce por cada uno,
    /// dentro de una sola conexión y una sola transacción.
    /// </summary>
    /// <returns>Cantidad de registros insertados.</returns>
    Task<int> InsertManyAsync(IReadOnlyCollection<CommerceRecord> records, CancellationToken ct = default);

    /// <summary>
    /// Ejecuta sp_process_commerce para la fecha indicada.
    /// </summary>
    /// <returns>Cantidad de registros enviados a cuarentena.</returns>
    Task<int> ProcessAsync(DateTime processDate, CancellationToken ct = default);

    /// <summary>
    /// Ejecuta sp_get_quarantine. Si no se envía fecha, devuelve todos los registros.
    /// </summary>
    Task<IEnumerable<QuarantineRecord>> GetQuarantineAsync(DateTime? processDate = null, CancellationToken ct = default);
}