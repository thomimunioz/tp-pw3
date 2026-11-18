using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using RuteaYa.Api.Autenticacion;
using RuteaYa.Api.Controllers;
using RuteaYa.Api.Dtos;
using RuteaYa.Entidades;
using RuteaYa.Logica.Account;

namespace RuteaYa.Tests.Autenticacion;

public class AutenticacionControllerTests
{
    private static GeneradorTokens CrearGenerador() => new(Options.Create(new JwtOpciones
    {
        Clave = "una-clave-de-prueba-bien-larga-de-mas-de-32-caracteres",
        Emisor = "test",
        Audiencia = "test",
        MinutosExpiracion = 5
    }));

    private static Usuario CrearUsuario() => new()
    {
        Id = 7,
        Email = "ana@test.com",
        UserName = "ana@test.com",
        Nombre = "Ana",
        Apellido = "Pérez"
    };

    private static RegistroRequest CrearRegistro() => new()
    {
        Nombre = "Ana",
        Apellido = "Pérez",
        Email = "ana@test.com",
        Password = "Clave1234",
        ConfirmarPassword = "Clave1234"
    };

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveTokenConElIdDelUsuario()
    {
        var account = new Mock<IAccountServicios>();
        account.Setup(a => a.ValidarCredencialesAsync("ana@test.com", "Clave1234"))
            .ReturnsAsync(CrearUsuario());
        var controller = new AutenticacionController(account.Object, CrearGenerador());

        var resultado = await controller.Login(new LoginRequest { Email = "ana@test.com", Password = "Clave1234" });

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var dto = Assert.IsType<AuthRespuestaDto>(ok.Value);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(dto.Token);
        Assert.Equal("7", token.Subject);
        Assert.Equal(7, dto.UsuarioId);
    }

    [Fact]
    public async Task Login_ConCredencialesIncorrectas_Devuelve401()
    {
        var account = new Mock<IAccountServicios>();
        account.Setup(a => a.ValidarCredencialesAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);
        var controller = new AutenticacionController(account.Object, CrearGenerador());

        var resultado = await controller.Login(new LoginRequest { Email = "ana@test.com", Password = "mala" });

        Assert.IsType<UnauthorizedObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task Registrar_ConDatosValidos_Devuelve201ConToken()
    {
        var account = new Mock<IAccountServicios>();
        account.Setup(a => a.RegistrarAsync("Ana", "Pérez", "ana@test.com", "Clave1234"))
            .ReturnsAsync(new ResultadoRegistro { Exito = true, Usuario = CrearUsuario() });
        var controller = new AutenticacionController(account.Object, CrearGenerador());

        var resultado = await controller.Registrar(CrearRegistro());

        var objeto = Assert.IsType<ObjectResult>(resultado.Result);
        Assert.Equal(201, objeto.StatusCode);
        var dto = Assert.IsType<AuthRespuestaDto>(objeto.Value);
        Assert.False(string.IsNullOrEmpty(dto.Token));
    }

    [Fact]
    public async Task Registrar_ConEmailRepetido_Devuelve409()
    {
        var account = new Mock<IAccountServicios>();
        account.Setup(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ResultadoRegistro
            {
                Exito = false,
                Errores = { "Ya existe una cuenta con ese email" }
            });
        var controller = new AutenticacionController(account.Object, CrearGenerador());

        var resultado = await controller.Registrar(CrearRegistro());

        Assert.IsType<ConflictObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task Registrar_ConPasswordInvalida_Devuelve400()
    {
        var account = new Mock<IAccountServicios>();
        account.Setup(a => a.RegistrarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ResultadoRegistro
            {
                Exito = false,
                Errores = { "La contraseña debe tener al menos 8 caracteres" }
            });
        var controller = new AutenticacionController(account.Object, CrearGenerador());

        var resultado = await controller.Registrar(CrearRegistro());

        Assert.IsType<BadRequestObjectResult>(resultado.Result);
    }
}