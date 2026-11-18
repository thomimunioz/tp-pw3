using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RuteaYa.Api.Autenticacion;

public static class ClaimsPrincipalExtensiones
{
    public static int ObtenerUsuarioId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? usuario.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(valor, out var id)
            ? id
            : throw new InvalidOperationException("El token no trae el id del usuario.");
    }
}