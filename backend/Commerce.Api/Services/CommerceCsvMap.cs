using Commerce.Api.Models;
using CsvHelper.Configuration;

namespace Commerce.Api.Services;

/// <summary>
/// Indica a CsvHelper qué columna del CSV corresponde a cada propiedad
/// de <see cref="CommerceRecord"/>. Se mapea por nombre de encabezado,
/// así el orden de las columnas en el archivo no importa.
/// </summary>
public sealed class CommerceCsvMap : ClassMap<CommerceRecord>
{
    /// <summary>Formatos de fecha aceptados en la columna pc_processdate.</summary>
    public static readonly string[] AcceptedDateFormats = { "yyyy-MM-dd", "dd/MM/yyyy" };

    public CommerceCsvMap()
    {
        Map(m => m.PcProcessDate).Name("pc_processdate")
            .TypeConverterOption.Format(AcceptedDateFormats);

        Map(m => m.PcCodComRed).Name("pc_codcomred");
        Map(m => m.PcNomComRed).Name("pc_nomcomred");
        Map(m => m.PcTipDoc).Name("pc_tipdoc");
        Map(m => m.PcNumDoc).Name("pc_numdoc");
        Map(m => m.PcCiudad).Name("pc_ciudad");
    }
}