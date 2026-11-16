using Moq;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Oportunidades.Dtos;
using RuteaYa.Logica.Optimizacion;

namespace RuteaYa.Tests.Optimizacion;

public class OptimizadorServiciosTests
{
    private readonly RuteaYaContext _context;
    private readonly Mock<IOportunidadesServicios> _oportunidades;
    private readonly OptimizadorServicios _servicio;

    public OptimizadorServiciosTests()
    {
        _context = ContextoDePrueba.Crear();
        _oportunidades = new Mock<IOportunidadesServicios>();
        _servicio = new OptimizadorServicios(_context, _oportunidades.Object);
    }

    private void ConOportunidades(params OportunidadDto[] lista)
    {
        _oportunidades
            .Setup(o => o.BuscarAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(lista.ToList());
    }

    private static OportunidadDto Oportunidad(int envioId, decimal pago, decimal pesoKg, DateOnly fecha,
        int paquetesLibres = 3, decimal pesoLibreKg = 10m)
    {
        return new OportunidadDto
        {
            EnvioId = envioId,
            OrigenDireccion = "A",
            DestinoDireccion = "B",
            Pago = pago,
            PesoKg = pesoKg,
            DesvioKm = 1m,
            FechaViaje = fecha,
            PaquetesDisponibles = paquetesLibres,
            PesoDisponibleKg = pesoLibreKg,
            DesvioDisponibleKm = 5m
        };
    }

    [Fact]
    public async Task CalcularAsync_ViajeDeOtroUsuario_DevuelveNull()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context, usuarioId: 1);

        // Act
        var resultado = await _servicio.CalcularAsync(99, viaje.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task CalcularAsync_SinOportunidades_DevuelveElViajeSinCombinacion()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context);
        ConOportunidades();

        // Act
        var resultado = await _servicio.CalcularAsync(viaje.UsuarioId, viaje.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.NotNull(resultado);
        Assert.Null(resultado.FechaViaje);
        Assert.Empty(resultado.Resultado.Seleccionados);
        Assert.Equal(viaje.MaxPaquetes, resultado.MaxPaquetes);
    }

    [Fact]
    public async Task CalcularAsync_TresEnvios_EligeLaCombinacionQueMasPagaSinPasarseDePeso()
    {
        // Arrange: 1 y 2 juntos pesan 12 kg y el viaje lleva 10
        var viaje = DatosDePrueba.CrearViaje(_context);
        ConOportunidades(
            Oportunidad(1, 5000m, 6m, DatosDePrueba.Hoy),
            Oportunidad(2, 4000m, 6m, DatosDePrueba.Hoy),
            Oportunidad(3, 2000m, 3m, DatosDePrueba.Hoy));

        // Act
        var resultado = await _servicio.CalcularAsync(viaje.UsuarioId, viaje.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(DatosDePrueba.Hoy, resultado.FechaViaje);
        Assert.Equal(new List<int> { 1, 3 }, resultado.Resultado.Seleccionados.Select(c => c.EnvioId).ToList());
        Assert.Equal(7000m, resultado.Resultado.GananciaTotal);
    }

    [Fact]
    public async Task CalcularAsync_EnviosDeDistintasFechas_SoloCombinaLosDeLaProximaFecha()
    {
        // Arrange
        var viaje = DatosDePrueba.CrearViaje(_context);
        ConOportunidades(
            Oportunidad(1, 5000m, 1m, DatosDePrueba.Hoy),
            Oportunidad(2, 9000m, 1m, DatosDePrueba.Hoy.AddDays(7)));

        // Act
        var resultado = await _servicio.CalcularAsync(viaje.UsuarioId, viaje.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(DatosDePrueba.Hoy, resultado.FechaViaje);
        var candidato = Assert.Single(resultado.Candidatos);
        Assert.Equal(1, candidato.EnvioId);
        Assert.Equal(1, Assert.Single(resultado.Resultado.Seleccionados).EnvioId);
    }

    [Fact]
    public async Task CalcularAsync_QuedaUnSoloLugarEseDia_EligeUnSoloEnvio()
    {
        // Arrange: el viaje ya tiene paquetes asignados ese día y le queda 1 lugar
        var viaje = DatosDePrueba.CrearViaje(_context);
        ConOportunidades(
            Oportunidad(1, 5000m, 1m, DatosDePrueba.Hoy, paquetesLibres: 1),
            Oportunidad(2, 4000m, 1m, DatosDePrueba.Hoy, paquetesLibres: 1));

        // Act
        var resultado = await _servicio.CalcularAsync(viaje.UsuarioId, viaje.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.PaquetesDisponibles);
        Assert.Equal(1, Assert.Single(resultado.Resultado.Seleccionados).EnvioId);
    }
}
