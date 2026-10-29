using RuteaYa.Entidades.Enums;

namespace RuteaYa.Logica.Transportes.Dtos
    {

    public class TransporteListadoDto
        {

        public int AsignacionId { get; set; }
        public int EnvioId { get; set; }
        public int ViajeId { get; set; }

        // Datos del viaje del conductor.
        public string ViajeOrigen { get; set; } = "";
        public string ViajeDestino { get; set; } = "";
        public DateOnly FechaViaje { get; set; }

        // Datos del paquete.
        public string PaqueteDescripcion { get; set; } = "";
        public string PaqueteOrigen { get; set; } = "";
        public string PaqueteDestino { get; set; } = "";
        public decimal PesoKg { get; set; }
        public TamanoPaquete Tamano { get; set; }

        // Ganancia y estado.
        public decimal Ganancia { get; set; }
        public EstadoEnvio Estado { get; set; }

        // Propiedades/atributos que ayuda a la interfaz.
        public bool PuedeMarcarRetirado()
            {
            return this.Estado == EstadoEnvio.ConductorAsignado;
            }

        public bool PuedeMarcarEnCamino()
            {
            return this.Estado == EstadoEnvio.Retirado;
            }

        public bool PuedeMarcarEntregado()
            {
            return this.Estado == EstadoEnvio.EnCamino;
            }

        public bool Finalizado()
            {
            return this.Estado == EstadoEnvio.Entregado || this.Estado == EstadoEnvio.Cancelado;
            }
        }
    }