using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("SeguimientoCompra")]
[Index("IdCompra", Name = "IX_SeguimientoCompra_Compra")]
[Index("IdEstadoCompra", Name = "IX_SeguimientoCompra_Estado")]
[Index("FechaCambio", Name = "IX_SeguimientoCompra_Fecha")]
public partial class SeguimientoCompra
{
    [Key]
    public int IdSeguimientoCompra { get; set; }

    public int IdCompra { get; set; }

    public int IdEstadoCompra { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaCambio { get; set; }

    [StringLength(300)]
    public string? Observacion { get; set; }

    [ForeignKey("IdCompra")]
    [InverseProperty("SeguimientoCompras")]
    public virtual Compra IdCompraNavigation { get; set; } = null!;

    [ForeignKey("IdEstadoCompra")]
    [InverseProperty("SeguimientoCompras")]
    public virtual EstadoCompra IdEstadoCompraNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("SeguimientoCompras")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
