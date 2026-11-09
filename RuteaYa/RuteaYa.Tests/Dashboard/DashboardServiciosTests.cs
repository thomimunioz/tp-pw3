using Moq;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Dashboard;
using RuteaYa.Logica.Envios;
using RuteaYa.Logica.Envios.Dtos;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Oportunidades.Dtos;
using RuteaYa.Logica.Viajes;

namespace RuteaYa.Tests.Dashboard;

public class DashboardServiciosTests
{
    private const int UsuarioId = 1;
    private static readonly DayOfWeek[] TodosLosDias = Enum.GetValues<DayOfWeek>();

    private readonly Mock<IViajeServicios> _viajes;
    private readonly Mock<IEnvioServicios> _envios;
    private readonly Mock<IOportunidadesServicios> _oportunidades;
    private readonly DashboardServicios _servicio;

    public DashboardServiciosTests()
    {
        _viajes = new Mock<IViajeServicios>();
        _envios = new Mock<IEnvioServicios>();
        _oportunidades = new Mock<IOportunidadesServicios>();

        // el usuario no tiene viajes ni envíos
        _viajes
            .Setup(s => s.ObtenerPorUsuarioAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Viaje>());
        _envios
            .Setup(s => s.ListarAsync(It.IsAny<int>(), It.IsAny<EstadoEnvio?>()))
            .ReturnsAsync(new List<EnvioListadoDto>());

        _servicio = new DashboardServicios(_viajes.Object, _envios.Object, _oportunidades.Object);
    }

    // Helpers

    private static Viaje CrearViaje(
        int id,
        bool activo = true,
        string origen = "Belgrano",
        string destino = "Lomas de Zamora",
        TimeOnly? salida = null,
        TimeOnly? llegada = null,
        IEnumerable<DayOfWeek>? dias = null)
    {
        var viaje = new Viaje
        {
            Id = id,
            UsuarioId = UsuarioId,
            Activo = activo,
            OrigenDir = origen,
            DestinoDir = destino,
            HoraSalida = salida ?? new TimeOnly(8, 0),
            HoraLlegada = llegada ?? new TimeOnly(9, 0)
        };

        foreach (var dia in dias ?? TodosLosDias)
            viaje.Dias.Add(new ViajeDia { ViajeId = id, DiaSemana = dia });

        return viaje;
    }

    // El listado no trae los días (igual que ViajeServicios real), el detalle sí
    private void ConfigurarViajes(params Viaje[] viajes)
    {
        var listado = viajes.Select(v => new Viaje
        {
            Id = v.Id,
            UsuarioId = v.UsuarioId,
            Activo = v.Activo,
            OrigenDir = v.OrigenDir,
            DestinoDir = v.DestinoDir,
            HoraSalida = v.HoraSalida,
            HoraLlegada = v.HoraLlegada
        }).ToList();

        _viajes.Setup(s => s.ObtenerPorUsuarioAsync(UsuarioId)).ReturnsAsync(listado);

        foreach (var viaje in viajes)
            _viajes.Setup(s => s.ObtenerDetallePorUsuarioAsync(viaje.Id, UsuarioId)).ReturnsAsync(viaje);
    }

    private void ConfigurarEnvios(params EnvioListadoDto[] envios)
    {
        _envios
            .Setup(s => s.ListarAsync(UsuarioId, It.IsAny<EstadoEnvio?>()))
            .ReturnsAsync(envios.ToList());
    }

    private static EnvioListadoDto CrearEnvio(int id, EstadoEnvio estado, decimal monto = 4000m)
    {
        return new EnvioListadoDto
        {
            Id = id,
            OrigenDir = "Plaza Italia",
            DestinoDir = "Obelisco",
            Descripcion = "Zapatillas",
            Monto = monto,
            Estado = estado
        };
    }

    private static List<OportunidadDto> CrearOportunidades(int cantidad)
    {
        return Enumerable.Range(1, cantidad)
            .Select(i => new OportunidadDto { EnvioId = i })
            .ToList();
    }

    // ObtenerAsync: sin datos

    [Fact]
    public async Task ObtenerAsync_SinViajesNiEnvios_DevuelveTodoEnCeroYListasVacias()
    {
        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        Assert.Equal(0, dto.ProximosViajes);
        Assert.Equal(0, dto.EnviosPendientes);
        Assert.Equal(0, dto.Oportunidades);
        Assert.Equal(0m, dto.PotencialMes);
        Assert.Empty(dto.Viajes);
        Assert.Empty(dto.Envios);
    }

