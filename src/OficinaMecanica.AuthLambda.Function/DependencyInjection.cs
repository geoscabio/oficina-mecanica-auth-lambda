using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OficinaMecanica.AuthLambda.Application.Autenticacao;
using OficinaMecanica.AuthLambda.Application.Autenticacao.Abstractions;
using OficinaMecanica.AuthLambda.Infrastructure.Atendimento;
using OficinaMecanica.AuthLambda.Infrastructure.Identidade;

namespace OficinaMecanica.AuthLambda.Function;

internal static class DependencyInjection
{
    public static AutenticarClientePorDocumentoUseCase CriarUseCase()
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
            ExpirationMinutes = int.TryParse(
                Environment.GetEnvironmentVariable("Jwt__ExpirationMinutes"),
                out var expirationMinutes)
                ? expirationMinutes
                : 60
        };

        jwtOptions.Validar();

        var services = new ServiceCollection();
        services.AddSingleton(jwtOptions);
        services.AddSingleton<IClienteAutenticacaoRepository>(
            new SqlClienteAutenticacaoRepository(connectionString));
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IValidator<AutenticarClientePorDocumentoRequest>,
            AutenticarClientePorDocumentoValidator>();
        services.AddSingleton<AutenticarClientePorDocumentoUseCase>();

        return services.BuildServiceProvider()
            .GetRequiredService<AutenticarClientePorDocumentoUseCase>();
    }
}
