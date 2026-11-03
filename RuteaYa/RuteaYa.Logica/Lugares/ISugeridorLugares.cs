using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.Logica.Lugares
{
        /// <summary>Un lugar que se le sugiere al usuario mientras escribe un origen, destino o parada.</summary>
        public record LugarSugeridoDto(string Nombre);

        /// <summary>Reglas de la búsqueda de lugares (el JS usa el mismo mínimo para no consultar de más).</summary>
        public static class LugaresReglas
        {
            public const int MinCaracteres = 3;
            public const int MaxResultados = 6;
        }

        /// <summary>
        /// Busca lugares a partir de un texto. La aplicación depende de esta interfaz y no del
        /// proveedor (Photon, otro servicio o la versión falsa para desarrollo), igual que con IGeocodificador.
        /// </summary>
        public interface ISugeridorLugares
        {
            Task<IReadOnlyList<LugarSugeridoDto>> BuscarAsync(string texto, CancellationToken cancellationToken = default);
        }
    }
