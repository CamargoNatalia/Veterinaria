using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class Tratamiento
    {
        [Key]
        public int IdTratamiento { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        // RELACIÓN CON CONSULTAS


        public ICollection<ConsultaTratamiento> ConsultaTratamientos { get; set; }
            = new List<ConsultaTratamiento>();
    }
}