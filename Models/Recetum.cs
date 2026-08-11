using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Index("IdProducto", Name = "UQ_Receta_Producto", IsUnique = true)]
public partial class Recetum
{
    [Key]
    public int IdReceta { get; set; }

    public int IdProducto { get; set; }

    public int IdEstadoReceta { get; set; }

    [StringLength(120)]
    public string Nombre { get; set; } = null!;

    public short Version { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Rendimiento { get; set; }

    [StringLength(300)]
    public string? Observacion { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("IdRecetaNavigation")]
    public virtual ICollection<DetalleRecetum> DetalleReceta { get; set; } = new List<DetalleRecetum>();

    [ForeignKey("IdEstadoReceta")]
    [InverseProperty("Receta")]
    public virtual EstadoRecetum IdEstadoRecetaNavigation { get; set; } = null!;

    [ForeignKey("IdProducto")]
    [InverseProperty("Recetum")]
    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
