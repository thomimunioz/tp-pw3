using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuteaYa.Api.Auth;
using RuteaYa.Api.Dtos;
using RuteaYa.Entidades;
using RuteaYa.Logica.Account;

namespace RuteaYa.Api.Controllers;

[ApiController]
[Route("api/autenticacion")]
public class AutenticacionController : ControllerBase
{
    private readonly IAccountServicios _account;
    private readonly IGeneradorTokens _tokens;

    public AutenticacionController(IAccountServicios account, IGeneradorTokens tokens)
    {
        _account = account;
        _tokens = tokens;
    }

    // POST api/auth/registro
    [HttpPost("registro")]
    [ProducesResponseType(typeof(AuthRespuestaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthRespuestaDto>> Registrar(RegistroRequest request)
    {
        var resultado = await _account.RegistrarAsync(
            request.Nombre, request.Apellido, request.Email, request.Password);

        if (!resultado.Exito || resultado.Usuario == null)
        {
            var detalle = string.Join(" ", resultado.Errores);

            // AccountServicios traduce el error de email repetido a "Ya existe una cuenta con ese email"
            if (resultado.Errores.Any(e => e.StartsWith("Ya existe")))
                return Conflict(new ProblemDetails
                {
                    Title = "Email ya registrado",
                    Detail = detalle,
                    Status = StatusCodes.Status409Conflict
                });

            return BadRequest(new ProblemDetails
            {
                Title = "No se pudo registrar el usuario",
                Detail = detalle,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return StatusCode(StatusCodes.Status201Created, CrearRespuesta(resultado.Usuario));
    }

    // POST api/auth/ingresar
    [HttpPost("ingresar")]
    [ProducesResponseType(typeof(AuthRespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthRespuestaDto>> Login(LoginRequest request)
    {
        var usuario = await _account.ValidarCredencialesAsync(request.Email, request.Password);
        if (usuario == null)
        {
            // Mensaje genérico: no se revela si falló el email o la contraseña
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciales incorrectas",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        return Ok(CrearRespuesta(usuario));
    }

    // GET api/auth/perfil  (requiere token)
    [Authorize]
    [HttpGet("perfil")]
    [ProducesResponseType(typeof(UsuarioActualDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UsuarioActualDto> Perfil()
    {
        var nombre = User.FindFirst("nombre")?.Value ?? "";
        return Ok(new UsuarioActualDto(User.ObtenerUsuarioId(), nombre));
    }

    private AuthRespuestaDto CrearRespuesta(Usuario usuario)
    {
        var token = _tokens.Generar(usuario);
        return new AuthRespuestaDto(
            token.Token, token.ExpiraEn, usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email ?? "");
    }
}