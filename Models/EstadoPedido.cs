using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("EstadoPedido")]
public partial class EstadoPedido
{
    [Key]
    public int IdEstadoPedido { get; set; }

    [StringLength(40)]
    public string? Nombre { get; set; }

    [StringLength(200)]
    public string? Descripcion { get; set; }

    [InverseProperty("IdEstadoPedidoNavigation")]
    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
