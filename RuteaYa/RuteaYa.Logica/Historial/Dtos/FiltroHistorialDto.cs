using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuteaYa.Logica.Historial.Dtos
    {

    public class FiltroHistorialDto
        {
            public int? Mes { get; set; }
            public int? Anio { get; set; }
            public EstadoEnvio? Estado { get; set; }
        }
    }
