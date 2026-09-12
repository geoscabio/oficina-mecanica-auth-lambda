using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.AuthLambda.Application.Identidade.Interfaces;
using OficinaMecanica.AuthLambda.Infrastructure.Identidade.Options;

namespace OficinaMecanica.AuthLambda.Infrastructure.Identidade.Services;

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(JwtOptions options)
    {
        _options = options;
    }

    public string GerarToken(Guid clienteId)
    {
        _options.Validar();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, clienteId.ToString()),
            new Claim("cliente_id", clienteId.ToString()),
            new Claim("role", "Cliente"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
