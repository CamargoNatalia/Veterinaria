using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador,Empleado")]
    public class VeterinarioController : Controller
    {
        private readonly IRepositorioVeterinario _repositorioVeterinario;

        public VeterinarioController(
            IRepositorioVeterinario repositorioVeterinario)
        {
            _repositorioVeterinario = repositorioVeterinario;
        }

        // Listado
        public IActionResult Index()
        {
            var veterinarios = _repositorioVeterinario.ObtenerTodos();
            return View(veterinarios);
        }

        // Detalles
        public IActionResult Details(int id)
        {
            var veterinario = _repositorioVeterinario.ObtenerPorId(id);

            if (veterinario == null)
                return NotFound();

            return View(veterinario);
        }

        // GET: Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View(new Veterinario());
        }

        // POST: Create
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            [Bind("Nombre,Apellido,Matricula,Especialidad,Telefono,Correo")]
            Veterinario veterinario)
        {
            NormalizarDatos(veterinario);
            veterinario.Activo = true;

            ModelState.Clear();
            TryValidateModel(veterinario);

            ValidarDatos(veterinario);

            if (ModelState.IsValid)
            {
                try
                {
                    _repositorioVeterinario.Crear(veterinario);

                    TempData["Mensaje"] =
                        "Veterinario registrado correctamente.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(
                        "",
                        "No se pudo registrar el veterinario. " +
                        "Verifique que la matrícula no esté duplicada.");
                }
            }

            return View(veterinario);
        }

        // GET: Edit
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id)
        {
            var veterinario = _repositorioVeterinario.ObtenerPorId(id);

            if (veterinario == null)
                return NotFound();

            return View(veterinario);
        }

        // POST: Edit
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            [Bind("IdVeterinario,Nombre,Apellido,Matricula,Especialidad,Telefono,Correo,Activo")]
            Veterinario datos)
        {
            if (id != datos.IdVeterinario)
                return NotFound();

            var veterinario = _repositorioVeterinario.ObtenerPorId(id);

            if (veterinario == null)
                return NotFound();

            NormalizarDatos(datos);

            ModelState.Clear();
            TryValidateModel(datos);

            ValidarDatos(datos, id);

            if (ModelState.IsValid)
            {
                veterinario.Nombre = datos.Nombre;
                veterinario.Apellido = datos.Apellido;
                veterinario.Matricula = datos.Matricula;
                veterinario.Especialidad = datos.Especialidad;
                veterinario.Telefono = datos.Telefono;
                veterinario.Correo = datos.Correo;
                veterinario.Activo = datos.Activo;

                try
                {
                    _repositorioVeterinario.Editar(veterinario);

                    TempData["Mensaje"] =
                        "Veterinario actualizado correctamente.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(
                        "",
                        "No se pudieron guardar los cambios. " +
                        "Verifique los datos y la matrícula.");
                }
            }

            return View(datos);
        }

        // GET: Delete
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var veterinario = _repositorioVeterinario.ObtenerPorId(id);

            if (veterinario == null)
                return NotFound();

            return View(veterinario);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var veterinario = _repositorioVeterinario.ObtenerPorId(id);

            if (veterinario == null)
                return NotFound();

            if (!veterinario.Activo)
            {
                TempData["Error"] =
                    "El veterinario ya está inactivo.";

                return RedirectToAction(nameof(Index));
            }

            _repositorioVeterinario.Eliminar(id);

            TempData["Mensaje"] =
                "Veterinario desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // Limpia espacios innecesarios
        private static void NormalizarDatos(Veterinario veterinario)
        {
            veterinario.Nombre = (veterinario.Nombre ?? "").Trim();
            veterinario.Apellido = (veterinario.Apellido ?? "").Trim();
            veterinario.Matricula = (veterinario.Matricula ?? "").Trim();
            veterinario.Especialidad = (veterinario.Especialidad ?? "").Trim();
            veterinario.Telefono = (veterinario.Telefono ?? "").Trim();
            veterinario.Correo = (veterinario.Correo ?? "").Trim();
        }

        // Reglas adicionales de validación
        private void ValidarDatos(
            Veterinario veterinario,
            int? idExcluir = null)
        {
            if (!string.IsNullOrWhiteSpace(veterinario.Matricula) &&
                _repositorioVeterinario.ExisteMatricula(
                    veterinario.Matricula, idExcluir))
            {
                ModelState.AddModelError(
                    nameof(Veterinario.Matricula),
                    "La matrícula ya pertenece a otro veterinario.");
            }

            if (!string.IsNullOrWhiteSpace(veterinario.Telefono) &&
                veterinario.Telefono.Count(char.IsDigit) < 7)
            {
                ModelState.AddModelError(
                    nameof(Veterinario.Telefono),
                    "El teléfono debe contener al menos 7 dígitos.");
            }
        }
    }
}