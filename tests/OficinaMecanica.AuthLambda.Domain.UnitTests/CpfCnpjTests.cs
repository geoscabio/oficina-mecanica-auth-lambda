using OficinaMecanica.AuthLambda.Domain.Atendimento.Enums;
using OficinaMecanica.AuthLambda.Domain.Atendimento.ValueObjects;
using OficinaMecanica.AuthLambda.Domain.Shared.Exceptions;
namespace OficinaMecanica.AuthLambda.Domain.UnitTests;

public sealed class CpfCnpjTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725", TipoDocumento.CPF)]
    [InlineData("52998224725", "52998224725", TipoDocumento.CPF)]
    [InlineData("04.252.011/0001-10", "04252011000110", TipoDocumento.CNPJ)]
    public void Criar_normaliza_documento_valido(string valor, string numero, TipoDocumento tipo)
    {
        var documento = CpfCnpj.Criar(valor);
        Assert.Equal(numero, documento.Numero);
        Assert.Equal(tipo, documento.Tipo);
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("11111111111")]
    [InlineData("123")]
    [InlineData("52998224724")]
    public void Criar_rejeita_documento_invalido(string? valor)
    {
        Assert.Throws<DomainException>(() => CpfCnpj.Criar(valor!));
    }
}
