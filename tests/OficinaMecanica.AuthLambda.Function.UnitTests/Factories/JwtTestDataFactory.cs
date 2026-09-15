using OficinaMecanica.AuthLambda.Infrastructure.Identidade.Options;

namespace OficinaMecanica.AuthLambda.Function.UnitTests.Factories;

internal static class JwtTestDataFactory
{
    public const string IssuerPadrao = "oficina-mecanica-auth";
    public const string AudiencePadrao = "oficina-mecanica-api";
    public const string SecretPadrao = "01234567890123456789012345678901";
    public const string SecretInvalido = "segredo-curto";
    public const int ExpirationMinutesPadrao = 30;
    public const int ExpiresInPadrao = 1800;

    public static readonly Guid ClienteIdPadrao = Guid.Parse("a1c9b672-6258-4a3f-8f1a-742ec5090d8f");

    public static JwtOptions CriarJwtOptionsValido(int expirationMinutes = ExpirationMinutesPadrao)
    {
        return new JwtOptions
        {
            Issuer = IssuerPadrao,
            Audience = AudiencePadrao,
            Secret = SecretPadrao,
            ExpirationMinutes = expirationMinutes
        };
    }

    public static JwtOptions CriarJwtOptionsComSecretInvalido()
    {
        return new JwtOptions
        {
            Issuer = IssuerPadrao,
            Audience = AudiencePadrao,
            Secret = SecretInvalido,
            ExpirationMinutes = ExpirationMinutesPadrao
        };
    }
}
