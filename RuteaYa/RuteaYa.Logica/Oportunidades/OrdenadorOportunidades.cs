using RuteaYa.Logica.Oportunidades.Dtos;

namespace RuteaYa.Logica.Oportunidades;

public static class OrdenadorOportunidades
{
    public static List<OportunidadDto> Ordenar(List<OportunidadDto> lista, OrdenOportunidades orden)
    {
        return orden switch
        {
            OrdenOportunidades.Desvio =>
                lista.OrderBy(o => o.DesvioKm).ThenByDescending(o => o.Pago).ToList(),
            OrdenOportunidades.Compatibilidad =>
                lista.OrderBy(o => o.Compatibilidad).ThenByDescending(o => o.Pago).ToList(),
            _ =>
                lista.OrderByDescending(o => o.Pago).ThenBy(o => o.DesvioKm).ToList()
        };
    }
}