using FluentValidation;
using OficinaMecanica.AuthLambda.Application.Autenticacao;
using OficinaMecanica.AuthLambda.Application.Common;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;
using OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;
namespace OficinaMecanica.AuthLambda.Application.UnitTests;

public sealed class AutenticarClientePorDocumentoUseCaseTests
{
    [Fact]
    public async Task Request_nulo_retorna_validacao_sem_acessar_dependencias()
    {
        var repo = new Repositorio(); var token = new Tokens();
        var result = await Criar(repo, token).ExecutarAsync(null, default);
        Assert.False(result.Sucesso); Assert.Equal(TipoErro.Validacao, result.Erro!.Tipo); Assert.Equal(0, repo.Chamadas); Assert.Equal(0, token.Chamadas);
    }
    [Theory][InlineData("")][InlineData("   ")][InlineData("123")]
    public async Task Documento_invalido_retorna_validacao_sem_acessar_dependencias(string documento)
    {
        var repo = new Repositorio(); var token = new Tokens(); var result = await Criar(repo, token).ExecutarAsync(new(documento), default);
        Assert.Equal(TipoErro.Validacao, result.Erro!.Tipo); Assert.Equal(0, repo.Chamadas); Assert.Equal(0, token.Chamadas);
    }
    [Fact]
    public async Task Cliente_inexistente_e_inativo_retornam_mesma_resposta()
    {
        var inexistente = await Criar(new Repositorio(), new Tokens()).ExecutarAsync(new("52998224725"), default);
        var inativo = await Criar(new Repositorio(new(Guid.NewGuid(), StatusCliente.Inativo)), new Tokens()).ExecutarAsync(new("52998224725"), default);
        Assert.Equal(TipoErro.NaoAutorizado, inexistente.Erro!.Tipo); Assert.Equal(inexistente.Erro, inativo.Erro);
    }
    [Fact]
    public async Task Cliente_ativo_gera_token()
    {
        var id = Guid.NewGuid(); var token = new Tokens(); var result = await Criar(new Repositorio(new(id, StatusCliente.Ativo)), token).ExecutarAsync(new("52998224725"), default);
        Assert.True(result.Sucesso); Assert.Equal("token", result.Valor!.AccessToken); Assert.Equal("Bearer", result.Valor.TokenType); Assert.Equal(3600, result.Valor.ExpiresIn); Assert.Equal(1, token.Chamadas);
    }
    private static AutenticarClientePorDocumentoUseCase Criar(Repositorio repo, Tokens token) => new(repo, token, new AutenticarClientePorDocumentoValidator());
    private sealed class Repositorio(ClienteAutenticacao? cliente = null) : IClienteAutenticacaoRepository { public int Chamadas { get; private set; } public Task<ClienteAutenticacao?> ObterAsync(CpfCnpj documento, CancellationToken ct) { Chamadas++; return Task.FromResult(cliente); } }
    private sealed class Tokens : ITokenService { public int Chamadas { get; private set; } public string GerarToken(Guid clienteId) { Chamadas++; return "token"; } }
}
