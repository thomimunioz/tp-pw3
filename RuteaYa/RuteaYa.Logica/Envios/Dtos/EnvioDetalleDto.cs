using RuteaYa.Entidades.Enums;

namespace RuteaYa.Logica.Envios.Dtos
{
    public class EnvioDetalleDto
    {
        public int Id { get; set; }
        public string OrigenDir { get; set; } = null!;
        public string DestinoDir { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Monto { get; set; }
        public decimal PesoKg { get; set; }
        public TamanoPaquete Tamano { get; set; }
        public EstadoEnvio Estado { get; set; }
        public DateOnly FechaLimite { get; set; }
        public DateTime CreadoEn { get; set; }
        public bool PuedeCancelar { get; set; }
    }
}