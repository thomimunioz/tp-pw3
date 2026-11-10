using RuteaYa.Entidades.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RuteaYa.Web.Models.Envios {

    public class CrearEnvioViewModel
    {

        [Required(ErrorMessage = "El origen es obligatorio.")]
        [StringLength(250, ErrorMessage = "El origen no puede superar los 250 caracteres.")]
        [Display(Name = "Origen")]
        public string OrigenDirrecion { get; set; } = "";

        [Required(ErrorMessage = "El destino es obligatorio.")]
        [StringLength(250, ErrorMessage = "El destino no puede superar los 250 caracteres.")]
        [Display(Name = "Destinoooo")]
        public string DestinoDireccion { get; set; } = "";

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha")]
        public DateOnly FechaLimite { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Required(ErrorMessage = "El peso es obligatorio.")]
        [Range(0.01, 9999.99, ErrorMessage = "El peso debe ser mayor a 0 kg.")]
        [Display(Name = "Peso (kg)")]
        public decimal Peso { get; set; }

        [Required(ErrorMessage = "El tamaño es obligatorio.")]
        [Display(Name = "Tamaño")]
        public TamanoPaquete tamano { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = "";

        [Required(ErrorMessage = "El monto ofrecido es obligatorio.")]
        [Range(1, 99999999, ErrorMessage = "El monto ofrecido debe ser mayor a $0.")]
        public decimal Monto { get; set; }
    }
}