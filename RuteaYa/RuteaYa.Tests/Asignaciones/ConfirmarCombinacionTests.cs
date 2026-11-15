using Moq;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Asignaciones;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Oportunidades.Dtos;

namespace RuteaYa.Tests.Asignaciones;

public class ConfirmarCombinacionTests
{
    private readonly RuteaYaContext _context;
    private readonly Mock<IOportunidadesServicios> _oportunidades;
    private readonly AsignacionesServicios _servicio;

    public ConfirmarCombinacionTests()
    {
        _context = ContextoDePrueba.Crear();
        _oportunidades = new Mock<IOportunidadesServicios>();
        _servicio = new AsignacionesServicios(_context, _oportunidades.Object);
    }

    private void ConOportunidades(params OportunidadDto[] lista)
    {
        _oportunidades
            .Setup(o => o.BuscarAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(lista.ToList());
    }

    private static OportunidadDto Oportunidad(Envio envio)
    {
        return new OportunidadDto
        {
            EnvioId = envio.Id,
            Pago = envio.Monto,
            PesoKg = envio.PesoKg,
            DesvioKm = 1m,
            TiempoExtraMin = 2,
            FechaViaje = DatosDePrueba.Hoy,
            PaquetesDisponibles = 3,
            PesoDisponibleKg = 10m
        };
    }

    [Fact]
    public async Task ConfirmarCombinacionAsync_DosEnviosLibres_AsignaLosDosAlViaje()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context);
        var envio1 = DatosDePrueba.CrearEnvio(_context);
        var envio2 = DatosDePrueba.CrearEnvio(_context);
        ConOportunidades(Oportunidad(envio1), Oportunidad(envio2));

        // Act
        var resultado = await _servicio.ConfirmarCombinacionAsync(
            viaje.UsuarioId, viaje.Id, new List<int> { envio1.Id, envio2.Id }, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.Exito, resultado);
        Assert.Equal(2, _context.Set<Asignacion>().Count(a => a.ViajeId == viaje.Id));
        Assert.Equal(2, _context.Set<EnvioEstadoHistorial>().Count());
        Assert.Equal(EstadoEnvio.ConductorAsignado, envio1.Estado);
        Assert.Equal(EstadoEnvio.ConductorAsignado, envio2.Estado);
    }

    [Fact]
    public async Task ConfirmarCombinacionAsync_OtroConductorTomoUno_NoAsignaNinguno()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context);
        var libre = DatosDePrueba.CrearEnvio(_context);
        var tomado = DatosDePrueba.CrearEnvio(_context, estado: EstadoEnvio.ConductorAsignado);
        ConOportunidades(Oportunidad(libre));

        // Act
        var resultado = await _servicio.ConfirmarCombinacionAsync(
            viaje.UsuarioId, viaje.Id, new List<int> { libre.Id, tomado.Id }, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.YaTomado, resultado);
        Assert.Empty(_context.Set<Asignacion>());
        Assert.Equal(EstadoEnvio.BuscandoConductor, libre.Estado);
    }

    [Fact]
    public async Task ConfirmarCombinacionAsync_JuntosSuperanElPeso_NoAsignaNinguno()
    {
        // Arrange: 6 kg + 6 kg y el viaje tiene 10 kg libres
        var viaje = DatosDePrueba.CrearViaje(_context);
        var envio1 = DatosDePrueba.CrearEnvio(_context);
        var envio2 = DatosDePrueba.CrearEnvio(_context);
        envio1.PesoKg = 6m;
        envio2.PesoKg = 6m;
        _context.SaveChanges();
        ConOportunidades(Oportunidad(envio1), Oportunidad(envio2));

        // Act
        var resultado = await _servicio.ConfirmarCombinacionAsync(
            viaje.UsuarioId, viaje.Id, new List<int> { envio1.Id, envio2.Id }, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.NoCumple, resultado);
        Assert.Empty(_context.Set<Asignacion>());
    }

    [Fact]
    public async Task ConfirmarCombinacionAsync_UnEnvioDejoDeSerOportunidad_NoAsignaNinguno()
    {
        // Arrange: el envío 2 sigue libre pero ya no entra en el viaje (por ejemplo, venció su fecha)
        var viaje = DatosDePrueba.CrearViaje(_context);
        var envio1 = DatosDePrueba.CrearEnvio(_context);
        var envio2 = DatosDePrueba.CrearEnvio(_context);
        ConOportunidades(Oportunidad(envio1));

        // Act
        var resultado = await _servicio.ConfirmarCombinacionAsync(
            viaje.UsuarioId, viaje.Id, new List<int> { envio1.Id, envio2.Id }, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.NoCumple, resultado);
        Assert.Empty(_context.Set<Asignacion>());
    }

    [Fact]
    public async Task ConfirmarCombinacionAsync_ViajeDeOtroUsuario_DevuelveNoEncontrado()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context, usuarioId: 1);
        var envio = DatosDePrueba.CrearEnvio(_context);

        // Act
        var resultado = await _servicio.ConfirmarCombinacionAsync(
            99, viaje.Id, new List<int> { envio.Id }, DatosDePrueba.Hoy);

        // Assert
        Assert.Equal(ResultadoAceptar.NoEncontrado, resultado);
    }
}
