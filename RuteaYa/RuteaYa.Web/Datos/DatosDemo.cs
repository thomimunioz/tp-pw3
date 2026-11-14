using Microsoft.AspNetCore.Identity;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Entidades.Enums;

namespace RuteaYa.Web.Datos;

public static class DatosDemo
{
    private const string Password = "Demo1234";

    public static async Task SembrarAsync(IServiceProvider servicios)
    {
        var context = servicios.GetRequiredService<RuteaYaContext>();
        var userManager = servicios.GetRequiredService<UserManager<Usuario>>();

        // Si el usuario demo ya existe, no se vuelve a sembrar
        if (await userManager.FindByEmailAsync("ana@ruteaya.com") != null) return;

        var ana = await CrearUsuarioAsync(userManager, "Ana", "Pérez", "ana@ruteaya.com");
        var carlos = await CrearUsuarioAsync(userManager, "Carlos", "Gómez", "carlos@ruteaya.com");
        var lucia = await CrearUsuarioAsync(userManager, "Lucía", "Fernández", "lucia@ruteaya.com");

        var carlosIda = CrearViaje(carlos,
            "Morón, Buenos Aires", -34.6534m, -58.6198m,
            "Microcentro, CABA", -34.6037m, -58.3816m,
            new TimeOnly(7, 30), new TimeOnly(8, 45),
            4, 15m, 5m, 22.5m, 65,
            NuevaParada(1, "Ramos Mejía, Buenos Aires", -34.6403m, -58.5645m),
            NuevaParada(2, "Caballito, CABA", -34.6186m, -58.4423m));

        var carlosVuelta = CrearViaje(carlos,
            "Microcentro, CABA", -34.6037m, -58.3816m,
            "Morón, Buenos Aires", -34.6534m, -58.6198m,
            new TimeOnly(18, 0), new TimeOnly(19, 15),
            4, 15m, 5m, 22.5m, 75,
            NuevaParada(1, "Caballito, CABA", -34.6186m, -58.4423m),
            NuevaParada(2, "Ramos Mejía, Buenos Aires", -34.6403m, -58.5645m));

        var luciaViaje = CrearViaje(lucia,
            "San Isidro, Buenos Aires", -34.4700m, -58.5100m,
            "Palermo, CABA", -34.5809m, -58.4208m,
            new TimeOnly(8, 0), new TimeOnly(9, 0),
            3, 10m, 4m, 21m, 55,
            NuevaParada(1, "Belgrano, CABA", -34.5627m, -58.4564m));

        var envio1 = CrearEnvio(ana,
            "Palermo, CABA", -34.5809m, -58.4208m,
            "Microcentro, CABA", -34.6037m, -58.3816m,
            3, 1.5m, TamanoPaquete.Pequeno, "Documentos para escribanía", 3500m);

        var envio2 = CrearEnvio(ana,
            "Caballito, CABA", -34.6186m, -58.4423m,
            "Morón, Buenos Aires", -34.6534m, -58.6198m,
            5, 4m, TamanoPaquete.Mediano, "Caja con repuestos", 6000m);

        var envio3 = CrearEnvio(ana,
            "Belgrano, CABA", -34.5627m, -58.4564m,
            "San Isidro, Buenos Aires", -34.4700m, -58.5100m,
            2, 0.8m, TamanoPaquete.Pequeno, "Auriculares", 4200m);

        var envio4 = CrearEnvio(lucia,
            "Microcentro, CABA", -34.6037m, -58.3816m,
            "Lomas de Zamora, Buenos Aires", -34.7609m, -58.4006m,
            7, 12m, TamanoPaquete.Grande, "Mini horno eléctrico", 9000m);

        // Este envío queda asignado a un viaje de Carlos
        var asignado = CrearEnvio(ana,
            "Once, CABA", -34.6090m, -58.4060m,
            "Ramos Mejía, Buenos Aires", -34.6403m, -58.5645m,
            4, 6m, TamanoPaquete.Mediano, "Indumentaria", 5500m);
        asignado.Estado = EstadoEnvio.ConductorAsignado;
        asignado.Historial.Add(new EnvioEstadoHistorial
        {
            EstadoAnterior = EstadoEnvio.BuscandoConductor,
            EstadoNuevo = EstadoEnvio.ConductorAsignado,
            Fecha = DateTime.UtcNow
        });
        asignado.Asignacion = new Asignacion
        {
            Viaje = carlosIda,
            FechaViaje = ProximoDia(DayOfWeek.Monday),
            DesvioKm = 1.8m,
            TiempoExtraMin = 7,
            Ganancia = 4400m,
            AsignadoEn = DateTime.UtcNow
        };

        context.AddRange(carlosIda, carlosVuelta, luciaViaje);
        context.AddRange(envio1, envio2, envio3, envio4, asignado);
        await context.SaveChangesAsync();
    }

