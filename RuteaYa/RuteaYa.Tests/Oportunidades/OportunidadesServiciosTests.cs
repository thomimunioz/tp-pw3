using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Oportunidades;
using RuteaYa.Logica.Oportunidades.Dtos;

namespace RuteaYa.Tests.Oportunidades;

public class OportunidadesServiciosTests
{
    private static readonly DateOnly Hoy = new DateOnly(2026, 11, 2);
    private const int Conductor = 1;
    private const int Remitente = 2;

    private static RuteaYaContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<RuteaYaContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RuteaYaContext(options);
    }

    private static Viaje CrearViaje(DayOfWeek dia)
    {
        var viaje = new Viaje
        {
            UsuarioId = Conductor,
            OrigenDir = "Origen del viaje",
            OrigenLat = -34.60m,
            OrigenLng = -58.40m,
            DestinoDir = "Destino del viaje",
            DestinoLat = -34.60m,
            DestinoLng = -58.30m,
            HoraSalida = new TimeOnly(8, 0),
            HoraLlegada = new TimeOnly(9, 0),
            MaxPaquetes = 3,
            PesoMaxKg = 10m,
            DesvioMaxKm = 3m,
            Activo = true
        };
        viaje.Dias.Add(new ViajeDia { DiaSemana = dia });
        return viaje;
    }

    private static Envio CrearEnvio(DateOnly fechaLimite, decimal pesoKg = 2m)
    {
        return new Envio
        {
            UsuarioId = Remitente,
            OrigenDir = "Retiro",
            OrigenLat = -34.60m,
            OrigenLng = -58.38m,
            DestinoDir = "Entrega",
            DestinoLat = -34.60m,
            DestinoLng = -58.32m,
            FechaLimite = fechaLimite,
            PesoKg = pesoKg,
            Tamano = TamanoPaquete.Mediano,
            Descripcion = "Caja con libros",
            Monto = 3000m,
            Estado = EstadoEnvio.BuscandoConductor,
            CreadoEn = DateTime.Now,
            RowVersion = new byte[8]
        };
    }

    [Fact]
    public async Task BuscarAsync_ViajeDeOtroUsuario_DevuelveNull()
    {
        using var context = CrearContexto();
        var viaje = CrearViaje(DayOfWeek.Wednesday);
        context.Viajes.Add(viaje);
        await context.SaveChangesAsync();
        var servicio = new OportunidadesServicios(context);

        var resultado = await servicio.BuscarAsync(Remitente, viaje.Id, Hoy);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task BuscarAsync_EnvioEnElCaminoConLugar_LoDevuelveConElPrimerDiaDelViaje()
    {
        using var context = CrearContexto();
        var viaje = CrearViaje(DayOfWeek.Wednesday);
        var envio = CrearEnvio(Hoy.AddDays(7));
        context.Viajes.Add(viaje);
        context.Envios.Add(envio);
        await context.SaveChangesAsync();
        var servicio = new OportunidadesServicios(context);

        var resultado = await servicio.BuscarAsync(Conductor, viaje.Id, Hoy);

        var oportunidad = Assert.Single(resultado!);
        Assert.Equal(envio.Id, oportunidad.EnvioId);
        Assert.Equal(Hoy.AddDays(2), oportunidad.FechaViaje);
    }

    // ObtenerDetalleAsync

    [Fact]
    public async Task ObtenerDetalleAsync_DatosValidos_DevuelveElDetalleConLosValoresDelEnvio()
    {
        using var context = CrearContexto();
        var viaje = CrearViaje(DayOfWeek.Wednesday);
        var envio = CrearEnvio(Hoy.AddDays(7));
        context.Viajes.Add(viaje);
        context.Envios.Add(envio);
        await context.SaveChangesAsync();
        var servicio = new OportunidadesServicios(context);

        var detalle = await servicio.ObtenerDetalleAsync(Conductor, viaje.Id, envio.Id, Hoy);

        Assert.NotNull(detalle);
        Assert.Equal(envio.Id, detalle.EnvioId);
        Assert.Equal(Hoy.AddDays(2), detalle.FechaViaje);
        Assert.Equal(3000m, detalle.Pago);
        Assert.Equal(2m, detalle.PesoKg);
        Assert.InRange(detalle.DesvioKm, -0.1m, 0.1m);
        Assert.Equal(3, detalle.PaquetesDisponibles);
        Assert.Equal(10m, detalle.PesoDisponibleKg);
    }

    [Fact]
    public async Task ObtenerDetalleAsync_ViajeDeOtroUsuario_DevuelveNull()
    {
        using var context = CrearContexto();
        var viaje = CrearViaje(DayOfWeek.Wednesday);
        var envio = CrearEnvio(Hoy.AddDays(7));
        context.Viajes.Add(viaje);
        context.Envios.Add(envio);
        await context.SaveChangesAsync();
        var servicio = new OportunidadesServicios(context);

        // Act: el remitente intenta ver el detalle usando el viaje del conductor
        var detalle = await servicio.ObtenerDetalleAsync(Remitente, viaje.Id, envio.Id, Hoy);

        Assert.Null(detalle);
    }

    [Fact]
    public async Task ObtenerDetalleAsync_EnvioInexistente_DevuelveNull()
    {
        using var context = CrearContexto();
        var viaje = CrearViaje(DayOfWeek.Wednesday);
        context.Viajes.Add(viaje);
        await context.SaveChangesAsync();
        var servicio = new OportunidadesServicios(context);

        var detalle = await servicio.ObtenerDetalleAsync(Conductor, viaje.Id, 999, Hoy);

        Assert.Null(detalle);
    }

    [Fact]
    public async Task ObtenerDetalleAsync_EnvioDelPropioConductor_DevuelveNull()
    {
        using var context = CrearContexto();
        var viaje = CrearViaje(DayOfWeek.Wednesday);
        var envio = CrearEnvio(Hoy.AddDays(7));
        envio.UsuarioId = Conductor;
        context.Viajes.Add(viaje);
        context.Envios.Add(envio);
        await context.SaveChangesAsync();
        var servicio = new OportunidadesServicios(context);

        var detalle = await servicio.ObtenerDetalleAsync(Conductor, viaje.Id, envio.Id, Hoy);

        Assert.Null(detalle);
    }

    [Fact]
    public async Task ObtenerDetalleAsync_EnvioCancelado_DevuelveNull()
    {
        using var context = CrearContexto();
        var viaje = CrearViaje(DayOfWeek.Wednesday);
        var envio = CrearEnvio(Hoy.AddDays(7));
        envio.Estado = EstadoEnvio.Cancelado;
        context.Viajes.Add(viaje);
        context.Envios.Add(envio);
        await context.SaveChangesAsync();
        var servicio = new OportunidadesServicios(context);

        var detalle = await servicio.ObtenerDetalleAsync(Conductor, viaje.Id, envio.Id, Hoy);

        Assert.Null(detalle);
    }

    [Fact]
    public async Task ObtenerDetalleAsync_DesvioMayorAlMaximo_DevuelveNull()
    {
        // Arrange: el envío queda unos 22 km al sur del recorrido, y el máximo es 3 km
        using var context = CrearContexto();
        var viaje = CrearViaje(DayOfWeek.Wednesday);
        var envio = CrearEnvio(Hoy.AddDays(7));
        envio.OrigenLat = -34.80m;
        envio.DestinoLat = -34.80m;
        context.Viajes.Add(viaje);
        context.Envios.Add(envio);
        await context.SaveChangesAsync();
        var servicio = new OportunidadesServicios(context);

        // Act
        var detalle = await servicio.ObtenerDetalleAsync(Conductor, viaje.Id, envio.Id, Hoy);

        // Assert
        Assert.Null(detalle);
    }

    [Fact]
    public async Task ObtenerDetalleAsync_ElMismoEnvio_DaLoMismoQueLaListaDeOportunidades()
    {
        using var context = CrearContexto();
        var viaje = CrearViaje(DayOfWeek.Wednesday);
        var envio = CrearEnvio(Hoy.AddDays(7));
        context.Viajes.Add(viaje);
        context.Envios.Add(envio);
        await context.SaveChangesAsync();
        var servicio = new OportunidadesServicios(context);

        var lista = await servicio.BuscarAsync(Conductor, viaje.Id, Hoy);
        var detalle = await servicio.ObtenerDetalleAsync(Conductor, viaje.Id, envio.Id, Hoy);

        // Assert: el detalle y la lista tienen que coincidir, porque usan el mismo cálculo
        var enLista = Assert.Single(lista!);
        Assert.NotNull(detalle);
        Assert.Equal(enLista.FechaViaje, detalle.FechaViaje);
        Assert.Equal(enLista.DistanciaHabitualKm, detalle.DistanciaHabitualKm);
        Assert.Equal(enLista.DistanciaConEnvioKm, detalle.DistanciaConEnvioKm);
        Assert.Equal(enLista.DesvioKm, detalle.DesvioKm);
        Assert.Equal(enLista.TiempoExtraMin, detalle.TiempoExtraMin);
        Assert.Equal(enLista.GananciaPorKm, detalle.GananciaPorKm);
        Assert.Equal(enLista.PaquetesDisponibles, detalle.PaquetesDisponibles);
        Assert.Equal(enLista.PesoDisponibleKg, detalle.PesoDisponibleKg);
    }
    [Fact]
    public void Ordenar_PorDesvio_PonePrimeroElQueMenosSeDesvia()
    {
        var lista = new List<OportunidadDto>
    {
        new() { EnvioId = 1, Pago = 5000m, DesvioKm = 2m },
        new() { EnvioId = 2, Pago = 3000m, DesvioKm = 0.5m }
    };

        var resultado = OrdenadorOportunidades.Ordenar(lista, OrdenOportunidades.Desvio);

        Assert.Equal(2, resultado[0].EnvioId);
    }

    [Fact]
    public void Ordenar_PorGananciaConEmpate_DesempataPorMenorDesvio()
    {
        var lista = new List<OportunidadDto>
    {
        new() { EnvioId = 1, Pago = 4000m, DesvioKm = 2m },
        new() { EnvioId = 2, Pago = 4000m, DesvioKm = 1m }
    };

        var resultado = OrdenadorOportunidades.Ordenar(lista, OrdenOportunidades.Ganancia);

        Assert.Equal(2, resultado[0].EnvioId);
    }
}
