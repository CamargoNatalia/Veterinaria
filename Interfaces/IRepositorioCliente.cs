using Veterinaria.Models;

namespace Veterinaria.Interfaces
{
    public interface IRepositorioCliente
    {
        IEnumerable<Cliente> ObtenerTodos();
        Cliente? ObtenerPorId(int id);
        void Crear(Cliente cliente);
        void Editar(Cliente cliente);
        void Eliminar(int id);
    }
}