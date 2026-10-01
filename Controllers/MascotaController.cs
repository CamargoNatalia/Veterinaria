using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Veterinaria.Interfaces;
using Veterinaria.Data;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    public class MascotaController : Controller
    {
        private readonly IRepositorioMascota _repositorioMascota;
        private readonly VeterinariaContext _context;

        public MascotaController(
            IRepositorioMascota repositorioMascota,
            VeterinariaContext context)
        {
            _repositorioMascota = repositorioMascota;
            _context = context;
        }

        public IActionResult Index()
        {
            var mascotas = _repositorioMascota.ObtenerTodos();
            return View(mascotas);
        }

        public IActionResult Details(int id)
        {
            var mascota = _repositorioMascota.ObtenerPorId(id);

            if (mascota == null)
            {
                return NotFound();
            }

            return View(mascota);
        }

        public IActionResult Create()
        {
            CargarListas();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Mascota mascota)
        {
            if (ModelState.IsValid)
            {
                _repositorioMascota.Crear(mascota);
                return RedirectToAction(nameof(Index));
            }

            CargarListas(mascota);
            return View(mascota);
        }

        public IActionResult Edit(int id)
        {
            var mascota = _repositorioMascota.ObtenerPorId(id);

            if (mascota == null)
            {
                return NotFound();
            }

            CargarListas(mascota);
            return View(mascota);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Mascota mascota)
        {
            if (id != mascota.IdMascota)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _repositorioMascota.Editar(mascota);
                return RedirectToAction(nameof(Index));
            }

            CargarListas(mascota);
            return View(mascota);
        }

        public IActionResult Delete(int id)
        {
            var mascota = _repositorioMascota.ObtenerPorId(id);

            if (mascota == null)
            {
                return NotFound();
            }

            return View(mascota);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioMascota.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }

        private void CargarListas(Mascota? mascota = null)
        {
            ViewBag.Clientes = new SelectList(
                _context.Clientes.Where(c => c.Activo),
                "IdCliente",
                "Nombre",
                mascota?.IdCliente);

            ViewBag.Especies = new SelectList(
                _context.Especies.Where(e => e.Activo),
                "IdEspecie",
                "Nombre",
                mascota?.IdEspecie);

            ViewBag.Razas = new SelectList(
                _context.Razas.Where(r => r.Activo),
                "IdRaza",
                "Nombre",
                mascota?.IdRaza);
        }
    }
}