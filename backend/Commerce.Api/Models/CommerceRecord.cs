namespace Commerce.Api.Models;

/// <summary>
/// Representa una fila del archivo commerce_DDMMYYYY.csv
/// y un registro de la tabla dbo.commerce.
/// </summary>
/// <remarks>
/// Los nombres de las propiedades corresponden a las columnas de la BD
/// sin guiones bajos (pc_processdate -> PcProcessDate). Dapper las asocia
/// automáticamente gracias a <c>MatchNamesWithUnderscores</c>.
/// Los campos de texto son nullable porque el archivo puede traer
/// valores vacíos; la validación ocurre en sp_process_commerce.
/// </remarks>
public class CommerceRecord
{
    /// <summary>Fecha de proceso (columna pc_processdate).</summary>
    public DateTime PcProcessDate { get; set; }

    /// <summary>Código del comercio en la red.</summary>
    public string? PcCodComRed { get; set; }

    /// <summary>Nombre del comercio en la red.</summary>
    public string? PcNomComRed { get; set; }

    /// <summary>Tipo de documento (RUC / CED).</summary>
    public string? PcTipDoc { get; set; }

    /// <summary>Número de documento.</summary>
    public string? PcNumDoc { get; set; }

    /// <summary>Ciudad del comercio.</summary>
    public string? PcCiudad { get; set; }
}