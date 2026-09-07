using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

[Table("Caja")]
public partial class Caja
{
    // ============================================================
    // CLAVE PRINCIPAL
    // ============================================================

    [Key]
    public int IdCaja { get; set; }


    // ============================================================
    // USUARIO
    // ============================================================

    [Required]
    public int IdUsuario { get; set; }


    // ============================================================
    // ESTADO DE CAJA
    // ============================================================

    [Required]
    public int IdEstadoCaja { get; set; }


    // ============================================================
    // APERTURA
    // ============================================================

    [Required]
    public DateTime FechaApertura { get; set; }


    // ============================================================
    // CIERRE
    // ============================================================

    public DateTime? FechaCierre { get; set; }


    // ============================================================
    // APERTURA POR MONEDA
    // ============================================================
    //
    // Estos son los valores reales con los que se abrió la caja.
    //
    // NIO = Córdobas
    // USD = Dólares
    //
    // IMPORTANTE:
    // No deben modificarse cuando se registran pagos o movimientos.
    //
    // ============================================================

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MontoInicialCordobas { get; set; }


    [Column(TypeName = "decimal(18, 2)")]
    public decimal MontoInicialDolares { get; set; }


    // ============================================================
    // TOTALES DE INGRESOS
    // ============================================================
    //
    // Solamente movimientos NO anulados.
    //
    // ============================================================

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalIngresosCordobas { get; set; }


    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalIngresosDolares { get; set; }


    // ============================================================
    // TOTALES DE EGRESOS
    // ============================================================
    //
    // Solamente movimientos NO anulados.
    //
    // ============================================================

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalEgresosCordobas { get; set; }


    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalEgresosDolares { get; set; }


    // ============================================================
    // TOTAL SISTEMA
    // ============================================================
    //
    // Representa cuánto debería existir en caja según el sistema.
    //
    // NIO:
    //
    // MontoInicialCordobas
    // + TotalIngresosCordobas
    // - TotalEgresosCordobas
    //
    // USD:
    //
    // MontoInicialDolares
    // + TotalIngresosDolares
    // - TotalEgresosDolares
    //
    // ============================================================

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalSistemaCordobas { get; set; }


    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalSistemaDolares { get; set; }


    // ============================================================
    // CONTADO AL CIERRE
    // ============================================================
    //
    // Dinero contado físicamente por el usuario al cerrar caja.
    //
    // No representa dinero calculado por el sistema.
    //
    // ============================================================

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalContadoCordobas { get; set; }


    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalContadoDolares { get; set; }


    // ============================================================
    // DIFERENCIA AL CIERRE
    // ============================================================
    //
    // Diferencia = Contado físico - Total sistema
    //
    // Puede ser:
    //
    //  0     = caja cuadrada
    //  > 0   = sobrante
    //  < 0   = faltante
    //
    // ============================================================

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? DiferenciaCordobas { get; set; }


    [Column(TypeName = "decimal(18, 2)")]
    public decimal? DiferenciaDolares { get; set; }


    // ============================================================
    // OBSERVACIÓN
    // ============================================================

    public string? Observacion { get; set; }


    // ============================================================
    // CAMPOS LEGACY / COMPATIBILIDAD
    // ============================================================
    //
    // Estos campos aparentemente ya existen en tu tabla.
    //
    // NO los utilizaremos para calcular la caja nueva.
    //
    // Los dejamos temporalmente para no romper el modelo existente
    // ni el código actual hasta que revisemos CajaController y
    // decidamos si realmente deben eliminarse de la base de datos.
    //
    // ============================================================

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MontoInicial { get; set; }


    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalIngresos { get; set; }


    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalEgresos { get; set; }


    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalSistema { get; set; }


    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalContado { get; set; }


    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Diferencia { get; set; }


    // ============================================================
    // NAVEGACIÓN HACIA USUARIO
    // ============================================================

    [ForeignKey(nameof(IdUsuario))]
    public virtual Usuario? IdUsuarioNavigation { get; set; }


    // ============================================================
    // NAVEGACIÓN HACIA ESTADO DE CAJA
    // ============================================================

    [ForeignKey(nameof(IdEstadoCaja))]
    public virtual EstadoCaja? IdEstadoCajaNavigation { get; set; }


    // ============================================================
    // MOVIMIENTOS DE CAJA
    // ============================================================
    //
    // Una caja puede tener muchos movimientos.
    //
    // Cada movimiento pertenece a una sola caja.
    //
    // ============================================================

    [InverseProperty(nameof(MovimientoCaja.IdCajaNavigation))]
    public virtual ICollection<MovimientoCaja> MovimientoCajas { get; set; }
        = new List<MovimientoCaja>();
}