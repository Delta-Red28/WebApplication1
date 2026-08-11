using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Index("Nombre", Name = "UQ_EstadoTransferencia", IsUnique = true)]
public partial class EstadoTransferencium
{
    [Key]
    public int IdEstadoTransferencia { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    [InverseProperty("IdEstadoTransferenciaNavigation")]
    public virtual ICollection<TransferenciaInventario> TransferenciaInventarios { get; set; } = new List<TransferenciaInventario>();
}
