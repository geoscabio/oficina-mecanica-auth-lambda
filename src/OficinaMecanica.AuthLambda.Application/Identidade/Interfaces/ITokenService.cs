namespace OficinaMecanica.AuthLambda.Application.Identidade.Interfaces;

public interface ITokenService
{
    string GerarToken(Guid clienteId);
}
