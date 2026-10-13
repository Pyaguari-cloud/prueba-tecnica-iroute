namespace Commerce.Api.Models;

/// <summary>
/// Respuesta del endpoint de procesamiento.
/// </summary>
/// <param name="ProcessDate">Fecha procesada.</param>
/// <param name="QuarantinedCount">Cantidad de registros enviados a commerce_quarantine.</param>
public record ProcessResult(DateTime ProcessDate, int QuarantinedCount);