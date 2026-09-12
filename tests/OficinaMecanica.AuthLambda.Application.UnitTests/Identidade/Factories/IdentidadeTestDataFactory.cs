using OficinaMecanica.AuthLambda.Application.Identidade.ClienteUseCases.AutenticarClientePorDocumento;
using OficinaMecanica.AuthLambda.Application.Identidade.Repositories;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;

namespace OficinaMecanica.AuthLambda.Application.UnitTests.Identidade.Factories;

internal static class IdentidadeTestDataFactory
{
    public const string DocumentoCpfValido = "529.982.247-25";
    public const string DocumentoCpfNormalizado = "52998224725";
    public const string DocumentoCnpjValido = "04.252.011/0001-10";
    public const string TokenPadrao = "token";

    public static readonly Guid ClienteIdPadrao = Guid.Parse("7c44f1de-f89d-4d96-a7d9-b2422a7d8f6e");

    public static AutenticarClientePorDocumentoRequest CriarRequestValido(string documento = DocumentoCpfValido)
    {
        return new AutenticarClientePorDocumentoRequest(documento);
    }

    public static ClienteAutenticacao CriarClienteAtivo(Guid? clienteId = null)
    {
        return new ClienteAutenticacao(clienteId ?? ClienteIdPadrao, StatusCliente.Ativo);
    }

    public static ClienteAutenticacao CriarClienteInativo(Guid? clienteId = null)
    {
        return new ClienteAutenticacao(clienteId ?? ClienteIdPadrao, StatusCliente.Inativo);
    }
}
