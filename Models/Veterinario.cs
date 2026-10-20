using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class Veterinario
    {
        [Key]
        public int IdVeterinario { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "La matrícula es obligatoria.")]
        [StringLength(50, MinimumLength = 3)]
        public string Matricula { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especialidad es obligatoria.")]
        [StringLength(150, MinimumLength = 2)]
        public string Especialidad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(30, MinimumLength = 7)]
        [RegularExpression(@"^\+?[0-9()\s.-]+$")]
        public string Telefono { get; set; } = string.Empty;

        // Relación con Usuario
        public Usuario Usuario { get; set; } = null!;
    }
}