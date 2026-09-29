using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veterinaria.Models
{
    public class ConsultaTratamiento
    {

        // RELACIÓN CON CONSULTA

        public int IdConsulta { get; set; }

        [ForeignKey(nameof(IdConsulta))]
        public Consulta Consulta { get; set; } = null!;



        // RELACIÓN CON TRATAMIENTO

        public int IdTratamiento { get; set; }

        [ForeignKey(nameof(IdTratamiento))]
        public Tratamiento Tratamiento { get; set; } = null!;


        // DATOS DEL TRATAMIENTO
        // EN ESTA CONSULTA

        public int? Cantidad { get; set; }

        [StringLength(1000)]
        public string? Indicaciones { get; set; }

        [StringLength(1000)]
        public string? Observaciones { get; set; }
    }
}