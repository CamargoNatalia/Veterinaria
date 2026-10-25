
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

        // Paginación desde MySQL
        public (IEnumerable<Consulta> Consultas, int Total)
            ObtenerPaginadas(int pagina, int cantidadPorPagina)
        {
            pagina = Math.Max(1, pagina);
            cantidadPorPagina = Math.Clamp(cantidadPorPagina, 1, 100);

            var consulta = _context.Consultas.AsNoTracking();

            int total = consulta.Count();

            var registros = consulta
                .OrderByDescending(c => c.FechaHora)
                .ThenByDescending(c => c.IdConsulta)
                .Skip((pagina - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina)
                .Include(c => c.Mascota)
                .Include(c => c.Usuario)
                .Include(c => c.Veterinario)
                    .ThenInclude(v => v!.Usuario)
                .ToList();

            return (registros, total);
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

        // Registrar atención y actualizar turno
        public bool RegistrarAtencion(Consulta consulta, int idTurno)
        {
            using var transaccion = _context.Database
                .BeginTransaction(IsolationLevel.ReadCommitted);

            var turnosActualizados = _context.Turnos
                .Where(t => t.IdTurno == idTurno &&
                            t.Estado == "Pendiente")
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
