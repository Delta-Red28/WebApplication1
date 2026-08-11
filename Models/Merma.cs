using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Merma")]
public partial class Merma
{
    [Key]
    public int IdMerma { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaMerma { get; set; }

    [StringLength(300)]
    public string? Observacion { get; set; }

    [InverseProperty("IdMermaNavigation")]
    public virtual ICollection<DetalleMerma> DetalleMermas { get; set; } = new List<DetalleMerma>();

    [ForeignKey("IdUsuario")]
    [InverseProperty("Mermas")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
