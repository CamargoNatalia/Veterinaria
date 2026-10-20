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

        // Listado con los datos del usuario
        public IEnumerable<Veterinario> ObtenerTodos()
        {
            return _context.Veterinarios
                .Include(v => v.Usuario)
                .AsNoTracking()
                .OrderBy(v => v.Usuario.Nombre)
                .ToList();
        }

        // Obtener veterinario con su usuario
        public Veterinario? ObtenerPorId(int id)
        {
            return _context.Veterinarios
                .Include(v => v.Usuario)
                .AsNoTracking()
                .FirstOrDefault(v => v.IdVeterinario == id);
        }

        // Validar matrícula única
        public bool ExisteMatricula(
            string matricula,
            int? idExcluir = null)
        {
            return _context.Veterinarios
                .Any(v => v.Matricula == matricula &&
                    (!idExcluir.HasValue ||
                     v.IdVeterinario != idExcluir.Value));
        }

        // Registrar ficha profesional
        public void Crear(Veterinario veterinario)
        {
            _context.Veterinarios.Add(veterinario);
            _context.SaveChanges();
        }

        // Actualizar solamente datos profesionales
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

        // Baja lógica mediante la cuenta Usuario
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