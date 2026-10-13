using System.ComponentModel.DataAnnotations;

namespace Commerce.Api.Models;

/// <summary>
/// Cuerpo de la petición del endpoint de procesamiento.
/// Ejemplo: { "processDate": "2026-10-13" }
/// </summary>
public class ProcessRequest
{
    /// <summary>Fecha a procesar (columna pc_processdate).</summary>
    [Required(ErrorMessage = "La fecha de proceso (processDate) es obligatoria.")]
    public DateTime? ProcessDate { get; set; }
}