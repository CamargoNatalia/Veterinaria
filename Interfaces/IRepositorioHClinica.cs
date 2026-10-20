using Veterinaria.Models;

namespace Veterinaria.Interfaces
{
    public interface IRepositorioHClinica
    {
        IEnumerable<HClinica> ObtenerTodos();
        HClinica? ObtenerPorId(int id);
        IEnumerable<HClinica> ObtenerPorConsulta(int idConsulta);
        void Crear(HClinica historiaClinica);
        void Editar(HClinica historiaClinica);
        void Eliminar(int id);
    }
}