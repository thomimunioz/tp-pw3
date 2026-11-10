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
        public int Oportunidades { get; set; }
        public decimal PotencialMes { get; set; }   // provisorio (Falta parte Potencial)
        public List<ViajeResumenDto> Viajes { get; set; } = new();
        public List<EnvioResumenDto> Envios { get; set; } = new();
    }

    public record ViajeResumenDto(string Origen, string Destino, string Dias, string Horario);
    public record EnvioResumenDto(string Origen, string Destino, string Descripcion, decimal Monto, EstadoEnvio Estado);
}