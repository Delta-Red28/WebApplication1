using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("AjusteInventario")]
public partial class AjusteInventario
{
    [Key]
    public int IdAjuste { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaAjuste { get; set; }

    [StringLength(300)]
    public string? Observacion { get; set; }

    [InverseProperty("IdAjusteNavigation")]
    public virtual ICollection<DetalleAjuste> DetalleAjustes { get; set; } = new List<DetalleAjuste>();

    [ForeignKey("IdUsuario")]
    [InverseProperty("AjusteInventarios")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