    // ObtenerAsync: viajes

    [Fact]
    public async Task ObtenerAsync_ViajesActivosEInactivos_SoloCuentaYMuestraLosActivos()
    {
        // Arrange
        ConfigurarViajes(
            CrearViaje(1, activo: true),
            CrearViaje(2, activo: true),
            CrearViaje(3, activo: false));

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        Assert.Equal(2, dto.ProximosViajes);
        Assert.Equal(2, dto.Viajes.Count);
    }

    [Fact]
    public async Task ObtenerAsync_ViajeSinDetalle_NoApareceEnElDashboard()
    {
        // Arrange: el listado lo trae pero el detalle devuelve null
        var viaje = CrearViaje(1);
        _viajes.Setup(s => s.ObtenerPorUsuarioAsync(UsuarioId)).ReturnsAsync(new List<Viaje> { viaje });
        _viajes.Setup(s => s.ObtenerDetallePorUsuarioAsync(1, UsuarioId)).ReturnsAsync((Viaje?)null);

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        Assert.Equal(0, dto.ProximosViajes);
        Assert.Empty(dto.Viajes);
    }

    [Fact]
    public async Task ObtenerAsync_Viaje_ArmaElResumenConDiasYHorario()
    {
        // Arrange
        var viaje = CrearViaje(
            1,
            origen: "Belgrano",
            destino: "Lomas de Zamora",
            salida: new TimeOnly(8, 0),
            llegada: new TimeOnly(8, 55),
            dias: new[] { DayOfWeek.Wednesday, DayOfWeek.Monday });
        ConfigurarViajes(viaje);

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert: los días salen ordenados desde el domingo y el horario como salida — llegada
        var resumen = Assert.Single(dto.Viajes);
        Assert.Equal("Belgrano", resumen.Origen);
        Assert.Equal("Lomas de Zamora", resumen.Destino);
        Assert.Equal("Lun, Mié", resumen.Dias);
        Assert.Equal("08:00 — 08:55", resumen.Horario);
    }

    [Fact]
    public async Task ObtenerAsync_ViajeConTodosLosDias_ListaLosSieteEmpezandoPorDomingo()
    {
        // Arrange
        ConfigurarViajes(CrearViaje(1));

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        var resumen = Assert.Single(dto.Viajes);
        Assert.Equal("Dom, Lun, Mar, Mié, Jue, Vie, Sáb", resumen.Dias);
    }

    [Fact]
    public async Task ObtenerAsync_VariosViajes_SeOrdenanPorLaProximaSalida()
    {
        // Arrange: el viaje de mañana siempre sale antes que el de pasado mañana,
        // sin importar a qué hora se ejecute el test
        var manana = DateTime.Today.AddDays(1).DayOfWeek;
        var pasadoManana = DateTime.Today.AddDays(2).DayOfWeek;

        var lejano = CrearViaje(1, origen: "Lejano", dias: new[] { pasadoManana });
        var cercano = CrearViaje(2, origen: "Cercano", dias: new[] { manana });
        ConfigurarViajes(lejano, cercano);

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        Assert.Equal("Cercano", dto.Viajes[0].Origen);
        Assert.Equal("Lejano", dto.Viajes[1].Origen);
    }

    [Fact]
    public async Task ObtenerAsync_MasDeTresViajes_MuestraTresPeroCuentaTodos()
    {
        // Arrange
        ConfigurarViajes(
            CrearViaje(1), CrearViaje(2), CrearViaje(3), CrearViaje(4), CrearViaje(5));

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        Assert.Equal(5, dto.ProximosViajes);
        Assert.Equal(3, dto.Viajes.Count);
    }

    // ObtenerAsync: envíos

    [Fact]
    public async Task ObtenerAsync_EnviosEntregadosOCancelados_NoSeCuentanComoPendientes()
    {
        // Arrange
        ConfigurarEnvios(
            CrearEnvio(1, EstadoEnvio.BuscandoConductor),
            CrearEnvio(2, EstadoEnvio.ConductorAsignado),
            CrearEnvio(3, EstadoEnvio.Entregado),
            CrearEnvio(4, EstadoEnvio.Cancelado));

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        Assert.Equal(2, dto.EnviosPendientes);
        Assert.Equal(2, dto.Envios.Count);
        Assert.All(dto.Envios, e =>
        {
            Assert.NotEqual(EstadoEnvio.Entregado, e.Estado);
            Assert.NotEqual(EstadoEnvio.Cancelado, e.Estado);
        });
    }

