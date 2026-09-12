using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;

namespace OficinaMecanica.AuthLambda.Application.UnitTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddAuthLambdaApplication_registra_validator_e_use_case()
    {
        var services = new ServiceCollection();

        services.AddAuthLambdaApplication();

        var validatorDescriptor = services.Single(
            service => service.ServiceType == typeof(IValidator<AutenticarClientePorDocumentoRequest>));

        var useCaseDescriptor = services.Single(
            service => service.ServiceType == typeof(AutenticarClientePorDocumentoUseCase));

        Assert.Equal(ServiceLifetime.Singleton, validatorDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, useCaseDescriptor.Lifetime);
        Assert.Equal(typeof(AutenticarClientePorDocumentoValidator), validatorDescriptor.ImplementationType);
        Assert.Equal(typeof(AutenticarClientePorDocumentoUseCase), useCaseDescriptor.ImplementationType);
    }
}
