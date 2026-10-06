using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    public class TurnoController : Controller
    {
        private readonly IRepositorioTurno _repositorioTurno;
        private readonly VeterinariaContext _context;

        public TurnoController(
            IRepositorioTurno repositorioTurno,
            VeterinariaContext context)
        {
            _repositorioTurno = repositorioTurno;
            _context = context;
        }

        public IActionResult Index()
        {
            var turnos = _repositorioTurno.ObtenerTodos();

            return View(turnos);
        }

        public IActionResult Details(int id)
        {
            var turno = _repositorioTurno.ObtenerPorId(id);

            if (turno == null)
            {
                return NotFound();
            }

            return View(turno);
        }

        public IActionResult Create()
        {
            CargarListas();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Turno turno)
        {
            if (ModelState.IsValid)
            {
                _repositorioTurno.Crear(turno);

                return RedirectToAction(nameof(Index));
            }

            CargarListas(turno);

            return View(turno);
        }

        public IActionResult Edit(int id)
        {
            var turno = _repositorioTurno.ObtenerPorId(id);

            if (turno == null)
            {
                return NotFound();
            }

            CargarListas(turno);

            return View(turno);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Turno turno)
        {
            if (id != turno.IdTurno)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _repositorioTurno.Editar(turno);

                return RedirectToAction(nameof(Index));
            }

            CargarListas(turno);

            return View(turno);
        }

        public IActionResult Delete(int id)
        {
            var turno = _repositorioTurno.ObtenerPorId(id);

            if (turno == null)
            {
                return NotFound();
            }

            return View(turno);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioTurno.Eliminar(id);

            return RedirectToAction(nameof(Index));
        }

        private void CargarListas(Turno? turno = null)
        {
            ViewBag.Mascotas = new SelectList(
                _context.Mascotas
                    .Where(m => m.Activo)
                    .Select(m => new
                    {
                        IdMascota = m.IdMascota,
                        Nombre = m.Nombre
                    }),
                "IdMascota",
                "Nombre",
                turno?.IdMascota);

            ViewBag.Usuarios = new SelectList(
                _context.Usuarios,
                "IdUsuario",
                "Nombre",
                turno?.IdUsuario);
        }
    }
}