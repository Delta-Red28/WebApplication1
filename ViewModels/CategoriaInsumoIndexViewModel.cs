namespace WebApplication1.ViewModels
{
    public class CategoriaInsumoIndexViewModel
    {
        public int IdCategoriaInsumo { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int CantidadInsumos { get; set; }
    }
}