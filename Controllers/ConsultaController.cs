
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador,Empleado")]
    public class ConsultaController : Controller
    {
        private readonly IRepositorioConsulta _repositorioConsulta;
        private readonly IRepositorioMascota _repositorioMascota;

        public ConsultaController(
            IRepositorioConsulta repositorioConsulta,
            IRepositorioMascota repositorioMascota)
        {
            _repositorioConsulta = repositorioConsulta;
            _repositorioMascota = repositorioMascota;
        }

        // GET: Consulta
        public IActionResult Index()
        {
            var consultas = _repositorioConsulta.ObtenerTodos();
            return View(consultas);
        }

        // GET: Consulta/Details
        public IActionResult Details(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            return View(consulta);
        }

        // GET: Consulta/Create
        public IActionResult Create()
        {
            CargarMascotas();

            return View(new Consulta
            {
                FechaHora = DateTime.Now
            });
        }

        // POST: Consulta/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            [Bind("FechaHora,Motivo,Diagnostico,Tratamiento,Observaciones,IdMascota")]
            Consulta consulta)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idUsuario))
                return Unauthorized();

            consulta.IdUsuario = idUsuario;

            ModelState.Remove(nameof(Consulta.IdUsuario));
            ModelState.Remove(nameof(Consulta.Usuario));
            ModelState.Remove(nameof(Consulta.Mascota));

            if (_repositorioMascota.ObtenerPorId(consulta.IdMascota) == null)
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdMascota),
                    "Seleccione una mascota válida."
                );
            }

            if (ModelState.IsValid)
            {
                _repositorioConsulta.Crear(consulta);
                return RedirectToAction(nameof(Index));
            }

            CargarMascotas();
            return View(consulta);
        }

        // GET: Consulta/Edit
        public IActionResult Edit(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            CargarMascotas();
            return View(consulta);
        }

        // POST: Consulta/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            [Bind("IdConsulta,FechaHora,Motivo,Diagnostico,Tratamiento,Observaciones,IdMascota")]
            Consulta datos)
        {
            if (id != datos.IdConsulta)
                return NotFound();

            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            ModelState.Remove(nameof(Consulta.IdUsuario));
            ModelState.Remove(nameof(Consulta.Usuario));
            ModelState.Remove(nameof(Consulta.Mascota));

            if (_repositorioMascota.ObtenerPorId(datos.IdMascota) == null)
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdMascota),
                    "Seleccione una mascota válida."
                );
            }

            if (ModelState.IsValid)
            {
                consulta.FechaHora = datos.FechaHora;
                consulta.Motivo = datos.Motivo;
                consulta.Diagnostico = datos.Diagnostico;
                consulta.Tratamiento = datos.Tratamiento;
                consulta.Observaciones = datos.Observaciones;
                consulta.IdMascota = datos.IdMascota;

                _repositorioConsulta.Editar(consulta);

                return RedirectToAction(nameof(Index));
            }

            CargarMascotas();
            return View(datos);
        }

        // GET: Consulta/Delete
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            return View(consulta);
        }

        // POST: Consulta/Delete
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            // Verificar archivos asociados
            if (consulta.HistoriasClinicas.Any())
            {
                TempData["Error"] =
                    "No se puede eliminar una consulta con archivos clínicos asociados.";

                return RedirectToAction(nameof(Index));
            }

            _repositorioConsulta.Eliminar(id);

            return RedirectToAction(nameof(Index));
        }

        // Mascotas
        private void CargarMascotas()
        {
            ViewBag.Mascotas = new SelectList(
                _repositorioMascota.ObtenerTodos(),
                "IdMascota",
                "Nombre"
            );
        }
    }
}
