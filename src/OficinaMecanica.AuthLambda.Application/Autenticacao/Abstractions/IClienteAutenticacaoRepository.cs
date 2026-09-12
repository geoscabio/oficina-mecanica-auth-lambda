using OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.AuthLambda.Application.Autenticacao.Abstractions;

public interface IClienteAutenticacaoRepository
{
    Task<ClienteAutenticacao?> ObterAsync(CpfCnpj documento, CancellationToken cancellationToken);
}
