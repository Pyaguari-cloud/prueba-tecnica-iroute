using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Commerce.Api.Exceptions;
using Commerce.Api.Models;
using Commerce.Api.Repositories;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Commerce.Api.Services;

/// <summary>
/// Implementación de <see cref="ICommerceService"/>.
/// Concentra las reglas de negocio de la API: el controlador solo
/// recibe la petición y el repositorio solo habla con la base de datos.
/// </summary>
public partial class CommerceService : ICommerceService
{
    /// <summary>Tamaño máximo permitido para el archivo (10 MB).</summary>
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private readonly ICommerceRepository _repository;
    private readonly ILogger<CommerceService> _logger;

    public CommerceService(ICommerceRepository repository, ILogger<CommerceService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Nombre esperado: commerce_DDMMYYYY.csv (ej. commerce_13102026.csv).
    /// Se genera en tiempo de compilación, más eficiente que crear el Regex en cada llamada.
    /// </summary>
    [GeneratedRegex(@"^commerce_(?<date>\d{8})\.csv$", RegexOptions.IgnoreCase)]
    private static partial Regex FileNameRegex();

    /// <inheritdoc />
    public async Task<UploadResult> UploadAsync(IFormFile? file, CancellationToken ct = default)
    {
        // 1. El archivo debe existir y no estar vacío (requisito del enunciado).
        if (file is null || file.Length == 0)
        {
            throw new BusinessValidationException("El archivo está vacío o no fue enviado.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new BusinessValidationException(
                $"El archivo supera el tamaño máximo permitido ({MaxFileSizeBytes / (1024 * 1024)} MB).");
        }

        // 2. El nombre debe cumplir el formato commerce_DDMMYYYY.csv.
        ValidateFileName(file.FileName);

        // 3. Leer el contenido del CSV.
        var records = await ReadCsvAsync(file, ct);

        // Un archivo que solo trae el encabezado también se considera vacío.
        if (records.Count == 0)
        {
            throw new BusinessValidationException("El archivo no contiene registros, solo el encabezado.");
        }

        // 4. Registrar cada fila invocando sp_create_commerce.
        var inserted = await _repository.InsertManyAsync(records, ct);

        _logger.LogInformation("Archivo {FileName} cargado: {Count} registros insertados.",
            file.FileName, inserted);

        return new UploadResult(file.FileName, inserted);
    }

    /// <inheritdoc />
    public async Task<ProcessResult> ProcessAsync(DateTime processDate, CancellationToken ct = default)
    {
        if (processDate == default)
        {
            throw new BusinessValidationException("Debe indicar la fecha de proceso (pc_processdate).");
        }

        var quarantined = await _repository.ProcessAsync(processDate.Date, ct);

        _logger.LogInformation("Fecha {ProcessDate:yyyy-MM-dd} procesada: {Count} registros en cuarentena.",
            processDate, quarantined);

        return new ProcessResult(processDate.Date, quarantined);
    }

    /// <inheritdoc />
    public Task<IEnumerable<QuarantineRecord>> GetQuarantineAsync(DateTime? processDate = null, CancellationToken ct = default)
        => _repository.GetQuarantineAsync(processDate?.Date, ct);

    /// <summary>
    /// Verifica que el nombre sea commerce_DDMMYYYY.csv y que DDMMYYYY sea una fecha real.
    /// </summary>
    private static void ValidateFileName(string fileName)
    {
        var match = FileNameRegex().Match(Path.GetFileName(fileName));

        var isValidDate = match.Success && DateTime.TryParseExact(
            match.Groups["date"].Value,
            "ddMMyyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);

        if (!isValidDate)
        {
            throw new BusinessValidationException(
                $"El nombre del archivo '{fileName}' no es válido. Formato esperado: commerce_DDMMYYYY.csv");
        }
    }

    /// <summary>
    /// Lee el CSV con CsvHelper y convierte los errores de formato
    /// en mensajes claros que indican la fila con problemas.
    /// </summary>
    private static async Task<List<CommerceRecord>> ReadCsvAsync(IFormFile file, CancellationToken ct)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            DetectDelimiter = true,       // Acepta ',' o ';' (Excel en español guarda con ';').
            IgnoreBlankLines = true,
            // Ignora mayúsculas y espacios alrededor de los nombres de columna.
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
        };

        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<CommerceCsvMap>();

        var records = new List<CommerceRecord>();

        try
        {
            await foreach (var record in csv.GetRecordsAsync<CommerceRecord>(ct))
            {
                records.Add(record);
            }
        }
        catch (HeaderValidationException ex)
        {
            throw new BusinessValidationException(
                "El archivo no tiene las columnas esperadas: pc_processdate, pc_codcomred, " +
                "pc_nomcomred, pc_tipdoc, pc_numdoc, pc_ciudad.", ex);
        }
        catch (TypeConverterException ex)
        {
            throw new BusinessValidationException(
                $"Fila {ex.Context?.Parser?.Row}: el valor '{ex.Text}' no es una fecha válida. " +
                $"Formatos aceptados: {string.Join(" o ", CommerceCsvMap.AcceptedDateFormats)}.", ex);
        }
        catch (CsvHelperException ex)
        {
            throw new BusinessValidationException(
                $"Fila {ex.Context?.Parser?.Row}: la fila no tiene el formato esperado " +
                "(revise que tenga todas las columnas).", ex);
        }

        return records;
    }
}