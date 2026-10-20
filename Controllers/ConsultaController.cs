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
    [Authorize(Roles = "Administrador,Empleado,Veterinario")]
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

        // Listado de consultas y turnos pendientes
        public IActionResult Index()
        {
            var consultas = _repositorioConsulta.ObtenerTodos();

            ViewBag.TurnosPendientes =
                _repositorioTurno.ObtenerPendientes();

            return View(consultas);
        }

        // Detalles de la consulta
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

        // GET: Nueva consulta
        [HttpGet]
        public IActionResult Create(int? idTurno)
        {
            var consulta = new Consulta
            {
                FechaHora = DateTime.Now
            };

            // Si ingresa un veterinario, identificar su ficha
            if (User.IsInRole("Veterinario"))
            {
                var veterinario = ObtenerVeterinarioActual();

                if (veterinario == null)
                {
                    TempData["Error"] =
                        "No tiene una ficha profesional activa asociada.";

                    return RedirectToAction(nameof(Index));
                }

                consulta.IdVeterinario = veterinario.IdVeterinario;
            }

            // Consulta originada desde un turno
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

        // POST: Registrar consulta
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

            // Usuario que registra la atención
            consulta.IdUsuario = idUsuario;

            // Si es Veterinario, ignorar el IdVeterinario
            // recibido del formulario y obtenerlo de la sesión
            if (User.IsInRole("Veterinario"))
            {
                var veterinario = ObtenerVeterinarioActual();

                if (veterinario == null)
                    return Forbid();

                consulta.IdVeterinario = veterinario.IdVeterinario;
            }

            // Validar solamente los datos correspondientes
            // al formulario
            ModelState.Remove(nameof(Consulta.IdUsuario));
            ModelState.Remove(nameof(Consulta.Usuario));
            ModelState.Remove(nameof(Consulta.Mascota));
            ModelState.Remove(nameof(Consulta.Turno));
            ModelState.Remove(nameof(Consulta.Veterinario));

            if (User.IsInRole("Veterinario"))
            {
                ModelState.Remove(
                    nameof(Consulta.IdVeterinario));
            }

            // Verificar mascota
            if (_repositorioMascota.ObtenerPorId(
                    consulta.IdMascota) == null)
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdMascota),
                    "Seleccione una mascota válida.");
            }

            // Verificar veterinario responsable
            if (!consulta.IdVeterinario.HasValue ||
                !_context.Veterinarios.Any(v =>
                    v.IdVeterinario ==
                        consulta.IdVeterinario.Value &&
                    v.Usuario.Activo &&
                    v.Usuario.Rol == "Veterinario"))
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdVeterinario),
                    "Seleccione un veterinario activo.");
            }

            // Verificar turno asociado
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
                bool guardado;

                if (consulta.IdTurno.HasValue)
                {
                    // Registra consulta y marca turno Atendido
                    guardado = _repositorioConsulta
                        .RegistrarAtencion(
                            consulta,
                            consulta.IdTurno.Value);
                }
                else
                {
                    // Consulta directa sin turno
                    _repositorioConsulta.Crear(consulta);
                    guardado = true;
                }

                if (guardado)
                {
                    // Guardar y reservar próximo turno
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
                    "No se pudo registrar la atención. " +
                    "El turno ya no está disponible.");
            }

            CargarListas(
                consulta.IdMascota,
                consulta.IdVeterinario);

            return View(consulta);
        }

        // GET: Editar consulta
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            // El veterinario solo puede editar
            // sus propias consultas
            if (!PuedeEditarConsulta(consulta))
                return Forbid();

            CargarListas(
                consulta.IdMascota,
                consulta.IdVeterinario);

            return View(consulta);
        }

        // POST: Editar consulta
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

            if (!PuedeEditarConsulta(consulta))
                return Forbid();

            // Conservar los datos que no se editan
            datos.IdUsuario = consulta.IdUsuario;
            datos.IdTurno = consulta.IdTurno;
            datos.IdVeterinario = consulta.IdVeterinario;

            ModelState.Remove(nameof(Consulta.IdUsuario));
            ModelState.Remove(nameof(Consulta.Usuario));
            ModelState.Remove(nameof(Consulta.Mascota));
            ModelState.Remove(nameof(Consulta.Turno));
            ModelState.Remove(nameof(Consulta.Veterinario));
            ModelState.Remove(nameof(Consulta.IdVeterinario));

            // Validar mascota
            if (_repositorioMascota.ObtenerPorId(
                    datos.IdMascota) == null)
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdMascota),
                    "Seleccione una mascota válida.");
            }

            // Si tiene turno asociado, no cambiar mascota
            if (consulta.IdTurno.HasValue &&
                consulta.IdMascota != datos.IdMascota)
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdMascota),
                    "No se puede cambiar la mascota de una " +
                    "consulta asociada a un turno.");
            }

            if (ModelState.IsValid)
            {
                consulta.FechaHora = datos.FechaHora;
                consulta.Motivo = datos.Motivo;
                consulta.Diagnostico = datos.Diagnostico;
                consulta.Tratamiento = datos.Tratamiento;
                consulta.Observaciones = datos.Observaciones;
                consulta.IdMascota = datos.IdMascota;

                // Conservar veterinario y usuario originales
                _repositorioConsulta.Editar(consulta);

                TempData["Mensaje"] =
                    "Consulta actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }

            datos.Veterinario = consulta.Veterinario;

            CargarListas(
                datos.IdMascota,
                datos.IdVeterinario);

            return View(datos);
        }

        // GET: Eliminar consulta
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            return View(consulta);
        }

        // POST: Eliminar consulta
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
                    "No se puede eliminar una consulta " +
                    "asociada a un turno atendido.";

                return RedirectToAction(nameof(Index));
            }

            if (consulta.HistoriasClinicas.Any())
            {
                TempData["Error"] =
                    "No se puede eliminar una consulta " +
                    "con archivos clínicos asociados.";

                return RedirectToAction(nameof(Index));
            }

            _repositorioConsulta.Eliminar(id);

            TempData["Mensaje"] =
                "Consulta eliminada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // Obtener la ficha del veterinario autenticado
        private Veterinario? ObtenerVeterinarioActual()
        {
            var idClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idUsuario))
                return null;

            return _context.Veterinarios
                .AsNoTracking()
                .FirstOrDefault(v =>
                    v.IdUsuario == idUsuario &&
                    v.Usuario.Activo &&
                    v.Usuario.Rol == "Veterinario");
        }

        // Controlar quién puede editar una consulta
        private bool PuedeEditarConsulta(Consulta consulta)
        {
            if (!User.IsInRole("Veterinario"))
                return true;

            var veterinario = ObtenerVeterinarioActual();

            return veterinario != null &&
                   consulta.IdVeterinario ==
                       veterinario.IdVeterinario;
        }

        // Listas para los formularios
        private void CargarListas(
            int? idMascota = null,
            int? idVeterinario = null)
        {
            ViewBag.Mascotas = new SelectList(
                _repositorioMascota.ObtenerTodos(),
                "IdMascota",
                "Nombre",
                idMascota);

            // Datos profesionales asociados a Usuario
            var veterinarios = _context.Veterinarios
                .AsNoTracking()
                .Where(v =>
                    (v.Usuario.Activo &&
                     v.Usuario.Rol == "Veterinario") ||
                    v.IdVeterinario == idVeterinario)
                .OrderBy(v => v.Usuario.Nombre)
                .Select(v => new
                {
                    v.IdVeterinario,
                    NombreCompleto = v.Usuario.Nombre
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