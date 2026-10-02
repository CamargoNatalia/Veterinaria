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

        public Raza? ObtenerPorId(int id)
        {
            return _context.Razas
                .Include(r => r.Especie)
                .FirstOrDefault(r => r.IdRaza == id);
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