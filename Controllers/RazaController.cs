using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
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

        public IActionResult Index()
        {
            var razas = _repositorioRaza.ObtenerTodos();

            return View(razas);
        }

        public IActionResult Details(int id)
        {
            var raza = _repositorioRaza.ObtenerPorId(id);

            if (raza == null)
            {
                return NotFound();
            }

            return View(raza);
        }

        public IActionResult Create()
        {
            CargarEspecies();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Raza raza)
        {
            if (ModelState.IsValid)
            {
                _repositorioRaza.Crear(raza);

                return RedirectToAction(nameof(Index));
            }

            CargarEspecies(raza);

            return View(raza);
        }

        public IActionResult Edit(int id)
        {
            var raza = _repositorioRaza.ObtenerPorId(id);

            if (raza == null)
            {
                return NotFound();
            }

            CargarEspecies(raza);

            return View(raza);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Raza raza)
        {
            if (id != raza.IdRaza)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _repositorioRaza.Editar(raza);

                return RedirectToAction(nameof(Index));
            }

            CargarEspecies(raza);

            return View(raza);
        }

        public IActionResult Delete(int id)
        {
            var raza = _repositorioRaza.ObtenerPorId(id);

            if (raza == null)
            {
                return NotFound();
            }

            return View(raza);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioRaza.Eliminar(id);

            return RedirectToAction(nameof(Index));
        }

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