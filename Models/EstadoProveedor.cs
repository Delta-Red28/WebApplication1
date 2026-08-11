using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("EstadoProveedor")]
[Index("Nombre", Name = "UQ_EstadoProveedor", IsUnique = true)]
public partial class EstadoProveedor
{
    [Key]
    public int IdEstadoProveedor { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    [StringLength(200)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    [InverseProperty("IdEstadoProveedorNavigation")]
    public virtual ICollection<Proveedor> Proveedors { get; set; } = new List<Proveedor>();
}
