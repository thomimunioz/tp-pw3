using RuteaYa.Logica.Envios.Dtos;

namespace RuteaYa.Web.Models.Envios
{
    public class MisEnviosViewModel
    {
        public EstadoEnvio? EstadoSeleccionado { get; set; }
        public List<EnvioListadoDto> Envios { get; set; } = new();
    }
}