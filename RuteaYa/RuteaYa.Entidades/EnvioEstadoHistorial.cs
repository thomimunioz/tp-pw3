using System;
using System.Collections.Generic;
using System.Text;
using RuteaYa.Entidades.Enums;


namespace RuteaYa.Entidades
{
    public class EnvioEstadoHistorial
    {
        public int Id { get; set; }

        public int EnvioId { get; set; }
        public Envio Envio { get; set; } = null!;

        public EstadoEnvio? EstadoAnterior { get; set; }
        public EstadoEnvio EstadoNuevo { get; set; }
        public DateTime Fecha { get; set; }
    }
}
