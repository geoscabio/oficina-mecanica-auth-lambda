using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Messages;
using OficinaMecanica.AuthLambda.Domain.Shared.Exceptions;
namespace OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;

public sealed record CpfCnpj
{
    private static readonly int[] PrimeiroDigitoCnpjPesos = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] SegundoDigitoCnpjPesos = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    private CpfCnpj(string numero, TipoDocumento tipo)
    {
        Numero = numero;
        Tipo = tipo;
    }

    public string Numero
    {
        get;
    }

    public TipoDocumento Tipo
    {
        get;
    }

    public static CpfCnpj Criar(string numero)
    {
        var normalizado = Normalizar(numero);
        return normalizado.Length switch
        {
            11 when CpfValido(normalizado) => new(normalizado, TipoDocumento.CPF),
            14 when CnpjValido(normalizado) => new(normalizado, TipoDocumento.CNPJ),
            _ => throw new DomainException(ClienteErrorMessages.DocumentoInvalido)
        };
    }

    private static string Normalizar(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            return string.Empty;
        }

        return new string(numero.Where(char.IsDigit).ToArray());
    }

    private static bool CpfValido(string numero)
    {
        if (TodosDigitosIguais(numero))
        {
            return false;
        }

        var primeiroDigito = CalcularDigito(numero[..9], 10);
        var segundoDigito = CalcularDigito(numero[..10], 11);

        return numero[9] == DigitoParaChar(primeiroDigito)
            && numero[10] == DigitoParaChar(segundoDigito);
    }

    private static bool CnpjValido(string numero)
    {
        if (TodosDigitosIguais(numero))
        {
            return false;
        }

        var primeiroDigito = CalcularDigitoCnpj(numero[..12], PrimeiroDigitoCnpjPesos);
        var segundoDigito = CalcularDigitoCnpj(numero[..13], SegundoDigitoCnpjPesos);

        return numero[12] == DigitoParaChar(primeiroDigito)
            && numero[13] == DigitoParaChar(segundoDigito);
    }

    private static bool TodosDigitosIguais(string numero)
    {
        return numero.All(digito => digito == numero[0]);
    }

    private static int CalcularDigito(string numero, int pesoInicial)
    {
        var soma = 0;

        for (var indice = 0; indice < numero.Length; indice++)
        {
            soma += (numero[indice] - '0') * (pesoInicial - indice);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static int CalcularDigitoCnpj(string numero, int[] pesos)
    {
        var soma = 0;

        for (var indice = 0; indice < numero.Length; indice++)
        {
            soma += (numero[indice] - '0') * pesos[indice];
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static char DigitoParaChar(int digito)
    {
        return (char)('0' + digito);
    }
}
