
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

        // LISTADO PAGINADO
        public IActionResult Index(int pagina = 1)
        {
            const int cantidadPorPagina = 3;

            pagina = Math.Max(1, pagina);

            var resultado = _repositorioTurno.ObtenerPaginados(
                pagina, cantidadPorPagina);

            int totalPaginas = (int)Math.Ceiling(
                (double)resultado.Total / cantidadPorPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;

                resultado = _repositorioTurno.ObtenerPaginados(
                    pagina, cantidadPorPagina);
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = resultado.Total;

            return View(resultado.Turnos);
        }

        // BUSCAR MASCOTAS MEDIANTE AJAX
        [HttpGet]
        public async Task<IActionResult> BuscarMascotas(string? termino)
        {
            termino = (termino ?? "").Trim();

            if (termino.Length < 2)
                return Json(Array.Empty<object>());

            if (termino.Length > 100)
                termino = termino[..100];

            var mascotas = await _context.Mascotas
                .AsNoTracking()
                .Where(m => m.Activo &&
                    (m.Nombre.Contains(termino) ||
                     m.Cliente.Nombre.Contains(termino) ||
                     m.Cliente.Apellido.Contains(termino)))
                .OrderBy(m => m.Nombre)
                .ThenBy(m => m.IdMascota)
                .Take(10)
                .Select(m => new
                {
                    idMascota = m.IdMascota,
                    nombre = m.Nombre,
                    propietario = m.Cliente.Nombre + " " +
                                  m.Cliente.Apellido,
                    especie = m.Especie.Nombre,
                    raza = m.Raza.Nombre
                })
                .ToListAsync();

            return Json(mascotas);
        }

        // DETALLES
        public IActionResult Details(int id)
        {
            var turno = _repositorioTurno.ObtenerPorId(id);

            if (turno == null)
                return NotFound();

            return View(turno);
        }

        // CREAR - GET
        [HttpGet]
        public IActionResult Create(int? idMascota)
        {
            var turno = new Turno
            {
                Estado = "Pendiente"
            };

            if (idMascota.HasValue)
                turno.IdMascota = idMascota.Value;

            CargarMascotaSeleccionada(turno);

            return View(turno);
        }

        // CREAR - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            [Bind("FechaHora,Motivo,Observaciones,IdMascota")]
            Turno turno)
        {
            var idClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idUsuario))
                return Unauthorized();

            turno.IdUsuario = idUsuario;
            turno.Estado = "Pendiente";

            ModelState.Remove(nameof(Turno.IdUsuario));
            ModelState.Remove(nameof(Turno.Usuario));
            ModelState.Remove(nameof(Turno.Mascota));

            if (!_context.Mascotas.Any(m =>
                    m.IdMascota == turno.IdMascota &&
                    m.Activo))
            {
                ModelState.AddModelError(
                    nameof(Turno.IdMascota),
                    "Seleccione una mascota activa.");
            }

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

            CargarMascotaSeleccionada(turno);
            return View(turno);
        }

        // EDITAR - GET
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

            CargarMascotaSeleccionada(turno);
            return View(turno);
        }

        // EDITAR - POST
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

            datos.IdUsuario = turno.IdUsuario;
            datos.Estado = turno.Estado;

            ModelState.Remove(nameof(Turno.IdUsuario));
            ModelState.Remove(nameof(Turno.Usuario));
            ModelState.Remove(nameof(Turno.Mascota));

            if (!_context.Mascotas.Any(m =>
                    m.IdMascota == datos.IdMascota &&
                    m.Activo))
            {
                ModelState.AddModelError(
                    nameof(Turno.IdMascota),
                    "Seleccione una mascota activa.");
            }

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

            CargarMascotaSeleccionada(datos);
            return View(datos);
        }

        // ELIMINAR - GET
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

        // ELIMINAR - POST
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

        // OBTENER SOLO LA MASCOTA SELECCIONADA
        private void CargarMascotaSeleccionada(Turno? turno = null)
        {
            ViewBag.MascotaSeleccionada = "";

            if (turno == null || turno.IdMascota <= 0)
                return;

            var mascota = _context.Mascotas
                .AsNoTracking()
                .Where(m => m.IdMascota == turno.IdMascota)
                .Select(m => new
                {
                    m.Nombre,
                    Propietario = m.Cliente.Nombre + " " +
                                  m.Cliente.Apellido,
                    Especie = m.Especie.Nombre,
                    Raza = m.Raza.Nombre
                })
                .FirstOrDefault();

            if (mascota != null)
            {
                ViewBag.MascotaSeleccionada =
                    $"{mascota.Nombre} - {mascota.Propietario} - " +
                    $"{mascota.Especie} - {mascota.Raza}";
            }
        }
    }
}
