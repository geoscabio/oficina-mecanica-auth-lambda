using System.Text;

namespace OficinaMecanica.AuthLambda.Infrastructure.Identidade.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string Secret { get; init; } = string.Empty;

    public int ExpirationMinutes { get; init; } = 60;

    public bool EhValido()
    {
        return !string.IsNullOrWhiteSpace(Issuer)
            && !string.IsNullOrWhiteSpace(Audience)
            && Encoding.UTF8.GetByteCount(Secret) >= 32
            && ExpirationMinutes > 0;
    }

    public void Validar()
    {
        if (!EhValido())
        {
            throw new InvalidOperationException("Configuração JWT inválida.");
        }
    }
}
