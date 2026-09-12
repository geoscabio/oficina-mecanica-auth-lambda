using Amazon.Lambda.APIGatewayEvents;
using OficinaMecanica.AuthLambda.Function;
using OficinaMecanica.AuthLambda.Function.Configuration;

namespace OficinaMecanica.AuthLambda.Function.UnitTests;

public sealed class FunctionStartupTests
{
    [Fact]
    public async Task Construtor_padrao_com_configuracao_valida_monta_function_e_retorna_400_sem_banco()
    {
        using var environment = new EnvironmentScope(configuracaoValida: true);

        var function = new Function();

        var response = await function.Handler(new APIGatewayHttpApiV2ProxyRequest { Body = string.Empty });

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("application/json", response.Headers["Content-Type"]);
        Assert.Contains("Validacao", response.Body);
    }

    [Fact]
    public void Construtor_padrao_com_secret_invalido_falha_sem_expor_valor()
    {
        using var environment = new EnvironmentScope(configuracaoValida: true);
        Environment.SetEnvironmentVariable("Jwt__Secret", "segredo-curto");

        var exception = Assert.Throws<InvalidOperationException>(() => new Function());

        Assert.DoesNotContain("segredo-curto", exception.Message);
    }

    [Fact]
    public void Construtor_padrao_sem_connection_string_falha_de_forma_segura()
    {
        using var environment = new EnvironmentScope(configuracaoValida: true);
        Environment.SetEnvironmentVariable("ConnectionStrings__SqlServer", null);

        var exception = Assert.Throws<InvalidOperationException>(() => new Function());

        Assert.DoesNotContain("Password", exception.Message);
    }

    [Fact]
    public async Task Expiracao_ausente_usa_default_e_bootstrap_permanece_funcional()
    {
        using var environment = new EnvironmentScope(configuracaoValida: true);
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", null);

        var configuration = AuthLambdaConfiguration.Carregar();
        var response = await new Function().Handler(new APIGatewayHttpApiV2ProxyRequest { Body = "{}" });

        Assert.Equal(60, configuration.JwtOptions.ExpirationMinutes);
        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public void Expiracao_invalida_usa_default_60()
    {
        using var environment = new EnvironmentScope(configuracaoValida: true);
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "0");

        var configuration = AuthLambdaConfiguration.Carregar();

        Assert.Equal(60, configuration.JwtOptions.ExpirationMinutes);
    }

    [Fact]
    public void FunctionStartup_cria_use_case_com_configuracao_valida()
    {
        using var environment = new EnvironmentScope(configuracaoValida: true);

        var useCase = FunctionStartup.CriarUseCase();

        Assert.NotNull(useCase);
    }

    [Fact]
    public void Configuracao_valida_retorna_connection_string_e_jwt_options()
    {
        using var environment = new EnvironmentScope(configuracaoValida: true);

        var configuration = AuthLambdaConfiguration.Carregar();

        Assert.Contains("Server=(local)", configuration.ConnectionString);
        Assert.Equal("oficina-mecanica-auth", configuration.JwtOptions.Issuer);
        Assert.Equal("oficina-mecanica-api", configuration.JwtOptions.Audience);
        Assert.Equal(60, configuration.JwtOptions.ExpirationMinutes);
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
