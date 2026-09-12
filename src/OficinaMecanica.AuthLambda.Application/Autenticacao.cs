using OficinaMecanica.AuthLambda.Domain;
namespace OficinaMecanica.AuthLambda.Application;
public sealed record ClienteAutenticacao(Guid Id, StatusCliente Status);
public interface IClienteAutenticacaoRepository { Task<ClienteAutenticacao?> ObterAsync(Documento documento, CancellationToken cancellationToken); }
public interface ITokenService { string Gerar(Guid clienteId); }
public enum ResultadoTipo { Sucesso, Invalido, NaoAutorizado, Indisponivel }
public sealed record ResultadoAutenticacao(ResultadoTipo Tipo, string? Token = null);
public sealed class AutenticarClientePorDocumento(IClienteAutenticacaoRepository repositorio, ITokenService tokenService)
{
    public async Task<ResultadoAutenticacao> ExecutarAsync(string? valor, CancellationToken ct)
    { if (!Documento.TentarCriar(valor, out var documento)) return new(ResultadoTipo.Invalido); try { var cliente = await repositorio.ObterAsync(documento!, ct); return cliente?.Status == StatusCliente.Ativo ? new(ResultadoTipo.Sucesso, tokenService.Gerar(cliente.Id)) : new(ResultadoTipo.NaoAutorizado); } catch { return new(ResultadoTipo.Indisponivel); } }
}
