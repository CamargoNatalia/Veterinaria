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