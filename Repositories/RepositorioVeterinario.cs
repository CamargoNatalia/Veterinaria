
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
                .Include(v => v.Usuario)
                .AsNoTracking()
                .OrderBy(v => v.Usuario.Nombre)
                .ToList();
        }

        // Paginación desde MySQL
        public (IEnumerable<Veterinario> Veterinarios, int Total)
            ObtenerPaginados(int pagina, int cantidadPorPagina)
        {
            pagina = Math.Max(1, pagina);
            cantidadPorPagina = Math.Clamp(cantidadPorPagina, 1, 100);

            var consulta = _context.Veterinarios.AsNoTracking();

            int total = consulta.Count();

            var veterinarios = consulta
                .OrderBy(v => v.Usuario.Nombre)
                .ThenBy(v => v.IdVeterinario)
                .Skip((pagina - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina)
                .Include(v => v.Usuario)
                .ToList();

            return (veterinarios, total);
        }

        public Veterinario? ObtenerPorId(int id)
        {
            return _context.Veterinarios
                .Include(v => v.Usuario)
                .AsNoTracking()
                .FirstOrDefault(v => v.IdVeterinario == id);
        }

        public bool ExisteMatricula(
            string matricula,
            int? idExcluir = null)
        {
            return _context.Veterinarios
                .Any(v => v.Matricula == matricula &&
                    (!idExcluir.HasValue ||
                     v.IdVeterinario != idExcluir.Value));
        }

        public void Crear(Veterinario veterinario)
        {
            _context.Veterinarios.Add(veterinario);
            _context.SaveChanges();
        }

        public void Editar(Veterinario veterinario)
        {
            var actual = _context.Veterinarios
                .Find(veterinario.IdVeterinario);

            if (actual == null)
                return;

            actual.Matricula = veterinario.Matricula;
            actual.Especialidad = veterinario.Especialidad;
            actual.Telefono = veterinario.Telefono;

            _context.SaveChanges();
        }

        // Baja lógica del usuario asociado
        public void Eliminar(int id)
        {
            var veterinario = _context.Veterinarios
                .Include(v => v.Usuario)
                .FirstOrDefault(v => v.IdVeterinario == id);

            if (veterinario == null)
                return;

            veterinario.Usuario.Activo = false;

            _context.SaveChanges();
        }
    }
}
