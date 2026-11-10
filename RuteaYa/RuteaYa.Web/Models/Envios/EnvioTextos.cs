using RuteaYa.Entidades.Enums;

namespace RuteaYa.Web.Models.Envios
{
    public static class EnvioTextos
    {
        public static string Texto(this EstadoEnvio estado) => estado switch
        {
            EstadoEnvio.BuscandoConductor => "Buscando conductor",
            EstadoEnvio.ConductorAsignado => "Conductor asignado",
            EstadoEnvio.Retirado => "Retirado",
            EstadoEnvio.EnCamino => "En camino",
            EstadoEnvio.Entregado => "Entregado",
            EstadoEnvio.Cancelado => "Cancelado",
            _ => estado.ToString()
        };

        public static string Texto(this TamanoPaquete tamano) => tamano switch
        {
            TamanoPaquete.Pequeno => "Pequeño",
            TamanoPaquete.Mediano => "Mediano",
            TamanoPaquete.Grande => "Grande",
            _ => tamano.ToString()
        };
    }
}