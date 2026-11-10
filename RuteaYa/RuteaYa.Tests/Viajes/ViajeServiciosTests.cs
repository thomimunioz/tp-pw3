using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Viajes;
using RuteaYa.Tests.Envios;

namespace RuteaYa.Tests.Viajes
{
    public class ViajeServiciosTests
    {
        private RuteaYaContext CrearContextoEnMemoria()
        {
            var options = new DbContextOptionsBuilder<RuteaYaContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new RuteaYaContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private ViajeServicios CrearServicio(RuteaYaContext context)
        {
            return new ViajeServicios(context, new MockGeocodificador());
        }

        [Fact]
        public async Task ObtenerPorUsuarioAsync_UsuarioSinViajes_DevuelveListaVacia()
        {
            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);

            var resultado = await servicio.ObtenerPorUsuarioAsync(999);

            Assert.NotNull(resultado);
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task CrearViajeAsync_ConDatosValidos_GuardaViajeYAsociaDiasYParadas()
        {
            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);

            var viaje = new Viaje
            {
                UsuarioId = 1,
                OrigenDir = "Buenos Aires",
                DestinoDir = "Catamarca",
                HoraSalida = new TimeOnly(8, 0),
                HoraLlegada = new TimeOnly(12, 0),
                MaxPaquetes = 5,
                PesoMaxKg = 10m,
                DesvioMaxKm = 2m,
                Activo = true
            };

            var diasSeleccionados = new List<int> { 1, 3, 5 };
            string paradasIntermedias = "Morón, Luján";

            await servicio.CrearViajeAsync(viaje, diasSeleccionados, paradasIntermedias);

            var viajesEnDb = await context.Viajes
                .Include(v => v.Dias)
                .Include(v => v.Paradas)
                .ToListAsync();

            Assert.Single(viajesEnDb);
            var guardado = viajesEnDb.First();
            Assert.Equal("Buenos Aires", guardado.OrigenDir);
            Assert.Equal(3, guardado.Dias.Count);
            Assert.Equal(2, guardado.Paradas.Count);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_ViajeAjeno_DevuelveNull()
        {
            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);

            var viaje = new Viaje
            {
                UsuarioId = 1,
                OrigenDir = "La Plata",
                DestinoDir = "Mar del Plata",
                HoraSalida = new TimeOnly(8, 0),
                HoraLlegada = new TimeOnly(14, 0),
                Activo = true
            };

            context.Viajes.Add(viaje);
            await context.SaveChangesAsync();

            // Intenta consultarlo otro usuario (ID 2)
            var resultado = await servicio.ObtenerPorIdAsync(viaje.Id, 2);

            Assert.Null(resultado);
        }
    }
}