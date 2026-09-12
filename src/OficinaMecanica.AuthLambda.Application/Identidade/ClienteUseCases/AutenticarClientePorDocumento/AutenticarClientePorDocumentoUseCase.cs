using FluentValidation;
using OficinaMecanica.AuthLambda.Application.Common;
using OficinaMecanica.AuthLambda.Application.Identidade.Interfaces;
using OficinaMecanica.AuthLambda.Application.Identidade.Repositories;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Messages;
using OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;
using OficinaMecanica.AuthLambda.Domain.Shared.Exceptions;

namespace OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;

public sealed class AutenticarClientePorDocumentoUseCase(
    IClienteAutenticacaoRepository repositorio,
    ITokenService tokens,
    IValidator<AutenticarClientePorDocumentoRequest> validator)
{
    public async Task<Result<AutenticarClientePorDocumentoResponse>> ExecutarAsync(
        AutenticarClientePorDocumentoRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result<AutenticarClientePorDocumentoResponse>.Falha(
                ["Requisição inválida."],
                TipoErro.Validacao);
        }

        var validacao = await validator.ValidateAsync(request!, cancellationToken);
        if (!validacao.IsValid)
        {
            return Result<AutenticarClientePorDocumentoResponse>.Falha(
                validacao.ObterMensagensErro(),
                TipoErro.Validacao);
        }

        try
        {
            var documento = CpfCnpj.Criar(request!.Documento!);
            var cliente = await repositorio.ObterAsync(documento, cancellationToken);

            if (cliente is null || cliente.Status != StatusCliente.Ativo)
            {
                return Result<AutenticarClientePorDocumentoResponse>.Falha(
                    "Documento não autorizado.",
                    TipoErro.NaoAutorizado);
            }

            var token = tokens.GerarToken(cliente.Id);

            return Result<AutenticarClientePorDocumentoResponse>.Ok(
                new AutenticarClientePorDocumentoResponse(
                    token.AccessToken,
                    "Bearer",
                    token.ExpiresIn));
        }
        catch (DomainException exception)
        {
            return Result<AutenticarClientePorDocumentoResponse>.Falha(
                exception.Message,
                TipoErro.Validacao);
        }
    }
}
