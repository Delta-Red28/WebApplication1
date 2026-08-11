using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Index("Nombre", Name = "IX_UnidadMedida_Nombre")]
[Index("Abreviatura", Name = "UQ_UnidadMedida_Abreviatura", IsUnique = true)]
[Index("Nombre", Name = "UQ_UnidadMedida_Nombre", IsUnique = true)]
public partial class UnidadMedidum
{
    [Key]
    public int IdUnidadMedida { get; set; }

    [StringLength(50)]
    public string Nombre { get; set; } = null!;

    [StringLength(10)]
    public string Abreviatura { get; set; } = null!;

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    public bool EsUnidadBase { get; set; }

    [InverseProperty("IdUnidadDestinoNavigation")]
    public virtual ICollection<ConversionUnidad> ConversionUnidadIdUnidadDestinoNavigations { get; set; } = new List<ConversionUnidad>();

    [InverseProperty("IdUnidadOrigenNavigation")]
    public virtual ICollection<ConversionUnidad> ConversionUnidadIdUnidadOrigenNavigations { get; set; } = new List<ConversionUnidad>();

    [InverseProperty("IdUnidadMedidaNavigation")]
    public virtual ICollection<DetalleRecetum> DetalleReceta { get; set; } = new List<DetalleRecetum>();

    [InverseProperty("IdUnidadBaseNavigation")]
    public virtual ICollection<Insumo> InsumoIdUnidadBaseNavigations { get; set; } = new List<Insumo>();

    [InverseProperty("IdUnidadMedidaNavigation")]
    public virtual ICollection<Insumo> InsumoIdUnidadMedidaNavigations { get; set; } = new List<Insumo>();
}
