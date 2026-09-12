using System.Text.Json.Serialization;
namespace OficinaMecanica.AuthLambda.Application.Common;

public enum TipoErro
{
    Validacao = 1, NaoAutorizado = 4, ErroInterno = 5
}
public sealed record ErrorResponse(string Mensagem, TipoErro Tipo, [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyCollection<string>? Erros = null);
public sealed class Result<T>
{
    private Result(bool sucesso, T? valor, ErrorResponse? erro) => (Sucesso, Valor, Erro) = (sucesso, valor, erro);
    public bool Sucesso
    {
        get;
    }
    public T? Valor
    {
        get;
    }
    public ErrorResponse? Erro
    {
        get;
    }
    public static Result<T> Ok(T valor) => new(true, valor, null);
    public static Result<T> Falha(string mensagem, TipoErro tipo) => new(false, default, new ErrorResponse(mensagem, tipo));
    public static Result<T> Falha(IReadOnlyCollection<string> mensagens, TipoErro tipo) => new(false, default, new ErrorResponse(mensagens.FirstOrDefault() ?? "Requisição inválida.", tipo, mensagens));
}
