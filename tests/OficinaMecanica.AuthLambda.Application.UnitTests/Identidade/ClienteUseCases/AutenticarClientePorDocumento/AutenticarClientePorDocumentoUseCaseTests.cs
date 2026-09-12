using FluentAssertions;
using Moq;
using OficinaMecanica.AuthLambda.Application.Common;
using OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;
using OficinaMecanica.AuthLambda.Application.Identidade.Interfaces;
using OficinaMecanica.AuthLambda.Application.Identidade.Models;
using OficinaMecanica.AuthLambda.Application.Identidade.Repositories;
using OficinaMecanica.AuthLambda.Application.UnitTests.Identidade.Factories;
using OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.AuthLambda.Application.UnitTests.Identidade.ClienteUseCases.AutenticarClientePorDocumento;

public sealed class AutenticarClientePorDocumentoUseCaseTests
{
    [Fact]
    public async Task Dado_RequestNulo_Quando_AutenticarClientePorDocumento_Entao_DeveRetornarFalhaDeValidacaoSemAcessarDependencias()
    {
        // Arrange
        var repositorio = new Mock<IClienteAutenticacaoRepository>();
        var tokenService = new Mock<ITokenService>();
        var useCase = CriarUseCase(repositorio, tokenService);

        // Act
        var resultado = await useCase.ExecutarAsync(null, CancellationToken.None);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Erro.Should().NotBeNull();
        resultado.Erro!.Tipo.Should().Be(TipoErro.Validacao);

        repositorio.Verify(
            repo => repo.ObterAsync(It.IsAny<CpfCnpj>(), It.IsAny<CancellationToken>()),
            Times.Never);

        tokenService.Verify(
            service => service.GerarToken(It.IsAny<Guid>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Dado_DocumentoVazio_Quando_AutenticarClientePorDocumento_Entao_DeveRetornarFalhaDeValidacaoSemAcessarDependencias(
        string documento)
    {
        // Arrange
        var repositorio = new Mock<IClienteAutenticacaoRepository>();
        var tokenService = new Mock<ITokenService>();
        var useCase = CriarUseCase(repositorio, tokenService);
        var request = IdentidadeTestDataFactory.CriarRequestValido(documento);

        // Act
        var resultado = await useCase.ExecutarAsync(request, CancellationToken.None);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Erro.Should().NotBeNull();
        resultado.Erro!.Tipo.Should().Be(TipoErro.Validacao);

        repositorio.Verify(
            repo => repo.ObterAsync(It.IsAny<CpfCnpj>(), It.IsAny<CancellationToken>()),
            Times.Never);

        tokenService.Verify(
            service => service.GerarToken(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Dado_DocumentoInvalido_Quando_AutenticarClientePorDocumento_Entao_DeveRetornarFalhaDeValidacaoSemAcessarDependenciasExternas()
    {
        // Arrange
        var repositorio = new Mock<IClienteAutenticacaoRepository>();
        var tokenService = new Mock<ITokenService>();
        var useCase = CriarUseCase(repositorio, tokenService);
        var request = IdentidadeTestDataFactory.CriarRequestValido("123");

        // Act
        var resultado = await useCase.ExecutarAsync(request, CancellationToken.None);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Erro.Should().NotBeNull();
        resultado.Erro!.Tipo.Should().Be(TipoErro.Validacao);

        repositorio.Verify(
            repo => repo.ObterAsync(It.IsAny<CpfCnpj>(), It.IsAny<CancellationToken>()),
            Times.Never);

        tokenService.Verify(
            service => service.GerarToken(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Dado_ClienteInexistente_Quando_AutenticarClientePorDocumento_Entao_DeveRetornarNaoAutorizadoSemGerarToken()
    {
        // Arrange
        var repositorio = CriarRepositorio(null);
        var tokenService = new Mock<ITokenService>();
        var useCase = CriarUseCase(repositorio, tokenService);
        var request = IdentidadeTestDataFactory.CriarRequestValido();

        // Act
        var resultado = await useCase.ExecutarAsync(request, CancellationToken.None);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Erro.Should().NotBeNull();
        resultado.Erro!.Tipo.Should().Be(TipoErro.NaoAutorizado);

        repositorio.Verify(
            repo => repo.ObterAsync(It.IsAny<CpfCnpj>(), It.IsAny<CancellationToken>()),
            Times.Once);

        tokenService.Verify(
            service => service.GerarToken(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Dado_ClienteInativo_Quando_AutenticarClientePorDocumento_Entao_DeveRetornarNaoAutorizadoIgualAoClienteInexistente()
    {
        // Arrange
        var tokenServiceClienteInexistente = new Mock<ITokenService>();
        var tokenServiceClienteInativo = new Mock<ITokenService>();

        var useCaseClienteInexistente = CriarUseCase(CriarRepositorio(null), tokenServiceClienteInexistente);
        var useCaseClienteInativo = CriarUseCase(
            CriarRepositorio(IdentidadeTestDataFactory.CriarClienteInativo()),
            tokenServiceClienteInativo);

        var request = IdentidadeTestDataFactory.CriarRequestValido();

        // Act
        var clienteInexistenteResultado = await useCaseClienteInexistente.ExecutarAsync(request, CancellationToken.None);
        var clienteInativoResultado = await useCaseClienteInativo.ExecutarAsync(request, CancellationToken.None);

        // Assert
        clienteInexistenteResultado.Sucesso.Should().BeFalse();
        clienteInativoResultado.Sucesso.Should().BeFalse();
        clienteInexistenteResultado.Erro.Should().Be(clienteInativoResultado.Erro);

        tokenServiceClienteInexistente.Verify(
            service => service.GerarToken(It.IsAny<Guid>()),
            Times.Never);

        tokenServiceClienteInativo.Verify(
            service => service.GerarToken(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Dado_DocumentoValidoDeClienteAtivo_Quando_AutenticarClientePorDocumento_Entao_DeveRetornarExpiresInDoTokenService()
    {
        // Arrange
        var cliente = IdentidadeTestDataFactory.CriarClienteAtivo();
        var repositorio = CriarRepositorio(cliente);
        var token = IdentidadeTestDataFactory.CriarTokenGerado();
        var tokenService = CriarTokenService(token);
        var useCase = CriarUseCase(repositorio, tokenService);
        var request = IdentidadeTestDataFactory.CriarRequestValido();

        // Act
        var resultado = await useCase.ExecutarAsync(request, CancellationToken.None);

        // Assert
        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().NotBeNull();
        resultado.Valor!.AccessToken.Should().Be(token.AccessToken);
        resultado.Valor.TokenType.Should().Be("Bearer");
        resultado.Valor.ExpiresIn.Should().Be(token.ExpiresIn);

        repositorio.Verify(
            repo => repo.ObterAsync(It.IsAny<CpfCnpj>(), It.IsAny<CancellationToken>()),
            Times.Once);

        tokenService.Verify(
            service => service.GerarToken(cliente.Id),
            Times.Once);
    }

    private static AutenticarClientePorDocumentoUseCase CriarUseCase(
        Mock<IClienteAutenticacaoRepository> repositorio,
        Mock<ITokenService> tokenService)
    {
        return new AutenticarClientePorDocumentoUseCase(
            repositorio.Object,
            tokenService.Object,
            new AutenticarClientePorDocumentoValidator());
    }

    private static Mock<IClienteAutenticacaoRepository> CriarRepositorio(ClienteAutenticacao? cliente)
    {
        var repositorio = new Mock<IClienteAutenticacaoRepository>();

        repositorio
            .Setup(repo => repo.ObterAsync(It.IsAny<CpfCnpj>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        return repositorio;
    }

    private static Mock<ITokenService> CriarTokenService(TokenGerado token)
    {
        var tokenService = new Mock<ITokenService>();

        tokenService
            .Setup(service => service.GerarToken(It.IsAny<Guid>()))
            .Returns(token);

        return tokenService;
    }
}
