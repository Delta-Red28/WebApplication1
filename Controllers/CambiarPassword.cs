using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class CambiarPasswordViewModel
    {
        [Required(
            ErrorMessage = "Debe ingresar su contraseña actual."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(
            ErrorMessage = "Debe ingresar una nueva contraseña."
        )]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "La nueva contraseña debe tener entre 8 y 100 caracteres."
        )]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required(
            ErrorMessage = "Debe confirmar la nueva contraseña."
        )]
        [DataType(DataType.Password)]
        [Compare(
            nameof(NuevaPassword),
            ErrorMessage = "Las contraseñas nuevas no coinciden."
        )]
        [Display(Name = "Confirmar nueva contraseña")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}