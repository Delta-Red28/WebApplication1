using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("ConversionUnidad")]
[Index("IdUnidadOrigen", "IdUnidadDestino", Name = "UQ_Conversion", IsUnique = true)]
public partial class ConversionUnidad
{
    [Key]
    public int IdConversionUnidad { get; set; }

    public int IdUnidadOrigen { get; set; }

    public int IdUnidadDestino { get; set; }

    [Column(TypeName = "decimal(18, 6)")]
    public decimal FactorConversion { get; set; }

    [StringLength(200)]
    public string? Observacion { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [ForeignKey("IdUnidadDestino")]
    [InverseProperty("ConversionUnidadIdUnidadDestinoNavigations")]
    public virtual UnidadMedidum IdUnidadDestinoNavigation { get; set; } = null!;

    [ForeignKey("IdUnidadOrigen")]
    [InverseProperty("ConversionUnidadIdUnidadOrigenNavigations")]
    public virtual UnidadMedidum IdUnidadOrigenNavigation { get; set; } = null!;
}
