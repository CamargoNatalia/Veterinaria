
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Data;
using Veterinaria.Interfaces;
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

        // LISTADO PAGINADO
        public IActionResult Index(int pagina = 1)
        {
            const int cantidadPorPagina = 3;

            pagina = Math.Max(1, pagina);

            var resultado = _repositorioMascota.ObtenerPaginadas(
                pagina, cantidadPorPagina);

            int totalPaginas = (int)Math.Ceiling(
                (double)resultado.Total / cantidadPorPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
                resultado = _repositorioMascota.ObtenerPaginadas(
                    pagina, cantidadPorPagina);
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = resultado.Total;

            return View(resultado.Mascotas);
        }

        // DETALLES
        public IActionResult Details(int id)
        {
            var mascota = _repositorioMascota.ObtenerPorId(id);

            if (mascota == null)
                return NotFound();

            return View(mascota);
        }

        // HISTORIA CLÍNICA PAGINADA
        [HttpGet]
        public async Task<IActionResult> HistoriaClinica(
            int id, int pagina = 1)
        {
            const int cantidadPorPagina = 3;

            pagina = Math.Max(1, pagina);

            var mascota = await _context.Mascotas
                .AsNoTracking()
                .Include(m => m.Cliente)
                .Include(m => m.Especie)
                .Include(m => m.Raza)
                .FirstOrDefaultAsync(m => m.IdMascota == id);

            if (mascota == null)
                return NotFound();

            var consultaBase = _context.Consultas
                .AsNoTracking()
                .Where(c => c.IdMascota == id);

            int totalConsultas = await consultaBase.CountAsync();

            int totalPaginas = (int)Math.Ceiling(
                (double)totalConsultas / cantidadPorPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
                pagina = totalPaginas;

            var consultas = await consultaBase
                .OrderByDescending(c => c.FechaHora)
                .ThenByDescending(c => c.IdConsulta)
                .Skip((pagina - 1) * cantidadPorPagina)
                .Take(cantidadPorPagina)
                .Include(c => c.Veterinario)
                    .ThenInclude(v => v!.Usuario)
                .Include(c => c.HistoriasClinicas)
                .AsSplitQuery()
                .ToListAsync();

            mascota.Consultas = consultas;

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalConsultas = totalConsultas;

            return View(mascota);
        }

        // BUSCAR CLIENTES POR AJAX
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<IActionResult> BuscarClientes(string? termino)
        {
            termino = (termino ?? "").Trim();

            if (termino.Length < 2)
                return Json(Array.Empty<object>());

            if (termino.Length > 100)
                termino = termino.Substring(0, 100);

            var clientes = await _context.Clientes
                .AsNoTracking()
                .Where(c => c.Activo &&
                    (c.Nombre.Contains(termino) ||
                     c.Apellido.Contains(termino) ||
                     (c.Dni != null && c.Dni.Contains(termino))))
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .Take(10)
                .Select(c => new
                {
                    idCliente = c.IdCliente,
                    nombre = c.Nombre + " " + c.Apellido,
                    dni = c.Dni
                })
                .ToListAsync();

            return Json(clientes);
        }

        // CREAR - GET
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Create()
        {
            CargarListas();
            return View();
        }

        // CREAR - POST
        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Mascota mascota)
        {
            mascota.Activo = true;

            ValidarCliente(mascota.IdCliente);

            ModelState.Remove(nameof(Mascota.Cliente));
            ModelState.Remove(nameof(Mascota.Especie));
            ModelState.Remove(nameof(Mascota.Raza));

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

        // OBTENER RAZAS POR ESPECIE
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

        // EDITAR - GET
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

        // EDITAR - POST
        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Mascota mascota)
        {
            if (id != mascota.IdMascota)
                return NotFound();

            var actual = _context.Mascotas
                .FirstOrDefault(m => m.IdMascota == id);

            if (actual == null)
                return NotFound();

            ValidarCliente(mascota.IdCliente);

            ModelState.Remove(nameof(Mascota.Cliente));
            ModelState.Remove(nameof(Mascota.Especie));
            ModelState.Remove(nameof(Mascota.Raza));

            if (ModelState.IsValid)
            {
                actual.Nombre = mascota.Nombre;
                actual.IdCliente = mascota.IdCliente;
                actual.IdEspecie = mascota.IdEspecie;
                actual.IdRaza = mascota.IdRaza;
                actual.FechaNacimiento = mascota.FechaNacimiento;
                actual.Sexo = mascota.Sexo;
                actual.Peso = mascota.Peso;
                actual.Observaciones = mascota.Observaciones;
                actual.Activo = mascota.Activo;

                _context.SaveChanges();

                TempData["Mensaje"] =
                    "Mascota actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            CargarListas(mascota);
            return View(mascota);
        }

        // ELIMINAR - GET
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Delete(int id)
        {
            var mascota = _repositorioMascota.ObtenerPorId(id);

            if (mascota == null)
                return NotFound();

            return View(mascota);
        }

        // ELIMINAR - POST
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

        // VALIDAR CLIENTE
        private void ValidarCliente(int idCliente)
        {
            bool existe = _context.Clientes.Any(c =>
                c.IdCliente == idCliente && c.Activo);

            if (!existe)
            {
                ModelState.AddModelError(
                    nameof(Mascota.IdCliente),
                    "Seleccione un cliente activo válido.");
            }
        }

        // LISTAS PARA FORMULARIOS
        private void CargarListas(Mascota? mascota = null)
        {
            // Solamente obtener el cliente seleccionado.
            // No se cargan todos los clientes.
            ViewBag.ClienteSeleccionado = null;

            if (mascota != null && mascota.IdCliente > 0)
            {
                var cliente = _context.Clientes
                    .AsNoTracking()
                    .Where(c => c.IdCliente == mascota.IdCliente)
                    .Select(c => new
                    {
                        c.IdCliente,
                        c.Nombre,
                        c.Apellido,
                        c.Dni
                    })
                    .FirstOrDefault();

                if (cliente != null)
                {
                    ViewBag.ClienteSeleccionado =
                        $"{cliente.Nombre} {cliente.Apellido}" +
                        (string.IsNullOrWhiteSpace(cliente.Dni)
                            ? ""
                            : $" - DNI: {cliente.Dni}");
                }
            }

            ViewBag.Especies = new SelectList(
                _context.Especies.Where(e => e.Activo).ToList(),
                "IdEspecie",
                "Nombre",
                mascota?.IdEspecie);

            if (mascota != null && mascota.IdEspecie > 0)
            {
                ViewBag.Razas = new SelectList(
                    _repositorioRaza.ObtenerPorEspecie(
                        mascota.IdEspecie),
                    "IdRaza",
                    "Nombre",
                    mascota.IdRaza);
            }
            else
            {
                ViewBag.Razas = new SelectList(
                    Enumerable.Empty<Raza>(),
                    "IdRaza",
                    "Nombre");
            }
        }
    }
}
