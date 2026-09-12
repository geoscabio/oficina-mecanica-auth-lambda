using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.AuthLambda.Function.UnitTests.Factories;
using OficinaMecanica.AuthLambda.Infrastructure.Identidade.Services;

namespace OficinaMecanica.AuthLambda.Function.UnitTests.Identidade.Services;

public sealed class TokenServiceTests
{
    [Fact]
    public void Dado_ClienteIdValido_Quando_GerarToken_Entao_DeveRetornarAccessTokenEExpiresInConfigurado()
    {
        // Arrange
        var options = JwtTestDataFactory.CriarJwtOptionsValido(expirationMinutes: 30);
        var service = new TokenService(options);

        // Act
        var tokenGerado = service.GerarToken(JwtTestDataFactory.ClienteIdPadrao);

        // Assert
        tokenGerado.AccessToken.Should().NotBeNullOrWhiteSpace();
        tokenGerado.ExpiresIn.Should().Be(JwtTestDataFactory.ExpiresInPadrao);
    }

    [Fact]
    public void Dado_ClienteIdValido_Quando_GerarToken_Entao_DeveGerarClaimsEsperadas()
    {
        // Arrange
        var options = JwtTestDataFactory.CriarJwtOptionsValido();
        var service = new TokenService(options);

        // Act
        var tokenGerado = service.GerarToken(JwtTestDataFactory.ClienteIdPadrao);
        var token = LerToken(tokenGerado.AccessToken);

        // Assert
        token.Issuer.Should().Be(JwtTestDataFactory.IssuerPadrao);
        token.Audiences.Should().ContainSingle(JwtTestDataFactory.AudiencePadrao);
        token.Header.Alg.Should().Be(SecurityAlgorithms.HmacSha256);
        token.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Sub
            && claim.Value == JwtTestDataFactory.ClienteIdPadrao.ToString());
        token.Claims.Should().Contain(claim => claim.Type == "cliente_id"
            && claim.Value == JwtTestDataFactory.ClienteIdPadrao.ToString());
        token.Claims.Should().Contain(claim => claim.Type == "role" && claim.Value == "Cliente");
        token.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Jti
            && EhGuid(claim.Value));
    }

    [Fact]
    public void Dado_ClienteIdValido_Quando_GerarToken_Entao_NaoDeveConterDocumentoCpfOuCnpj()
    {
        // Arrange
        var options = JwtTestDataFactory.CriarJwtOptionsValido();
        var service = new TokenService(options);

        // Act
        var tokenGerado = service.GerarToken(JwtTestDataFactory.ClienteIdPadrao);
        var token = LerToken(tokenGerado.AccessToken);

        // Assert
        token.Claims.Should().NotContain(claim => claim.Type == "documento");
        token.Claims.Should().NotContain(claim => claim.Type == "cpf");
        token.Claims.Should().NotContain(claim => claim.Type == "cnpj");
    }

    [Fact]
    public void Dado_ConfiguracaoJwtInvalida_Quando_GerarToken_Entao_DeveFalharSemExporSecret()
    {
        // Arrange
        var options = JwtTestDataFactory.CriarJwtOptionsComSecretInvalido();
        var service = new TokenService(options);

        // Act
        var acao = () => service.GerarToken(JwtTestDataFactory.ClienteIdPadrao);

        // Assert
        acao.Should()
            .Throw<InvalidOperationException>()
            .Which.Message.Should().NotContain(JwtTestDataFactory.SecretInvalido);
    }

    private static JwtSecurityToken LerToken(string accessToken)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
    }

    private static bool EhGuid(string valor)
    {
        return Guid.TryParse(valor, out _);
    }
}
