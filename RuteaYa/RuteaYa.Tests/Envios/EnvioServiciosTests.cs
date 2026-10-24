using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades.Data;
using RuteaYa.Entidades.Enums;
using RuteaYa.Logica.Envios;
using RuteaYa.Logica.Envios.Dtos;
using RuteaYa.Logica.Mapas;

namespace RuteaYa.Tests.Envios
    {

    public class MockGeocodificador : IGeocodificador
        {

        private readonly bool _simularFalloOrigen;
        private readonly bool _simularFalloDestino;

        public MockGeocodificador(bool simularFalloOrigen = false, bool simularFalloDestino = false)
            {
            _simularFalloOrigen = simularFalloOrigen;
            _simularFalloDestino = simularFalloDestino;
            }

        public Task<Coordenada?> GeocodificarAsync(string direccion)
            {

            if (_simularFalloOrigen && direccion.Contains("OrigenInvalido"))
                return Task.FromResult<Coordenada?>(null);

            if (_simularFalloDestino && direccion.Contains("DestinoInvalido"))
                return Task.FromResult<Coordenada?>(null);

            return Task.FromResult<Coordenada?>(new Coordenada
                {
                Lat = -34.6037m,
                Lng = -58.3816m
                });
            }
        }

    public class EnvioServiciosTests
        {

        private RuteaYaContext CrearContextoEnMemoria()
            {

            var options = new DbContextOptionsBuilder<RuteaYaContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new RuteaYaContext(options);
            context.Database.EnsureCreated();
            return context;
            }

        private CrearEnvioDto CrearDtoValido()
            {

            return new CrearEnvioDto
                {
                OrigenDirrecion = "Av. Corrientes 1234, CABA",
                DestinoDireccion = "Av. Santa Fe 4321, CABA",
                FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                Peso = 2.5m,
                Tamano = TamanoPaquete.Mediano,
                Descripcion = "Zapatillas y ropa deportiva",
                Monto = 3500
                };
            }

        [Fact]
        public async Task CrearAsync_ConDatosValidos_CreaEnvioConEstadoBuscandoConductor()
            {

            using var context = CrearContextoEnMemoria();
            var servicio = new EnvioServicios(context, new MockGeocodificador());

            var dto = CrearDtoValido();
            var resultado = await servicio.CrearAsync(usuarioId: 1, dto);

            Assert.True(resultado.ResultadoExitoso);
            Assert.True(resultado.EnvioId > 0);

            var envioEnBd = await context.Envios.Include(e => e.Historial).FirstOrDefaultAsync(e => e.Id == resultado.EnvioId);
            Assert.NotNull(envioEnBd);
            Assert.Equal(EstadoEnvio.BuscandoConductor, envioEnBd.Estado);
            Assert.Equal(1, envioEnBd.UsuarioId);
            Assert.Single(envioEnBd.Historial);
            Assert.Equal(EstadoEnvio.BuscandoConductor, envioEnBd.Historial.First().EstadoNuevo);
            }

        [Fact]
        public async Task CrearAsync_ConDatosValidos_GuardaCoordenadasCorrectamente()
            {

            using var context = CrearContextoEnMemoria();
            var servicio = new EnvioServicios(context, new MockGeocodificador());

            var dto = CrearDtoValido();
            var resultado = await servicio.CrearAsync(usuarioId: 1, dto);
            var envioEnBd = await context.Envios.FindAsync(resultado.EnvioId);

            Assert.NotNull(envioEnBd);
            Assert.Equal(-34.6037m, envioEnBd.OrigenLat);
            Assert.Equal(-58.3816m, envioEnBd.OrigenLng);
            }

        [Fact]
        public async Task CrearAsync_ConOrigenIgualADestino_RetornaError()
            {

            using var context = CrearContextoEnMemoria();
            var servicio = new EnvioServicios(context, new MockGeocodificador());

            var dto = CrearDtoValido();
            dto.OrigenDirrecion = "Av. Mayo 500";
            dto.DestinoDireccion = "Av. Mayo 500";

            var resultado = await servicio.CrearAsync(1, dto);
            Assert.False(resultado.ResultadoExitoso);
            Assert.Contains(resultado.Errores, e => e.Contains("diferentes"));
            }

        [Fact]
        public async Task CrearAsync_ConCamposObligatoriosVacios_RetornaErrores()
            {

            using var context = CrearContextoEnMemoria();
            var servicio = new EnvioServicios(context, new MockGeocodificador());

            var dto = new CrearEnvioDto
                {
                OrigenDirrecion = "",
                DestinoDireccion = "   ",
                FechaLimite = DateOnly.FromDateTime(DateTime.Today),
                Peso = 1m,
                Tamano = TamanoPaquete.Pequeno,
                Descripcion = "",
                Monto = 1000
                };

            var resultado = await servicio.CrearAsync(1, dto);

            Assert.False(resultado.ResultadoExitoso);
            Assert.Contains(resultado.Errores, e => e.Contains("origen es obligatorio"));
            Assert.Contains(resultado.Errores, e => e.Contains("destino es obligatorio"));
            Assert.Contains(resultado.Errores, e => e.Contains("descripción es obligatoria"));
            }

        [Fact]
        public async Task CrearAsync_ConFechaAnteriorAHoy_RetornaError()
            {

            using var context = CrearContextoEnMemoria();
            var servicio = new EnvioServicios(context, new MockGeocodificador());

            var dto = CrearDtoValido();
            dto.FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var resultado = await servicio.CrearAsync(1, dto);

            Assert.False(resultado.ResultadoExitoso);
            Assert.Contains(resultado.Errores, e => e.Contains("anterior a la fecha actual"));
            }

        [Fact]
        public async Task CrearAsync_ConPesoOMontoInvalido_RetornaError()
            {

            using var context = CrearContextoEnMemoria();
            var servicio = new EnvioServicios(context, new MockGeocodificador());

            var dto = CrearDtoValido();
            dto.Peso = 0;
            dto.Monto = -500;

            var resultado = await servicio.CrearAsync(1, dto);

            Assert.False(resultado.ResultadoExitoso);
            Assert.Contains(resultado.Errores, e => e.Contains("peso"));
            Assert.Contains(resultado.Errores, e => e.Contains("monto"));
            }

        [Fact]
        public async Task CrearAsync_ConDescripcionSupera500Caracteres_RetornaError()
            {

            using var context = CrearContextoEnMemoria();
            var servicio = new EnvioServicios(context, new MockGeocodificador());

            var dto = CrearDtoValido();
            dto.Descripcion = new string('A', 501);
            var resultado = await servicio.CrearAsync(1, dto);

            Assert.False(resultado.ResultadoExitoso);
            Assert.Contains(resultado.Errores, e => e.Contains("500 caracteres"));
            }

        [Fact]
        public async Task CrearAsync_CuandoGeocodificadorNoEncuentraDireccion_RetornaError()
            {

            using var context = CrearContextoEnMemoria();
            var servicio = new EnvioServicios(context, new MockGeocodificador(simularFalloOrigen: true));

            var dto = CrearDtoValido();
            dto.OrigenDirrecion = "Calle OrigenInvalido 999999";
            var resultado = await servicio.CrearAsync(1, dto);

            Assert.False(resultado.ResultadoExitoso);
            Assert.Contains(resultado.Errores, e => e.Contains("No se pudo ubicar la dirección de origen"));
            }
        }
    }