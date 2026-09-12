namespace OficinaMecanica.AuthLambda.Application.Autenticacao;

public sealed record AutenticarClientePorDocumentoResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn);
