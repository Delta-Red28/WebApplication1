using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Index("CodigoIso", Name = "UQ_Moneda_Codigo", IsUnique = true)]
public partial class Monedum
{
    [Key]
    public int IdMoneda { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    [Column("CodigoISO")]
    [StringLength(3)]
    [Unicode(false)]
    public string CodigoIso { get; set; } = null!;

    [StringLength(5)]
    public string Simbolo { get; set; } = null!;

    [Column(TypeName = "decimal(18, 6)")]
    public decimal TipoCambio { get; set; }

    public bool EsMonedaBase { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("IdMonedaNavigation")]
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    [InverseProperty("IdMonedaNavigation")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("IdMonedaNavigation")]
    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
