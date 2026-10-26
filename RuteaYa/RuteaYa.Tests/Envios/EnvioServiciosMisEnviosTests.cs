using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Envios;
using RuteaYa.Logica.Envios.Dtos;

namespace RuteaYa.Tests.Envios
{

    public class EnvioServiciosMisEnviosTests
    {

        private RuteaYaContext CrearContextoEnMemoria()
        {

            var options = new DbContextOptionsBuilder<RuteaYaContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new RuteaYaContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private EnvioServicios CrearServicio(RuteaYaContext context)
        {

            return new EnvioServicios(context, new MockGeocodificador());
        }

        private async Task<Envio> CrearEnvioEnBdAsync(RuteaYaContext context, int usuarioId, EstadoEnvio estado = EstadoEnvio.BuscandoConductor, DateTime? creadoEn = null)
        {

            var envio = new Envio
            {
                UsuarioId = usuarioId,
                OrigenDir = "Av. Corrientes 1234, CABA",
                OrigenLat = -34.6037m,
                OrigenLng = -58.3816m,
                DestinoDir = "Av. Santa Fe 4321, CABA",
                DestinoLat = -34.5900m,
                DestinoLng = -58.4100m,
                FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                PesoKg = 2.5m,
                Tamano = TamanoPaquete.Mediano,
                Descripcion = "Zapatillas y ropa deportiva",
                Monto = 3500,
                Estado = estado,
                CreadoEn = creadoEn ?? DateTime.Now,
                RowVersion = new byte[8]
            };

            context.Envios.Add(envio);
            await context.SaveChangesAsync();
            return envio;
        }


        [Fact]
        public async Task CrearAsync_ConMontoMenorAUno_RetornaError()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);

            var dto = new CrearEnvioDto
            {
                OrigenDirrecion = "Av. Corrientes 1234, CABA",
                DestinoDireccion = "Av. Santa Fe 4321, CABA",
                FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                Peso = 2.5m,
                Tamano = TamanoPaquete.Mediano,
                Descripcion = "Zapatillas",
                Monto = 0.5m
            };

            var resultado = await servicio.CrearAsync(1, dto);

            Assert.False(resultado.ResultadoExitoso);
            Assert.Contains(resultado.Errores, e => e.Contains("monto"));
        }


