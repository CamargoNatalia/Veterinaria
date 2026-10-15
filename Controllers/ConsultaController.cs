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

        public IActionResult Index()
        {
            var consultas = _repositorioConsulta.ObtenerTodos();

            ViewBag.TurnosPendientes =
                _repositorioTurno.ObtenerPendientes();

            return View(consultas);
        }

        public IActionResult Details(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            return View(consulta);
        }

        // Datos de mascota y propietario para el formulario
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

        // Formulario de nueva consulta
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

            CargarListas(
                consulta.IdMascota,
                consulta.IdVeterinario);

            return View(consulta);
        }

        // Guardar consulta: normal o con atajo a nuevo turno
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            [Bind("FechaHora,Motivo,Diagnostico,Tratamiento,Observaciones,IdMascota,IdTurno,IdVeterinario")]
            Consulta consulta,
            string? accion)
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
            ModelState.Remove(nameof(Consulta.Veterinario));

            if (_repositorioMascota.ObtenerPorId(
                    consulta.IdMascota) == null)
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdMascota),
                    "Seleccione una mascota válida.");
            }

            if (!consulta.IdVeterinario.HasValue ||
                !_context.Veterinarios.Any(v =>
                    v.IdVeterinario ==
                        consulta.IdVeterinario.Value &&
                    v.Activo))
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdVeterinario),
                    "Seleccione un veterinario activo.");
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
                bool guardado = false;

                if (consulta.IdTurno.HasValue)
                {
                    guardado =
                        _repositorioConsulta.RegistrarAtencion(
                            consulta,
                            consulta.IdTurno.Value);
                }
                else
                {
                    _repositorioConsulta.Crear(consulta);
                    guardado = true;
                }

                if (guardado)
                {
                    // Segundo botón: guardar y abrir nuevo turno
                    if (accion == "reservarTurno")
                    {
                        TempData["Mensaje"] =
                            "Consulta guardada. Ahora puede reservar el próximo turno.";

                        return RedirectToAction(
                            "Create",
                            "Turno",
                            new { idMascota = consulta.IdMascota });
                    }

                    TempData["Mensaje"] =
                        "Consulta registrada correctamente.";

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(
                    "",
                    "No se pudo registrar la atención. El turno ya no está disponible.");
            }

            CargarListas(
                consulta.IdMascota,
                consulta.IdVeterinario);

            return View(consulta);
        }

        public IActionResult Edit(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            CargarListas(
                consulta.IdMascota,
                consulta.IdVeterinario);

            return View(consulta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            [Bind("IdConsulta,FechaHora,Motivo,Diagnostico,Tratamiento,Observaciones,IdMascota,IdVeterinario")]
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
            ModelState.Remove(nameof(Consulta.Turno));
            ModelState.Remove(nameof(Consulta.Veterinario));

            if (_repositorioMascota.ObtenerPorId(
                    datos.IdMascota) == null)
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

            if (datos.IdVeterinario.HasValue &&
                !_context.Veterinarios.Any(v =>
                    v.IdVeterinario ==
                        datos.IdVeterinario.Value))
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdVeterinario),
                    "Seleccione un veterinario válido.");
            }

            if (ModelState.IsValid)
            {
                consulta.FechaHora = datos.FechaHora;
                consulta.Motivo = datos.Motivo;
                consulta.Diagnostico = datos.Diagnostico;
                consulta.Tratamiento = datos.Tratamiento;
                consulta.Observaciones = datos.Observaciones;
                consulta.IdMascota = datos.IdMascota;

                // Conserva el veterinario que tenía asignado
                _repositorioConsulta.Editar(consulta);

                TempData["Mensaje"] =
                    "Consulta actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            CargarListas(
                datos.IdMascota,
                datos.IdVeterinario);

            return View(datos);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            return View(consulta);
        }

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

        private void CargarListas(
            int? idMascota = null,
            int? idVeterinario = null)
        {
            ViewBag.Mascotas = new SelectList(
                _repositorioMascota.ObtenerTodos(),
                "IdMascota",
                "Nombre",
                idMascota);

            var veterinarios = _context.Veterinarios
                .Where(v => v.Activo ||
                    v.IdVeterinario == idVeterinario)
                .OrderBy(v => v.Apellido)
                .ThenBy(v => v.Nombre)
                .Select(v => new
                {
                    v.IdVeterinario,
                    NombreCompleto =
                        v.Apellido + ", " + v.Nombre
                })
                .ToList();

            ViewBag.Veterinarios = new SelectList(
                veterinarios,
                "IdVeterinario",
                "NombreCompleto",
                idVeterinario);
        }
    }
}