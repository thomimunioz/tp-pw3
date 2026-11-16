using RuteaYa.Entidades;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Oportunidades;

namespace RuteaYa.Tests.Oportunidades;

public class OportunidadesFechasTests
{
    private const int Conductor = 1;

    [Fact]
    public async Task BuscarAsync_EseDiaYaSeUsoCasiTodoElDesvio_OfreceElLunesSiguiente()
    {
        // Arrange: el lunes ya hay un envío asignado que usa 4 de los 5 km de desvío
        using var context = ContextoDePrueba.Crear();
        var viaje = DatosDePrueba.CrearViaje(context, Conductor);
        var asignado = DatosDePrueba.CrearEnvio(context, estado: EstadoEnvio.ConductorAsignado);
        context.Add(new Asignacion
        {
            EnvioId = asignado.Id,
            ViajeId = viaje.Id,
            FechaViaje = DatosDePrueba.Hoy,
            DesvioKm = 4m,
            Ganancia = asignado.Monto
        });

        // Este envío agrega 1,53 km: ese lunes no entra (4 + 1,53 > 5)
        var envio = DatosDePrueba.CrearEnvio(context);
        envio.OrigenLat = -34.62m;
        envio.OrigenLng = -58.44m;
        envio.DestinoLat = -34.67m;
        envio.DestinoLng = -58.45m;
        context.SaveChanges();

        var servicio = new OportunidadesServicios(context);

        // Act
        var oportunidades = await servicio.BuscarAsync(Conductor, viaje.Id, DatosDePrueba.Hoy);

        // Assert
        var oportunidad = Assert.Single(oportunidades!);
        Assert.Equal(envio.Id, oportunidad.EnvioId);
        Assert.Equal(DatosDePrueba.Hoy.AddDays(7), oportunidad.FechaViaje);
        Assert.Equal(5m, oportunidad.DesvioDisponibleKm);
    }

    [Fact]
    public async Task BuscarAsync_FechaLimiteMuyLejanaYNingunDiaSirve_NoTiraError()
    {
        // Arrange: pesa 50 kg y el viaje lleva 10, así que no entra ningún día
        using var context = ContextoDePrueba.Crear();
        var viaje = DatosDePrueba.CrearViaje(context, Conductor);
        var envio = DatosDePrueba.CrearEnvio(context);
        envio.PesoKg = 50;
        envio.FechaLimite = DateOnly.MaxValue;
        context.SaveChanges();

        var servicio = new OportunidadesServicios(context);

        // Act
        var oportunidades = await servicio.BuscarAsync(Conductor, viaje.Id, DatosDePrueba.Hoy);

        // Assert
        Assert.NotNull(oportunidades);
        Assert.Empty(oportunidades);
    }
}
