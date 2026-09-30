using Microsoft.AspNetCore.Mvc;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
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
            var clientes = _repositorio.ObtenerTodos();
            return View(clientes);
        }

        // GET: Cliente/Details/5
        public IActionResult Details(int id)
        {
            var cliente = _repositorio.ObtenerPorId(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Cliente/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cliente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                _repositorio.Crear(cliente);
                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        // GET: Cliente/Edit/5
        public IActionResult Edit(int id)
        {
            var cliente = _repositorio.ObtenerPorId(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Cliente/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Cliente cliente)
        {
            if (id != cliente.IdCliente)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _repositorio.Editar(cliente);
                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        // GET: Cliente/Delete/5
        public IActionResult Delete(int id)
        {
            var cliente = _repositorio.ObtenerPorId(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Cliente/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorio.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }
    }
}