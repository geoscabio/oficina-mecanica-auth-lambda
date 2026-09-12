using FluentValidation;
using OficinaMecanica.AuthLambda.Application.Common;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Messages;
using OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;
using OficinaMecanica.AuthLambda.Domain.Shared.Exceptions;
namespace OficinaMecanica.AuthLambda.Application.Autenticacao;
public sealed record AutenticarClientePorDocumentoRequest(string? Documento);
public sealed record AutenticarClientePorDocumentoResponse(string AccessToken, string TokenType, int ExpiresIn);
public sealed record ClienteAutenticacao(Guid Id, StatusCliente Status);
public interface IClienteAutenticacaoRepository { Task<ClienteAutenticacao?> ObterAsync(CpfCnpj documento, CancellationToken cancellationToken); }
public interface ITokenService { string GerarToken(Guid clienteId); }
public sealed class AutenticarClientePorDocumentoValidator : AbstractValidator<AutenticarClientePorDocumentoRequest>
{ public AutenticarClientePorDocumentoValidator() { RuleFor(x => x.Documento).NotEmpty().WithMessage(ClienteErrorMessages.DocumentoObrigatorio); } }
public sealed class AutenticarClientePorDocumentoUseCase(IClienteAutenticacaoRepository repositorio, ITokenService tokens, IValidator<AutenticarClientePorDocumentoRequest> validator)
{ public async Task<Result<AutenticarClientePorDocumentoResponse>> ExecutarAsync(AutenticarClientePorDocumentoRequest? request, CancellationToken ct) { if (request is null) return Result<AutenticarClientePorDocumentoResponse>.Falha(["Requisição inválida."], TipoErro.Validacao); var validacao = await validator.ValidateAsync(request, ct); if (!validacao.IsValid) return Result<AutenticarClientePorDocumentoResponse>.Falha(validacao.ObterMensagensErro(), TipoErro.Validacao); CpfCnpj documento; try { documento = CpfCnpj.Criar(request.Documento!); } catch (DomainException) { return Result<AutenticarClientePorDocumentoResponse>.Falha(["Requisição inválida."], TipoErro.Validacao); } var cliente = await repositorio.ObterAsync(documento, ct); if (cliente is null || cliente.Status != StatusCliente.Ativo) return Result<AutenticarClientePorDocumentoResponse>.Falha("Documento não autorizado.", TipoErro.NaoAutorizado); return Result<AutenticarClientePorDocumentoResponse>.Ok(new(tokens.GerarToken(cliente.Id), "Bearer", 3600)); } }
