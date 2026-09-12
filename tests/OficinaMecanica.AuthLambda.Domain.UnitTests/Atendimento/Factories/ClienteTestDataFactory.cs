namespace OficinaMecanica.AuthLambda.Domain.UnitTests.Atendimento.Factories;

internal static class ClienteTestDataFactory
{
    public const string DocumentoPadrao = "529.982.247-25";
    public const string DocumentoNormalizadoPadrao = "52998224725";

    public const string CnpjPadrao = "04.252.011/0001-10";
    public const string CnpjNormalizadoPadrao = "04252011000110";

    public const string DocumentoCpfComDigitosIguais = "111.111.111-11";
    public const string DocumentoCnpjInvalido = "12.345.678/0001-99";
    public const string DocumentoCurto = "123";
}
