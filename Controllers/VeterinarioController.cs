
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador,Empleado")]
    public class VeterinarioController : Controller
    {
        private readonly IRepositorioVeterinario _repositorioVeterinario;
        private readonly IRepositorioUsuario _repositorioUsuario;

        public VeterinarioController(
            IRepositorioVeterinario repositorioVeterinario,
            IRepositorioUsuario repositorioUsuario)
        {
            _repositorioVeterinario = repositorioVeterinario;
            _repositorioUsuario = repositorioUsuario;
        }

        // Listado paginado
        public IActionResult Index(int pagina = 1)
        {
            const int cantidadPorPagina = 3;

            pagina = Math.Max(1, pagina);

            var resultado = _repositorioVeterinario
                .ObtenerPaginados(pagina, cantidadPorPagina);

            int totalPaginas = (int)Math.Ceiling(
                (double)resultado.Total / cantidadPorPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;

                resultado = _repositorioVeterinario
                    .ObtenerPaginados(pagina, cantidadPorPagina);
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = resultado.Total;

            return View(resultado.Veterinarios);
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
            CargarUsuarios();
            return View(new Veterinario());
        }

        // POST: Create
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            [Bind("IdUsuario,Matricula,Especialidad,Telefono")]
            Veterinario veterinario)
        {
            NormalizarDatos(veterinario);

            ModelState.Remove(nameof(Veterinario.Usuario));
            ValidarDatos(veterinario);
            ValidarUsuario(veterinario.IdUsuario);

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
                    ModelState.AddModelError("",
                        "No se pudo registrar el veterinario. " +
                        "Verifique la matrícula y el usuario.");
                }
            }

            CargarUsuarios();
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
            [Bind("IdVeterinario,Matricula,Especialidad,Telefono")]
            Veterinario datos)
        {
            if (id != datos.IdVeterinario)
                return NotFound();

            var veterinario = _repositorioVeterinario.ObtenerPorId(id);

            if (veterinario == null)
                return NotFound();

            NormalizarDatos(datos);

            datos.IdUsuario = veterinario.IdUsuario;

            ModelState.Remove(nameof(Veterinario.Usuario));
            ValidarDatos(datos, id);

            if (ModelState.IsValid)
            {
                try
                {
                    _repositorioVeterinario.Editar(datos);

                    TempData["Mensaje"] =
                        "Veterinario actualizado correctamente.";

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("",
                        "No se pudieron guardar los cambios.");
                }
            }

            datos.Usuario = veterinario.Usuario;

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

            if (veterinario.Usuario?.Activo != true)
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

        // Normalizar datos profesionales
        private static void NormalizarDatos(Veterinario veterinario)
        {
            veterinario.Matricula =
                (veterinario.Matricula ?? "").Trim();

            veterinario.Especialidad =
                (veterinario.Especialidad ?? "").Trim();

            veterinario.Telefono =
                (veterinario.Telefono ?? "").Trim();
        }

        // Validaciones profesionales
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

        // Validar usuario asociado
        private void ValidarUsuario(int idUsuario)
        {
            var usuario = _repositorioUsuario.ObtenerPorId(idUsuario);

            if (usuario == null ||
                !usuario.Activo ||
                usuario.Rol != "Veterinario")
            {
                ModelState.AddModelError(
                    nameof(Veterinario.IdUsuario),
                    "Seleccione un usuario activo con rol Veterinario.");

                return;
            }

            bool asociado = _repositorioVeterinario
                .ObtenerTodos()
                .Any(v => v.IdUsuario == idUsuario);

            if (asociado)
            {
                ModelState.AddModelError(
                    nameof(Veterinario.IdUsuario),
                    "Este usuario ya tiene una ficha de veterinario.");
            }
        }

        // Usuarios disponibles para asociar
        private void CargarUsuarios()
        {
            var asociados = _repositorioVeterinario
                .ObtenerTodos()
                .Select(v => v.IdUsuario)
                .ToHashSet();

            var usuarios = _repositorioUsuario
                .ObtenerTodos()
                .Where(u => u.Activo &&
                            u.Rol == "Veterinario" &&
                            !asociados.Contains(u.IdUsuario))
                .OrderBy(u => u.Nombre)
                .ToList();

            ViewBag.Usuarios = new SelectList(
                usuarios,
                "IdUsuario",
                "Nombre");
        }
    }
}
