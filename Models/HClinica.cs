using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veterinaria.Models
{
    public class HClinica
    {
        [Key]
        public int IdHClinica { get; set; }

        [Required]
        [StringLength(255)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string CarpetaDescarga { get; set; } = string.Empty;

        [StringLength(50)]
        public string? FormatoArchivo { get; set; }

        public DateTime CargaArchivo { get; set; } = DateTime.Now;

        [StringLength(1000)]
        public string? Descripcion { get; set; }


        // RELACIÓN CON CONSULTA


        [Required]
        public int IdConsulta { get; set; }

        [ForeignKey(nameof(IdConsulta))]
        public Consulta Consulta { get; set; } = null!;
    }
}