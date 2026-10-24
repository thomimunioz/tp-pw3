using System;
using System.Collections.Generic;
using System.Text;

namespace RuteaYa.Logica.Envios.Dtos {

    public class ResultadoCrearEnvio {

        public bool ResultadoExitoso { get; set; } = false;
        public int EnvioId { get; set; }
        public List<string> Errores { get; set; } = new List<string>();

        public static ResultadoCrearEnvio Exitoso(int idIngresado)
            {
            
            return new ResultadoCrearEnvio
            {
                ResultadoExitoso = true,
                EnvioId = idIngresado
            };
        }

        public static ResultadoCrearEnvio Fallido(string[] errores)
        {
            
            return new ResultadoCrearEnvio
            {
                ResultadoExitoso = false,
                Errores = errores.ToList()
            };
        }
    }
}