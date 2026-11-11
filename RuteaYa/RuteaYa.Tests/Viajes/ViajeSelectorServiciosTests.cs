using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Viajes;

namespace RuteaYa.Tests.Viajes;

public class ViajeSelectorServiciosTests
{
    private static readonly DateTime Ahora = new DateTime(2026, 11, 2, 15, 0, 0);
    private const int Conductor = 1;
    private const int Otro = 2;

    private static RuteaYaContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<RuteaYaContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RuteaYaContext(options);
    }

    private static Viaje CrearViaje(TimeOnly horaSalida, params DayOfWeek[] dias)
    {
        var viaje = new Viaje
        {
            UsuarioId = Conductor,
            OrigenDir = "Origen del viaje",
            DestinoDir = "Destino del viaje",
            HoraSalida = horaSalida,
            HoraLlegada = horaSalida.AddHours(1),
            MaxPaquetes = 3,
            PesoMaxKg = 10m,
            DesvioMaxKm = 3m,
            Activo = true
        };

        foreach (var dia in dias)
            viaje.Dias.Add(new ViajeDia { DiaSemana = dia });

        return viaje;
    }


    [Fact]
    public async Task ObtenerActivosAsync_UsuarioSinViajes_DevuelveListaVacia()
    {
        using var context = CrearContexto();
        var servicio = new ViajeSelectorServicios(context);

        var viajes = await servicio.ObtenerActivosAsync(Conductor);

        Assert.Empty(viajes);
    }

    [Fact]
    public async Task ObtenerActivosAsync_HayActivosInactivosYDeOtroUsuario_DevuelveSoloLosActivosDelUsuario()
    {
        using var context = CrearContexto();
        var propioActivo = CrearViaje(new TimeOnly(8, 0), DayOfWeek.Monday);
        var propioInactivo = CrearViaje(new TimeOnly(9, 0), DayOfWeek.Tuesday);
        propioInactivo.Activo = false;
        var ajeno = CrearViaje(new TimeOnly(10, 0), DayOfWeek.Wednesday);
        ajeno.UsuarioId = Otro;
        context.Viajes.AddRange(propioActivo, propioInactivo, ajeno);
        await context.SaveChangesAsync();
        var servicio = new ViajeSelectorServicios(context);

        var viajes = await servicio.ObtenerActivosAsync(Conductor);

        var viaje = Assert.Single(viajes);
        Assert.Equal(propioActivo.Id, viaje.Id);
        Assert.Equal("Origen del viaje", viaje.OrigenDir);
        Assert.Equal("Destino del viaje", viaje.DestinoDir);
        Assert.Equal(new TimeOnly(8, 0), viaje.HoraSalida);
        Assert.Equal(new[] { DayOfWeek.Monday }, viaje.Dias);
    }


    [Fact]
    public async Task ObtenerViajeIdPorDefectoAsync_SinViajesActivos_DevuelveNull()
    {
        using var context = CrearContexto();
        var inactivo = CrearViaje(new TimeOnly(18, 0), DayOfWeek.Monday);
        inactivo.Activo = false;
        context.Viajes.Add(inactivo);
        await context.SaveChangesAsync();
        var servicio = new ViajeSelectorServicios(context);

        var id = await servicio.ObtenerViajeIdPorDefectoAsync(Conductor, Ahora);

        Assert.Null(id);
    }

    [Fact]
    public async Task ObtenerViajeIdPorDefectoAsync_SaleHoyMasTardeYOtroSaleManana_DevuelveElDeHoy()
    {
        using var context = CrearContexto();
        var hoyMasTarde = CrearViaje(new TimeOnly(18, 0), DayOfWeek.Monday);
        var manana = CrearViaje(new TimeOnly(8, 0), DayOfWeek.Tuesday);
        context.Viajes.AddRange(hoyMasTarde, manana);
        await context.SaveChangesAsync();
        var servicio = new ViajeSelectorServicios(context);

        var id = await servicio.ObtenerViajeIdPorDefectoAsync(Conductor, Ahora);

        Assert.Equal(hoyMasTarde.Id, id);
    }

    [Fact]
    public async Task ObtenerViajeIdPorDefectoAsync_ElViajeDeHoyYaSalio_DevuelveElDelProximoDia()
    {
        using var context = CrearContexto();
        var yaSalio = CrearViaje(new TimeOnly(8, 0), DayOfWeek.Monday);
        var miercoles = CrearViaje(new TimeOnly(8, 0), DayOfWeek.Wednesday);
        context.Viajes.AddRange(yaSalio, miercoles);
        await context.SaveChangesAsync();
        var servicio = new ViajeSelectorServicios(context);

        var id = await servicio.ObtenerViajeIdPorDefectoAsync(Conductor, Ahora);

        Assert.Equal(miercoles.Id, id);
    }

    [Fact]
    public async Task ObtenerViajeIdPorDefectoAsync_VariosViajesHoy_DevuelveElQueSaleDespuesDeAhoraMasCercano()
    {
        using var context = CrearContexto();
        var salio = CrearViaje(new TimeOnly(9, 0), DayOfWeek.Monday);
        var alas18 = CrearViaje(new TimeOnly(18, 0), DayOfWeek.Monday);
        var alas16 = CrearViaje(new TimeOnly(16, 0), DayOfWeek.Monday);
        context.Viajes.AddRange(salio, alas18, alas16);
        await context.SaveChangesAsync();
        var servicio = new ViajeSelectorServicios(context);

        var id = await servicio.ObtenerViajeIdPorDefectoAsync(Conductor, Ahora);

        Assert.Equal(alas16.Id, id);
    }

    [Fact]
    public async Task ObtenerViajeIdPorDefectoAsync_ViajeConVariosDias_UsaElDiaMasCercano()
    {
        using var context = CrearContexto();
        var lunesYMartes = CrearViaje(new TimeOnly(8, 0), DayOfWeek.Monday, DayOfWeek.Tuesday);
        var miercoles = CrearViaje(new TimeOnly(8, 0), DayOfWeek.Wednesday);
        context.Viajes.AddRange(miercoles, lunesYMartes);
        await context.SaveChangesAsync();
        var servicio = new ViajeSelectorServicios(context);

        var id = await servicio.ObtenerViajeIdPorDefectoAsync(Conductor, Ahora);

        Assert.Equal(lunesYMartes.Id, id);
    }

    [Fact]
    public async Task ObtenerViajeIdPorDefectoAsync_HoyEsDomingoYElViajeEsLunes_PasaDeUnaSemanaALaOtra()
    {
        var domingo = new DateTime(2026, 11, 8, 15, 0, 0);
        using var context = CrearContexto();
        var lunes = CrearViaje(new TimeOnly(8, 0), DayOfWeek.Monday);
        var sabado = CrearViaje(new TimeOnly(8, 0), DayOfWeek.Saturday);
        context.Viajes.AddRange(sabado, lunes);
        await context.SaveChangesAsync();
        var servicio = new ViajeSelectorServicios(context);

        var id = await servicio.ObtenerViajeIdPorDefectoAsync(Conductor, domingo);

        Assert.Equal(lunes.Id, id);
    }

    [Fact]
    public async Task ObtenerViajeIdPorDefectoAsync_UnicoViajeYaSalioHoy_LoDevuelveIgual()
    {
        using var context = CrearContexto();
        var viaje = CrearViaje(new TimeOnly(8, 0), DayOfWeek.Monday);
        context.Viajes.Add(viaje);
        await context.SaveChangesAsync();
        var servicio = new ViajeSelectorServicios(context);

        var id = await servicio.ObtenerViajeIdPorDefectoAsync(Conductor, Ahora);

        Assert.Equal(viaje.Id, id);
    }

    [Fact]
    public async Task ObtenerViajeIdPorDefectoAsync_HayInactivosYViajesDeOtroUsuario_LosIgnora()
    {
        using var context = CrearContexto();
        var inactivo = CrearViaje(new TimeOnly(18, 0), DayOfWeek.Monday);
        inactivo.Activo = false;
        var ajeno = CrearViaje(new TimeOnly(18, 0), DayOfWeek.Monday);
        ajeno.UsuarioId = Otro;
        var propio = CrearViaje(new TimeOnly(8, 0), DayOfWeek.Wednesday);
        context.Viajes.AddRange(inactivo, ajeno, propio);
        await context.SaveChangesAsync();
        var servicio = new ViajeSelectorServicios(context);

        var id = await servicio.ObtenerViajeIdPorDefectoAsync(Conductor, Ahora);

        Assert.Equal(propio.Id, id);
    }

    [Fact]
    public async Task ObtenerViajeIdPorDefectoAsync_ViajeSinDiasCargados_LoDevuelveComoRespaldo()
    {
        using var context = CrearContexto();
        var sinDias = CrearViaje(new TimeOnly(8, 0));
        context.Viajes.Add(sinDias);
        await context.SaveChangesAsync();
        var servicio = new ViajeSelectorServicios(context);

        var id = await servicio.ObtenerViajeIdPorDefectoAsync(Conductor, Ahora);

        Assert.Equal(sinDias.Id, id);
    }
}