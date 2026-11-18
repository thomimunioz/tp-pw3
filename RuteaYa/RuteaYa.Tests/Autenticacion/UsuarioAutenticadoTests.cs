using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RuteaYa.Api.Autenticacion;
using RuteaYa.Api.Controllers;
using RuteaYa.Api.Dtos;
using RuteaYa.Logica.Asignaciones;
using RuteaYa.Logica.Historial;
using RuteaYa.Logica.Historial.Dtos;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Optimizacion;
using RuteaYa.Logica.Transportes;
using RuteaYa.Logica.Transportes.Dtos;

namespace RuteaYa.Tests.Autenticacion;

public class UsuarioAutenticadoTests
{
    private const int UsuarioDelToken = 7;

    // Simula un pedido con token: el usuario autenticado tiene id 7
    private static void ConUsuario(ControllerBase controller)
    {
        var identidad = new ClaimsIdentity(
            new[] { new Claim("sub", UsuarioDelToken.ToString()) }, "test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identidad) }
        };
    }

    [Fact]
    public async Task Oportunidades_Buscar_UsaElIdDelToken()
    {
        var servicio = new Mock<IOportunidadesServicios>();
        var controller = new OportunidadesController(servicio.Object);
        ConUsuario(controller);

        await controller.Buscar(viajeId: 5);

        servicio.Verify(s => s.BuscarAsync(UsuarioDelToken, 5), Times.Once);
    }

    [Fact]
    public async Task Oportunidades_Detalle_UsaElIdDelToken()
    {
        var servicio = new Mock<IOportunidadesServicios>();
        var controller = new OportunidadesController(servicio.Object);
        ConUsuario(controller);

        await controller.Detalle(viajeId: 5, envioId: 8);

        servicio.Verify(s => s.ObtenerDetalleAsync(UsuarioDelToken, 5, 8), Times.Once);
    }

    [Fact]
    public async Task Asignaciones_Aceptar_UsaElIdDelToken()
    {
        var servicio = new Mock<IAsignacionesServicios>();
        var controller = new AsignacionesController(servicio.Object);
        ConUsuario(controller);

        await controller.Aceptar(viajeId: 5, envioId: 8);

        servicio.Verify(s => s.AceptarAsync(UsuarioDelToken, 5, 8), Times.Once);
    }

    [Fact]
    public async Task Asignaciones_ConfirmarCombinacion_UsaElIdDelToken()
    {
        var servicio = new Mock<IAsignacionesServicios>();
        var controller = new AsignacionesController(servicio.Object);
        ConUsuario(controller);
        var request = new ConfirmarCombinacionRequest { EnvioIds = new List<int> { 8, 9 } };

        await controller.ConfirmarCombinacion(5, request);

        servicio.Verify(s => s.ConfirmarCombinacionAsync(
            UsuarioDelToken, 5, It.Is<List<int>>(l => l.SequenceEqual(new[] { 8, 9 }))), Times.Once);
    }

    [Fact]
    public async Task Optimizacion_Calcular_UsaElIdDelToken()
    {
        var servicio = new Mock<IOptimizadorServicios>();
        var controller = new OptimizacionController(servicio.Object);
        ConUsuario(controller);

        await controller.Calcular(viajeId: 5);

        servicio.Verify(s => s.CalcularAsync(UsuarioDelToken, 5), Times.Once);
    }

    [Fact]
    public async Task Historial_Obtener_UsaElIdDelToken()
    {
        var servicio = new Mock<IHistorialServicios>();
        var controller = new HistorialController(servicio.Object);
        ConUsuario(controller);

        await controller.Obtener(null, null, null);

        servicio.Verify(s => s.ObtenerHistorialAsync(
            UsuarioDelToken, It.IsAny<FiltroHistorialDto?>()), Times.Once);
    }

    [Fact]
    public async Task Transportes_Listar_UsaElIdDelToken()
    {
        var servicio = new Mock<ITransporteServicios>();
        servicio.Setup(s => s.ListarMisTransportesAsync(UsuarioDelToken))
            .ReturnsAsync(new List<TransporteListadoDto>());
        var controller = new TransportesController(servicio.Object);
        ConUsuario(controller);

        await controller.Listar();

        servicio.Verify(s => s.ListarMisTransportesAsync(UsuarioDelToken), Times.Once);
    }

    [Fact]
    public async Task Transportes_MarcarRetirado_UsaElIdDelToken()
    {
        var servicio = new Mock<ITransporteServicios>();
        servicio.Setup(s => s.MarcarRetiradoAsync(UsuarioDelToken, 5))
            .ReturnsAsync(ResultadoActualizarTransporte.Exitoso());
        var controller = new TransportesController(servicio.Object);
        ConUsuario(controller);

        await controller.MarcarRetirado(envioId: 5);

        servicio.Verify(s => s.MarcarRetiradoAsync(UsuarioDelToken, 5), Times.Once);
    }

    [Theory]
    [InlineData(typeof(OportunidadesController))]
    [InlineData(typeof(AsignacionesController))]
    [InlineData(typeof(OptimizacionController))]
    [InlineData(typeof(HistorialController))]
    [InlineData(typeof(TransportesController))]
    public void Controller_ExigeAutenticacion(Type controller)
    {
        Assert.NotEmpty(controller.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));
    }

    [Fact]
    public void ObtenerUsuarioId_ConSub_DevuelveElId()
    {
        var usuario = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "7") }));

        Assert.Equal(7, usuario.ObtenerUsuarioId());
    }

    [Fact]
    public void ObtenerUsuarioId_SinIdEnElToken_LanzaExcepcion()
    {
        var usuario = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.Throws<InvalidOperationException>(() => usuario.ObtenerUsuarioId());
    }
}