using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veterinaria.Models
{
    public class Turno
    {
        [Key]
        public int IdTurno { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [StringLength(500)]
        public string? Motivo { get; set; }

        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";

        [StringLength(500)]
        public string? Observaciones { get; set; }


        // RELACIÓN CON MASCOTA

        [Required]
        public int IdMascota { get; set; }

        [ForeignKey(nameof(IdMascota))]
        public Mascota? Mascota { get; set; }


        // RELACIÓN CON USUARIO

        [Required]
        public int IdUsuario { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario? Usuario { get; set; }
    }
}