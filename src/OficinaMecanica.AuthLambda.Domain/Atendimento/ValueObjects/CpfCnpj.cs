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
        var normalizado = new string((numero ?? string.Empty).Where(char.IsDigit).ToArray());
        return normalizado.Length switch
        {
            11 when CpfValido(normalizado) => new(normalizado, TipoDocumento.CPF),
            14 when CnpjValido(normalizado) => new(normalizado, TipoDocumento.CNPJ),
            _ => throw new DomainException(ClienteErrorMessages.DocumentoInvalido)
        };
    }
    private static bool CpfValido(string numero)
    {
        if (TodosDigitosIguais(numero))
            return false;
        return CalcularDigito(numero[..9], 10) == numero[9] - '0'
            && CalcularDigito(numero[..10], 11) == numero[10] - '0';
    }

    private static bool CnpjValido(string numero)
    {
        if (TodosDigitosIguais(numero))
            return false;
        return CalcularDigito(numero[..12], PrimeiroDigitoCnpjPesos) == numero[12] - '0'
            && CalcularDigito(numero[..13], SegundoDigitoCnpjPesos) == numero[13] - '0';
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

        return Ajustar(soma);
    }

    private static int CalcularDigito(string numero, int[] pesos)
    {
        var soma = 0;

        for (var indice = 0; indice < numero.Length; indice++)
        {
            soma += (numero[indice] - '0') * pesos[indice];
        }

        return Ajustar(soma);
    }
    private static int Ajustar(int soma)
    {
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
