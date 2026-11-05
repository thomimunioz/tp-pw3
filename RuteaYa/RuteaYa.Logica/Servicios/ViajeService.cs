using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Mapas;
using RuteaYa.Logica.Servicios.Dtos;

namespace RuteaYa.Logica.Servicios;

public class ViajeServicios : IViajeServicios
{
    private readonly RuteaYaContext _context;
    private readonly IGeocodificador _geocodificador;

    public ViajeServicios(RuteaYaContext context, IGeocodificador geocodificador)
    {
        _context = context;
        _geocodificador = geocodificador;
    }

    public async Task<List<Viaje>> ObtenerPorUsuarioAsync(int usuarioId)
    {
        return await _context.Viajes
            .AsNoTracking()
            .Where(v => v.UsuarioId == usuarioId)
            .OrderByDescending(v => v.Id)
            .ToListAsync();
    }

    public async Task<Viaje?> ObtenerDetallePorUsuarioAsync(int viajeId, int usuarioId)
    {
        return await _context.Viajes
            .AsNoTracking()
            .Where(v => v.Id == viajeId && v.UsuarioId == usuarioId)
            .Include(v => v.Dias)
            .Include(v => v.Paradas)
            .Include(v => v.Asignaciones)
                .ThenInclude(a => a.Envio)
            .FirstOrDefaultAsync();
    }

