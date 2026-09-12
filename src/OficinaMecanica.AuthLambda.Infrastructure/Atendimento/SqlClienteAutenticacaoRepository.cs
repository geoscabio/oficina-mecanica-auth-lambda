using System.Data;
using Microsoft.Data.SqlClient;
using OficinaMecanica.AuthLambda.Application.Autenticacao;
using OficinaMecanica.AuthLambda.Application.Autenticacao.Abstractions;
using OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;
namespace OficinaMecanica.AuthLambda.Infrastructure.Atendimento;

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

    public async Task<ClienteAutenticacao?> ObterAsync(CpfCnpj documento, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand(Query, connection);
        command.Parameters.Add("@documento", SqlDbType.NVarChar, 14).Value = documento.Numero;
        command.Parameters.Add("@tipoDocumento", SqlDbType.Int).Value = (int)documento.Tipo;
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new ClienteAutenticacao(reader.GetGuid(0), (StatusCliente)reader.GetInt32(1));
    }
}
