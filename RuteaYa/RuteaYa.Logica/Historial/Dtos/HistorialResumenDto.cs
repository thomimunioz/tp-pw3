using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuteaYa.Logica.Historial.Dtos
    {

    public class HistorialResumenDto
        {
            public int ViajesRealizados { get; set; }
            public int EnviosTransportados { get; set; }
            public decimal GananciaEfectiva { get; set; }
            public List<ItemHistorialDto> Operaciones { get; set; } = new();
        }
    }