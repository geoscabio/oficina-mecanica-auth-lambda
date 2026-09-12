using Amazon.Lambda.APIGatewayEvents;
using FluentAssertions;
using OficinaMecanica.AuthLambda.Function.Configuration;

namespace OficinaMecanica.AuthLambda.Function.UnitTests;

public sealed class FunctionStartupTests
{
    [Fact]
    public async Task Dado_ConfiguracaoValida_Quando_CriarFunctionPadrao_Entao_DeveMontarFunctionERetornarBadRequestSemAcessarBanco()
    {
        // Arrange
        using var environment = new EnvironmentScope(configuracaoValida: true);
        var request = new APIGatewayHttpApiV2ProxyRequest { Body = string.Empty };

        // Act
        var function = new Function();
        var response = await function.Handler(request);

        // Assert
        response.StatusCode.Should().Be(400);
        response.Headers["Content-Type"].Should().Be("application/json");
        response.Body.Should().Contain("Validacao");
    }

    [Fact]
    public void Dado_ConfiguracaoJwtInvalida_Quando_CriarFunction_Entao_DeveFalharSemExporSecret()
    {
        // Arrange
        using var environment = new EnvironmentScope(configuracaoValida: true);
        Environment.SetEnvironmentVariable("Jwt__Secret", "segredo-curto");

        // Act
        var acao = () => new Function();

        // Assert
        acao.Should()
            .Throw<InvalidOperationException>()
            .Which.Message.Should().NotContain("segredo-curto");
    }

    [Fact]
    public void Dado_ConnectionStringAusente_Quando_CriarFunction_Entao_DeveFalharDeFormaSegura()
    {
        // Arrange
        using var environment = new EnvironmentScope(configuracaoValida: true);
        Environment.SetEnvironmentVariable("ConnectionStrings__SqlServer", null);

        // Act
        var acao = () => new Function();

        // Assert
        acao.Should()
            .Throw<InvalidOperationException>()
            .Which.Message.Should().NotContain("Password");
    }

    [Fact]
    public async Task Dado_ExpiracaoAusente_Quando_CarregarConfiguracao_Entao_DeveUsarDefault60EFunctionPermaneceFuncional()
    {
        // Arrange
        using var environment = new EnvironmentScope(configuracaoValida: true);
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", null);
        var request = new APIGatewayHttpApiV2ProxyRequest { Body = "{}" };

        // Act
        var configuration = AuthLambdaConfiguration.Carregar();
        var response = await new Function().Handler(request);

        // Assert
        configuration.JwtOptions.ExpirationMinutes.Should().Be(60);
        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public void Dado_ExpiracaoInvalida_Quando_CarregarConfiguracao_Entao_DeveUsarDefault60()
    {
        // Arrange
        using var environment = new EnvironmentScope(configuracaoValida: true);
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "0");

        // Act
        var configuration = AuthLambdaConfiguration.Carregar();

        // Assert
        configuration.JwtOptions.ExpirationMinutes.Should().Be(60);
    }

    [Fact]
    public void Dado_ConfiguracaoValida_Quando_CriarUseCasePeloFunctionStartup_Entao_DeveRetornarUseCase()
    {
        // Arrange
        using var environment = new EnvironmentScope(configuracaoValida: true);

        // Act
        var useCase = FunctionStartup.CriarUseCase();

        // Assert
        useCase.Should().NotBeNull();
    }

    [Fact]
    public void Dado_ConfiguracaoValida_Quando_CarregarConfiguracao_Entao_DeveRetornarConnectionStringEJwtOptions()
    {
        // Arrange
        using var environment = new EnvironmentScope(configuracaoValida: true);

        // Act
        var configuration = AuthLambdaConfiguration.Carregar();

        // Assert
        configuration.ConnectionString.Should().Contain("Server=(local)");
        configuration.JwtOptions.Issuer.Should().Be("oficina-mecanica-auth");
        configuration.JwtOptions.Audience.Should().Be("oficina-mecanica-api");
        configuration.JwtOptions.ExpirationMinutes.Should().Be(60);
    }

    private sealed class EnvironmentScope : IDisposable
    {
        private static readonly string[] Names =
        [
            "ConnectionStrings__SqlServer",
            "Jwt__Issuer",
            "Jwt__Audience",
            "Jwt__Secret",
            "Jwt__ExpirationMinutes"
        ];

        private readonly Dictionary<string, string?> _previousValues;

        public EnvironmentScope(bool configuracaoValida)
        {
            _previousValues = Names.ToDictionary(name => name, Environment.GetEnvironmentVariable);

            if (configuracaoValida)
            {
                Environment.SetEnvironmentVariable("ConnectionStrings__SqlServer", "Server=(local);Database=Oficina;User Id=reader;Password=not-used;");
                Environment.SetEnvironmentVariable("Jwt__Issuer", "oficina-mecanica-auth");
                Environment.SetEnvironmentVariable("Jwt__Audience", "oficina-mecanica-api");
                Environment.SetEnvironmentVariable("Jwt__Secret", "01234567890123456789012345678901");
                Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
            }
        }

        public void Dispose()
        {
            foreach (var pair in _previousValues)
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }
    }
}
