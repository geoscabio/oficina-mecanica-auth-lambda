namespace OficinaMecanica.AuthLambda.Application.Identidade.Models;

public sealed record TokenGerado(string AccessToken, int ExpiresIn);
