
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
    }
}
