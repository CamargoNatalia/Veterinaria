
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador,Empleado")]
    public class ConsultaController : Controller
    {
        private readonly IRepositorioConsulta _repositorioConsulta;
        private readonly IRepositorioMascota _repositorioMascota;
        private readonly IRepositorioTurno _repositorioTurno;
        private readonly VeterinariaContext _context;

        public ConsultaController(
            IRepositorioConsulta repositorioConsulta,
            IRepositorioMascota repositorioMascota,
            IRepositorioTurno repositorioTurno,
            VeterinariaContext context)
        {
            _repositorioConsulta = repositorioConsulta;
            _repositorioMascota = repositorioMascota;
            _repositorioTurno = repositorioTurno;
            _context = context;
        }

        // GET: Consulta
        public IActionResult Index()
        {
            var consultas = _repositorioConsulta.ObtenerTodos();

            ViewBag.TurnosPendientes =
                _repositorioTurno.ObtenerPendientes();

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

        // Datos de mascota y propietario para AJAX
        [HttpGet]
        public IActionResult ObtenerDatosMascota(int id)
        {
            var mascota = _context.Mascotas
                .AsNoTracking()
                .Include(m => m.Especie)
                .Include(m => m.Raza)
                .Include(m => m.Cliente)
                .FirstOrDefault(m => m.IdMascota == id);

            if (mascota == null)
                return NotFound();

            return Json(new
            {
                mascota.IdMascota,
                mascota.Nombre,
                Especie = mascota.Especie?.Nombre,
                Raza = mascota.Raza?.Nombre,
                mascota.Sexo,
                mascota.FechaNacimiento,
                mascota.Peso,
                mascota.Observaciones,

                Propietario = mascota.Cliente == null
                    ? null
                    : new
                    {
                        NombreCompleto =
                            $"{mascota.Cliente.Nombre} {mascota.Cliente.Apellido}",
                        mascota.Cliente.Dni,
                        mascota.Cliente.Telefono,
                        mascota.Cliente.Correo,
                        mascota.Cliente.Direccion
                    }
            });
        }

        // GET: Consulta/Create
        public IActionResult Create(int? idTurno)
        {
            var consulta = new Consulta
            {
                FechaHora = DateTime.Now
            };

            if (idTurno.HasValue)
            {
                var turno = _repositorioTurno
                    .ObtenerPorId(idTurno.Value);

                if (turno == null)
                    return NotFound();

                if (turno.Estado != "Pendiente" ||
                    _repositorioConsulta.ExisteConsultaParaTurno(
                        turno.IdTurno))
                {
                    TempData["Error"] =
                        "El turno seleccionado ya no está disponible.";

                    return RedirectToAction(nameof(Index));
                }

                consulta.IdTurno = turno.IdTurno;
                consulta.IdMascota = turno.IdMascota;
                consulta.Motivo = turno.Motivo;

                ViewBag.TurnoSeleccionado = turno;
            }

            CargarMascotas(consulta.IdMascota);

            return View(consulta);
        }

        // POST: Consulta/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            [Bind("FechaHora,Motivo,Diagnostico,Tratamiento,Observaciones,IdMascota,IdTurno")]
            Consulta consulta)
        {
            var idClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idUsuario))
                return Unauthorized();

            consulta.IdUsuario = idUsuario;

            ModelState.Remove(nameof(Consulta.IdUsuario));
            ModelState.Remove(nameof(Consulta.Usuario));
            ModelState.Remove(nameof(Consulta.Mascota));
            ModelState.Remove(nameof(Consulta.Turno));

            var mascota = _repositorioMascota
                .ObtenerPorId(consulta.IdMascota);

            if (mascota == null)
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdMascota),
                    "Seleccione una mascota válida.");
            }

            if (consulta.IdTurno.HasValue)
            {
                var turno = _repositorioTurno
                    .ObtenerPorId(consulta.IdTurno.Value);

                if (turno == null)
                {
                    ModelState.AddModelError(
                        nameof(Consulta.IdTurno),
                        "El turno seleccionado no existe.");
                }
                else
                {
                    ViewBag.TurnoSeleccionado = turno;

                    if (turno.Estado != "Pendiente" ||
                        _repositorioConsulta.ExisteConsultaParaTurno(
                            turno.IdTurno))
                    {
                        ModelState.AddModelError(
                            nameof(Consulta.IdTurno),
                            "El turno ya fue atendido o no está disponible.");
                    }

                    if (turno.IdMascota != consulta.IdMascota)
                    {
                        ModelState.AddModelError(
                            nameof(Consulta.IdMascota),
                            "La mascota no corresponde al turno seleccionado.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                if (consulta.IdTurno.HasValue)
                {
                    bool registrado =
                        _repositorioConsulta.RegistrarAtencion(
                            consulta,
                            consulta.IdTurno.Value);

                    if (registrado)
                    {
                        TempData["Mensaje"] =
                            "Consulta registrada. El turno fue marcado como atendido.";

                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(
                        "",
                        "No se pudo registrar la atención. El turno puede haber sido atendido por otro usuario.");
                }
                else
                {
                    _repositorioConsulta.Crear(consulta);

                    TempData["Mensaje"] =
                        "Consulta registrada correctamente.";

                    return RedirectToAction(nameof(Index));
                }
            }

            CargarMascotas(consulta.IdMascota);

            return View(consulta);
        }

        // GET: Consulta/Edit
        public IActionResult Edit(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            CargarMascotas(consulta.IdMascota);

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

            if (_repositorioMascota.ObtenerPorId(datos.IdMascota)
                == null)
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdMascota),
                    "Seleccione una mascota válida.");
            }

            if (consulta.IdTurno.HasValue &&
                consulta.IdMascota != datos.IdMascota)
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdMascota),
                    "No se puede cambiar la mascota de una consulta asociada a un turno.");
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

                TempData["Mensaje"] =
                    "Consulta actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            CargarMascotas(datos.IdMascota);

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

            if (consulta.IdTurno.HasValue)
            {
                TempData["Error"] =
                    "No se puede eliminar una consulta asociada a un turno atendido.";

                return RedirectToAction(nameof(Index));
            }

            if (consulta.HistoriasClinicas.Any())
            {
                TempData["Error"] =
                    "No se puede eliminar una consulta con archivos clínicos asociados.";

                return RedirectToAction(nameof(Index));
            }

            _repositorioConsulta.Eliminar(id);

            TempData["Mensaje"] =
                "Consulta eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // Carga el desplegable de mascotas
        private void CargarMascotas(int? idMascota = null)
        {
            ViewBag.Mascotas = new SelectList(
                _repositorioMascota.ObtenerTodos(),
                "IdMascota",
                "Nombre",
                idMascota);
        }
    }
}
