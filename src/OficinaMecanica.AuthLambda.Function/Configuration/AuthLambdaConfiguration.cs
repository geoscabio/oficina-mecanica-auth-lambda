using OficinaMecanica.AuthLambda.Infrastructure.Identidade.Options;

namespace OficinaMecanica.AuthLambda.Function.Configuration;

internal sealed class AuthLambdaConfiguration
{
    private const int DefaultExpirationMinutes = 60;

    private AuthLambdaConfiguration(string connectionString, JwtOptions jwtOptions)
    {
        ConnectionString = connectionString;
        JwtOptions = jwtOptions;
    }

    public string ConnectionString
    {
        get;
    }

    public JwtOptions JwtOptions
    {
        get;
    }

    public static AuthLambdaConfiguration Carregar()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Configuração de banco de dados ausente.");
        }

        var jwtOptions = new JwtOptions
        {
            Issuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? string.Empty,
            Audience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? string.Empty,
            Secret = Environment.GetEnvironmentVariable("Jwt__Secret") ?? string.Empty,
            ExpirationMinutes = ObterExpirationMinutes()
        };

        jwtOptions.Validar();

        return new AuthLambdaConfiguration(connectionString, jwtOptions);
    }

    private static int ObterExpirationMinutes()
    {
        var expirationValue = Environment.GetEnvironmentVariable("Jwt__ExpirationMinutes");
        if (!int.TryParse(expirationValue, out var expirationMinutes))
        {
            return DefaultExpirationMinutes;
        }

        if (expirationMinutes <= 0)
        {
            return DefaultExpirationMinutes;
        }

        return expirationMinutes;
    }
}
