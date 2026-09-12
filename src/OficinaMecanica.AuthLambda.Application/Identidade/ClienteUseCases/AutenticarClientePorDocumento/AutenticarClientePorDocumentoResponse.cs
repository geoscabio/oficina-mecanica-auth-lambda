namespace OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;

public sealed record AutenticarClientePorDocumentoResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn);
