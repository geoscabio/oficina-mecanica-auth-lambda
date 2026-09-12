using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;

namespace OficinaMecanica.AuthLambda.Application.Autenticacao;

public sealed record ClienteAutenticacao(Guid Id, StatusCliente Status);