    private static async Task<Usuario> CrearUsuarioAsync(
        UserManager<Usuario> userManager, string nombre, string apellido, string email)
    {
        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            Nombre = nombre,
            Apellido = apellido,
            CreadoEn = DateTime.UtcNow
        };

        var resultado = await userManager.CreateAsync(usuario, Password);
        if (!resultado.Succeeded)
        {
            var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"No se pudo crear el usuario demo {email}: {errores}");
        }

        return usuario;
    }

    private static Viaje CrearViaje(Usuario conductor,
        string origen, decimal origenLat, decimal origenLng,
        string destino, decimal destinoLat, decimal destinoLng,
        TimeOnly salida, TimeOnly llegada,
        int maxPaquetes, decimal pesoMaxKg, decimal desvioMaxKm,
        decimal distanciaKm, int duracionMin,
        params Parada[] paradas)
    {
        var viaje = new Viaje
        {
            Usuario = conductor,
            OrigenDir = origen,
            OrigenLat = origenLat,
            OrigenLng = origenLng,
            DestinoDir = destino,
            DestinoLat = destinoLat,
            DestinoLng = destinoLng,
            HoraSalida = salida,
            HoraLlegada = llegada,
            MaxPaquetes = maxPaquetes,
            PesoMaxKg = pesoMaxKg,
            DesvioMaxKm = desvioMaxKm,
            DistanciaKm = distanciaKm,
            DuracionMin = duracionMin,
            Activo = true
        };

        for (var dia = DayOfWeek.Monday; dia <= DayOfWeek.Friday; dia++)
        {
            viaje.Dias.Add(new ViajeDia { DiaSemana = dia });
        }

        foreach (var parada in paradas)
        {
            viaje.Paradas.Add(parada);
        }

        return viaje;
    }

    private static Parada NuevaParada(int orden, string direccion, decimal lat, decimal lng)
        => new() { Orden = orden, Direccion = direccion, Lat = lat, Lng = lng };

    private static Envio CrearEnvio(Usuario remitente,
        string origen, decimal origenLat, decimal origenLng,
        string destino, decimal destinoLat, decimal destinoLng,
        int diasLimite, decimal pesoKg, TamanoPaquete tamano,
        string descripcion, decimal monto)
    {
        var envio = new Envio
        {
            Usuario = remitente,
            OrigenDir = origen,
            OrigenLat = origenLat,
            OrigenLng = origenLng,
            DestinoDir = destino,
            DestinoLat = destinoLat,
            DestinoLng = destinoLng,
            FechaLimite = DateOnly.FromDateTime(DateTime.Today).AddDays(diasLimite),
            PesoKg = pesoKg,
            Tamano = tamano,
            Descripcion = descripcion,
            Monto = monto,
            Estado = EstadoEnvio.BuscandoConductor,
            CreadoEn = DateTime.UtcNow
        };

        envio.Historial.Add(new EnvioEstadoHistorial
        {
            EstadoAnterior = null,
            EstadoNuevo = EstadoEnvio.BuscandoConductor,
            Fecha = DateTime.UtcNow
        });

        return envio;
    }

    private static DateOnly ProximoDia(DayOfWeek dia)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var falta = ((int)dia - (int)hoy.DayOfWeek + 7) % 7;
        return hoy.AddDays(falta == 0 ? 7 : falta);
    }
}