using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veterinaria.Models
{
    public class Raza
    {
        [Key]
        public int IdRaza { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;


        // RELACIÓN CON ESPECIE


        [Required]
        public int IdEspecie { get; set; }

        [ForeignKey(nameof(IdEspecie))]
        public Especie Especie { get; set; } = null!;


        // RELACIÓN CON MASCOTAS


        // Una raza puede pertenecer a muchas mascotas
        public ICollection<Mascota> Mascotas { get; set; }
            = new List<Mascota>();
    }
}