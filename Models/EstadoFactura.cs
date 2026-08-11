using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("EstadoFactura")]
[Index("Nombre", Name = "UQ_EstadoFactura", IsUnique = true)]
public partial class EstadoFactura
{
    [Key]
    public int IdEstadoFactura { get; set; }

    [StringLength(40)]
    public string Nombre { get; set; } = null!;

    [StringLength(150)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    [InverseProperty("IdEstadoFacturaNavigation")]
    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    [InverseProperty("IdEstadoFacturaNavigation")]
    public virtual ICollection<HistorialFactura> HistorialFacturas { get; set; } = new List<HistorialFactura>();
}
