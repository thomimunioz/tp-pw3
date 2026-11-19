using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Historial;
using RuteaYa.Logica.Historial.Dtos;

namespace RuteaYa.Tests.Historial;

public class HistorialServiciosTests
{
    private const int ConductorId = 1;

    private static void Asignar(RuteaYaContext context, Viaje viaje, EstadoEnvio estado, decimal ganancia)
    {
        var envio = DatosDePrueba.CrearEnvio(context, estado: estado);
        context.Add(new Asignacion
        {
            EnvioId = envio.Id,
            ViajeId = viaje.Id,
            FechaViaje = DatosDePrueba.Hoy,
            Ganancia = ganancia,
            AsignadoEn = DateTime.Now
        });
        context.SaveChanges();
    }

    private static RuteaYaContext CrearDatos()
    {
        var context = ContextoDePrueba.Crear();
        var viaje = DatosDePrueba.CrearViaje(context, ConductorId);
        Asignar(context, viaje, EstadoEnvio.Entregado, 1000);
        Asignar(context, viaje, EstadoEnvio.Entregado, 2000);
        Asignar(context, viaje, EstadoEnvio.Cancelado, 500);
        Asignar(context, viaje, EstadoEnvio.EnCamino, 700);
        return context;
    }

    [Fact]
    public async Task SinFiltros_MuestraSoloLosTerminados()
    {
        using var context = CrearDatos();
        var servicio = new HistorialServicios(context);

        var resumen = await servicio.ObtenerHistorialAsync(ConductorId);

        Assert.Equal(3, resumen.Operaciones.Count);
        Assert.Equal(2, resumen.EnviosTransportados);
        Assert.Equal(1, resumen.EnviosCancelados);
        Assert.Equal(3000, resumen.GananciaEfectiva);
        Assert.Equal(1, resumen.ViajesRealizados);
    }

    [Fact]
    public async Task FiltroSoloCancelados_LosTotalesCoincidenConLaLista()
    {
        using var context = CrearDatos();
        var servicio = new HistorialServicios(context);

        var resumen = await servicio.ObtenerHistorialAsync(ConductorId, new FiltroHistorialDto { Estado = EstadoEnvio.Cancelado });

        Assert.Single(resumen.Operaciones);
        Assert.Equal(0, resumen.EnviosTransportados);
        Assert.Equal(0, resumen.GananciaEfectiva);
        Assert.Equal(1, resumen.EnviosCancelados);
    }

    [Fact]
    public async Task FiltroOtroMes_NoMuestraNada()
    {
        using var context = CrearDatos();
        var servicio = new HistorialServicios(context);
        var otroMes = DatosDePrueba.Hoy.AddMonths(1).Month;

        var resumen = await servicio.ObtenerHistorialAsync(ConductorId, new FiltroHistorialDto { Mes = otroMes });

        Assert.Empty(resumen.Operaciones);
        Assert.Equal(0, resumen.GananciaEfectiva);
    }
}
