using RuteaYa.Entidades;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Optimizacion;

namespace RuteaYa.Tests.Optimizacion;

public class ProyeccionMensualServiciosTests
{
    [Fact]
    public void Optimizar_MaximizarGanancias_DesempataConMenorDesvio()
    {
        var resultado = Optimizador.Optimizar(
            new List<CandidatoEnvio>
            {
                new() { EnvioId = 1, Pago = 100m, DesvioKm = 2m },
                new() { EnvioId = 2, Pago = 100m, DesvioKm = 1m }
            },
            1,
            10m,
            10m,
            CriterioOptimizacion.MaximizarGanancias);

        Assert.Equal(2, Assert.Single(resultado.Seleccionados).EnvioId);
    }

    [Fact]
    public void Optimizar_MinimizarDesvios_PriorizaMenorDesvio()
    {
        var resultado = Optimizador.Optimizar(
            new List<CandidatoEnvio>
            {
                new() { EnvioId = 1, Pago = 100m, DesvioKm = 2m },
                new() { EnvioId = 2, Pago = 50m, DesvioKm = 0.5m }
            },
            1,
            10m,
            10m,
            CriterioOptimizacion.MinimizarDesvios);

        Assert.Equal(2, Assert.Single(resultado.Seleccionados).EnvioId);
    }

    [Fact]
    public void Optimizar_Equilibrio_PriorizaGananciaPorDesvio()
    {
        var resultado = Optimizador.Optimizar(
            new List<CandidatoEnvio>
            {
                new() { EnvioId = 1, Pago = 100m, DesvioKm = 4m },
                new() { EnvioId = 2, Pago = 60m, DesvioKm = 1m }
            },
            1,
            10m,
            10m,
            CriterioOptimizacion.Equilibrio);

        Assert.Equal(2, Assert.Single(resultado.Seleccionados).EnvioId);
    }

    [Fact]
    public async Task CalcularAsync_NoDuplicaEnviosEntreOcurrenciasDeUnViajeRecurrente()
    {
        using var context = ContextoDePrueba.Crear();
        CrearViaje(context, usuarioId: 1, dia: DayOfWeek.Monday);
        CrearEnvio(context, usuarioId: 2, fechaLimite: new DateOnly(2026, 11, 30));
        var servicio = new ProyeccionMensualServicios(context);

        var resultado = await servicio.CalcularAsync(
            1,
            new DateOnly(2026, 11, 1),
            CriterioOptimizacion.MaximizarGanancias,
            DatosDePrueba.Hoy);

        Assert.Equal(5, resultado.ViajesAnalizados);
        Assert.Equal(1, resultado.PaquetesPosibles);
        Assert.Equal(1, resultado.Viajes.Sum(v => v.Envios.Count));
        Assert.Equal(1000m, resultado.GananciaPotencial);
        Assert.Equal(200m, resultado.PromedioPorViaje);
        Assert.Equal(1000m, resultado.Semanas.Sum(s => s.GananciaPotencial));
    }

    [Fact]
    public async Task CalcularAsync_ExcluyeEnviosDelUsuarioConPesoExcesivoYConFechaVencida()
    {
        using var context = ContextoDePrueba.Crear();
        CrearViaje(context, usuarioId: 1, dia: DayOfWeek.Monday, pesoMaxKg: 2m);
        CrearEnvio(context, usuarioId: 1, fechaLimite: new DateOnly(2026, 11, 30));
        CrearEnvio(context, usuarioId: 2, fechaLimite: new DateOnly(2026, 11, 30), pesoKg: 3m);
        CrearEnvio(context, usuarioId: 2, fechaLimite: new DateOnly(2026, 11, 1));
        var servicio = new ProyeccionMensualServicios(context);

        var resultado = await servicio.CalcularAsync(
            1,
            new DateOnly(2026, 11, 1),
            CriterioOptimizacion.MaximizarGanancias,
            DatosDePrueba.Hoy);

        Assert.Equal(0, resultado.PaquetesPosibles);
        Assert.Equal(0m, resultado.GananciaPotencial);
        Assert.All(resultado.Viajes, viaje => Assert.Empty(viaje.Envios));
    }

    private static Viaje CrearViaje(
        RuteaYa.Entidades.Data.RuteaYaContext context,
        int usuarioId,
        DayOfWeek dia,
        decimal pesoMaxKg = 10m)
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
            MaxPaquetes = 2,
            PesoMaxKg = pesoMaxKg,
            DesvioMaxKm = 20m,
            Activo = true
        };
        viaje.Dias.Add(new ViajeDia { DiaSemana = dia });
        context.Viajes.Add(viaje);
        context.SaveChanges();
        return viaje;
    }

    private static void CrearEnvio(
        RuteaYa.Entidades.Data.RuteaYaContext context,
        int usuarioId,
        DateOnly fechaLimite,
        decimal pesoKg = 1m)
    {
        context.Envios.Add(new Envio
        {
            UsuarioId = usuarioId,
            OrigenDir = "Punto A",
            OrigenLat = -34.61m,
            OrigenLng = -58.41m,
            DestinoDir = "Punto B",
            DestinoLat = -34.69m,
            DestinoLng = -58.49m,
            FechaLimite = fechaLimite,
            PesoKg = pesoKg,
            Descripcion = "Paquete de prueba",
            Monto = 1000m,
            Estado = EstadoEnvio.BuscandoConductor,
            CreadoEn = DateTime.UtcNow,
            RowVersion = new byte[] { 1 }
        });
        context.SaveChanges();
    }
}
