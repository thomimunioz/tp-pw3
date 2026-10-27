using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Oportunidades;

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
        //Arrange
        using var context = CrearContexto();
        var viaje = CrearViaje(DayOfWeek.Wednesday);
        context.Viajes.Add(viaje);
        await context.SaveChangesAsync();
        var servicio = new OportunidadesServicios(context);

        // Act
        var resultado = await servicio.BuscarAsync(Remitente, viaje.Id, Hoy);

        //Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task BuscarAsync_EnvioEnElCaminoConLugar_LoDevuelveConElPrimerDiaDelViaje()
    {
        
    }
}
