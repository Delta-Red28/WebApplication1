using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[Table("Insumo")]
[Index("IdCategoriaInsumo", Name = "IX_Insumo_Categoria")]
[Index("IdEstadoInsumo", Name = "IX_Insumo_Estado")]
[Index("IdMarca", Name = "IX_Insumo_Marca")]
[Index("Nombre", Name = "IX_Insumo_Nombre")]
[Index("CodigoInsumo", Name = "UQ_Insumo_Codigo", IsUnique = true)]
public partial class Insumo
{
    [Key]
    public int IdInsumo { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string CodigoInsumo { get; set; } = null!;

    public int IdCategoriaInsumo { get; set; }

    public int IdUnidadMedida { get; set; }

    public int IdMarca { get; set; }

    public int IdEstadoInsumo { get; set; }

    [StringLength(120)]
    public string Nombre { get; set; } = null!;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CostoPromedio { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal StockMinimo { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? StockMaximo { get; set; }

    public bool EsPerecedero { get; set; }

    public int? DiasVencimiento { get; set; }

    public bool Estado { get; set; }

    public DateTime FechaRegistro { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CostoUltimaCompra { get; set; }

    public int? IdUnidadBase { get; set; }

    [InverseProperty("IdInsumoNavigation")]
    public virtual ICollection<DetalleAjuste> DetalleAjustes { get; set; } = new List<DetalleAjuste>();

    [InverseProperty("IdInsumoNavigation")]
    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();

    [InverseProperty("IdInsumoNavigation")]
    public virtual ICollection<DetalleMerma> DetalleMermas { get; set; } = new List<DetalleMerma>();

    [InverseProperty("IdInsumoNavigation")]
    public virtual ICollection<DetalleRecetum> DetalleReceta { get; set; } = new List<DetalleRecetum>();

    [InverseProperty("IdInsumoNavigation")]
    public virtual ICollection<DetalleTransferencium> DetalleTransferencia { get; set; } = new List<DetalleTransferencium>();

    [InverseProperty("IdInsumoNavigation")]
    public virtual ICollection<Existencium> Existencia { get; set; } = new List<Existencium>();

    [ForeignKey("IdCategoriaInsumo")]
    [InverseProperty("Insumos")]
    public virtual CategoriaInsumo IdCategoriaInsumoNavigation { get; set; } = null!;

    [ForeignKey("IdEstadoInsumo")]
    [InverseProperty("Insumos")]
    public virtual EstadoInsumo IdEstadoInsumoNavigation { get; set; } = null!;

    [ForeignKey("IdMarca")]
    [InverseProperty("Insumos")]
    public virtual Marca IdMarcaNavigation { get; set; } = null!;

    [ForeignKey("IdUnidadBase")]
    [InverseProperty("InsumoIdUnidadBaseNavigations")]
    public virtual UnidadMedidum? IdUnidadBaseNavigation { get; set; }

    [ForeignKey("IdUnidadMedida")]
    [InverseProperty("InsumoIdUnidadMedidaNavigations")]
    public virtual UnidadMedidum IdUnidadMedidaNavigation { get; set; } = null!;

    [InverseProperty("IdInsumoNavigation")]
    public virtual ICollection<LoteInsumo> LoteInsumos { get; set; } = new List<LoteInsumo>();

    [InverseProperty("IdInsumoNavigation")]
    public virtual ICollection<MovimientoInventario> MovimientoInventarios { get; set; } = new List<MovimientoInventario>();
}
