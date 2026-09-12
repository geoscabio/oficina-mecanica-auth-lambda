using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;
using OficinaMecanica.AuthLambda.Domain.Atendimento.Messages;
using OficinaMecanica.AuthLambda.Domain.Shared.Exceptions;
namespace OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;
public sealed record CpfCnpj
{
    private CpfCnpj(string numero, TipoDocumento tipo) => (Numero, Tipo) = (numero, tipo);
    public string Numero { get; }
    public TipoDocumento Tipo { get; }
    public static CpfCnpj Criar(string numero)
    {
        var normalizado = new string((numero ?? string.Empty).Where(char.IsDigit).ToArray());
        return normalizado.Length switch { 11 when CpfValido(normalizado) => new(normalizado, TipoDocumento.CPF), 14 when CnpjValido(normalizado) => new(normalizado, TipoDocumento.CNPJ), _ => throw new DomainException(ClienteErrorMessages.DocumentoInvalido) };
    }
    private static bool CpfValido(string n) => !Iguais(n) && Calcular(n[..9], 10) == n[9] - '0' && Calcular(n[..10], 11) == n[10] - '0';
    private static bool CnpjValido(string n) => !Iguais(n) && Calcular(n[..12], [5,4,3,2,9,8,7,6,5,4,3,2]) == n[12] - '0' && Calcular(n[..13], [6,5,4,3,2,9,8,7,6,5,4,3,2]) == n[13] - '0';
    private static bool Iguais(string n) => n.All(d => d == n[0]);
    private static int Calcular(string n, int peso) => Ajustar(n.Select((d, i) => (d - '0') * (peso - i)).Sum());
    private static int Calcular(string n, int[] pesos) => Ajustar(n.Select((d, i) => (d - '0') * pesos[i]).Sum());
    private static int Ajustar(int soma) { var resto = soma % 11; return resto < 2 ? 0 : 11 - resto; }
}
