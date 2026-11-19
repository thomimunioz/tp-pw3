using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.Logica.Dashboard
{
    public class DashboardDto
    {
        public string Nombre { get; set; } = "";
        public int ProximosViajes { get; set; }
        public int EnviosPendientes { get; set; }
        // Estado que comparten todos los envíos pendientes (null si no hay o si están en estados distintos)
        public EstadoEnvio? EstadoEnviosPendientes { get; set; }
        // Paquetes que el usuario tiene que llevar como conductor
        public int ParaEntregar { get; set; }
        public int Oportunidades { get; set; }
        public decimal PotencialMes { get; set; }
        public List<ViajeResumenDto> Viajes { get; set; } = new();
        public List<EnvioResumenDto> Envios { get; set; } = new();
    }

    public record ViajeResumenDto(string Origen, string Destino, string Dias, string Horario);
    public record EnvioResumenDto(string Origen, string Destino, string Descripcion, decimal Monto, EstadoEnvio Estado);
}