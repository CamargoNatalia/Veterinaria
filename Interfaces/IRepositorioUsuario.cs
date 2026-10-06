using Veterinaria.Models;

namespace Veterinaria.Interfaces
{
    public interface IRepositorioUsuario
    {
        IEnumerable<Usuario> ObtenerTodos();
        Usuario ObtenerPorId(int id);
        void Crear(Usuario usuario);
        void Editar(Usuario usuario);
        void Eliminar(int id);
    }
}