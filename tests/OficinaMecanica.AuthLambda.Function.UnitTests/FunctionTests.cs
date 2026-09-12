using System.Text.Json;
using FluentAssertions;
using Moq;
using OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;
using OficinaMecanica.AuthLambda.Application.Identidade.Interfaces;
using OficinaMecanica.AuthLambda.Application.Identidade.Repositories;
using OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;
using OficinaMecanica.AuthLambda.Function.UnitTests.Factories;

namespace OficinaMecanica.AuthLambda.Function.UnitTests;

public sealed class FunctionTests
{
    [Theory]
    [InlineData(FunctionTestDataFactory.BodyVazio)]
    [InlineData(FunctionTestDataFactory.JsonInvalido)]
    [InlineData(FunctionTestDataFactory.BodySemDocumento)]
    [InlineData(FunctionTestDataFactory.BodyComDocumentoInvalido)]
    public async Task Dado_EntradaInvalida_Quando_ExecutarHandler_Entao_DeveRetornarBadRequestComErroEmJson(
        string body)
    {
        // Arrange
        var function = CriarFunction();
        var request = FunctionTestDataFactory.CriarRequestApiGateway(body);

        // Act
        var response = await function.Handler(request);

        // Assert
        response.StatusCode.Should().Be(400);
        response.Headers["Content-Type"].Should().Be("application/json");
        response.Body.Should().Contain("Validacao");
        response.Body.Should().NotContain(FunctionTestDataFactory.DocumentoInvalido);
        response.Body.Should().NotContain("documento");
    }

    [Fact]
    public async Task Dado_ClienteInexistenteEClienteInativo_Quando_ExecutarHandler_Entao_DeveRetornarUnauthorizedIndistinguivel()
    {
        // Arrange
        var request = FunctionTestDataFactory.CriarRequestApiGateway(FunctionTestDataFactory.BodyComDocumentoValido);
        var clienteInexistenteFunction = CriarFunction();
        var clienteInativoFunction = CriarFunction(FunctionTestDataFactory.CriarClienteInativo());

        // Act
        var clienteInexistenteResponse = await clienteInexistenteFunction.Handler(request);
        var clienteInativoResponse = await clienteInativoFunction.Handler(request);

        // Assert
        clienteInexistenteResponse.StatusCode.Should().Be(401);
        clienteInativoResponse.StatusCode.Should().Be(401);
        clienteInexistenteResponse.Body.Should().Be(clienteInativoResponse.Body);
        clienteInexistenteResponse.Body.Should().NotContain(FunctionTestDataFactory.DocumentoCpfValido);
    }

    [Fact]
    public async Task Dado_DocumentoValidoDeClienteAtivo_Quando_ExecutarHandler_Entao_DeveRetornarOkComTokenBearer()
    {
        // Arrange
        var cliente = FunctionTestDataFactory.CriarClienteAtivo();
        var function = CriarFunction(cliente);
        var request = FunctionTestDataFactory.CriarRequestApiGateway(FunctionTestDataFactory.BodyComDocumentoValidoComMascara);

        // Act
        var response = await function.Handler(request);

        // Assert
        using var json = JsonDocument.Parse(response.Body);

        response.StatusCode.Should().Be(200);
        response.Headers["Content-Type"].Should().Be("application/json");
        json.RootElement.GetProperty("accessToken").GetString().Should().Be(FunctionTestDataFactory.TokenPadrao);
        json.RootElement.GetProperty("tokenType").GetString().Should().Be("Bearer");
        json.RootElement.GetProperty("expiresIn").GetInt32().Should().Be(FunctionTestDataFactory.ExpiresInPadrao);
        response.Body.Should().NotContain(FunctionTestDataFactory.DocumentoCpfValidoComMascara);
    }

    [Fact]
    public async Task Dado_ExcecaoInesperada_Quando_ExecutarHandler_Entao_DeveRetornarInternalServerErrorSanitizado()
    {
        // Arrange
        var function = CriarFunctionComRepositorioFalhando();
        var request = FunctionTestDataFactory.CriarRequestApiGateway(FunctionTestDataFactory.BodyComDocumentoValido);

        // Act
        var response = await function.Handler(request);

        // Assert
        response.StatusCode.Should().Be(500);
        response.Headers["Content-Type"].Should().Be("application/json");
        response.Body.Should().Contain("ErroInterno");
        response.Body.Should().NotContain("segredo interno");
        response.Body.Should().NotContain(FunctionTestDataFactory.DocumentoCpfValido);
    }

    private static Function CriarFunction(ClienteAutenticacao? cliente = null)
    {
        var repositorio = new Mock<IClienteAutenticacaoRepository>();

        repositorio
            .Setup(repo => repo.ObterAsync(It.IsAny<CpfCnpj>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var tokenService = new Mock<ITokenService>();

        tokenService
            .Setup(service => service.GerarToken(It.IsAny<Guid>()))
            .Returns(FunctionTestDataFactory.CriarTokenGerado());

        return CriarFunction(repositorio, tokenService);
    }

    private static Function CriarFunctionComRepositorioFalhando()
    {
        var repositorio = new Mock<IClienteAutenticacaoRepository>();

        repositorio
            .Setup(repo => repo.ObterAsync(It.IsAny<CpfCnpj>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("segredo interno"));

        var tokenService = new Mock<ITokenService>();

        return CriarFunction(repositorio, tokenService);
    }

    private static Function CriarFunction(
        Mock<IClienteAutenticacaoRepository> repositorio,
        Mock<ITokenService> tokenService)
    {
        var useCase = new AutenticarClientePorDocumentoUseCase(
            repositorio.Object,
            tokenService.Object,
            new AutenticarClientePorDocumentoValidator());

        return new Function(useCase);
    }
}
