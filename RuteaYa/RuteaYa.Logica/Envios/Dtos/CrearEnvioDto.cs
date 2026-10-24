using RuteaYa.Entidades.Enums;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.Logica.Envios.Dtos {

    public class CrearEnvioDto
    {

        public string OrigenDirrecion { get; set; } = "";
        public string DestinoDireccion { get; set; } = "";
        public DateOnly FechaLimite { get; set; }
        public decimal Peso { get; set; }
        public decimal Monto { get; set; }
        public TamanoPaquete Tamano { get; set; }
        public string Descripcion { get; set; } = "";
    }
}