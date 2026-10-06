using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

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

        public Usuario ObtenerPorId(int id)
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
    }
}