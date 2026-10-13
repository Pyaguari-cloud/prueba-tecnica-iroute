using System.Data;
using Commerce.Api.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Commerce.Api.Repositories;

/// <summary>
/// Implementación de <see cref="ICommerceRepository"/> con Dapper.
/// Toda la lógica de datos vive en los procedimientos almacenados;
/// esta clase solo los invoca y convierte los resultados en objetos C#.
/// </summary>
public class CommerceRepository : ICommerceRepository
{
    private const string SpCreateCommerce = "dbo.sp_create_commerce";
    private const string SpProcessCommerce = "dbo.sp_process_commerce";
    private const string SpGetQuarantine = "dbo.sp_get_quarantine";

    private readonly string _connectionString;

    static CommerceRepository()
    {
        // Permite que Dapper asocie columnas snake_case (pc_processdate)
        // con propiedades PascalCase (PcProcessDate).
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public CommerceRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("CommerceDb")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'CommerceDb' en appsettings.json.");
    }

    /// <inheritdoc />
    public async Task<int> InsertManyAsync(IReadOnlyCollection<CommerceRecord> records, CancellationToken ct = default)
    {
        if (records.Count == 0)
        {
            return 0;
        }

        // Una sola conexión y una sola transacción para todo el archivo:
        // si una fila falla, no queda el archivo cargado a medias.
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        try
        {
            // Los nombres de las propiedades deben coincidir con los
            // parámetros del procedimiento almacenado.
            var parameters = records.Select(r => new
            {
                pc_processdate = r.PcProcessDate.Date,
                pc_codcomred = r.PcCodComRed,
                pc_nomcomred = r.PcNomComRed,
                pc_tipdoc = r.PcTipDoc,
                pc_numdoc = r.PcNumDoc,
                pc_ciudad = r.PcCiudad
            });

            // Al recibir una colección de parámetros, Dapper ejecuta el SP
            // una vez por cada elemento, reutilizando el mismo comando.
            await connection.ExecuteAsync(new CommandDefinition(
                SpCreateCommerce,
                parameters,
                transaction,
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct));

            await transaction.CommitAsync(ct);
            return records.Count;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> ProcessAsync(DateTime processDate, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionString);

        // El SP devuelve una sola fila con la columna quarantined_count.
        return await connection.QuerySingleAsync<int>(new CommandDefinition(
            SpProcessCommerce,
            new { pc_processdate = processDate.Date },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<QuarantineRecord>> GetQuarantineAsync(DateTime? processDate = null, CancellationToken ct = default)
    {
        await using var connection = new SqlConnection(_connectionString);

        return await connection.QueryAsync<QuarantineRecord>(new CommandDefinition(
            SpGetQuarantine,
            new { pc_processdate = processDate?.Date },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }
}