using Veterinaria.Models;

namespace Veterinaria.Interfaces
{
    public interface IRepositorioRaza
    {
        IEnumerable<Raza> ObtenerTodos();
        Raza? ObtenerPorId(int id);
        void Crear(Raza raza);
        void Editar(Raza raza);
        void Eliminar(int id);
    }
}