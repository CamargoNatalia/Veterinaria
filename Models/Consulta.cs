using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veterinaria.Models
{
    public class Consulta
    {
        [Key]
        public int IdConsulta { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [StringLength(1000)]
        public string? Motivo { get; set; }

        [StringLength(2000)]
        public string? Diagnostico { get; set; }

        [StringLength(2000)]
        public string? Observaciones { get; set; }

        // RELACIÓN CON MASCOTA

        [Required]
        public int IdMascota { get; set; }

        [ForeignKey(nameof(IdMascota))]
        public Mascota Mascota { get; set; } = null!;


        // RELACIÓN CON USUARIO

        [Required]
        public int IdUsuario { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario Usuario { get; set; } = null!;

        // RELACIÓN CON TRATAMIENTOS

        public ICollection<ConsultaTratamiento> ConsultaTratamientos { get; set; }
            = new List<ConsultaTratamiento>();


        // HISTORIA CLÍNICA / ARCHIVOS

        public ICollection<HClinica> HistoriasClinicas { get; set; }
            = new List<HClinica>();
    }
}