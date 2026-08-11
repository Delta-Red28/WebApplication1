using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("EstadoProducto")]
public partial class EstadoProducto
{
    [Key]
    public int IdEstadoProducto { get; set; }

    [StringLength(40)]
    public string Nombre { get; set; } = null!;

    [StringLength(150)]
    public string? Descripcion { get; set; }

    [InverseProperty("IdEstadoProductoNavigation")]
    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
