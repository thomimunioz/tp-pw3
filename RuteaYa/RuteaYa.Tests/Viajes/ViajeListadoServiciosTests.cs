using RuteaYa.Logica.Viajes;

namespace RuteaYa.Tests.Viajes;

public class ViajeListadoServiciosTests
{
    [Fact]
    public async Task ObtenerActivosAsync_DevuelveSoloLosViajesActivos()
    {
        using var context = ContextoDePrueba.Crear();
        var activo = DatosDePrueba.CrearViaje(context, activo: true);
        DatosDePrueba.CrearViaje(context, activo: false);
        var servicio = new ViajeListadoServicios(context);

        var resultado = await servicio.ObtenerActivosAsync();

        var unico = Assert.Single(resultado);
        Assert.Equal(activo.Id, unico.Id);
        Assert.Equal("Origen", unico.Origen);
    }
}