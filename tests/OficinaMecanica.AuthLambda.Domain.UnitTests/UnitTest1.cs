namespace OficinaMecanica.AuthLambda.Domain.UnitTests;

using OficinaMecanica.AuthLambda.Domain;
public class DocumentoTests { [Theory][InlineData("529.982.247-25", "52998224725", TipoDocumento.CPF)][InlineData("04.252.011/0001-10", "04252011000110", TipoDocumento.CNPJ)] public void Normaliza_documento_valido(string entrada,string numero,TipoDocumento tipo) { Assert.True(Documento.TentarCriar(entrada,out var d)); Assert.Equal(numero,d!.Numero); Assert.Equal(tipo,d.Tipo); } [Theory][InlineData("11111111111")][InlineData("123")][InlineData("")][InlineData(null)] public void Rejeita_documento_invalido(string? entrada)=>Assert.False(Documento.TentarCriar(entrada,out _)); }
