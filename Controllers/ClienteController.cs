using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    // Acceso general a los tres roles
    [Authorize(Roles = "Administrador,Empleado,Veterinario")]
    public class ClienteController : Controller
    {
        private readonly IRepositorioCliente _repositorio;

        public ClienteController(IRepositorioCliente repositorio)
        {
            _repositorio = repositorio;
        }

        // GET: Cliente
        // Los tres roles pueden consultar el listado
        public IActionResult Index()
        {
            var clientes = _repositorio.ObtenerTodos();

            return View(clientes);
        }

        // GET: Cliente/Details/5
        // Los tres roles pueden consultar los detalles
        public IActionResult Details(int id)
        {
            var cliente = _repositorio.ObtenerPorId(id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // GET: Cliente/Create
        // Solo Administrador y Empleado
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