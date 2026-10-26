using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades;
using RuteaYa.Entidades.Data;

namespace RuteaYa.Logica.Viajes;

public class ViajeServicios : IViajeServicios
{
    private readonly RuteaYaContext _context;

    public ViajeServicios(RuteaYaContext context)
    {
        _context = context;
    }

    public async Task<List<Viaje>> ObtenerPorUsuarioAsync(int usuarioId)
    {
        return await _context.Viajes
            .AsNoTracking()
            .Where(v => v.UsuarioId == usuarioId)
            .OrderByDescending(v => v.Id)
            .ToListAsync();
    }
}