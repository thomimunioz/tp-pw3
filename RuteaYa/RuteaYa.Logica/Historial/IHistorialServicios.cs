using RuteaYa.Logica.Historial.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuteaYa.Logica.Historial
    {

    public interface IHistorialServicios
        {

            Task<HistorialResumenDto> ObtenerHistorialAsync(int usuarioId, FiltroHistorialDto? filtro = null);
        }
    }
