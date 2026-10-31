using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Entidades.Enums;

namespace RuteaYa.Tests;

public static class DatosDePrueba
{
    // Fecha fija (es lunes) para que los tests den siempre lo mismo
    public static readonly DateOnly Hoy = new(2026, 11, 2);

    public static Viaje CrearViaje(RuteaYaContext context, int usuarioId = 1, bool activo = true)
    {
        var viaje = new Viaje
        {
            UsuarioId = usuarioId,
            OrigenDir = "Origen",
            OrigenLat = -34.60m,
            OrigenLng = -58.40m,
            DestinoDir = "Destino",
            DestinoLat = -34.70m,
            DestinoLng = -58.50m,
            HoraSalida = new TimeOnly(8, 0),
            HoraLlegada = new TimeOnly(9, 0),
            MaxPaquetes = 3,
            PesoMaxKg = 10,
            DesvioMaxKm = 5,
            Activo = activo
        };
        viaje.Dias.Add(new ViajeDia { DiaSemana = DayOfWeek.Monday });

        context.Add(viaje);
        context.SaveChanges();
        return viaje;
    }

    public static Envio CrearEnvio(RuteaYaContext context, int usuarioId = 2,
        EstadoEnvio estado = EstadoEnvio.BuscandoConductor)
    {
        var envio = new Envio
        {
            UsuarioId = usuarioId,
            OrigenDir = "A",
            OrigenLat = -34.61m,
            OrigenLng = -58.41m,
            DestinoDir = "B",
            DestinoLat = -34.69m,
            DestinoLng = -58.49m,
            FechaLimite = Hoy.AddDays(7),
            PesoKg = 2,
            Descripcion = "Paquete de prueba",
            Monto = 1000,
            Estado = estado,
            CreadoEn = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }   // la BD en memoria no lo genera sola
        };

        context.Add(envio);
        context.SaveChanges();
        return envio;
    }
}