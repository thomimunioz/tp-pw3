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
        var martin = await CrearUsuarioAsync(userManager, "Martín", "Díaz", "martin@ruteaya.com");
        var sofia = await CrearUsuarioAsync(userManager, "Sofía", "Romero", "sofia@ruteaya.com");

        // ---------- Viajes ----------
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

        // Viaje que Carlos ya no hace: sirve para mostrar el filtro de inactivos
        var carlosLaPlata = CrearViaje(carlos,
            "Morón, Buenos Aires", -34.6534m, -58.6198m,
            "La Plata, Buenos Aires", -34.9214m, -57.9544m,
            new TimeOnly(9, 0), new TimeOnly(10, 45),
            3, 10m, 4m, 75m, 105);
        carlosLaPlata.Activo = false;

        var luciaViaje = CrearViaje(lucia,
            "San Isidro, Buenos Aires", -34.4700m, -58.5100m,
            "Palermo, CABA", -34.5809m, -58.4208m,
            new TimeOnly(8, 0), new TimeOnly(9, 0),
            3, 10m, 4m, 21m, 55,
            NuevaParada(1, "Belgrano, CABA", -34.5627m, -58.4564m));

        // Otra conductora que pasa por la misma zona que Carlos: sirve para mostrar
        // el conflicto de "otro conductor tomó el envío" al confirmar una combinación
        var sofiaViaje = CrearViaje(sofia,
            "Caballito, CABA", -34.6186m, -58.4423m,
            "Microcentro, CABA", -34.6037m, -58.3816m,
            new TimeOnly(8, 0), new TimeOnly(8, 40),
            3, 10m, 4m, 7.5m, 40);

        var envios = new List<Envio>();

        // ---------- Envíos de la semilla original ----------
        envios.Add(CrearEnvio(ana,
            "Palermo, CABA", -34.5809m, -58.4208m,
            "Microcentro, CABA", -34.6037m, -58.3816m,
            3, 1.5m, TamanoPaquete.Pequeno, "Documentos para escribanía", 3500m));

        envios.Add(CrearEnvio(ana,
            "Caballito, CABA", -34.6186m, -58.4423m,
            "Morón, Buenos Aires", -34.6534m, -58.6198m,
            5, 4m, TamanoPaquete.Mediano, "Caja con repuestos", 6000m));

        envios.Add(CrearEnvio(ana,
            "Belgrano, CABA", -34.5627m, -58.4564m,
            "San Isidro, Buenos Aires", -34.4700m, -58.5100m,
            2, 0.8m, TamanoPaquete.Pequeno, "Auriculares", 4200m));

        envios.Add(CrearEnvio(lucia,
            "Microcentro, CABA", -34.6037m, -58.3816m,
            "Lomas de Zamora, Buenos Aires", -34.7609m, -58.4006m,
            7, 12m, TamanoPaquete.Grande, "Mini horno eléctrico", 9000m));

        // Este envío queda asignado a un viaje de Carlos
        var indumentaria = CrearEnvio(ana,
            "Once, CABA", -34.6090m, -58.4060m,
            "Ramos Mejía, Buenos Aires", -34.6403m, -58.5645m,
            4, 6m, TamanoPaquete.Mediano, "Indumentaria", 5500m);
        Asignar(indumentaria, carlosIda, ProximoDia(DayOfWeek.Monday), 1.8m, EstadoEnvio.ConductorAsignado);
        envios.Add(indumentaria);

        // Un envío publicado por Carlos (para que su inicio muestre envíos pendientes)
        envios.Add(CrearEnvio(carlos,
            "Microcentro, CABA", -34.6037m, -58.3816m,
            "Lomas de Zamora, Buenos Aires", -34.7609m, -58.4006m,
            6, 3m, TamanoPaquete.Mediano, "Caja de libros", 4000m));

        // ---------- Oportunidades para el viaje de ida de Carlos ----------
        envios.Add(CrearEnvio(ana,
            "Almagro, CABA", -34.6100m, -58.4222m,
            "San Nicolás, CABA", -34.6045m, -58.3845m,
            4, 3m, TamanoPaquete.Pequeno, "Libros de texto", 4000m));

        envios.Add(CrearEnvio(martin,
            "Caballito, CABA", -34.6206m, -58.4414m,
            "San Nicolás, CABA", -34.6045m, -58.3845m,
            5, 4m, TamanoPaquete.Mediano, "Caja de herramientas", 5000m));

        envios.Add(CrearEnvio(ana,
            "Caballito, CABA", -34.6206m, -58.4414m,
            "Balvanera, CABA", -34.6092m, -58.4031m,
            6, 5m, TamanoPaquete.Mediano, "Ropa de invierno", 6000m));

        envios.Add(CrearEnvio(martin,
            "Balvanera, CABA", -34.6092m, -58.4031m,
            "Microcentro, CABA", -34.6017m, -58.3759m,
            3, 6m, TamanoPaquete.Grande, "Monitor de 27 pulgadas", 8000m));

        envios.Add(CrearEnvio(martin,
            "Almagro, CABA", -34.6100m, -58.4222m,
            "Microcentro, CABA", -34.6017m, -58.3759m,
            12, 2m, TamanoPaquete.Pequeno, "Cuadros enmarcados", 2500m));

        envios.Add(CrearEnvio(ana,
            "Caballito, CABA", -34.6206m, -58.4414m,
            "Microcentro, CABA", -34.6017m, -58.3759m,
            16, 3m, TamanoPaquete.Mediano, "Lámpara de pie", 3000m));

        // ---------- Oportunidades para el viaje de vuelta de Carlos ----------
        envios.Add(CrearEnvio(ana,
            "Haedo, Buenos Aires", -34.6431m, -58.5947m,
            "Morón, Buenos Aires", -34.6511m, -58.6217m,
            4, 2m, TamanoPaquete.Pequeno, "Plantas de interior", 3000m));

        envios.Add(CrearEnvio(martin,
            "Ramos Mejía, Buenos Aires", -34.6482m, -58.5623m,
            "Haedo, Buenos Aires", -34.6431m, -58.5947m,
            5, 3m, TamanoPaquete.Mediano, "Juguetes", 3500m));

        envios.Add(CrearEnvio(ana,
            "Ramos Mejía, Buenos Aires", -34.6482m, -58.5623m,
            "Morón, Buenos Aires", -34.6511m, -58.6217m,
            7, 4m, TamanoPaquete.Mediano, "Vajilla", 4500m));

        envios.Add(CrearEnvio(martin,
            "Haedo, Buenos Aires", -34.6431m, -58.5947m,
            "Morón, Buenos Aires", -34.6511m, -58.6217m,
            18, 2m, TamanoPaquete.Pequeno, "Repuestos de PC", 2800m));

        // ---------- Oportunidades para el viaje de Lucía ----------
        envios.Add(CrearEnvio(ana,
            "Belgrano, Comuna 13, Buenos Aires", -34.5613m, -58.4565m,
            "Palermo, CABA", -34.5803m, -58.4245m,
            4, 2m, TamanoPaquete.Pequeno, "Libros", 3000m));

        envios.Add(CrearEnvio(martin,
            "Belgrano, Comuna 13, Buenos Aires", -34.5613m, -58.4565m,
            "Palermo, CABA", -34.5803m, -58.4245m,
            5, 3m, TamanoPaquete.Mediano, "Plantas", 4500m));

        envios.Add(CrearEnvio(ana,
            "Belgrano, Comuna 13, Buenos Aires", -34.5613m, -58.4565m,
            "Palermo, CABA", -34.5803m, -58.4245m,
            6, 4m, TamanoPaquete.Mediano, "Cafetera", 5000m));

        envios.Add(CrearEnvio(martin,
            "Belgrano, Comuna 13, Buenos Aires", -34.5613m, -58.4565m,
            "Palermo, CABA", -34.5803m, -58.4245m,
            3, 5m, TamanoPaquete.Grande, "Silla plegable", 7000m));

        envios.Add(CrearEnvio(ana,
            "Colegiales, CABA", -34.5741m, -58.4487m,
            "Palermo, CABA", -34.5803m, -58.4245m,
            14, 1m, TamanoPaquete.Pequeno, "Juego de sábanas", 2000m));

        // ---------- Transportes en curso de Carlos (viaje de hoy) ----------
        var notebook = CrearEnvio(ana,
            "Almagro, CABA", -34.6100m, -58.4222m,
            "Microcentro, CABA", -34.6017m, -58.3759m,
            2, 2.5m, TamanoPaquete.Pequeno, "Notebook", 6500m);
        Asignar(notebook, carlosIda, DiaHabil(0), 1.5m, EstadoEnvio.Retirado);
        envios.Add(notebook);

        var bicicleta = CrearEnvio(martin,
            "Caballito, CABA", -34.6206m, -58.4414m,
            "San Nicolás, CABA", -34.6045m, -58.3845m,
            2, 9m, TamanoPaquete.Grande, "Bicicleta plegable", 9500m);
        Asignar(bicicleta, carlosIda, DiaHabil(0), 0.3m, EstadoEnvio.EnCamino);
        envios.Add(bicicleta);

        // Envío que Ana canceló cuando ya lo tenía Carlos
        var cancelado = CrearEnvio(ana,
            "Caballito, CABA", -34.6206m, -58.4414m,
            "Balvanera, CABA", -34.6092m, -58.4031m,
            -3, 3m, TamanoPaquete.Mediano, "Regalo de cumpleaños", 4000m);
        Asignar(cancelado, carlosIda, DiaHabil(-4), 0.3m, EstadoEnvio.ConductorAsignado);
        Cancelar(cancelado);
        envios.Add(cancelado);

        // ---------- Historial: envíos que Carlos y Lucía ya entregaron ----------
        envios.Add(Entregado(martin, carlosIda, -2, 1.2m, "Almagro, CABA", "Microcentro, CABA", 4m, "Electrodoméstico chico", 7000m));
        envios.Add(Entregado(ana, carlosVuelta, -6, 0.6m, "Haedo, Buenos Aires", "Morón, Buenos Aires", 2m, "Medicamentos", 3500m));
        envios.Add(Entregado(martin, carlosIda, -9, 0.3m, "Caballito, CABA", "San Nicolás, CABA", 5m, "Teclado y mouse", 4500m));
        envios.Add(Entregado(ana, carlosIda, -13, 1.5m, "Balvanera, CABA", "Microcentro, CABA", 3m, "Documentación", 5000m));
        envios.Add(Entregado(martin, carlosVuelta, -20, 1.6m, "Ramos Mejía, Buenos Aires", "Haedo, Buenos Aires", 6m, "Cajas de mudanza", 8000m));
        envios.Add(Entregado(ana, carlosIda, -27, 0.1m, "Almagro, CABA", "San Nicolás, CABA", 2m, "Libros usados", 3000m));
        envios.Add(Entregado(martin, carlosIda, -34, 1.8m, "Caballito, CABA", "Microcentro, CABA", 4m, "Impresora", 6000m));
        envios.Add(Entregado(ana, luciaViaje, -3, 0.4m, "Belgrano, Comuna 13, Buenos Aires", "Palermo, CABA", 2m, "Vinilos", 3800m));
        envios.Add(Entregado(martin, luciaViaje, -10, 0.4m, "Belgrano, Comuna 13, Buenos Aires", "Palermo, CABA", 4m, "Ventilador", 5200m));
        envios.Add(Entregado(ana, luciaViaje, -24, 0.4m, "Colegiales, CABA", "Palermo, CABA", 1m, "Perfumes", 2500m));

        context.AddRange(carlosIda, carlosVuelta, carlosLaPlata, luciaViaje, sofiaViaje);
        context.AddRange(envios);
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
            CreadoEn = DateTime.Now
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
            CreadoEn = DateTime.Now
        };

        envio.Historial.Add(new EnvioEstadoHistorial
        {
            EstadoAnterior = null,
            EstadoNuevo = EstadoEnvio.BuscandoConductor,
            Fecha = DateTime.Now
        });

        return envio;
    }

    // Asigna el envío al viaje y lo avanza hasta "estadoFinal", dejando un registro por cada cambio
    private static void Asignar(Envio envio, Viaje viaje, DateOnly fechaViaje, decimal desvioKm, EstadoEnvio estadoFinal)
    {
        var salida = fechaViaje.ToDateTime(viaje.HoraSalida);
        var asignadoEn = salida.AddDays(-1);
        if (asignadoEn > DateTime.Now)
        {
            asignadoEn = DateTime.Now.AddHours(-2);
        }

        envio.CreadoEn = asignadoEn.AddDays(-1);
        envio.Historial.First().Fecha = envio.CreadoEn;
        envio.Asignacion = new Asignacion
        {
            Viaje = viaje,
            FechaViaje = fechaViaje,
            DesvioKm = desvioKm,
            TiempoExtraMin = (int)Math.Round(desvioKm * 2),
            Ganancia = envio.Monto,
            AsignadoEn = asignadoEn
        };

        var pasos = new[] { EstadoEnvio.ConductorAsignado, EstadoEnvio.Retirado, EstadoEnvio.EnCamino, EstadoEnvio.Entregado };
        var anterior = EstadoEnvio.BuscandoConductor;
        var fecha = asignadoEn;
        foreach (var paso in pasos)
        {
            envio.Historial.Add(new EnvioEstadoHistorial
            {
                EstadoAnterior = anterior,
                EstadoNuevo = paso,
                Fecha = fecha
            });

            anterior = paso;
            fecha = paso == EstadoEnvio.ConductorAsignado ? salida : fecha.AddMinutes(30);
            if (paso == estadoFinal) break;
        }

        envio.Estado = estadoFinal;
    }

    private static void Cancelar(Envio envio)
    {
        envio.Historial.Add(new EnvioEstadoHistorial
        {
            EstadoAnterior = envio.Estado,
            EstadoNuevo = EstadoEnvio.Cancelado,
            Fecha = envio.Asignacion!.AsignadoEn.AddHours(2)
        });
        envio.Estado = EstadoEnvio.Cancelado;
    }

    // Envío que ya se entregó hace "dias" días (para el historial)
    private static Envio Entregado(Usuario remitente, Viaje viaje, int dias, decimal desvioKm,
        string origen, string destino, decimal pesoKg, string descripcion, decimal monto)
    {
        var envio = CrearEnvio(remitente,
            origen, viaje.OrigenLat, viaje.OrigenLng,
            destino, viaje.DestinoLat, viaje.DestinoLng,
            dias + 1, pesoKg, TamanoPaquete.Mediano, descripcion, monto);
        Asignar(envio, viaje, DiaHabil(dias), desvioKm, EstadoEnvio.Entregado);
        return envio;
    }

    // Día hábil a "dias" días de hoy (si cae sábado o domingo, se corre al viernes anterior)
    private static DateOnly DiaHabil(int dias)
    {
        var fecha = DateOnly.FromDateTime(DateTime.Today).AddDays(dias);
        while (fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday)
        {
            fecha = fecha.AddDays(-1);
        }
        return fecha;
    }

    private static DateOnly ProximoDia(DayOfWeek dia)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var falta = ((int)dia - (int)hoy.DayOfWeek + 7) % 7;
        return hoy.AddDays(falta == 0 ? 7 : falta);
    }
}
