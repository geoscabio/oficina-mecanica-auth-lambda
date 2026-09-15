using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;

namespace OficinaMecanica.AuthLambda.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthLambdaApplication(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<AutenticarClientePorDocumentoRequest>,
            AutenticarClientePorDocumentoValidator>();

        services.AddSingleton<AutenticarClientePorDocumentoUseCase>();

        return services;
    }
}
