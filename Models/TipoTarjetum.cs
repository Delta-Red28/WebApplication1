using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public partial class TipoTarjetum
{
    [Key]
    public int IdTipoTarjeta { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    [InverseProperty("IdTipoTarjetaNavigation")]
    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
