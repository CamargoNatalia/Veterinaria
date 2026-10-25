
using Veterinaria.Models;

namespace Veterinaria.Interfaces
{
    public interface IRepositorioTurno
    {
        IEnumerable<Turno> ObtenerTodos();
        Turno? ObtenerPorId(int id);
        void Crear(Turno turno);
        void Editar(Turno turno);
        void Eliminar(int id);

        IEnumerable<Turno> ObtenerPendientes();

        // Paginación
        (IEnumerable<Turno> Turnos, int Total)
            ObtenerPaginados(int pagina, int cantidadPorPagina);
    }
}
