using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;

namespace OficinaMecanica.AuthLambda.Application.UnitTests.Common;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void Dado_ServiceCollectionVazia_Quando_AdicionarApplication_Entao_DeveRegistrarValidatorEUseCase()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAuthLambdaApplication();

        var validatorDescriptor = services.Single(
            service => service.ServiceType == typeof(IValidator<AutenticarClientePorDocumentoRequest>));

        var useCaseDescriptor = services.Single(
            service => service.ServiceType == typeof(AutenticarClientePorDocumentoUseCase));

        // Assert
        validatorDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        validatorDescriptor.ImplementationType.Should().Be(typeof(AutenticarClientePorDocumentoValidator));

        useCaseDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        useCaseDescriptor.ImplementationType.Should().Be(typeof(AutenticarClientePorDocumentoUseCase));
    }
}
