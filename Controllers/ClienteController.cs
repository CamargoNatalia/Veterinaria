
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador,Empleado,Veterinario")]
    public class ClienteController : Controller
    {
        private readonly IRepositorioCliente _repositorio;

        public ClienteController(IRepositorioCliente repositorio)
        {
            _repositorio = repositorio;
        }

        // GET: Cliente
        public IActionResult Index()
        {
            return View();
        }

        // GET: Cliente/Buscar
        // Búsqueda AJAX con paginación
        [HttpGet]
        public IActionResult Buscar(
            string? busqueda,
            int pagina = 1)
        {
            const int cantidadPorPagina = 3;

            pagina = Math.Max(1, pagina);

            var resultado = _repositorio.BuscarPaginados(
                busqueda,
                pagina,
                cantidadPorPagina
            );

            int totalPaginas = (int)Math.Ceiling(
                (double)resultado.Total / cantidadPorPagina
            );

            // Evitar páginas fuera del rango
            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;

                resultado = _repositorio.BuscarPaginados(
                    busqueda,
                    pagina,
                    cantidadPorPagina
                );
            }

            return Json(new
            {
                clientes = resultado.Clientes.Select(c => new
                {
                    c.IdCliente,
                    c.Nombre,
                    c.Apellido,
                    c.Dni,
                    c.Telefono,
                    c.Correo,
                    c.Direccion
                }),
                paginaActual = pagina,
                totalPaginas,
                totalRegistros = resultado.Total
            });
        }

        // GET: Cliente/Details/5
        public IActionResult Details(int id)
        {
            var cliente = _repositorio.ObtenerPorId(id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // GET: Cliente/Create
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cliente/Create
        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                _repositorio.Crear(cliente);

                TempData["Mensaje"] =
                    "Cliente registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        // GET: Cliente/Edit/5
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Edit(int id)
        {
            var cliente = _repositorio.ObtenerPorId(id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // POST: Cliente/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrador,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Cliente cliente)
        {
            if (id != cliente.IdCliente)
                return NotFound();

            if (ModelState.IsValid)
            {
                _repositorio.Editar(cliente);

                TempData["Mensaje"] =
                    "Cliente actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        // GET: Cliente/Delete/5
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public IActionResult Delete(int id)
        {
            var cliente = _repositorio.ObtenerPorId(id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // POST: Cliente/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorio.Eliminar(id);

            TempData["Mensaje"] =
                "Cliente eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
