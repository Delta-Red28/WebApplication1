using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Factura")]
[Index("IdEstadoFactura", Name = "IX_Factura_Estado")]
[Index("FechaFactura", Name = "IX_Factura_Fecha")]
[Index("IdMoneda", Name = "IX_Factura_Moneda")]
[Index("NumeroFactura", Name = "UQ_Factura_Numero", IsUnique = true)]
[Index("IdPedido", Name = "UQ_Factura_Pedido", IsUnique = true)]
public partial class Factura
{
    [Key]
    public int IdFactura { get; set; }

    public int IdPedido { get; set; }

    public int IdEstadoFactura { get; set; }

    public int IdMoneda { get; set; }

    [StringLength(25)]
    [Unicode(false)]
    public string NumeroFactura { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Descuento { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Impuesto { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Total { get; set; }

    [StringLength(300)]
    public string? Observacion { get; set; }

    public DateTime FechaFactura { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SaldoPendiente { get; set; }

    public int IdTipoComprobante { get; set; }

    public int IdSerieFactura { get; set; }

    public int? IdMotivoAnulacion { get; set; }

    public DateTime? FechaAnulacion { get; set; }

    public int? IdUsuarioAnulo { get; set; }

    [StringLength(300)]
    public string? ObservacionAnulacion { get; set; }

    public int? IdImpuesto { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal? TipoCambioAplicado { get; set; }

    [InverseProperty("IdFacturaNavigation")]
    public virtual ICollection<HistorialFactura> HistorialFacturas { get; set; } = new List<HistorialFactura>();

    [ForeignKey("IdEstadoFactura")]
    [InverseProperty("Facturas")]
    public virtual EstadoFactura IdEstadoFacturaNavigation { get; set; } = null!;

    [ForeignKey("IdImpuesto")]
    [InverseProperty("Facturas")]
    public virtual Impuesto? IdImpuestoNavigation { get; set; }

    [ForeignKey("IdMoneda")]
    [InverseProperty("Facturas")]
    public virtual Monedum IdMonedaNavigation { get; set; } = null!;

    [ForeignKey("IdMotivoAnulacion")]
    [InverseProperty("Facturas")]
    public virtual MotivoAnulacion? IdMotivoAnulacionNavigation { get; set; }

    [ForeignKey("IdPedido")]
    [InverseProperty("Factura")]
    public virtual Pedido IdPedidoNavigation { get; set; } = null!;

    [ForeignKey("IdSerieFactura")]
    [InverseProperty("Facturas")]
    public virtual SerieFactura IdSerieFacturaNavigation { get; set; } = null!;

    [ForeignKey("IdTipoComprobante")]
    [InverseProperty("Facturas")]
    public virtual TipoComprobante IdTipoComprobanteNavigation { get; set; } = null!;

    [ForeignKey("IdUsuarioAnulo")]
    [InverseProperty("Facturas")]
    public virtual Usuario? IdUsuarioAnuloNavigation { get; set; }

    [InverseProperty("IdFacturaNavigation")]
    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
