
using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Repositories
{
    public class RepositorioEspecie : IRepositorioEspecie
    {
        private readonly VeterinariaContext _context;

        public RepositorioEspecie(VeterinariaContext context)
        {
            _context = context;
        }

        public IEnumerable<Especie> ObtenerTodos()
        {
            return _context.Especies.ToList();
        }

        // Paginación desde MySQL
        public (IEnumerable<Especie> Especies, int Total)
            ObtenerPaginadas(int pagina, int cantidadPorPagina)
        {
            pagina = Math.Max(1, pagina);
            cantidadPorPagina = Math.Clamp(cantidadPorPagina, 1, 100);

            var consulta = _context.Especies.AsNoTracking();

            int total = consulta.Count();

            var especies = consulta
                .OrderBy(e => e.Nombre)
                .ThenBy(e => e.IdEspecie)
                .Skip((pagina - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina)
                .ToList();

            return (especies, total);
        }

        public Especie? ObtenerPorId(int id)
        {
            return _context.Especies
                .FirstOrDefault(e => e.IdEspecie == id);
        }

        public void Crear(Especie especie)
        {
            _context.Especies.Add(especie);
            _context.SaveChanges();
        }

        public void Editar(Especie especie)
        {
            _context.Especies.Update(especie);
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var especie = _context.Especies.Find(id);

            if (especie != null)
            {
                _context.Especies.Remove(especie);
                _context.SaveChanges();
            }
        }
    }
}
