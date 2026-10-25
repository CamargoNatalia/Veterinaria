
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;
using Microsoft.EntityFrameworkCore;

namespace Veterinaria.Repositories
{
    public class RepositorioUsuario : IRepositorioUsuario
    {
        private readonly VeterinariaContext _context;

        public RepositorioUsuario(VeterinariaContext context)
        {
            _context = context;
        }

        public IEnumerable<Usuario> ObtenerTodos()
        {
            return _context.Usuarios.ToList();
        }

        // Paginación desde MySQL
        public (IEnumerable<Usuario> Usuarios, int Total)
            ObtenerPaginados(int pagina, int cantidadPorPagina)
        {
            pagina = Math.Max(1, pagina);
            cantidadPorPagina = Math.Clamp(cantidadPorPagina, 1, 100);

            var consulta = _context.Usuarios.AsNoTracking();

            int total = consulta.Count();

            var usuarios = consulta
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.IdUsuario)
                .Skip((pagina - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina)
                .ToList();

            return (usuarios, total);
        }

        public Usuario? ObtenerPorId(int id)
        {
            return _context.Usuarios.Find(id);
        }

        public void Crear(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
        }

        public void Editar(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var usuario = _context.Usuarios.Find(id);

            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();
            }
        }

        // Login
        public Usuario? ObtenerPorEmail(string email)
        {
            return _context.Usuarios
                .FirstOrDefault(u => u.Email == email);
        }

        // Perfil
        public bool ActualizarPerfil(
            int idUsuario, string nombre, string email)
        {
            var usuario = _context.Usuarios.Find(idUsuario);

            if (usuario == null)
                return false;

            usuario.Nombre = nombre;
            usuario.Email = email;

            _context.SaveChanges();
            return true;
        }

        // Contraseña
        public bool ActualizarPassword(
            int idUsuario, string passwordHash)
        {
            var usuario = _context.Usuarios.Find(idUsuario);

            if (usuario == null)
                return false;

            usuario.Clave = passwordHash;

            _context.SaveChanges();
            return true;
        }

        // Avatar
        public bool ActualizarAvatar(
            int idUsuario, string rutaAvatar)
        {
            var usuario = _context.Usuarios.Find(idUsuario);

            if (usuario == null)
                return false;

            usuario.Avatar = rutaAvatar;

            _context.SaveChanges();
            return true;
        }

        // Cambiar rol
        public bool CambiarRol(int idUsuario, string nuevoRol)
        {
            string[] rolesPermitidos =
            {
                "Administrador",
                "Empleado",
                "Veterinario"
            };

            if (!rolesPermitidos.Contains(nuevoRol))
                return false;

            var usuario = _context.Usuarios.Find(idUsuario);

            if (usuario == null || !usuario.Activo)
                return false;

            if (usuario.Rol == nuevoRol)
                return false;

            bool tieneFicha = TieneFichaVeterinario(idUsuario);

            if (tieneFicha && nuevoRol != "Veterinario")
                return false;

            if (usuario.Rol == "Administrador" &&
                nuevoRol != "Administrador")
            {
                int administradores = _context.Usuarios.Count(u =>
                    u.Rol == "Administrador" && u.Activo);

                if (administradores <= 1)
                    return false;
            }

            usuario.Rol = nuevoRol;

            _context.SaveChanges();
            return true;
        }

        public bool TieneFichaVeterinario(int idUsuario)
        {
            return _context.Veterinarios.Any(v =>
                v.IdUsuario == idUsuario);
        }
    }
}
