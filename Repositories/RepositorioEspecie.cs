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