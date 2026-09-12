using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using OficinaMecanica.AuthLambda.Application.Autenticacao;
using OficinaMecanica.AuthLambda.Application.Common;
namespace OficinaMecanica.AuthLambda.Function;

public sealed class Function
{
    private readonly AutenticarClientePorDocumentoUseCase _useCase;

    public Function()
        : this(Bootstrap.CriarUseCase())
    {
    }

    public Function(AutenticarClientePorDocumentoUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> Handler(APIGatewayHttpApiV2ProxyRequest request)
    {
        try
        {
            var input = Desserializar(request.Body);
            if (input is null)
            {
                return Json(400, new ErrorResponse(
                    "Requisição inválida.",
                    TipoErro.Validacao,
                    ["Requisição inválida."]));
            }

            var result = await _useCase.ExecutarAsync(input, default);
            if (result.Sucesso)
            {
                return Json(200, result.Valor!);
            }

            var statusCode = result.Erro!.Tipo == TipoErro.NaoAutorizado ? 401 : 400;
            return Json(statusCode, result.Erro);
        }
        catch (JsonException)
        {
            return Json(400, new ErrorResponse("Requisição inválida.", TipoErro.Validacao, ["Requisição inválida."]));
        }
        catch
        {
            return Json(500, new ErrorResponse("Erro interno inesperado.", TipoErro.ErroInterno));
        }
    }
    private static AutenticarClientePorDocumentoRequest? Desserializar(string? body) => string.IsNullOrWhiteSpace(body) ? null : JsonSerializer.Deserialize<AutenticarClientePorDocumentoRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    private static APIGatewayHttpApiV2ProxyResponse Json(int status, object body) => new() { StatusCode = status, Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }, Body = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } }) };
}
