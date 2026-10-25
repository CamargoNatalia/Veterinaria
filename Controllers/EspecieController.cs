
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador,Empleado")]
    public class EspecieController : Controller
    {
        private readonly IRepositorioEspecie _repositorioEspecie;

        public EspecieController(
            IRepositorioEspecie repositorioEspecie)
        {
            _repositorioEspecie = repositorioEspecie;
        }

        // LISTADO PAGINADO
        public IActionResult Index(int pagina = 1)
        {
            const int cantidadPorPagina = 3;

            pagina = Math.Max(1, pagina);

            var resultado = _repositorioEspecie.ObtenerPaginadas(
                pagina, cantidadPorPagina);

            int totalPaginas = (int)Math.Ceiling(
                (double)resultado.Total / cantidadPorPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;

                resultado = _repositorioEspecie.ObtenerPaginadas(
                    pagina, cantidadPorPagina);
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = resultado.Total;

            return View(resultado.Especies);
        }

        // CRUD VUE.JS - SE MANTIENE
        [HttpGet]
        public IActionResult Vue()
        {
            return View();
        }

        // DETALLES
        public IActionResult Details(int id)
        {
            var especie = _repositorioEspecie.ObtenerPorId(id);

            if (especie == null)
                return NotFound();

            return View(especie);
        }

        // CREAR - GET
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // CREAR - POST
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

        // EDITAR - GET
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var especie = _repositorioEspecie.ObtenerPorId(id);

            if (especie == null)
                return NotFound();

            return View(especie);
        }

        // EDITAR - POST
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

        // ELIMINAR - GET
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var especie = _repositorioEspecie.ObtenerPorId(id);

            if (especie == null)
                return NotFound();

            return View(especie);
        }

        // ELIMINAR - POST
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
