using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    // Solo Administrador y Empleado pueden gestionar especies
    [Authorize(Roles = "Administrador,Empleado")]
    public class EspecieController : Controller
    {
        private readonly IRepositorioEspecie _repositorioEspecie;

        public EspecieController(
            IRepositorioEspecie repositorioEspecie)
        {
            _repositorioEspecie = repositorioEspecie;
        }

        // GET: Especie
        public IActionResult Index()
        {
            var especies = _repositorioEspecie.ObtenerTodos();

            return View(especies);
        }

        // GET: Especie/Details/5
        public IActionResult Details(int id)
        {
            var especie = _repositorioEspecie.ObtenerPorId(id);

            if (especie == null)
                return NotFound();

            return View(especie);
        }

        // GET: Especie/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Especie/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Especie especie)
        {
            if (ModelState.IsValid)
            {
                _repositorioEspecie.Crear(especie);

                TempData["Mensaje"] =
                    "Especie registrada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(especie);
        }

        // GET: Especie/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var especie = _repositorioEspecie.ObtenerPorId(id);

            if (especie == null)
                return NotFound();

            return View(especie);
        }

        // POST: Especie/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Especie especie)
        {
            if (id != especie.IdEspecie)
                return NotFound();

            if (ModelState.IsValid)
            {
                _repositorioEspecie.Editar(especie);

                TempData["Mensaje"] =
                    "Especie actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(especie);
        }

        // GET: Especie/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var especie = _repositorioEspecie.ObtenerPorId(id);

            if (especie == null)
                return NotFound();

            return View(especie);
        }

        // POST: Especie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioEspecie.Eliminar(id);

            TempData["Mensaje"] =
                "Especie eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}