    public async Task<ViajeRecorridoDto?> ObtenerRecorridoPorUsuarioAsync(int viajeId, int usuarioId)
    {
        var viaje = await _context.Viajes
            .AsNoTracking()
            .Where(v => v.Id == viajeId && v.UsuarioId == usuarioId)
            .Select(v => new ViajeRecorridoDto
            {
                Id = v.Id,
                Origen = new PuntoRecorridoDto
                {
                    Direccion = v.OrigenDir,
                    Latitud = v.OrigenLat,
                    Longitud = v.OrigenLng
                },
                Destino = new PuntoRecorridoDto
                {
                    Direccion = v.DestinoDir,
                    Latitud = v.DestinoLat,
                    Longitud = v.DestinoLng
                },
                Paradas = v.Paradas
                    .OrderBy(p => p.Orden)
                    .Select(p => new PuntoRecorridoDto
                    {
                        Direccion = p.Direccion,
                        Latitud = p.Lat,
                        Longitud = p.Lng,
                        Orden = p.Orden
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return viaje;
    }

    public async Task CrearViajeAsync(Viaje viaje, List<int> diasSeleccionados, string? paradasIntermedias)
    {
        ArgumentNullException.ThrowIfNull(viaje);

        viaje.Activo = true;

        var diasUnicos = diasSeleccionados
            .Where(d => Enum.IsDefined(typeof(DayOfWeek), d))
            .Distinct()
            .ToList();

        foreach (var dia in diasUnicos)
        {
            viaje.Dias.Add(new ViajeDia
            {
                Viaje = viaje,
                DiaSemana = (DayOfWeek)dia
            });
        }

        var origen = await _geocodificador.GeocodificarAsync(viaje.OrigenDir);
        var destino = await _geocodificador.GeocodificarAsync(viaje.DestinoDir);
        if (origen == null || destino == null)
        {
            throw new InvalidOperationException("No se pudo ubicar el origen o el destino.");
        }

        viaje.OrigenLat = origen.Lat;
        viaje.OrigenLng = origen.Lng;
        viaje.DestinoLat = destino.Lat;
        viaje.DestinoLng = destino.Lng;

        if (!string.IsNullOrWhiteSpace(paradasIntermedias))
        {
            var origenNormalizado = viaje.OrigenDir?.Trim();
            var destinoNormalizado = viaje.DestinoDir?.Trim();

            var paradas = paradasIntermedias
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(p =>
                    !string.Equals(p, origenNormalizado, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(p, destinoNormalizado, StringComparison.OrdinalIgnoreCase))
                .Select((direccion, indice) => new Parada
                {
                    Viaje = viaje,
                    Orden = indice + 1,
                    Direccion = direccion,
                    Lat = 0m,
                    Lng = 0m
                })
                .ToList();

            foreach (var parada in paradas)
            {
                var coord = await _geocodificador.GeocodificarAsync(parada.Direccion);
                if (coord == null)
                {
                    throw new InvalidOperationException($"No se pudo ubicar la parada {parada.Direccion}.");
                }

                parada.Lat = coord.Lat;
                parada.Lng = coord.Lng;
                viaje.Paradas.Add(parada);
            }
        }

        _context.Viajes.Add(viaje);
        await _context.SaveChangesAsync();
    }

    // --- ACÁ EMPIEZAN LOS MÉTODOS NUEVOS ---

    public async Task<Viaje?> ObtenerPorIdAsync(int id, int usuarioId)
    {
        return await _context.Viajes
            .Include(v => v.Dias)
            .Include(v => v.Paradas)
            .FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == usuarioId);
    }

    public async Task EditarViajeAsync(Viaje viajeActualizado, List<int> diasSeleccionados, string? paradasIntermedias)
    {
        // 1. Buscamos el viaje original
        var viajeDb = await _context.Viajes
            .Include(v => v.Dias)
            .Include(v => v.Paradas)
            .FirstOrDefaultAsync(v => v.Id == viajeActualizado.Id);

        if (viajeDb == null) throw new Exception("Viaje no encontrado");

        // 2. Si el usuario modificó el Origen, buscamos las nuevas coordenadas
        if (viajeDb.OrigenDir != viajeActualizado.OrigenDir)
        {
            var origen = await _geocodificador.GeocodificarAsync(viajeActualizado.OrigenDir);
            if (origen == null) throw new InvalidOperationException("No se pudo ubicar el nuevo origen.");
            viajeDb.OrigenLat = origen.Lat;
            viajeDb.OrigenLng = origen.Lng;
            viajeDb.OrigenDir = viajeActualizado.OrigenDir;
        }

        // 3. Si modificó el Destino, buscamos las nuevas coordenadas
        if (viajeDb.DestinoDir != viajeActualizado.DestinoDir)
        {
            var destino = await _geocodificador.GeocodificarAsync(viajeActualizado.DestinoDir);
            if (destino == null) throw new InvalidOperationException("No se pudo ubicar el nuevo destino.");
            viajeDb.DestinoLat = destino.Lat;
            viajeDb.DestinoLng = destino.Lng;
            viajeDb.DestinoDir = viajeActualizado.DestinoDir;
        }

        // 4. Actualizamos los datos numéricos y de horarios
        viajeDb.HoraSalida = viajeActualizado.HoraSalida;
        viajeDb.HoraLlegada = viajeActualizado.HoraLlegada;
        viajeDb.MaxPaquetes = viajeActualizado.MaxPaquetes;
        viajeDb.PesoMaxKg = viajeActualizado.PesoMaxKg;
        viajeDb.DesvioMaxKm = viajeActualizado.DesvioMaxKm;

        // 5. Reemplazamos los días
        _context.ViajeDias.RemoveRange(viajeDb.Dias);
        var diasUnicos = diasSeleccionados.Where(d => Enum.IsDefined(typeof(DayOfWeek), d)).Distinct().ToList();
        foreach (var dia in diasUnicos)
        {
            viajeDb.Dias.Add(new ViajeDia
            {
                Viaje = viajeDb,
                DiaSemana = (DayOfWeek)dia
            });
        }

        // 6. Reemplazamos las paradas calculando coordenadas nuevas
        _context.Paradas.RemoveRange(viajeDb.Paradas);
        if (!string.IsNullOrWhiteSpace(paradasIntermedias))
        {
            var origenNorm = viajeDb.OrigenDir?.Trim();
            var destinoNorm = viajeDb.DestinoDir?.Trim();

            var nuevasParadas = paradasIntermedias
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(p => !string.Equals(p, origenNorm, StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(p, destinoNorm, StringComparison.OrdinalIgnoreCase))
                .Select((direccion, indice) => new Parada
                {
                    Viaje = viajeDb,
                    Orden = indice + 1,
                    Direccion = direccion
                }).ToList();

            foreach (var parada in nuevasParadas)
            {
                var coord = await _geocodificador.GeocodificarAsync(parada.Direccion);
                if (coord == null) throw new InvalidOperationException($"No se pudo ubicar la parada {parada.Direccion}.");

                parada.Lat = coord.Lat;
                parada.Lng = coord.Lng;
                viajeDb.Paradas.Add(parada);
            }
        }

        // 7. Guardamos en la base de datos
        await _context.SaveChangesAsync();
    }
}