        [Fact]
        public async Task ListarAsync_SinFiltro_DevuelveSoloLosEnviosDelUsuario()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);
            await CrearEnvioEnBdAsync(context, usuarioId: 1);
            await CrearEnvioEnBdAsync(context, usuarioId: 1);
            await CrearEnvioEnBdAsync(context, usuarioId: 2);

            var resultado = await servicio.ListarAsync(usuarioId: 1, estado: null);

            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public async Task ListarAsync_ConFiltroDeEstado_DevuelveSoloEnviosDeEseEstado()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);
            await CrearEnvioEnBdAsync(context, 1, EstadoEnvio.BuscandoConductor);
            await CrearEnvioEnBdAsync(context, 1, EstadoEnvio.Entregado);
            await CrearEnvioEnBdAsync(context, 1, EstadoEnvio.Cancelado);

            var resultado = await servicio.ListarAsync(1, EstadoEnvio.Entregado);

            Assert.Single(resultado);
            Assert.Equal(EstadoEnvio.Entregado, resultado[0].Estado);
        }

        [Fact]
        public async Task ListarAsync_DevuelveElMasNuevoPrimero()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);
            var viejo = await CrearEnvioEnBdAsync(context, 1, creadoEn: DateTime.Now.AddDays(-5));
            var nuevo = await CrearEnvioEnBdAsync(context, 1, creadoEn: DateTime.Now);

            var resultado = await servicio.ListarAsync(1, null);

            Assert.Equal(nuevo.Id, resultado[0].Id);
            Assert.Equal(viejo.Id, resultado[1].Id);
        }

        [Fact]
        public async Task ListarAsync_UsuarioSinEnvios_DevuelveListaVacia()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);
            await CrearEnvioEnBdAsync(context, usuarioId: 2);

            var resultado = await servicio.ListarAsync(usuarioId: 1, estado: null);

            Assert.Empty(resultado);
        }

        [Theory]
        [InlineData(EstadoEnvio.BuscandoConductor, true)]
        [InlineData(EstadoEnvio.ConductorAsignado, true)]
        [InlineData(EstadoEnvio.Retirado, false)]
        [InlineData(EstadoEnvio.EnCamino, false)]
        [InlineData(EstadoEnvio.Entregado, false)]
        [InlineData(EstadoEnvio.Cancelado, false)]
        public async Task ListarAsync_IndicaSiElEnvioSePuedeCancelarSegunSuEstado(EstadoEnvio estado, bool esperado)
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);
            await CrearEnvioEnBdAsync(context, 1, estado);

            var resultado = await servicio.ListarAsync(1, null);

            Assert.Equal(esperado, resultado[0].PuedeCancelar);
        }


        [Fact]
        public async Task ObtenerDetalleAsync_ConEnvioPropio_DevuelveElDetalle()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);
            var envio = await CrearEnvioEnBdAsync(context, 1);

            var detalle = await servicio.ObtenerDetalleAsync(1, envio.Id);

            Assert.NotNull(detalle);
            Assert.Equal(envio.Id, detalle.Id);
            Assert.Equal("Av. Corrientes 1234, CABA", detalle.OrigenDir);
            Assert.Equal(3500, detalle.Monto);
            Assert.Equal(EstadoEnvio.BuscandoConductor, detalle.Estado);
            Assert.True(detalle.PuedeCancelar);
        }

        [Fact]
        public async Task ObtenerDetalleAsync_ConEnvioDeOtroUsuario_DevuelveNull()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);
            var envioAjeno = await CrearEnvioEnBdAsync(context, usuarioId: 2);

            var detalle = await servicio.ObtenerDetalleAsync(usuarioId: 1, envioId: envioAjeno.Id);

            Assert.Null(detalle);
        }

        [Fact]
        public async Task ObtenerDetalleAsync_ConEnvioInexistente_DevuelveNull()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);

            var detalle = await servicio.ObtenerDetalleAsync(1, 9999);

            Assert.Null(detalle);
        }


        [Fact]
        public async Task CancelarAsync_DesdeBuscandoConductor_CancelaYRegistraHistorial()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);
            var envio = await CrearEnvioEnBdAsync(context, 1, EstadoEnvio.BuscandoConductor);

            var resultado = await servicio.CancelarAsync(1, envio.Id);

            Assert.True(resultado.ResultadoExitoso);

            var envioEnBd = await context.Envios.Include(e => e.Historial).FirstAsync(e => e.Id == envio.Id);
            Assert.Equal(EstadoEnvio.Cancelado, envioEnBd.Estado);
            Assert.Single(envioEnBd.Historial);
            Assert.Equal(EstadoEnvio.BuscandoConductor, envioEnBd.Historial.First().EstadoAnterior);
            Assert.Equal(EstadoEnvio.Cancelado, envioEnBd.Historial.First().EstadoNuevo);
        }

        [Fact]
        public async Task CancelarAsync_DesdeConductorAsignado_CancelaYEliminaLaAsignacion()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);
            var envio = await CrearEnvioEnBdAsync(context, 1, EstadoEnvio.ConductorAsignado);

            context.Add(new Asignacion
            {
                EnvioId = envio.Id,
                ViajeId = 1,
                FechaViaje = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                DesvioKm = 1.5m,
                TiempoExtraMin = 10,
                Ganancia = 3500,
                AsignadoEn = DateTime.Now
            });
            await context.SaveChangesAsync();
            await context.SaveChangesAsync();

            var resultado = await servicio.CancelarAsync(1, envio.Id);

            Assert.True(resultado.ResultadoExitoso);
            Assert.Equal(EstadoEnvio.Cancelado, (await context.Envios.FindAsync(envio.Id))!.Estado);
            Assert.Empty(context.Set<Asignacion>().Where(a => a.EnvioId == envio.Id));
        }

        [Theory]
        [InlineData(EstadoEnvio.Retirado)]
        [InlineData(EstadoEnvio.EnCamino)]
        [InlineData(EstadoEnvio.Entregado)]
        [InlineData(EstadoEnvio.Cancelado)]
        public async Task CancelarAsync_DesdeEstadoNoCancelable_RetornaErrorYNoCambiaNada(EstadoEnvio estado)
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);
            var envio = await CrearEnvioEnBdAsync(context, 1, estado);

            var resultado = await servicio.CancelarAsync(1, envio.Id);

            Assert.False(resultado.ResultadoExitoso);
            Assert.False(resultado.NoEncontrado);
            Assert.NotNull(resultado.Error);

            var envioEnBd = await context.Envios.Include(e => e.Historial).FirstAsync(e => e.Id == envio.Id);
            Assert.Equal(estado, envioEnBd.Estado);
            Assert.Empty(envioEnBd.Historial);
        }

        [Fact]
        public async Task CancelarAsync_ConEnvioDeOtroUsuario_RetornaNoEncontradoYNoCancela()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);
            var envioAjeno = await CrearEnvioEnBdAsync(context, usuarioId: 2);

            var resultado = await servicio.CancelarAsync(usuarioId: 1, envioId: envioAjeno.Id);

            Assert.False(resultado.ResultadoExitoso);
            Assert.True(resultado.NoEncontrado);
            Assert.Equal(EstadoEnvio.BuscandoConductor, (await context.Envios.FindAsync(envioAjeno.Id))!.Estado);
        }

        [Fact]
        public async Task CancelarAsync_ConEnvioInexistente_RetornaNoEncontrado()
        {

            using var context = CrearContextoEnMemoria();
            var servicio = CrearServicio(context);

            var resultado = await servicio.CancelarAsync(1, 9999);

            Assert.False(resultado.ResultadoExitoso);
            Assert.True(resultado.NoEncontrado);
        }
    }
}