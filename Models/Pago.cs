using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Pago")]
[Index("IdFactura", Name = "IX_Pago_Factura")]
[Index("FechaPago", Name = "IX_Pago_Fecha")]
[Index("IdMetodoPago", Name = "IX_Pago_Metodo")]
public partial class Pago
{
    [Key]
    public int IdPago { get; set; }

    public int IdFactura { get; set; }

    public int IdMetodoPago { get; set; }

    public int IdMoneda { get; set; }

    public int? IdBanco { get; set; }

    public int? IdTipoTarjeta { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Monto { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Referencia { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? NumeroAutorizacion { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    public DateTime FechaPago { get; set; }

    public int IdEstadoPago { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MontoRecibido { get; set; }

    // =========================
    // RELACIONES
    // =========================

    [ForeignKey(nameof(IdBanco))]
    [InverseProperty(nameof(Banco.Pagos))]
    public virtual Banco? IdBancoNavigation { get; set; }

    [ForeignKey(nameof(IdEstadoPago))]
    [InverseProperty(nameof(EstadoPago.Pagos))]
    public virtual EstadoPago IdEstadoPagoNavigation { get; set; } = null!;

    [ForeignKey(nameof(IdFactura))]
    [InverseProperty(nameof(Factura.Pagos))]
    public virtual Factura IdFacturaNavigation { get; set; } = null!;

    [ForeignKey(nameof(IdMetodoPago))]
    [InverseProperty(nameof(MetodoPago.Pagos))]
    public virtual MetodoPago IdMetodoPagoNavigation { get; set; } = null!;

    [ForeignKey(nameof(IdMoneda))]
    [InverseProperty(nameof(Monedum.Pagos))]
    public virtual Monedum IdMonedaNavigation { get; set; } = null!;

    [ForeignKey(nameof(IdTipoTarjeta))]
    [InverseProperty(nameof(TipoTarjetum.Pagos))]
    public virtual TipoTarjetum? IdTipoTarjetaNavigation { get; set; }

    [InverseProperty(nameof(MovimientoCaja.IdPagoNavigation))]
    public virtual ICollection<MovimientoCaja> MovimientoCajas { get; set; }
        = new List<MovimientoCaja>();
}