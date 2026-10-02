using Veterinaria.Models;

namespace Veterinaria.Interfaces
{
    public interface IRepositorioMascota
    {
        IEnumerable<Mascota> ObtenerTodos();
        Mascota? ObtenerPorId(int id);
        void Crear(Mascota mascota);
        void Editar(Mascota mascota);
        void Eliminar(int id);
    }
}