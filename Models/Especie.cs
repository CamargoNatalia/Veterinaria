using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class Especie
    {
        [Key]
        public int IdEspecie { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;


        // RELACIÓN CON RAZAS

        // Una especie puede tener muchas razas
        public ICollection<Raza> Razas { get; set; }
            = new List<Raza>();
    }
}