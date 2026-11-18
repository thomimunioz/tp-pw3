using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RuteaYa.Entidades;

namespace RuteaYa.Api.Autenticacion;

public record TokenGenerado(string Token, DateTime ExpiraEn);

public interface IGeneradorTokens
{
    TokenGenerado Generar(Usuario usuario);
}

public class GeneradorTokens : IGeneradorTokens
{
    private readonly JwtOpciones _opciones;

    public GeneradorTokens(IOptions<JwtOpciones> opciones)
    {
        _opciones = opciones.Value;
    }

    public TokenGenerado Generar(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email ?? ""),
            new Claim("nombre", $"{usuario.Nombre} {usuario.Apellido}")
        };

        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Clave));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddMinutes(_opciones.MinutosExpiracion);

        var token = new JwtSecurityToken(
            issuer: _opciones.Emisor,
            audience: _opciones.Audiencia,
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return new TokenGenerado(new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}