using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Repositories
{
    public class RepositorioVeterinario : IRepositorioVeterinario
    {
        private readonly VeterinariaContext _context;

        public RepositorioVeterinario(VeterinariaContext context)
        {
            _context = context;
        }

        public IEnumerable<Veterinario> ObtenerTodos()
        {
            return _context.Veterinarios
                .AsNoTracking()
                .OrderBy(v => v.Apellido)
                .ThenBy(v => v.Nombre)
                .ToList();
        }

        public Veterinario? ObtenerPorId(int id)
        {
            return _context.Veterinarios
                .FirstOrDefault(v => v.IdVeterinario == id);
        }

        public bool ExisteMatricula(
            string matricula,
            int? idExcluir = null)
        {
            return _context.Veterinarios
                .Any(v => v.Matricula == matricula
                    && (!idExcluir.HasValue ||
                        v.IdVeterinario != idExcluir.Value));
        }

        public void Crear(Veterinario veterinario)
        {
            _context.Veterinarios.Add(veterinario);
            _context.SaveChanges();
        }

        public void Editar(Veterinario veterinario)
        {
            _context.Veterinarios.Update(veterinario);
            _context.SaveChanges();
        }

        // Baja lógica: conserva el registro
        public void Eliminar(int id)
        {
            var veterinario = _context.Veterinarios.Find(id);

            if (veterinario != null)
            {
                veterinario.Activo = false;
                _context.SaveChanges();
            }
        }
    }
}