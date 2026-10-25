
using Veterinaria.Models;

namespace Veterinaria.Interfaces
{
    public interface IRepositorioConsulta
    {
        IEnumerable<Consulta> ObtenerTodos();
        Consulta? ObtenerPorId(int id);
        void Crear(Consulta consulta);
        void Editar(Consulta consulta);
        void Eliminar(int id);

        bool ExisteConsultaParaTurno(int idTurno);
        bool RegistrarAtencion(Consulta consulta, int idTurno);

        // Paginación
        (IEnumerable<Consulta> Consultas, int Total)
            ObtenerPaginadas(int pagina, int cantidadPorPagina);
    }
}
