using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using OficinaMecanica.AuthLambda.Application.Autenticacao;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;
using OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;
using OficinaMecanica.AuthLambda.Function;
namespace OficinaMecanica.AuthLambda.Function.UnitTests;

public sealed class FunctionTests
{
    [Theory][InlineData("{")][InlineData("")][InlineData("{}")][InlineData("{\"documento\":\"123\"}")]
    public async Task Entrada_invalida_retorna_400(string body) => Assert.Equal(400, (await Criar().Handler(new() { Body = body })).StatusCode);
    [Fact]
    public async Task Inexistente_e_inativo_retornam_401_iguais()
    { var a = await Criar().Handler(new() { Body = "{\"documento\":\"52998224725\"}" }); var b = await Criar(new(Guid.NewGuid(), StatusCliente.Inativo)).Handler(new() { Body = "{\"documento\":\"52998224725\"}" }); Assert.Equal(401, a.StatusCode); Assert.Equal(a.Body, b.Body); }
    [Fact]
    public async Task Sucesso_retorna_contrato_bearer()
    { var response = await Criar(new(Guid.NewGuid(), StatusCliente.Ativo)).Handler(new() { Body = "{\"documento\":\"529.982.247-25\"}" }); using var json = JsonDocument.Parse(response.Body); Assert.Equal(200, response.StatusCode); Assert.Equal("token", json.RootElement.GetProperty("accessToken").GetString()); Assert.Equal("Bearer", json.RootElement.GetProperty("tokenType").GetString()); Assert.Equal(3600, json.RootElement.GetProperty("expiresIn").GetInt32()); }
    [Fact]
    public async Task Excecao_inesperada_retorna_500_sanitizado()
    { var response = await new Function(new AutenticarClientePorDocumentoUseCase(new Repo(null, true), new Token(), new AutenticarClientePorDocumentoValidator())).Handler(new() { Body = "{\"documento\":\"52998224725\"}" }); Assert.Equal(500, response.StatusCode); Assert.DoesNotContain("segredo interno", response.Body); Assert.Contains("ErroInterno", response.Body); }
    private static Function Criar(ClienteAutenticacao? cliente = null) => new(new AutenticarClientePorDocumentoUseCase(new Repo(cliente), new Token(), new AutenticarClientePorDocumentoValidator()));
    private sealed class Repo(ClienteAutenticacao? cliente, bool falhar = false) : IClienteAutenticacaoRepository { public Task<ClienteAutenticacao?> ObterAsync(CpfCnpj documento, CancellationToken ct) => falhar ? throw new InvalidOperationException("segredo interno") : Task.FromResult(cliente); }
    private sealed class Token : ITokenService { public string GerarToken(Guid clienteId) => "token"; }
}
