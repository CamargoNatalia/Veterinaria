using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Rol { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Avatar { get; set; }

        public bool Activo { get; set; } = true;


        // RELACIÓN CON TURNOS


        public ICollection<Turno> Turnos { get; set; }
            = new List<Turno>();


        // RELACIÓN CON CONSULTAS

        public ICollection<Consulta> Consultas { get; set; }
            = new List<Consulta>();
    }
}