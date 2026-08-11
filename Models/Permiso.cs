using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Permiso")]
[Index("Nombre", Name = "UQ__Permiso__75E3EFCF7DC646C2", IsUnique = true)]
public partial class Permiso
{
    [Key]
    public int IdPermiso { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(50)]
    public string Modulo { get; set; } = null!;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    [ForeignKey("IdPermiso")]
    [InverseProperty("IdPermisos")]
    public virtual ICollection<Rol> IdRols { get; set; } = new List<Rol>();
}
