
using Microsoft.EntityFrameworkCore;
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
                .OrderByDescending(c => c.FechaHora)
                .ToList();
        }

        public Consulta? ObtenerPorId(int id)
        {
            return _context.Consultas
                .Include(c => c.Mascota)
                .Include(c => c.Usuario)
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
    }
}
