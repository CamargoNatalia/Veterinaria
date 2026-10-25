
using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Repositories
{
    public class RepositorioTurno : IRepositorioTurno
    {
        private readonly VeterinariaContext _context;

        public RepositorioTurno(VeterinariaContext context)
        {
            _context = context;
        }

        public IEnumerable<Turno> ObtenerTodos()
        {
            return _context.Turnos
                .Include(t => t.Mascota)
                .Include(t => t.Usuario)
                .ToList();
        }

        public Turno? ObtenerPorId(int id)
        {
            return _context.Turnos
                .Include(t => t.Mascota)
                .Include(t => t.Usuario)
                .FirstOrDefault(t => t.IdTurno == id);
        }

        // Paginación desde MySQL
        public (IEnumerable<Turno> Turnos, int Total)
            ObtenerPaginados(int pagina, int cantidadPorPagina)
        {
            pagina = Math.Max(1, pagina);
            cantidadPorPagina = Math.Clamp(cantidadPorPagina, 1, 100);

            var consulta = _context.Turnos.AsNoTracking();

            int total = consulta.Count();

            var turnos = consulta
                .OrderByDescending(t => t.FechaHora)
                .ThenByDescending(t => t.IdTurno)
                .Skip((pagina - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina)
                .Include(t => t.Mascota)
                .Include(t => t.Usuario)
                .ToList();

            return (turnos, total);
        }

        // Turnos pendientes sin consulta asociada
        public IEnumerable<Turno> ObtenerPendientes()
        {
            return _context.Turnos
                .AsNoTracking()
                .Include(t => t.Mascota)
                .Where(t => t.Estado == "Pendiente")
                .Where(t => !_context.Consultas
                    .Any(c => c.IdTurno == t.IdTurno))
                .OrderBy(t => t.FechaHora)
                .ToList();
        }

        public void Crear(Turno turno)
        {
            _context.Turnos.Add(turno);
            _context.SaveChanges();
        }

        public void Editar(Turno turno)
        {
            _context.Turnos.Update(turno);
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var turno = _context.Turnos.Find(id);

            if (turno != null)
            {
                _context.Turnos.Remove(turno);
                _context.SaveChanges();
            }
        }
    }
}
