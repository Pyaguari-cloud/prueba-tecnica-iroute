using Commerce.Api.Models;
using Commerce.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Api.Controllers;

/// <summary>
/// Endpoints para la carga, procesamiento y consulta de comercios.
/// El controlador solo recibe la petición y delega en <see cref="ICommerceService"/>;
/// los errores de validación los convierte en 400 el GlobalExceptionHandler.
/// </summary>
[ApiController]
[Route("api/commerce")]
[Produces("application/json")]
public class CommerceController : ControllerBase
{
    /// <summary>Límite de tamaño de la petición de carga (10 MB).</summary>
    private const long MaxUploadBytes = 10 * 1024 * 1024;

    private readonly ICommerceService _commerceService;

    public CommerceController(ICommerceService commerceService)
    {
        _commerceService = commerceService;
    }

    /// <summary>
    /// Carga el archivo commerce_DDMMYYYY.csv y registra cada fila en la tabla commerce
    /// invocando sp_create_commerce.
    /// </summary>
    /// <param name="file">Archivo CSV (campo "file" de un formulario multipart).</param>
    /// <param name="ct">Token de cancelación de la petición.</param>
    /// <response code="200">Archivo cargado; retorna la cantidad de registros insertados.</response>
    /// <response code="400">Archivo vacío, nombre inválido o formato incorrecto.</response>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(typeof(UploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadResult>> Upload(IFormFile? file, CancellationToken ct)
    {
        var result = await _commerceService.UploadAsync(file, ct);
        return Ok(result);
    }

    /// <summary>
    /// Valida los registros de la fecha indicada (sp_process_commerce) y mueve
    /// los inválidos a commerce_quarantine.
    /// </summary>
    /// <param name="request">Fecha de proceso, ej. { "processDate": "2026-10-13" }.</param>
    /// <param name="ct">Token de cancelación de la petición.</param>
    /// <response code="200">Retorna la cantidad de registros enviados a cuarentena.</response>
    /// <response code="400">Fecha no enviada o inválida.</response>
    [HttpPost("process")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ProcessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProcessResult>> Process([FromBody] ProcessRequest request, CancellationToken ct)
    {
        // [ApiController] ya garantiza que ProcessDate no es nulo ([Required]).
        var result = await _commerceService.ProcessAsync(request.ProcessDate!.Value, ct);
        return Ok(result);
    }

    /// <summary>
    /// Lista los comercios en cuarentena con su motivo de rechazo.
    /// </summary>
    /// <param name="processDate">Filtro opcional por fecha (yyyy-MM-dd).</param>
    /// <param name="ct">Token de cancelación de la petición.</param>
    /// <response code="200">Listado de registros en cuarentena.</response>
    [HttpGet("quarantine")]
    [ProducesResponseType(typeof(IEnumerable<QuarantineRecord>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<QuarantineRecord>>> GetQuarantine(
        [FromQuery] DateTime? processDate,
        CancellationToken ct)
    {
        var records = await _commerceService.GetQuarantineAsync(processDate, ct);
        return Ok(records);
    }
}