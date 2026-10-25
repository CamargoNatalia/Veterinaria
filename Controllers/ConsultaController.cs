
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

        // LISTADO PAGINADO: CONSULTAS Y TURNOS PENDIENTES
        public IActionResult Index(
            int paginaConsultas = 1,
            int paginaTurnos = 1)
        {
            const int porPagina = 3;

            paginaConsultas = Math.Max(1, paginaConsultas);
            paginaTurnos = Math.Max(1, paginaTurnos);

            var resultadoConsultas = _repositorioConsulta
                .ObtenerPaginadas(paginaConsultas, porPagina);

            int totalPaginasConsultas = (int)Math.Ceiling(
                (double)resultadoConsultas.Total / porPagina);

            if (totalPaginasConsultas > 0 &&
                paginaConsultas > totalPaginasConsultas)
            {
                paginaConsultas = totalPaginasConsultas;

                resultadoConsultas = _repositorioConsulta
                    .ObtenerPaginadas(paginaConsultas, porPagina);
            }

            var pendientes = _context.Turnos
                .AsNoTracking()
                .Where(t => t.Estado == "Pendiente")
                .Where(t => !_context.Consultas
                    .Any(c => c.IdTurno == t.IdTurno));

            int totalPendientes = pendientes.Count();

            int totalPaginasTurnos = (int)Math.Ceiling(
                (double)totalPendientes / porPagina);

            if (totalPaginasTurnos > 0 &&
                paginaTurnos > totalPaginasTurnos)
            {
                paginaTurnos = totalPaginasTurnos;
            }

            ViewBag.TurnosPendientes = pendientes
                .OrderBy(t => t.FechaHora)
                .ThenBy(t => t.IdTurno)
                .Skip((paginaTurnos - 1) * porPagina)
                .Take(porPagina)
                .Include(t => t.Mascota)
                .ToList();

            ViewBag.PaginaConsultas = paginaConsultas;
            ViewBag.TotalPaginasConsultas = totalPaginasConsultas;
            ViewBag.TotalConsultas = resultadoConsultas.Total;

            ViewBag.PaginaTurnos = paginaTurnos;
            ViewBag.TotalPaginasTurnos = totalPaginasTurnos;
            ViewBag.TotalTurnosPendientes = totalPendientes;

            return View(resultadoConsultas.Consultas);
        }

        // DETALLES DE CONSULTA
        public IActionResult Details(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            return View(consulta);
        }

        // DATOS DE MASCOTA Y PROPIETARIO PARA AJAX
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

        // NUEVA CONSULTA - GET
        [HttpGet]
        public IActionResult Create(int? idTurno)
        {
            var consulta = new Consulta
            {
                FechaHora = DateTime.Now
            };

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

        // NUEVA CONSULTA - POST
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

            // El veterinario autenticado siempre es el responsable
            if (User.IsInRole("Veterinario"))
            {
                var veterinario = ObtenerVeterinarioActual();

                if (veterinario == null)
                    return Forbid();

                consulta.IdVeterinario = veterinario.IdVeterinario;
            }

            ModelState.Remove(nameof(Consulta.IdUsuario));
            ModelState.Remove(nameof(Consulta.Usuario));
            ModelState.Remove(nameof(Consulta.Mascota));
            ModelState.Remove(nameof(Consulta.Turno));
            ModelState.Remove(nameof(Consulta.Veterinario));

            if (User.IsInRole("Veterinario"))
            {
                ModelState.Remove(nameof(Consulta.IdVeterinario));
            }

            // Validar mascota
            if (_repositorioMascota.ObtenerPorId(
                    consulta.IdMascota) == null)
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdMascota),
                    "Seleccione una mascota válida.");
            }

            // Validar veterinario responsable
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

            // Validar turno asociado
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
                    guardado = _repositorioConsulta
                        .RegistrarAtencion(
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

        // EDITAR CONSULTA - GET
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            if (!PuedeEditarConsulta(consulta))
                return Forbid();

            CargarListas(
                consulta.IdMascota,
                consulta.IdVeterinario);

            return View(consulta);
        }

        // EDITAR CONSULTA - POST
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

            // Mantener usuario, turno y veterinario originales
            datos.IdUsuario = consulta.IdUsuario;
            datos.IdTurno = consulta.IdTurno;
            datos.IdVeterinario = consulta.IdVeterinario;

            ModelState.Remove(nameof(Consulta.IdUsuario));
            ModelState.Remove(nameof(Consulta.Usuario));
            ModelState.Remove(nameof(Consulta.Mascota));
            ModelState.Remove(nameof(Consulta.Turno));
            ModelState.Remove(nameof(Consulta.Veterinario));
            ModelState.Remove(nameof(Consulta.IdVeterinario));

            if (_repositorioMascota.ObtenerPorId(
                    datos.IdMascota) == null)
            {
                ModelState.AddModelError(
                    nameof(Consulta.IdMascota),
                    "Seleccione una mascota válida.");
            }

            // No permitir cambiar la mascota si tiene turno
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

        // ELIMINAR CONSULTA - GET
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var consulta = _repositorioConsulta.ObtenerPorId(id);

            if (consulta == null)
                return NotFound();

            return View(consulta);
        }

        // ELIMINAR CONSULTA - POST
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

        // VETERINARIO AUTENTICADO
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

        // CONTROL DE PERMISOS PARA EDITAR
        private bool PuedeEditarConsulta(Consulta consulta)
        {
            if (!User.IsInRole("Veterinario"))
                return true;

            var veterinario = ObtenerVeterinarioActual();

            return veterinario != null &&
                   consulta.IdVeterinario ==
                       veterinario.IdVeterinario;
        }

        // CARGAR DATOS PARA FORMULARIOS
        // Sin cargar todas las mascotas en un select
        private void CargarListas(
            int? idMascota = null,
            int? idVeterinario = null)
        {
            ViewBag.MascotaSeleccionada = "";

            if (idMascota.HasValue && idMascota.Value > 0)
            {
                var mascota = _context.Mascotas
                    .AsNoTracking()
                    .Where(m => m.IdMascota == idMascota.Value)
                    .Select(m => new
                    {
                        m.Nombre,

                        Propietario = m.Cliente != null
                            ? m.Cliente.Nombre + " " +
                              m.Cliente.Apellido
                            : "Sin propietario",

                        Especie = m.Especie != null
                            ? m.Especie.Nombre
                            : "Sin especie",

                        Raza = m.Raza != null
                            ? m.Raza.Nombre
                            : "Sin raza"
                    })
                    .FirstOrDefault();

                if (mascota != null)
                {
                    ViewBag.MascotaSeleccionada =
                        $"{mascota.Nombre} - " +
                        $"{mascota.Propietario} - " +
                        $"{mascota.Especie} - " +
                        $"{mascota.Raza}";
                }
            }

            // Veterinarios disponibles
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
