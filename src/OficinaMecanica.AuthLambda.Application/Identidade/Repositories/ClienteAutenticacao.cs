using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;

namespace OficinaMecanica.AuthLambda.Application.Identidade.Repositories;

public sealed record ClienteAutenticacao(Guid Id, StatusCliente Status);
