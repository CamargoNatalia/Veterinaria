using Microsoft.AspNetCore.Mvc;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    public class EspecieController : Controller
    {
        private readonly IRepositorioEspecie _repositorioEspecie;

        public EspecieController(IRepositorioEspecie repositorioEspecie)
        {
            _repositorioEspecie = repositorioEspecie;
        }

        public IActionResult Index()
        {
            var especies = _repositorioEspecie.ObtenerTodos();

            return View(especies);
        }

        public IActionResult Details(int id)
        {
            var especie = _repositorioEspecie.ObtenerPorId(id);

            if (especie == null)
            {
                return NotFound();
            }

            return View(especie);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Especie especie)
        {
            if (ModelState.IsValid)
            {
                _repositorioEspecie.Crear(especie);

                return RedirectToAction(nameof(Index));
            }

            return View(especie);
        }

        public IActionResult Edit(int id)
        {
            var especie = _repositorioEspecie.ObtenerPorId(id);

            if (especie == null)
            {
                return NotFound();
            }

            return View(especie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Especie especie)
        {
            if (id != especie.IdEspecie)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _repositorioEspecie.Editar(especie);

                return RedirectToAction(nameof(Index));
            }

            return View(especie);
        }

        public IActionResult Delete(int id)
        {
            var especie = _repositorioEspecie.ObtenerPorId(id);

            if (especie == null)
            {
                return NotFound();
            }

            return View(especie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioEspecie.Eliminar(id);

            return RedirectToAction(nameof(Index));
        }
    }
}