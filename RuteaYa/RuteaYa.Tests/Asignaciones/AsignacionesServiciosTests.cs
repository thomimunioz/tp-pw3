using Moq;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Asignaciones;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Oportunidades.Dtos;

namespace RuteaYa.Tests.Asignaciones;

public class AsignacionesServiciosTests
{
    private readonly RuteaYaContext _context;
    private readonly Mock<IOportunidadesServicios> _oportunidades;
    private readonly AsignacionesServicios _servicio;

    public AsignacionesServiciosTests()
    {
        _context = ContextoDePrueba.Crear();
        _oportunidades = new Mock<IOportunidadesServicios>();
        _servicio = new AsignacionesServicios(_context, _oportunidades.Object);
    }

    private void DetalleValido(Envio envio)
    {
        _oportunidades
            .Setup(o => o.ObtenerDetalleAsync(It.IsAny<int>(), It.IsAny<int>(), envio.Id, It.IsAny<DateOnly>()))
            .ReturnsAsync(new OportunidadDto
            {
                EnvioId = envio.Id,
                FechaViaje = DatosDePrueba.Hoy,
                DesvioKm = 1.5m,
                TiempoExtraMin = 3,
                Pago = envio.Monto
            });
    }

    [Fact]
    public async Task AceptarAsync_TodoValido_DevuelveExitoYCreaLaAsignacion()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context);
        var envio = DatosDePrueba.CrearEnvio(_context);
        DetalleValido(envio);

        // Act
        var resultado = await _servicio.AceptarAsync(viaje.UsuarioId, viaje.Id, envio.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.Exito, resultado);

        var asignacion = Assert.Single(_context.Set<Asignacion>());
        Assert.Equal(envio.Id, asignacion.EnvioId);
        Assert.Equal(viaje.Id, asignacion.ViajeId);
        Assert.Equal(1.5m, asignacion.DesvioKm);
        Assert.Equal(3, asignacion.TiempoExtraMin);
        Assert.Equal(envio.Monto, asignacion.Ganancia);
        Assert.Equal(EstadoEnvio.ConductorAsignado, envio.Estado);
    }

    [Fact]
    public async Task AceptarAsync_TodoValido_RegistraElCambioDeEstadoEnElHistorial()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context);
        var envio = DatosDePrueba.CrearEnvio(_context);
        DetalleValido(envio);

        // Act
        await _servicio.AceptarAsync(viaje.UsuarioId, viaje.Id, envio.Id, DatosDePrueba.Hoy);

        // Assert
        var historial = Assert.Single(_context.Set<EnvioEstadoHistorial>());
        Assert.Equal(envio.Id, historial.EnvioId);
        Assert.Equal(EstadoEnvio.BuscandoConductor, historial.EstadoAnterior);
        Assert.Equal(EstadoEnvio.ConductorAsignado, historial.EstadoNuevo);
    }

    [Fact]
    public async Task AceptarAsync_ViajeDeOtroUsuario_DevuelveNoEncontradoYNoGuardaNada()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context, usuarioId: 1);
        var envio = DatosDePrueba.CrearEnvio(_context);
        DetalleValido(envio);

        // Act: el usuario 99 intenta usar el viaje del usuario 1
        var resultado = await _servicio.AceptarAsync(99, viaje.Id, envio.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.NoEncontrado, resultado);
        Assert.Empty(_context.Set<Asignacion>());
        Assert.Equal(EstadoEnvio.BuscandoConductor, envio.Estado);
    }

    [Fact]
    public async Task AceptarAsync_ViajeInactivo_DevuelveNoEncontrado()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context, activo: false);
        var envio = DatosDePrueba.CrearEnvio(_context);
        DetalleValido(envio);

        // Act
        var resultado = await _servicio.AceptarAsync(viaje.UsuarioId, viaje.Id, envio.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.NoEncontrado, resultado);
    }

    [Fact]
    public async Task AceptarAsync_EnvioInexistente_DevuelveNoEncontrado()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context);

        // Act
        var resultado = await _servicio.AceptarAsync(viaje.UsuarioId, viaje.Id, 999, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.NoEncontrado, resultado);
    }

    [Fact]
    public async Task AceptarAsync_EnvioYaTomadoPorOtroConductor_DevuelveYaTomado()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context);
        var envio = DatosDePrueba.CrearEnvio(_context, estado: EstadoEnvio.ConductorAsignado);
        DetalleValido(envio);

        // Act
        var resultado = await _servicio.AceptarAsync(viaje.UsuarioId, viaje.Id, envio.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.YaTomado, resultado);
        Assert.Empty(_context.Set<Asignacion>());
    }

    [Fact]
    public async Task AceptarAsync_EnvioCancelado_DevuelveNoCumple()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context);
        var envio = DatosDePrueba.CrearEnvio(_context, estado: EstadoEnvio.Cancelado);
        DetalleValido(envio);

        // Act
        var resultado = await _servicio.AceptarAsync(viaje.UsuarioId, viaje.Id, envio.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.NoCumple, resultado);
        Assert.Empty(_context.Set<Asignacion>());
    }

    [Fact]
    public async Task AceptarAsync_ElEnvioDejoDeSerCompatible_DevuelveNoCumpleYNoGuardaNada()
    {
        // Arrange: el detalle devuelve null (ya no entra por peso, desvío, fecha...)
        var viaje = DatosDePrueba.CrearViaje(_context);
        var envio = DatosDePrueba.CrearEnvio(_context);
        _oportunidades
            .Setup(o => o.ObtenerDetalleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
            .ReturnsAsync((OportunidadDto?)null);

        // Act
        var resultado = await _servicio.AceptarAsync(viaje.UsuarioId, viaje.Id, envio.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.NoCumple, resultado);
        Assert.Empty(_context.Set<Asignacion>());
        Assert.Equal(EstadoEnvio.BuscandoConductor, envio.Estado);
    }
}