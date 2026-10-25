
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
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

        // LISTADO PAGINADO
        public IActionResult Index(int pagina = 1)
        {
            const int cantidadPorPagina = 3;

            pagina = Math.Max(1, pagina);

            var resultado = _repositorioRaza.ObtenerPaginadas(
                pagina, cantidadPorPagina);

            int totalPaginas = (int)Math.Ceiling(
                (double)resultado.Total / cantidadPorPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;

                resultado = _repositorioRaza.ObtenerPaginadas(
                    pagina, cantidadPorPagina);
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = resultado.Total;

            return View(resultado.Razas);
        }

        // DETALLES
        public IActionResult Details(int id)
        {
            var raza = _repositorioRaza.ObtenerPorId(id);

            if (raza == null)
                return NotFound();

            return View(raza);
        }

        // CREAR - GET
        [HttpGet]
        public IActionResult Create()
        {
            CargarEspecies();
            return View();
        }

        // CREAR - POST
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

        // EDITAR - GET
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var raza = _repositorioRaza.ObtenerPorId(id);

            if (raza == null)
                return NotFound();

            CargarEspecies(raza);
            return View(raza);
        }

        // EDITAR - POST
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

        // ELIMINAR - GET
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var raza = _repositorioRaza.ObtenerPorId(id);

            if (raza == null)
                return NotFound();

            return View(raza);
        }

        // ELIMINAR - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioRaza.Eliminar(id);

            TempData["Mensaje"] =
                "Raza eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // CARGAR ESPECIES ACTIVAS
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
