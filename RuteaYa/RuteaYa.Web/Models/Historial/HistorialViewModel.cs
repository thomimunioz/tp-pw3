using RuteaYa.Logica.Historial.Dtos;
using RuteaYa.Entidades.Enums;

namespace RuteaYa.Web.Models.Historial
    {

    public class HistorialViewModel
        {

        public HistorialResumenDto Resumen { get; set; } = new();
        public int? MesSeleccionado { get; set; }
        public int? AnioSeleccionado { get; set; }
        public EstadoEnvio? EstadoSeleccionado { get; set; }
        }
    }