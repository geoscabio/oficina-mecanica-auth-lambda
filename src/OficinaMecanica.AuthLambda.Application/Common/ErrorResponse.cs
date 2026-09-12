using System.Text.Json.Serialization;

namespace OficinaMecanica.AuthLambda.Application.Common;

public sealed record ErrorResponse(
    string Mensagem,
    TipoErro Tipo,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyCollection<string>? Erros = null);
