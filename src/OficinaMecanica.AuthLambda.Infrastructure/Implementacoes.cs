using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.AuthLambda.Application;
using OficinaMecanica.AuthLambda.Domain;
namespace OficinaMecanica.AuthLambda.Infrastructure;
public sealed class JwtOptions { public string Issuer { get; init; } = "oficina-mecanica-auth"; public string Audience { get; init; } = "oficina-mecanica-api"; public string Secret { get; init; } = string.Empty; public int ExpirationMinutes { get; init; } = 60; }
public sealed class JwtTokenService(JwtOptions options) : ITokenService { public string Gerar(Guid id) { if (Encoding.UTF8.GetByteCount(options.Secret) < 32) throw new InvalidOperationException("JWT inválido."); var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret)); var now = DateTime.UtcNow; return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(options.Issuer, options.Audience, [new(JwtRegisteredClaimNames.Sub,id.ToString()),new("cliente_id",id.ToString()),new(ClaimTypes.Role,"Cliente"),new(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())], now, now.AddMinutes(options.ExpirationMinutes),new SigningCredentials(key,SecurityAlgorithms.HmacSha256))); } }
public sealed class SqlClienteAutenticacaoRepository(string connectionString) : IClienteAutenticacaoRepository { public async Task<ClienteAutenticacao?> ObterAsync(Documento d,CancellationToken ct) { const string sql="SELECT [Id], [Status] FROM [Atendimento].[Clientes] WHERE [Documento] = @documento AND [TipoDocumento] = @tipoDocumento"; await using var c=new SqlConnection(connectionString); await c.OpenAsync(ct); await using var cmd=new SqlCommand(sql,c); cmd.Parameters.AddWithValue("@documento",d.Numero); cmd.Parameters.AddWithValue("@tipoDocumento",(int)d.Tipo); await using var r=await cmd.ExecuteReaderAsync(ct); return await r.ReadAsync(ct)?new(r.GetGuid(0),(StatusCliente)r.GetInt32(1)):null; } }
