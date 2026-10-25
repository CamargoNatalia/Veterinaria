
using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Repositories
{
    public class RepositorioMascota : IRepositorioMascota
    {
        private readonly VeterinariaContext _context;

        public RepositorioMascota(VeterinariaContext context)
        {
            _context = context;
        }

        public IEnumerable<Mascota> ObtenerTodos()
        {
            return _context.Mascotas
                .Include(m => m.Cliente)
                .Include(m => m.Especie)
                .Include(m => m.Raza)
                .ToList();
        }

        public Mascota? ObtenerPorId(int id)
        {
            return _context.Mascotas
                .Include(m => m.Cliente)
                .Include(m => m.Especie)
                .Include(m => m.Raza)
                .FirstOrDefault(m => m.IdMascota == id);
        }

        // Paginación desde MySQL
        public (IEnumerable<Mascota> Mascotas, int Total)
            ObtenerPaginadas(int pagina, int cantidadPorPagina)
        {
            pagina = Math.Max(1, pagina);
            cantidadPorPagina = Math.Clamp(cantidadPorPagina, 1, 100);

            var consulta = _context.Mascotas
                .AsNoTracking();

            int total = consulta.Count();

            var mascotas = consulta
                .OrderBy(m => m.Nombre)
                .ThenBy(m => m.IdMascota)
                .Skip((pagina - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina)
                .Include(m => m.Cliente)
                .Include(m => m.Especie)
                .Include(m => m.Raza)
                .ToList();

            return (mascotas, total);
        }

        public void Crear(Mascota mascota)
        {
            _context.Mascotas.Add(mascota);
            _context.SaveChanges();
        }

        public void Editar(Mascota mascota)
        {
            _context.Mascotas.Update(mascota);
            _context.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var mascota = _context.Mascotas.Find(id);

            if (mascota != null)
            {
                _context.Mascotas.Remove(mascota);
                _context.SaveChanges();
            }
        }
    }
}
