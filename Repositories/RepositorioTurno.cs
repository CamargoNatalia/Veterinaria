
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
                .ToList();
        }

        public Turno? ObtenerPorId(int id)
        {
            return _context.Turnos
                .Include(t => t.Mascota)
                .FirstOrDefault(t => t.IdTurno == id);
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
