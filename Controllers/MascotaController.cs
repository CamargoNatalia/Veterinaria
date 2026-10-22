using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Veterinaria.Interfaces;
using Veterinaria.Data;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador,Empleado,Veterinario")]
    public class MascotaController : Controller
    {
        private readonly IRepositorioMascota _repositorioMascota;
        private readonly IRepositorioRaza _repositorioRaza;
        private readonly VeterinariaContext _context;

        public MascotaController(
            IRepositorioMascota repositorioMascota,
            IRepositorioRaza repositorioRaza,
            VeterinariaContext context)
        {
            _repositorioMascota = repositorioMascota;
            _repositorioRaza = repositorioRaza;
            _context = context;
        }

        // Listado de mascotas
        // Acceso: Administrador, Empleado y Veterinario
        public IActionResult Index()
        {
            var mascotas = _repositorioMascota.ObtenerTodos();

            return View(mascotas);
        }

        // Detalles de una mascota
        // Acceso: Administrador, Empleado y Veterinario
        public IActionResult Details(int id)
        {
            var mascota = _repositorioMascota.ObtenerPorId(id);

            if (mascota == null)
                return NotFound();

            return View(mascota);
        }

        // GET: Crear mascota
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Create()
        {
            CargarListas();

            return View();
        }

        // POST: Crear mascota
        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Mascota mascota)
        {
            mascota.Activo = true;

            if (ModelState.IsValid)
            {
                _repositorioMascota.Crear(mascota);

                TempData["Mensaje"] =
                    "Mascota registrada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            CargarListas(mascota);

            return View(mascota);
        }

        // Obtener razas según especie
        // Utilizado por los formularios de creación y edición
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult ObtenerRazasPorEspecie(int idEspecie)
        {
            var razas = _repositorioRaza
                .ObtenerPorEspecie(idEspecie)
                .Select(r => new
                {
                    idRaza = r.IdRaza,
                    nombre = r.Nombre
                })
                .ToList();

            return Json(razas);
        }

        // GET: Editar mascota
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Edit(int id)
        {
            var mascota = _repositorioMascota.ObtenerPorId(id);

            if (mascota == null)
                return NotFound();

            CargarListas(mascota);

            return View(mascota);
        }

        // POST: Editar mascota
        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Mascota mascota)
        {
            if (id != mascota.IdMascota)
                return NotFound();

            if (ModelState.IsValid)
            {
                _repositorioMascota.Editar(mascota);

                TempData["Mensaje"] =
                    "Mascota actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            CargarListas(mascota);

            return View(mascota);
        }

        // GET: Eliminar mascota
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Delete(int id)
        {
            var mascota = _repositorioMascota.ObtenerPorId(id);

            if (mascota == null)
                return NotFound();

            return View(mascota);
        }

        // POST: Eliminar mascota
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioMascota.Eliminar(id);

            TempData["Mensaje"] =
                "Mascota eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // Cargar listas para los formularios
        private void CargarListas(Mascota? mascota = null)
        {
            // Propietarios activos
            ViewBag.Clientes = new SelectList(
                _context.Clientes
                    .Where(c => c.Activo)
                    .Select(c => new
                    {
                        IdCliente = c.IdCliente,
                        NombreCompleto =
                            c.Nombre + " " + c.Apellido
                    }),
                "IdCliente",
                "NombreCompleto",
                mascota?.IdCliente
            );

            // Especies activas
            ViewBag.Especies = new SelectList(
                _context.Especies
                    .Where(e => e.Activo),
                "IdEspecie",
                "Nombre",
                mascota?.IdEspecie
            );

            // Razas de la especie seleccionada
            if (mascota != null && mascota.IdEspecie > 0)
            {
                var razas = _repositorioRaza
                    .ObtenerPorEspecie(mascota.IdEspecie);

                ViewBag.Razas = new SelectList(
                    razas,
                    "IdRaza",
                    "Nombre",
                    mascota.IdRaza
                );
            }
            else
            {
                ViewBag.Razas = new SelectList(
                    Enumerable.Empty<Raza>(),
                    "IdRaza",
                    "Nombre"
                );
            }
        }
    }
}