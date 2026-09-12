namespace OficinaMecanica.AuthLambda.Application.Autenticacao.Abstractions;

public interface ITokenService
{
    string GerarToken(Guid clienteId);
}
