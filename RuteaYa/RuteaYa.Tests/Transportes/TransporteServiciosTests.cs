using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Transportes;

namespace RuteaYa.Tests.Transportes;

public class TransporteServiciosTests
{
    private const int ConductorId = 1;

    private static Envio CrearTransporte(RuteaYaContext context, DateOnly fechaViaje)
    {
        var viaje = DatosDePrueba.CrearViaje(context, ConductorId);
        var envio = DatosDePrueba.CrearEnvio(context, estado: EstadoEnvio.ConductorAsignado);
        context.Add(new Asignacion
        {
            EnvioId = envio.Id,
            ViajeId = viaje.Id,
            FechaViaje = fechaViaje,
            Ganancia = envio.Monto,
            AsignadoEn = DateTime.Now
        });
        context.SaveChanges();
        return envio;
    }

    [Fact]
    public async Task MarcarRetirado_AntesDelDiaDelViaje_NoLoPermite()
    {
        using var context = ContextoDePrueba.Crear();
        var envio = CrearTransporte(context, DateOnly.FromDateTime(DateTime.Today).AddDays(3));
        var servicio = new TransporteServicios(context);

        var resultado = await servicio.MarcarRetiradoAsync(ConductorId, envio.Id);

        Assert.False(resultado.ResultadoExitoso);
        Assert.Equal(EstadoEnvio.ConductorAsignado, envio.Estado);
    }

    [Fact]
    public async Task MarcarRetirado_ElDiaDelViaje_LoPermite()
    {
        using var context = ContextoDePrueba.Crear();
        var envio = CrearTransporte(context, DateOnly.FromDateTime(DateTime.Today));
        var servicio = new TransporteServicios(context);

        var resultado = await servicio.MarcarRetiradoAsync(ConductorId, envio.Id);

        Assert.True(resultado.ResultadoExitoso);
        Assert.Equal(EstadoEnvio.Retirado, envio.Estado);
    }
}
