using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    [Table("Reservacion")]
    [Index("IdCliente", Name = "IX_Reservacion_Cliente")]
    [Index("FechaReserva", Name = "IX_Reservacion_Fecha")]
    [Index("IdMesa", Name = "IX_Reservacion_Mesa")]
    [Index("IdMesa", "FechaReserva", "HoraInicio", Name = "IX_Reservacion_Mesa_Fecha")]
    public partial class Reservacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdReservacion { get; set; }

        public int IdCliente { get; set; }

        public int IdMesa { get; set; }

        public int IdEstadoReservacion { get; set; }

        public DateOnly FechaReserva { get; set; }

        public int CantidadPersonas { get; set; }

        [StringLength(300)]
        public string? Observacion { get; set; }

        public DateTime FechaRegistro { get; set; }

        [Precision(0)]
        public TimeOnly HoraInicio { get; set; }

        [Precision(0)]
        public TimeOnly HoraFin { get; set; }


        // =========================================================
        // RELACIONES
        // =========================================================

        [ForeignKey(nameof(IdCliente))]
        [InverseProperty(nameof(Cliente.Reservacions))]
        public virtual Cliente? IdClienteNavigation { get; set; }


        [ForeignKey(nameof(IdEstadoReservacion))]
        [InverseProperty(nameof(EstadoReservacion.Reservacions))]
        public virtual EstadoReservacion? IdEstadoReservacionNavigation { get; set; }


        [ForeignKey(nameof(IdMesa))]
        [InverseProperty(nameof(Mesa.Reservacions))]
        public virtual Mesa? IdMesaNavigation { get; set; }
    }
}