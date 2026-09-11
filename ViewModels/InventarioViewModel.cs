using System;

namespace WebApplication1.ViewModels
{
    public class InventarioViewModel
    {
        public int IdExistencia { get; set; }

        public int IdInsumo { get; set; }

        public string CodigoInsumo { get; set; } = "";

        public string NombreInsumo { get; set; } = "";

        public string UnidadMedida { get; set; } = "";

        public string AbreviaturaUnidad { get; set; } = "";

        public int IdUbicacion { get; set; }

        public string Ubicacion { get; set; } = "";

        public decimal StockActual { get; set; }

        public decimal StockMinimo { get; set; }

        public decimal? StockMaximo { get; set; }

        public DateTime UltimaActualizacion { get; set; }

        public bool StockAgotado =>
            StockActual <= 0;

        public bool StockBajo =>
            StockActual > 0 &&
            StockActual <= StockMinimo;
    }
}