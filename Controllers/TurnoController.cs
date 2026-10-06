using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        // GET: Turno/Create
        public IActionResult Create()
        {
            CargarListas();

            return View();
        }

        // POST: Turno/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Turno turno)
        {
            if (ModelState.IsValid)
            {
                var turnoExistente = _context.Turnos
                    .Any(t => t.IdMascota == turno.IdMascota
                           && t.FechaHora == turno.FechaHora);

                if (turnoExistente)
                {
                    ModelState.AddModelError(
                        "FechaHora",
                        "La mascota ya tiene un turno registrado para esa fecha y hora."
                    );

                    CargarListas(turno);

                    return View(turno);
                }

                _repositorioTurno.Crear(turno);

                return RedirectToAction(nameof(Index));
            }

            CargarListas(turno);

            return View(turno);
        }

        // GET: Turno/Edit/5
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

        // POST: Turno/Edit/5
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

        // GET: Turno/Delete/5
        public IActionResult Delete(int id)
        {
            var turno = _repositorioTurno.ObtenerPorId(id);

            if (turno == null)
            {
                return NotFound();
            }

            return View(turno);
        }

        // POST: Turno/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioTurno.Eliminar(id);

            return RedirectToAction(nameof(Index));
        }

        // Cargar listas para Mascotas y Usuarios
        private void CargarListas(Turno? turno = null)
        {
            var mascotas = _context.Mascotas
                .Include(m => m.Especie)
                .Include(m => m.Raza)
                .Where(m => m.Activo)
                .ToList();

            ViewBag.Mascotas = new SelectList(
                mascotas.Select(m => new
                {
                    IdMascota = m.IdMascota,
                    Descripcion = $"{m.Nombre} - {m.Especie!.Nombre} - {m.Raza!.Nombre}"
                }),
                "IdMascota",
                "Descripcion",
                turno?.IdMascota);

            var usuarios = _context.Usuarios
                .Where(u => u.Activo)
                .ToList();

            ViewBag.Usuarios = new SelectList(
                usuarios.Select(u => new
                {
                    IdUsuario = u.IdUsuario,
                    Descripcion = $"{u.Nombre} - {u.Rol}"
                }),
                "IdUsuario",
                "Descripcion",
                turno?.IdUsuario);
        }
    }
}