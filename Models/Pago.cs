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
    // ============================================================
    // DATOS PRINCIPALES
    // ============================================================

    [Key]
    public int IdPago { get; set; }

    [Required]
    public int IdFactura { get; set; }

    [Required]
    public int IdMetodoPago { get; set; }

    [Required]
    public int IdMoneda { get; set; }

    public int? IdBanco { get; set; }

    public int? IdTipoTarjeta { get; set; }

    [Required]
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

    [Required]
    public DateTime FechaPago { get; set; }

    [Required]
    public int IdEstadoPago { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MontoRecibido { get; set; }

    // ============================================================
    // RELACIONES
    // ============================================================
    //
    // IMPORTANTE:
    // Estas propiedades son NULLABLE porque cuando se recibe el
    // formulario Create/Edit, ASP.NET Core solamente recibe los
    // valores IdFactura, IdMetodoPago, IdMoneda, etc.
    //
    // Entity Framework carga las Navigation Properties mediante
    // Include() después.
    //
    // Si se dejan como `= null!`, ASP.NET Core las puede considerar
    // campos obligatorios del formulario y aparecen errores como:
    //
    // "The IdFacturaNavigation field is required."
    //
    // ============================================================

    [ForeignKey(nameof(IdBanco))]
    [InverseProperty(nameof(Banco.Pagos))]
    public virtual Banco? IdBancoNavigation { get; set; }

    [ForeignKey(nameof(IdEstadoPago))]
    [InverseProperty(nameof(EstadoPago.Pagos))]
    public virtual EstadoPago? IdEstadoPagoNavigation { get; set; }

    [ForeignKey(nameof(IdFactura))]
    [InverseProperty(nameof(Factura.Pagos))]
    public virtual Factura? IdFacturaNavigation { get; set; }

    [ForeignKey(nameof(IdMetodoPago))]
    [InverseProperty(nameof(MetodoPago.Pagos))]
    public virtual MetodoPago? IdMetodoPagoNavigation { get; set; }

    [ForeignKey(nameof(IdMoneda))]
    [InverseProperty(nameof(Monedum.Pagos))]
    public virtual Monedum? IdMonedaNavigation { get; set; }

    [ForeignKey(nameof(IdTipoTarjeta))]
    [InverseProperty(nameof(TipoTarjetum.Pagos))]
    public virtual TipoTarjetum? IdTipoTarjetaNavigation { get; set; }

    // ============================================================
    // MOVIMIENTOS DE CAJA
    // ============================================================

    [InverseProperty(nameof(MovimientoCaja.IdPagoNavigation))]
    public virtual ICollection<MovimientoCaja> MovimientoCajas { get; set; }
        = new List<MovimientoCaja>();
}