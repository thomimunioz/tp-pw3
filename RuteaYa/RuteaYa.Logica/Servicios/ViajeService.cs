using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;
using RuteaYa.Logica.Mapas;

namespace RuteaYa.Logica.Viajes;

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
}



