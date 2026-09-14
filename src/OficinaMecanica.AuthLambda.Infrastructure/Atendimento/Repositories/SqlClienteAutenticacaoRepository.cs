using System.Data;
using Microsoft.Data.SqlClient;
using OficinaMecanica.AuthLambda.Application.Identidade.Repositories;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;
using OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.AuthLambda.Infrastructure.Atendimento.Repositories;

public sealed class SqlClienteAutenticacaoRepository : IClienteAutenticacaoRepository
{
    internal const string Query = """
        SELECT [Id], [Status]
        FROM [Atendimento].[Clientes]
        WHERE [Documento] = @documento
          AND [TipoDocumento] = @tipoDocumento
        """;

    private readonly string _connectionString;

    public SqlClienteAutenticacaoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<ClienteAutenticacao?> ObterAsync(CpfCnpj documento, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(Query, connection);
        command.Parameters.Add("@documento", SqlDbType.NVarChar, 14).Value = documento.Numero;
        command.Parameters.Add("@tipoDocumento", SqlDbType.Int).Value = (int)documento.Tipo;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new ClienteAutenticacao(
            reader.GetGuid(0),
            (StatusCliente)reader.GetInt32(1));
    }
}
