using Veterinaria.Models;

namespace Veterinaria.Interfaces
{
    public interface IRepositorioVeterinario
    {
        IEnumerable<Veterinario> ObtenerTodos();
        Veterinario? ObtenerPorId(int id);
        void Crear(Veterinario veterinario);
        void Editar(Veterinario veterinario);
        void Eliminar(int id);
        bool ExisteMatricula(string matricula, int? idExcluir = null);
    }
}