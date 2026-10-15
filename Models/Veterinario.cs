using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class Veterinario
    {
        [Key]
        public int IdVeterinario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La matrícula es obligatoria.")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "La matrícula debe tener entre 3 y 50 caracteres.")]
        public string Matricula { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especialidad es obligatoria.")]
        [StringLength(150, MinimumLength = 2,
            ErrorMessage = "La especialidad debe tener entre 2 y 150 caracteres.")]
        public string Especialidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(30, MinimumLength = 7,
            ErrorMessage = "El teléfono debe tener entre 7 y 30 caracteres.")]
        [RegularExpression(@"^\+?[0-9()\s.-]+$",
            ErrorMessage = "Ingrese un teléfono válido.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string Correo { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}