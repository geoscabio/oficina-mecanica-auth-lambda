using OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.AuthLambda.Application.Identidade.Repositories;

public interface IClienteAutenticacaoRepository
{
    Task<ClienteAutenticacao?> ObterAsync(CpfCnpj documento, CancellationToken cancellationToken);
}
