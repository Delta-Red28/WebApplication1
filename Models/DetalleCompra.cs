using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class DetalleCompra
{
    public int IdDetalleCompra { get; set; }

    public int IdCompra { get; set; }

    public int IdInsumo { get; set; }

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuesto { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TotalLinea { get; set; }

    public virtual Compra IdCompraNavigation { get; set; } = null!;

    public virtual Insumo IdInsumoNavigation { get; set; } = null!;

    public virtual ICollection<LoteInsumo> LoteInsumos { get; set; }
        = new List<LoteInsumo>();
}