using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Dni { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Telefono { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? Correo { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        public bool Activo { get; set; } = true;


        // Relación: un Cliente puede tener muchas Mascotas
        public ICollection<Mascota> Mascotas { get; set; }
            = new List<Mascota>();
    }
}