using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.AuthLambda.Application.Identidade.Interfaces;
using OficinaMecanica.AuthLambda.Application.Identidade.Repositories;
using OficinaMecanica.AuthLambda.Infrastructure.Atendimento.Repositories;
using OficinaMecanica.AuthLambda.Infrastructure.Identidade.Options;
using OficinaMecanica.AuthLambda.Infrastructure.Identidade.Services;

namespace OficinaMecanica.AuthLambda.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthLambdaInfrastructure(
        this IServiceCollection services,
        string connectionString,
        JwtOptions jwtOptions)
    {
        services.AddSingleton(jwtOptions);

        services.AddSingleton<IClienteAutenticacaoRepository>(
            new SqlClienteAutenticacaoRepository(connectionString));

        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
