using Veterinaria.Models;

namespace Veterinaria.Interfaces
{
    public interface IRepositorioRaza
    {
        IEnumerable<Raza> ObtenerTodos();

        Raza? ObtenerPorId(int id);

        IEnumerable<Raza> ObtenerPorEspecie(int idEspecie);

        void Crear(Raza raza);

        void Editar(Raza raza);

        void Eliminar(int id);
    }
}