namespace OficinaMecanica.AuthLambda.Function.Configuration;

internal sealed record AuthLambdaObservabilityConfiguration(
    string Environment,
    string Service,
    string Version)
{
    public static AuthLambdaObservabilityConfiguration Carregar()
    {
        return new AuthLambdaObservabilityConfiguration(
            GetEnvironmentVariableOrDefault("DD_ENV", "development"),
            GetEnvironmentVariableOrDefault("DD_SERVICE", "oficina-mecanica-auth-lambda"),
            GetEnvironmentVariableOrDefault("DD_VERSION", "unknown"));
    }

    private static string GetEnvironmentVariableOrDefault(string name, string defaultValue)
    {
        var value = System.Environment.GetEnvironmentVariable(name);

        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}
