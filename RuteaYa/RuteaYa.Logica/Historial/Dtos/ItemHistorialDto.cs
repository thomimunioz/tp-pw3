using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuteaYa.Logica.Historial.Dtos
    {

    public class ItemHistorialDto
        {
            public int AsignacionId { get; set; }
            public int EnvioId { get; set; }
            public int ViajeId { get; set; }
            public string Descripcion { get; set; } = "";
            public string Recorrido { get; set; } = "";
            public string ViajeRecorrido { get; set; } = "";
            public string Detalle { get; set; } = "";
            public DateOnly Fecha { get; set; }
            public DateTime? FechaCierre { get; set; }
            public decimal MontoGanancia { get; set; }
            public EstadoEnvio Estado { get; set; }
        }
    }