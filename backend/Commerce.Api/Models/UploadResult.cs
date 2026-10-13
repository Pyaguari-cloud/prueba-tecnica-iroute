namespace Commerce.Api.Models;

/// <summary>
/// Respuesta del endpoint de carga de archivo.
/// </summary>
/// <param name="FileName">Nombre del archivo recibido.</param>
/// <param name="InsertedCount">Cantidad de registros insertados en commerce.</param>
public record UploadResult(string FileName, int InsertedCount);