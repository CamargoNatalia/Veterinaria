
using Veterinaria.Models;

namespace Veterinaria.Interfaces
{
    public interface IRepositorioUsuario
    {
        IEnumerable<Usuario> ObtenerTodos();
        Usuario? ObtenerPorId(int id);
        void Crear(Usuario usuario);
        void Editar(Usuario usuario);
        void Eliminar(int id);

        // Login
        Usuario? ObtenerPorEmail(string email);

        // Perfil
        bool ActualizarPerfil(int idUsuario, string nombre, string email);
        bool ActualizarPassword(int idUsuario, string passwordHash);
        bool ActualizarAvatar(int idUsuario, string rutaAvatar);

        // Roles
        bool CambiarRol(int idUsuario, string nuevoRol);
    }
}
