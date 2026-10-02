using Veterinaria.Models;

namespace Veterinaria.Interfaces
{
    public interface IRepositorioEspecie
    {
        IEnumerable<Especie> ObtenerTodos();
        Especie? ObtenerPorId(int id);
        void Crear(Especie especie);
        void Editar(Especie especie);
        void Eliminar(int id);
    }
}