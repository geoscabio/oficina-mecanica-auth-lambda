using OficinaMecanica.AuthLambda.Application.Identidade.Models;

namespace OficinaMecanica.AuthLambda.Application.Identidade.Interfaces;

public interface ITokenService
{
    TokenGerado GerarToken(Guid clienteId);
}
