using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veterinaria.Models
{
    public class Mascota
    {
        [Key]
        public int IdMascota { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(20)]
        public string? Sexo { get; set; }

        public decimal? Peso { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public bool Activo { get; set; } = true;


        // RELACIÓN CON CLIENTE

        [Required]
        public int IdCliente { get; set; }

        [ForeignKey(nameof(IdCliente))]
        public Cliente Cliente { get; set; } = null!;


        // RELACIÓN CON ESPECIE

        [Required]
        public int IdEspecie { get; set; }

        [ForeignKey(nameof(IdEspecie))]
        public Especie Especie { get; set; } = null!;


        // RELACIÓN CON RAZA

        [Required]
        public int IdRaza { get; set; }

        [ForeignKey(nameof(IdRaza))]
        public Raza Raza { get; set; } = null!;


        // RELACIÓN CON TURNOS

        public ICollection<Turno> Turnos { get; set; }
            = new List<Turno>();


        // RELACIÓN CON CONSULTAS

        public ICollection<Consulta> Consultas { get; set; }
            = new List<Consulta>();
    }
}