using Amazon.Lambda.APIGatewayEvents;
using OficinaMecanica.AuthLambda.Application.Identidade.Models;
using OficinaMecanica.AuthLambda.Application.Identidade.Repositories;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;

namespace OficinaMecanica.AuthLambda.Function.UnitTests.Factories;

internal static class FunctionTestDataFactory
{
    public const string DocumentoCpfValido = "52998224725";
    public const string DocumentoCpfValidoComMascara = "529.982.247-25";
    public const string DocumentoInvalido = "123";
    public const string TokenPadrao = "token";
    public const int ExpiresInPadrao = 1800;
    public const string BodyVazio = "";
    public const string JsonInvalido = "{";
    public const string BodySemDocumento = "{}";
    public const string BodyComDocumentoInvalido = "{\"documento\":\"123\"}";
    public const string BodyComDocumentoValido = "{\"documento\":\"52998224725\"}";
    public const string BodyComDocumentoValidoComMascara = "{\"documento\":\"529.982.247-25\"}";

    public static readonly Guid ClienteIdPadrao = Guid.Parse("a80e1974-f2c1-4dd1-8b30-a9878a8a874d");

    public static APIGatewayHttpApiV2ProxyRequest CriarRequestApiGateway(string body)
    {
        return new APIGatewayHttpApiV2ProxyRequest
        {
            Body = body
        };
    }

    public static ClienteAutenticacao CriarClienteAtivo(Guid? clienteId = null)
    {
        return new ClienteAutenticacao(clienteId ?? ClienteIdPadrao, StatusCliente.Ativo);
    }

    public static ClienteAutenticacao CriarClienteInativo(Guid? clienteId = null)
    {
        return new ClienteAutenticacao(clienteId ?? ClienteIdPadrao, StatusCliente.Inativo);
    }

    public static TokenGerado CriarTokenGerado(string accessToken = TokenPadrao, int expiresIn = ExpiresInPadrao)
    {
        return new TokenGerado(accessToken, expiresIn);
    }
}
