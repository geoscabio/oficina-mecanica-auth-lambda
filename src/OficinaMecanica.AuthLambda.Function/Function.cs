using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OficinaMecanica.AuthLambda.Application.Common;
using OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;
using OficinaMecanica.AuthLambda.Function.Configuration;

namespace OficinaMecanica.AuthLambda.Function;

public sealed class Function
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private const int MaxCorrelationIdLength = 128;
    private readonly AutenticarClientePorDocumentoUseCase _useCase;
    private readonly ILogger<Function> _logger;
    private readonly AuthLambdaObservabilityConfiguration _observabilityConfiguration;

    public Function()
    {
        var dependencies = FunctionStartup.CriarDependencias();
        _useCase = dependencies.UseCase;
        _logger = dependencies.Logger;
        _observabilityConfiguration = dependencies.ObservabilityConfiguration;
    }

    public Function(AutenticarClientePorDocumentoUseCase useCase)
        : this(useCase, NullLogger<Function>.Instance, AuthLambdaObservabilityConfiguration.Carregar())
    {
    }

    internal Function(
        AutenticarClientePorDocumentoUseCase useCase,
        ILogger<Function> logger,
        AuthLambdaObservabilityConfiguration observabilityConfiguration)
    {
        _useCase = useCase;
        _logger = logger;
        _observabilityConfiguration = observabilityConfiguration;
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> Handler(APIGatewayHttpApiV2ProxyRequest request)
    {
        var correlationId = GetOrCreateCorrelationId(request.Headers);

        using var loggingScope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["service"] = _observabilityConfiguration.Service,
                ["env"] = _observabilityConfiguration.Environment,
                ["version"] = _observabilityConfiguration.Version,
                ["x_correlation_id"] = correlationId
            });

        try
        {
            var input = Desserializar(request.Body);
            if (input is null)
            {
                return CreateResponse(400, CriarErroRequisicaoInvalida(), correlationId);
            }

            var result = await _useCase.ExecutarAsync(input, default);
            if (result.Sucesso)
            {
                _logger.LogInformation(
                    "Autenticacao processada com sucesso. {operation} {http.status_code}",
                    "AutenticarClientePorDocumento",
                    200);

                return CreateResponse(200, result.Valor!, correlationId);
            }

            var statusCode = result.Erro!.Tipo == TipoErro.NaoAutorizado ? 401 : 400;
            _logger.LogInformation(
                "Autenticacao nao concluida. {operation} {http.status_code}",
                "AutenticarClientePorDocumento",
                statusCode);

            return CreateResponse(statusCode, result.Erro, correlationId);
        }
        catch (JsonException)
        {
            return CreateResponse(400, CriarErroRequisicaoInvalida(), correlationId);
        }
        catch (DependenciaIndisponivelException)
        {
            _logger.LogError(
                "Dependencia indisponivel durante autenticacao. {operation} {http.status_code}",
                "AutenticarClientePorDocumento",
                503);

            return CreateResponse(503, new ErrorResponse("Serviço temporariamente indisponível.", TipoErro.ErroInterno), correlationId);
        }
        catch
        {
            _logger.LogError(
                "Erro inesperado durante autenticacao. {operation} {http.status_code}",
                "AutenticarClientePorDocumento",
                500);

            return CreateResponse(500, new ErrorResponse("Erro interno inesperado.", TipoErro.ErroInterno), correlationId);
        }
    }

    private static AutenticarClientePorDocumentoRequest? Desserializar(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AutenticarClientePorDocumentoRequest>(body, CriarOpcoesJson());
    }

    private static APIGatewayHttpApiV2ProxyResponse CreateResponse(int status, object body, string correlationId)
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = status,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                [CorrelationIdHeaderName] = correlationId
            },
            Body = JsonSerializer.Serialize(body, CriarOpcoesJson())
        };
    }

    private static string GetOrCreateCorrelationId(IDictionary<string, string>? headers)
    {
        var candidate = headers?
            .FirstOrDefault(header => string.Equals(header.Key, CorrelationIdHeaderName, StringComparison.OrdinalIgnoreCase))
            .Value?
            .Trim();

        return IsValidCorrelationId(candidate) ? candidate! : Guid.NewGuid().ToString("N");
    }

    private static bool IsValidCorrelationId(string? candidate)
    {
        return !string.IsNullOrWhiteSpace(candidate)
            && candidate.Length <= MaxCorrelationIdLength
            && candidate.All(character => !char.IsControl(character));
    }

    private static JsonSerializerOptions CriarOpcoesJson()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static ErrorResponse CriarErroRequisicaoInvalida()
    {
        return new ErrorResponse(
            "Requisição inválida.",
            TipoErro.Validacao,
            ["Requisição inválida."]);
    }
}
