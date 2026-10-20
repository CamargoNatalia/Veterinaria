using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador,Empleado,Veterinario")]
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

        // Listado de turnos
        public IActionResult Index()
        {
            var turnos = _repositorioTurno.ObtenerTodos();

            return View(turnos);
        }

        // Detalles de un turno
        public IActionResult Details(int id)
        {
            var turno = _repositorioTurno.ObtenerPorId(id);

            if (turno == null)
                return NotFound();

            return View(turno);
        }

        // GET: Crear turno
        [HttpGet]
        public IActionResult Create(int? idMascota)
        {
            var turno = new Turno
            {
                Estado = "Pendiente"
            };

            if (idMascota.HasValue)
                turno.IdMascota = idMascota.Value;

            CargarListas(turno);

            return View(turno);
        }

        // POST: Crear turno
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            [Bind("FechaHora,Motivo,Observaciones,IdMascota")]
            Turno turno)
        {
            // Identificar al usuario autenticado
            var idClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idUsuario))
                return Unauthorized();

            // Asignar automáticamente el usuario
            turno.IdUsuario = idUsuario;

            // Estado inicial
            turno.Estado = "Pendiente";

            // Las propiedades de navegación no llegan
            // desde el formulario
            ModelState.Remove(nameof(Turno.IdUsuario));
            ModelState.Remove(nameof(Turno.Usuario));
            ModelState.Remove(nameof(Turno.Mascota));

            // Verificar mascota activa
            if (!_context.Mascotas.Any(m =>
                    m.IdMascota == turno.IdMascota &&
                    m.Activo))
            {
                ModelState.AddModelError(
                    nameof(Turno.IdMascota),
                    "Seleccione una mascota activa.");
            }

            // Verificar duplicación de turnos
            if (_context.Turnos.Any(t =>
                    t.IdMascota == turno.IdMascota &&
                    t.FechaHora == turno.FechaHora))
            {
                ModelState.AddModelError(
                    nameof(Turno.FechaHora),
                    "La mascota ya tiene un turno registrado para esa fecha y hora.");
            }

            if (ModelState.IsValid)
            {
                _repositorioTurno.Crear(turno);

                TempData["Mensaje"] =
                    "Turno registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            CargarListas(turno);

            return View(turno);
        }

        // GET: Editar turno
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var turno = _repositorioTurno.ObtenerPorId(id);

            if (turno == null)
                return NotFound();

            if (turno.Estado == "Atendido")
            {
                TempData["Error"] =
                    "No se puede editar un turno que ya fue atendido.";

                return RedirectToAction(nameof(Index));
            }

            CargarListas(turno);

            return View(turno);
        }

        // POST: Editar turno
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            [Bind("IdTurno,FechaHora,Motivo,Observaciones,IdMascota")]
            Turno datos)
        {
            if (id != datos.IdTurno)
                return NotFound();

            var turno = _repositorioTurno.ObtenerPorId(id);

            if (turno == null)
                return NotFound();

            if (turno.Estado == "Atendido")
            {
                TempData["Error"] =
                    "No se puede modificar un turno atendido.";

                return RedirectToAction(nameof(Index));
            }

            // Conservar usuario y estado originales
            datos.IdUsuario = turno.IdUsuario;
            datos.Estado = turno.Estado;

            ModelState.Remove(nameof(Turno.IdUsuario));
            ModelState.Remove(nameof(Turno.Usuario));
            ModelState.Remove(nameof(Turno.Mascota));

            // Validar mascota
            if (!_context.Mascotas.Any(m =>
                    m.IdMascota == datos.IdMascota &&
                    m.Activo))
            {
                ModelState.AddModelError(
                    nameof(Turno.IdMascota),
                    "Seleccione una mascota activa.");
            }

            // Evitar turnos duplicados
            if (_context.Turnos.Any(t =>
                    t.IdTurno != id &&
                    t.IdMascota == datos.IdMascota &&
                    t.FechaHora == datos.FechaHora))
            {
                ModelState.AddModelError(
                    nameof(Turno.FechaHora),
                    "La mascota ya tiene otro turno en esa fecha y hora.");
            }

            if (ModelState.IsValid)
            {
                turno.FechaHora = datos.FechaHora;
                turno.Motivo = datos.Motivo;
                turno.Observaciones = datos.Observaciones;
                turno.IdMascota = datos.IdMascota;

                _repositorioTurno.Editar(turno);

                TempData["Mensaje"] =
                    "Turno actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            CargarListas(datos);

            return View(datos);
        }

        // GET: Eliminar turno
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Delete(int id)
        {
            var turno = _repositorioTurno.ObtenerPorId(id);

            if (turno == null)
                return NotFound();

            if (turno.Estado == "Atendido" ||
                _context.Consultas.Any(c => c.IdTurno == id))
            {
                TempData["Error"] =
                    "No se puede eliminar un turno atendido o con una consulta asociada.";

                return RedirectToAction(nameof(Index));
            }

            return View(turno);
        }

        // POST: Eliminar turno
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var turno = _repositorioTurno.ObtenerPorId(id);

            if (turno == null)
                return NotFound();

            if (turno.Estado == "Atendido" ||
                _context.Consultas.Any(c => c.IdTurno == id))
            {
                TempData["Error"] =
                    "No se puede eliminar un turno atendido o con una consulta asociada.";

                return RedirectToAction(nameof(Index));
            }

            _repositorioTurno.Eliminar(id);

            TempData["Mensaje"] =
                "Turno eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // Listas necesarias para los formularios
        private void CargarListas(Turno? turno = null)
        {
            // Mascotas activas o la mascota ya asociada
            var mascotas = _context.Mascotas
                .Include(m => m.Especie)
                .Include(m => m.Raza)
                .Where(m => m.Activo ||
                    (turno != null &&
                     m.IdMascota == turno.IdMascota))
                .ToList();

            ViewBag.Mascotas = new SelectList(
                mascotas.Select(m => new
                {
                    m.IdMascota,
                    Descripcion =
                        $"{m.Nombre} - " +
                        $"{m.Especie?.Nombre ?? "Sin especie"} - " +
                        $"{m.Raza?.Nombre ?? "Sin raza"}"
                }),
                "IdMascota",
                "Descripcion",
                turno?.IdMascota);

            // Compatibilidad temporal con vistas anteriores
            // No se utilizará para asignar el usuario
            var usuarios = _context.Usuarios
                .Where(u => u.Activo)
                .ToList();

            ViewBag.Usuarios = new SelectList(
                usuarios.Select(u => new
                {
                    u.IdUsuario,
                    Descripcion = $"{u.Nombre} - {u.Rol}"
                }),
                "IdUsuario",
                "Descripcion",
                turno?.IdUsuario);
        }
    }
}