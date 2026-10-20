using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Repositories
{
    public class RepositorioHClinica : IRepositorioHClinica
    {
        private readonly VeterinariaContext _context;

        public RepositorioHClinica(VeterinariaContext context)
        {
            _context = context;
        }

        public IEnumerable<HClinica> ObtenerTodos()
        {
            return _context.HistoriasClinicas
                .Include(h => h.Consulta)
                .ThenInclude(c => c.Mascota)
                .OrderByDescending(h => h.CargaArchivo)
                .ToList();
        }

        public HClinica? ObtenerPorId(int id)
        {
            return _context.HistoriasClinicas
                .Include(h => h.Consulta)
                .ThenInclude(c => c.Mascota)
                .FirstOrDefault(h => h.IdHClinica == id);
        }

        public IEnumerable<HClinica> ObtenerPorConsulta(int idConsulta)
        {
            return _context.HistoriasClinicas
                .Where(h => h.IdConsulta == idConsulta)
                .OrderByDescending(h => h.CargaArchivo)
                .ToList();
        }

        public void Crear(HClinica historiaClinica)
        {
            _context.HistoriasClinicas.Add(historiaClinica);
            _context.SaveChanges();
        }

        public void Editar(HClinica historiaClinica)
        {
            _context.HistoriasClinicas.Update(historiaClinica);
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var historiaClinica = _context.HistoriasClinicas.Find(id);

            if (historiaClinica != null)
            {
                _context.HistoriasClinicas.Remove(historiaClinica);
                _context.SaveChanges();
            }
        }
    }
}