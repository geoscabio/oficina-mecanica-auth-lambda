namespace OficinaMecanica.AuthLambda.Domain;
public enum TipoDocumento { CPF = 1, CNPJ = 2 }
public enum StatusCliente { Ativo = 1, Inativo = 2 }
public sealed record Documento(string Numero, TipoDocumento Tipo)
{
    public static bool TentarCriar(string? valor, out Documento? documento)
    {
        var numero = new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
        documento = numero.Length == 11 && CpfValido(numero) ? new(numero, TipoDocumento.CPF) : numero.Length == 14 && CnpjValido(numero) ? new(numero, TipoDocumento.CNPJ) : null;
        return documento is not null;
    }
    private static bool CpfValido(string n) => !Iguais(n) && Digito(n[..9], 10) == n[9] - '0' && Digito(n[..10], 11) == n[10] - '0';
    private static bool CnpjValido(string n) => !Iguais(n) && Digito(n[..12], [5,4,3,2,9,8,7,6,5,4,3,2]) == n[12] - '0' && Digito(n[..13], [6,5,4,3,2,9,8,7,6,5,4,3,2]) == n[13] - '0';
    private static bool Iguais(string n) => n.All(x => x == n[0]);
    private static int Digito(string n, int peso) => Ajustar(n.Select((x, i) => (x - '0') * (peso - i)).Sum());
    private static int Digito(string n, int[] pesos) => Ajustar(n.Select((x, i) => (x - '0') * pesos[i]).Sum());
    private static int Ajustar(int soma) { var resto = soma % 11; return resto < 2 ? 0 : 11 - resto; }
}
