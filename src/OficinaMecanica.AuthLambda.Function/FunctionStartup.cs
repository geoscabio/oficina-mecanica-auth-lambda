using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OficinaMecanica.AuthLambda.Application;
using OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;
using OficinaMecanica.AuthLambda.Function.Configuration;
using OficinaMecanica.AuthLambda.Infrastructure;

namespace OficinaMecanica.AuthLambda.Function;

internal static class FunctionStartup
{
    public static AutenticarClientePorDocumentoUseCase CriarUseCase()
    {
        return CriarDependencias().UseCase;
    }

    public static FunctionDependencies CriarDependencias()
    {
        var configuration = AuthLambdaConfiguration.Carregar();
        var observabilityConfiguration = AuthLambdaObservabilityConfiguration.Carregar();

        var services = new ServiceCollection();
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddJsonConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "O";
                options.UseUtcTimestamp = true;
            });
        });
        services.AddAuthLambdaApplication();
        services.AddAuthLambdaInfrastructure(
            configuration.ConnectionString,
            configuration.JwtOptions);

        var serviceProvider = services.BuildServiceProvider();

        return new FunctionDependencies(
            serviceProvider.GetRequiredService<AutenticarClientePorDocumentoUseCase>(),
            serviceProvider.GetRequiredService<ILogger<Function>>(),
            observabilityConfiguration);
    }
}

internal sealed record FunctionDependencies(
    AutenticarClientePorDocumentoUseCase UseCase,
    ILogger<Function> Logger,
    AuthLambdaObservabilityConfiguration ObservabilityConfiguration);
