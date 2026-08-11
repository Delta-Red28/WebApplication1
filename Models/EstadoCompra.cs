using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("EstadoCompra")]
[Index("Nombre", Name = "UQ_EstadoCompra", IsUnique = true)]
public partial class EstadoCompra
{
    [Key]
    public int IdEstadoCompra { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    [InverseProperty("IdEstadoCompraNavigation")]
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();

    [InverseProperty("IdEstadoCompraNavigation")]
    public virtual ICollection<SeguimientoCompra> SeguimientoCompras { get; set; } = new List<SeguimientoCompra>();
}
