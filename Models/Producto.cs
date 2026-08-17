using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Producto")]
[Index("IdCategoria", Name = "IX_Producto_Categoria")]
[Index("IdEstadoProducto", Name = "IX_Producto_Estado")]
[Index("Nombre", Name = "IX_Producto_Nombre")]
[Index("CodigoProducto", Name = "UQ_Producto_Codigo", IsUnique = true)]
public partial class Producto
{
    [Key]
    public int IdProducto { get; set; }

    public int IdCategoria { get; set; }

    public int IdEstadoProducto { get; set; }

    [StringLength(120)]
    public string Nombre { get; set; } = null!;

    [StringLength(400)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PrecioVenta { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Costo { get; set; }

    [StringLength(250)]
    public string? Imagen { get; set; }

    public int? TiempoPreparacion { get; set; }

    public bool Estado { get; set; }

    public DateTime? FechaRegistro { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string CodigoProducto { get; set; } = null!;

    public bool ControlaInventario { get; set; }

    [InverseProperty("IdProductoNavigation")]
    public virtual ICollection<DetallePedido> DetallePedidos { get; set; }
        = new List<DetallePedido>();

    [ForeignKey("IdCategoria")]
    [InverseProperty("Productos")]
    public virtual Categorium? IdCategoriaNavigation { get; set; }

    [ForeignKey("IdEstadoProducto")]
    [InverseProperty("Productos")]
    public virtual EstadoProducto? IdEstadoProductoNavigation { get; set; }

    [InverseProperty("IdProductoNavigation")]
    public virtual Recetum? Recetum { get; set; }
}