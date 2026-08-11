using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("MotivoMerma")]
[Index("Nombre", Name = "UQ_MotivoMerma", IsUnique = true)]
public partial class MotivoMerma
{
    [Key]
    public int IdMotivoMerma { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }

    [InverseProperty("IdMotivoMermaNavigation")]
    public virtual ICollection<DetalleMerma> DetalleMermas { get; set; } = new List<DetalleMerma>();
}
