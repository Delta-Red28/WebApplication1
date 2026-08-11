using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("TipoMovimientoInventario")]
[Index("Nombre", Name = "UQ_TipoMovimientoInventario", IsUnique = true)]
public partial class TipoMovimientoInventario
{
    [Key]
    public int IdTipoMovimientoInventario { get; set; }

    [StringLength(60)]
    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    [InverseProperty("IdTipoMovimientoInventarioNavigation")]
    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();
}
