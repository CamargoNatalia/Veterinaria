using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    // Solo Administrador y Empleado pueden gestionar razas
    [Authorize(Roles = "Administrador,Empleado")]
    public class RazaController : Controller
    {
        private readonly IRepositorioRaza _repositorioRaza;
        private readonly VeterinariaContext _context;

        public RazaController(
            IRepositorioRaza repositorioRaza,
            VeterinariaContext context)
        {
            _repositorioRaza = repositorioRaza;
            _context = context;
        }

        // GET: Raza
        public IActionResult Index()
        {
            var razas = _repositorioRaza.ObtenerTodos();

            return View(razas);
        }

        // GET: Raza/Details/5
        public IActionResult Details(int id)
        {
            var raza = _repositorioRaza.ObtenerPorId(id);

            if (raza == null)
                return NotFound();

            return View(raza);
        }

        // GET: Raza/Create
        [HttpGet]
        public IActionResult Create()
        {
            CargarEspecies();

            return View();
        }

        // POST: Raza/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Raza raza)
        {
            if (ModelState.IsValid)
            {
                _repositorioRaza.Crear(raza);

                TempData["Mensaje"] =
                    "Raza registrada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            CargarEspecies(raza);

            return View(raza);
        }

        // GET: Raza/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var raza = _repositorioRaza.ObtenerPorId(id);

            if (raza == null)
                return NotFound();

            CargarEspecies(raza);

            return View(raza);
        }

        // POST: Raza/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Raza raza)
        {
            if (id != raza.IdRaza)
                return NotFound();

            if (ModelState.IsValid)
            {
                _repositorioRaza.Editar(raza);

                TempData["Mensaje"] =
                    "Raza actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            CargarEspecies(raza);

            return View(raza);
        }

        // GET: Raza/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var raza = _repositorioRaza.ObtenerPorId(id);

            if (raza == null)
                return NotFound();

            return View(raza);
        }

        // POST: Raza/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioRaza.Eliminar(id);

            TempData["Mensaje"] =
                "Raza eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // Cargar especies activas en el formulario
        private void CargarEspecies(Raza? raza = null)
        {
            ViewBag.Especies = new SelectList(
                _context.Especies.Where(e => e.Activo),
                "IdEspecie",
                "Nombre",
                raza?.IdEspecie);
        }
    }
}