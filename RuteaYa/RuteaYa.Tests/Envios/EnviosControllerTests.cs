using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RuteaYa.Api.Controllers;
using RuteaYa.Logica.Envios;
using RuteaYa.Logica.Envios.Dtos;

namespace RuteaYa.Tests.Envios;

public class EnviosControllerTests
{
    private const int UsuarioDelToken = 7;

    private static EnviosController CrearController(Mock<IEnvioServicios> servicio)
    {
        var controller = new EnviosController(servicio.Object);
        var identidad = new ClaimsIdentity(
            new[] { new Claim("sub", UsuarioDelToken.ToString()) }, "test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identidad) }
        };
        return controller;
    }

    [Fact]
    public async Task Listar_UsaElIdDelToken_Y_Devuelve200()
    {
        var servicio = new Mock<IEnvioServicios>();
        var lista = new List<EnvioListadoDto> { new() { Id = 1 } };
        servicio.Setup(s => s.ListarAsync(UsuarioDelToken, null)).ReturnsAsync(lista);
        var controller = CrearController(servicio);

        var resultado = await controller.Listar(null);

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Same(lista, ok.Value);
        servicio.Verify(s => s.ListarAsync(UsuarioDelToken, null), Times.Once);
    }

    [Fact]
    public async Task Listar_ConEstadoInexistente_Devuelve400_Y_NoConsultaElServicio()
    {
        var servicio = new Mock<IEnvioServicios>();
        var controller = CrearController(servicio);

        var resultado = await controller.Listar((EstadoEnvio)99);

        Assert.IsType<BadRequestObjectResult>(resultado.Result);
        servicio.Verify(s => s.ListarAsync(It.IsAny<int>(), It.IsAny<EstadoEnvio?>()), Times.Never);
    }

    [Fact]
    public async Task Detalle_Existente_UsaElIdDelToken_Y_Devuelve200()
    {
        var servicio = new Mock<IEnvioServicios>();
        var detalle = new EnvioDetalleDto { Id = 5 };
        servicio.Setup(s => s.ObtenerDetalleAsync(UsuarioDelToken, 5)).ReturnsAsync(detalle);
        var controller = CrearController(servicio);

        var resultado = await controller.Detalle(5);

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Same(detalle, ok.Value);
        servicio.Verify(s => s.ObtenerDetalleAsync(UsuarioDelToken, 5), Times.Once);
    }

    [Fact]
    public async Task Detalle_DeOtroUsuarioOInexistente_Devuelve404()
    {
        var servicio = new Mock<IEnvioServicios>();
        servicio.Setup(s => s.ObtenerDetalleAsync(UsuarioDelToken, 5))
            .ReturnsAsync((EnvioDetalleDto?)null);
        var controller = CrearController(servicio);

        var resultado = await controller.Detalle(5);

        Assert.IsType<NotFoundResult>(resultado.Result);
    }

    [Fact]
    public async Task Crear_Exitoso_UsaElIdDelToken_Y_Devuelve201()
    {
        var servicio = new Mock<IEnvioServicios>();
        var dto = new CrearEnvioDto { OrigenDireccion = "A", DestinoDireccion = "B" };
        servicio.Setup(s => s.CrearAsync(UsuarioDelToken, dto))
            .ReturnsAsync(ResultadoCrearEnvio.Exitoso(15));
        var controller = CrearController(servicio);

        var resultado = await controller.Crear(dto);

        var creado = Assert.IsType<CreatedAtActionResult>(resultado);
        Assert.Equal(nameof(EnviosController.Detalle), creado.ActionName);
        Assert.Equal(15, creado.RouteValues!["id"]);
        servicio.Verify(s => s.CrearAsync(UsuarioDelToken, dto), Times.Once);
    }

    [Fact]
    public async Task Crear_ConErrores_Devuelve400_ConLosMensajes()
    {
        var servicio = new Mock<IEnvioServicios>();
        servicio.Setup(s => s.CrearAsync(UsuarioDelToken, It.IsAny<CrearEnvioDto>()))
            .ReturnsAsync(ResultadoCrearEnvio.Fallido(new[] { "El origen es obligatorio." }));
        var controller = CrearController(servicio);

        var resultado = await controller.Crear(new CrearEnvioDto());

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        var problema = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        Assert.Contains("El origen es obligatorio.", problema.Errors["errores"]);
    }

    [Fact]
    public async Task Cancelar_Exitoso_UsaElIdDelToken_Y_Devuelve204()
    {
        var servicio = new Mock<IEnvioServicios>();
        servicio.Setup(s => s.CancelarAsync(UsuarioDelToken, 5))
            .ReturnsAsync(ResultadoCancelarEnvio.Exitoso());
        var controller = CrearController(servicio);

        var resultado = await controller.Cancelar(5);

        Assert.IsType<NoContentResult>(resultado);
        servicio.Verify(s => s.CancelarAsync(UsuarioDelToken, 5), Times.Once);
    }

    [Fact]
    public async Task Cancelar_DeOtroUsuarioOInexistente_Devuelve404()
    {
        var servicio = new Mock<IEnvioServicios>();
        servicio.Setup(s => s.CancelarAsync(UsuarioDelToken, 5))
            .ReturnsAsync(ResultadoCancelarEnvio.NoExiste());
        var controller = CrearController(servicio);

        var resultado = await controller.Cancelar(5);

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task Cancelar_EnEstadoQueNoLoPermite_Devuelve409()
    {
        var servicio = new Mock<IEnvioServicios>();
        servicio.Setup(s => s.CancelarAsync(UsuarioDelToken, 5))
            .ReturnsAsync(ResultadoCancelarEnvio.Fallido("Solo se pueden cancelar envíos ..."));
        var controller = CrearController(servicio);

        var resultado = await controller.Cancelar(5);

        Assert.IsType<ConflictObjectResult>(resultado);
    }

    [Fact]
    public void EnviosController_ExigeAutenticacion()
    {
        Assert.NotEmpty(typeof(EnviosController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));
    }
}