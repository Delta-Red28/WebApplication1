using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

[Table("Caja")]
public partial class Caja
{
    public int IdCaja { get; set; }

    public int IdUsuario { get; set; }

    public int IdEstadoCaja { get; set; }

    public DateTime FechaApertura { get; set; }

    public DateTime? FechaCierre { get; set; }

    public decimal MontoInicial { get; set; }

    public decimal TotalIngresos { get; set; }

    public decimal TotalEgresos { get; set; }

    public decimal TotalSistema { get; set; }

    public decimal? TotalContado { get; set; }

    public decimal? Diferencia { get; set; }

    public string? Observacion { get; set; }


    // ============================================================
    // TOTALES POR MONEDA
    // ============================================================

    public decimal MontoInicialCordobas { get; set; }

    public decimal MontoInicialDolares { get; set; }

    public decimal TotalIngresosCordobas { get; set; }

    public decimal TotalIngresosDolares { get; set; }

    public decimal TotalEgresosCordobas { get; set; }

    public decimal TotalEgresosDolares { get; set; }

    public decimal TotalSistemaCordobas { get; set; }

    public decimal TotalSistemaDolares { get; set; }

    public decimal? TotalContadoCordobas { get; set; }

    public decimal? TotalContadoDolares { get; set; }

    public decimal? DiferenciaCordobas { get; set; }

    public decimal? DiferenciaDolares { get; set; }


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
    // MOVIMIENTOS
    // ============================================================

    public virtual ICollection<MovimientoCaja> MovimientoCajas { get; set; }
        = new List<MovimientoCaja>();
}