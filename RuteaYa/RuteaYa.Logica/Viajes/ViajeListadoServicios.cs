using Microsoft.EntityFrameworkCore;
using RuteaYa.Entidades.Data;

namespace RuteaYa.Logica.Viajes;

public class ViajeListadoDto
{
    public int Id { get; set; }
    public string Origen { get; set; } = null!;
    public string Destino { get; set; } = null!;
    public TimeOnly HoraSalida { get; set; }
    public TimeOnly HoraLlegada { get; set; }
    public int MaxPaquetes { get; set; }
}

public interface IViajeListadoServicios
{
    Task<List<ViajeListadoDto>> ObtenerActivosAsync();
}

public class ViajeListadoServicios : IViajeListadoServicios
{
    private readonly RuteaYaContext _context;

    public ViajeListadoServicios(RuteaYaContext context) => _context = context;

    public async Task<List<ViajeListadoDto>> ObtenerActivosAsync()
    {
        return await _context.Viajes
            .AsNoTracking()
            .Where(v => v.Activo)
            .OrderByDescending(v => v.Id)
            .Select(v => new ViajeListadoDto
            {
                Id = v.Id,
                Origen = v.OrigenDir,
                Destino = v.DestinoDir,
                HoraSalida = v.HoraSalida,
                HoraLlegada = v.HoraLlegada,
                MaxPaquetes = v.MaxPaquetes
            })
            .ToListAsync();
    }
}