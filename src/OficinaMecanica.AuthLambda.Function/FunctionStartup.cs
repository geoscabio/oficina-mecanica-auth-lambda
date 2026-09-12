using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.AuthLambda.Application;
using OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;
using OficinaMecanica.AuthLambda.Function.Configuration;
using OficinaMecanica.AuthLambda.Infrastructure;

namespace OficinaMecanica.AuthLambda.Function;

internal static class FunctionStartup
{
    public static AutenticarClientePorDocumentoUseCase CriarUseCase()
    {
        var configuration = AuthLambdaConfiguration.Carregar();

        var services = new ServiceCollection();
        services.AddAuthLambdaApplication();
        services.AddAuthLambdaInfrastructure(
            configuration.ConnectionString,
            configuration.JwtOptions);

        return services.BuildServiceProvider()
            .GetRequiredService<AutenticarClientePorDocumentoUseCase>();
    }
}