    [Fact]
    public async Task ObtenerAsync_Envio_ArmaElResumenConSusDatos()
    {
        // Arrange
        ConfigurarEnvios(CrearEnvio(1, EstadoEnvio.BuscandoConductor, monto: 4000m));

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        var resumen = Assert.Single(dto.Envios);
        Assert.Equal("Plaza Italia", resumen.Origen);
        Assert.Equal("Obelisco", resumen.Destino);
        Assert.Equal("Zapatillas", resumen.Descripcion);
        Assert.Equal(4000m, resumen.Monto);
        Assert.Equal(EstadoEnvio.BuscandoConductor, resumen.Estado);
    }

    [Fact]
    public async Task ObtenerAsync_MasDeTresEnviosPendientes_MuestraTresPeroCuentaTodos()
    {
        // Arrange
        ConfigurarEnvios(
            CrearEnvio(1, EstadoEnvio.BuscandoConductor),
            CrearEnvio(2, EstadoEnvio.BuscandoConductor),
            CrearEnvio(3, EstadoEnvio.ConductorAsignado),
            CrearEnvio(4, EstadoEnvio.Retirado),
            CrearEnvio(5, EstadoEnvio.EnCamino));

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        Assert.Equal(5, dto.EnviosPendientes);
        Assert.Equal(3, dto.Envios.Count);
    }

    // ObtenerAsync: oportunidades

    [Fact]
    public async Task ObtenerAsync_VariosViajesActivos_SumaLasOportunidadesDeTodos()
    {
        // Arrange
        ConfigurarViajes(CrearViaje(1), CrearViaje(2));
        _oportunidades.Setup(o => o.BuscarAsync(UsuarioId, 1)).ReturnsAsync(CrearOportunidades(2));
        _oportunidades.Setup(o => o.BuscarAsync(UsuarioId, 2)).ReturnsAsync(CrearOportunidades(3));

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        Assert.Equal(5, dto.Oportunidades);
    }

    [Fact]
    public async Task ObtenerAsync_BusquedaDeOportunidadesNula_CuentaCeroParaEseViaje()
    {
        // Arrange: el servicio de oportunidades devuelve null para el viaje 1
        ConfigurarViajes(CrearViaje(1), CrearViaje(2));
        _oportunidades.Setup(o => o.BuscarAsync(UsuarioId, 1)).ReturnsAsync((List<OportunidadDto>?)null);
        _oportunidades.Setup(o => o.BuscarAsync(UsuarioId, 2)).ReturnsAsync(CrearOportunidades(4));

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        Assert.Equal(4, dto.Oportunidades);
    }

    [Fact]
    public async Task ObtenerAsync_ViajeInactivo_NoBuscaOportunidades()
    {
        // Arrange
        ConfigurarViajes(CrearViaje(1, activo: true), CrearViaje(2, activo: false));
        _oportunidades.Setup(o => o.BuscarAsync(UsuarioId, It.IsAny<int>())).ReturnsAsync(CrearOportunidades(1));

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        Assert.Equal(1, dto.Oportunidades);
        _oportunidades.Verify(o => o.BuscarAsync(UsuarioId, 2), Times.Never);
    }

    // ObtenerAsync: potencial y usuario

    [Fact]
    public async Task ObtenerAsync_PotencialDelMes_EsProvisorioYVale0()
    {
        // Arrange
        ConfigurarViajes(CrearViaje(1));
        ConfigurarEnvios(CrearEnvio(1, EstadoEnvio.BuscandoConductor));

        // Act
        var dto = await _servicio.ObtenerAsync(UsuarioId);

        // Assert: hasta que exista la Pantalla 14
        Assert.Equal(0m, dto.PotencialMes);
    }

    [Fact]
    public async Task ObtenerAsync_PideLosDatosUnicamenteDelUsuarioIndicado()
    {
        // Act
        await _servicio.ObtenerAsync(UsuarioId);

        // Assert
        _viajes.Verify(s => s.ObtenerPorUsuarioAsync(UsuarioId), Times.Once);
        _envios.Verify(s => s.ListarAsync(UsuarioId, It.IsAny<EstadoEnvio?>()), Times.Once);
        _viajes.Verify(s => s.ObtenerPorUsuarioAsync(It.Is<int>(id => id != UsuarioId)), Times.Never);
    }
}