using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Banco")]
[Index("Nombre", Name = "UQ_Banco_Nombre", IsUnique = true)]
public partial class Banco
{
    [Key]
    public int IdBanco { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [InverseProperty("IdBancoNavigation")]
    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
