using Microsoft.EntityFrameworkCore;
using System.Data;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Repositories
{
    public class RepositorioConsulta : IRepositorioConsulta
    {
        private readonly VeterinariaContext _context;

        public RepositorioConsulta(VeterinariaContext context)
        {
            _context = context;
        }

        public IEnumerable<Consulta> ObtenerTodos()
        {
            return _context.Consultas
                .Include(c => c.Mascota)
                .Include(c => c.Usuario)
                .Include(c => c.Veterinario)
                .ThenInclude(v => v!.Usuario)
                .OrderByDescending(c => c.FechaHora)
                .ToList();
        }

        public Consulta? ObtenerPorId(int id)
        {
            return _context.Consultas
                .Include(c => c.Mascota)
                .Include(c => c.Usuario)
                .Include(c => c.Veterinario)
                .ThenInclude(v => v!.Usuario)
                .Include(c => c.HistoriasClinicas)
                .FirstOrDefault(c => c.IdConsulta == id);
        }

        public void Crear(Consulta consulta)
        {
            _context.Consultas.Add(consulta);
            _context.SaveChanges();
        }

        public void Editar(Consulta consulta)
        {
            _context.Consultas.Update(consulta);
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var consulta = _context.Consultas.Find(id);

            if (consulta != null)
            {
                _context.Consultas.Remove(consulta);
                _context.SaveChanges();
            }
        }

        public bool ExisteConsultaParaTurno(int idTurno)
        {
            return _context.Consultas
                .Any(c => c.IdTurno == idTurno);
        }

        // Guarda la atención y actualiza el turno
        public bool RegistrarAtencion(Consulta consulta, int idTurno)
        {
            using var transaccion = _context.Database
                .BeginTransaction(IsolationLevel.ReadCommitted);

            var turnosActualizados = _context.Turnos
                .Where(t => t.IdTurno == idTurno
                    && t.Estado == "Pendiente")
                .Where(t => !_context.Consultas
                    .Any(c => c.IdTurno == t.IdTurno))
                .Where(t => t.IdMascota == consulta.IdMascota)
                .ExecuteUpdate(t => t
                    .SetProperty(x => x.Estado, "Atendido"));

            if (turnosActualizados != 1)
            {
                transaccion.Rollback();
                return false;
            }

            consulta.IdTurno = idTurno;

            _context.Consultas.Add(consulta);
            _context.SaveChanges();

            transaccion.Commit();

            return true;
        }
    }
}