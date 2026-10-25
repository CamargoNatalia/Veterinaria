
using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Repositories
{
    public class RepositorioRaza : IRepositorioRaza
    {
        private readonly VeterinariaContext _context;

        public RepositorioRaza(VeterinariaContext context)
        {
            _context = context;
        }

        public IEnumerable<Raza> ObtenerTodos()
        {
            return _context.Razas
                .Include(r => r.Especie)
                .ToList();
        }

        // Paginación desde MySQL
        public (IEnumerable<Raza> Razas, int Total)
            ObtenerPaginadas(int pagina, int cantidadPorPagina)
        {
            pagina = Math.Max(1, pagina);
            cantidadPorPagina = Math.Clamp(cantidadPorPagina, 1, 100);

            var consulta = _context.Razas.AsNoTracking();

            int total = consulta.Count();

            var razas = consulta
                .OrderBy(r => r.Nombre)
                .ThenBy(r => r.IdRaza)
                .Skip((pagina - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina)
                .Include(r => r.Especie)
                .ToList();

            return (razas, total);
        }

        public Raza? ObtenerPorId(int id)
        {
            return _context.Razas
                .Include(r => r.Especie)
                .FirstOrDefault(r => r.IdRaza == id);
        }

        public IEnumerable<Raza> ObtenerPorEspecie(int idEspecie)
        {
            return _context.Razas
                .Where(r => r.IdEspecie == idEspecie && r.Activo)
                .OrderBy(r => r.Nombre)
                .ToList();
        }

        public void Crear(Raza raza)
        {
            _context.Razas.Add(raza);
            _context.SaveChanges();
        }

        public void Editar(Raza raza)
        {
            _context.Razas.Update(raza);
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var raza = _context.Razas.Find(id);

            if (raza != null)
            {
                _context.Razas.Remove(raza);
                _context.SaveChanges();
            }
        }
    }
}
