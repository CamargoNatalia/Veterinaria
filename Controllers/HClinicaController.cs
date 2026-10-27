
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador,Empleado,Veterinario")]
    public class HClinicaController : Controller
    {
        private readonly IRepositorioHClinica _repositorioHClinica;
        private readonly VeterinariaContext _context;
        private readonly IWebHostEnvironment _environment;

        public HClinicaController(
            IRepositorioHClinica repositorioHClinica,
            VeterinariaContext context,
            IWebHostEnvironment environment)
        {
            _repositorioHClinica = repositorioHClinica;
            _context = context;
            _environment = environment;
        }

        // Formulario para adjuntar un archivo clínico
        [HttpGet]
        public IActionResult Create(int idConsulta)
        {
            var consulta = _context.Consultas
                .AsNoTracking()
                .FirstOrDefault(c => c.IdConsulta == idConsulta);

            if (consulta == null)
                return NotFound();

            if (!PuedeAdjuntar(consulta))
                return Forbid();

            ViewBag.IdConsulta = idConsulta;

            return View();
        }

        // Guardar archivo clínico
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Create(
            int idConsulta,
            IFormFile? archivo,
            string? descripcion)
        {
            var consulta = _context.Consultas
                .AsNoTracking()
                .FirstOrDefault(c => c.IdConsulta == idConsulta);

            if (consulta == null)
                return NotFound();

            if (!PuedeAdjuntar(consulta))
                return Forbid();

            ViewBag.IdConsulta = idConsulta;

            if (archivo == null || archivo.Length == 0)
            {
                ModelState.AddModelError(
                    "archivo",
                    "Seleccione un archivo.");
            }
            else
            {
                string extension = Path.GetExtension(
                    archivo.FileName).ToLowerInvariant();

                string[] permitidas =
                {
                    ".pdf", ".jpg", ".jpeg", ".png"
                };

                if (!permitidas.Contains(extension))
                {
                    ModelState.AddModelError(
                        "archivo",
                        "Solo se permiten PDF, JPG, JPEG o PNG.");
                }

                if (archivo.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "archivo",
                        "El archivo no debe superar los 10 MB.");
                }

                if (Path.GetFileName(archivo.FileName).Length > 255)
                {
                    ModelState.AddModelError(
                        "archivo",
                        "El nombre del archivo es demasiado largo.");
                }

                if (ModelState.IsValid)
                {
                    bool archivoValido =
                        await ValidarContenidoArchivo(archivo);

                    if (!archivoValido)
                    {
                        ModelState.AddModelError(
                            "archivo",
                            "El contenido no corresponde a un " +
                            "PDF o imagen permitida.");
                    }
                }
            }

            if (descripcion?.Length > 1000)
            {
                ModelState.AddModelError(
                    "descripcion",
                    "La descripción no puede superar los 1000 caracteres.");
            }

            if (!ModelState.IsValid)
                return View();

            string nombreOriginal =
                Path.GetFileName(archivo!.FileName);

            string extensionArchivo = Path.GetExtension(
                nombreOriginal).ToLowerInvariant();

            string nombreGuardado =
                Guid.NewGuid().ToString("N") + extensionArchivo;

            string carpeta = Path.Combine(
                _environment.ContentRootPath,
                "ArchivosClinicos");

            Directory.CreateDirectory(carpeta);

            string rutaCompleta = Path.Combine(
                carpeta, nombreGuardado);

            try
            {
                using (var stream = new FileStream(
                    rutaCompleta, FileMode.CreateNew))
                {
                    await archivo.CopyToAsync(stream);
                }

                var historiaClinica = new HClinica
                {
                    IdConsulta = idConsulta,
                    NombreArchivo = nombreOriginal,
                    CarpetaDescarga = nombreGuardado,
                    FormatoArchivo = extensionArchivo,
                    CargaArchivo = DateTime.Now,
                    Descripcion = descripcion?.Trim()
                };

                _repositorioHClinica.Crear(historiaClinica);
            }
            catch
            {
                if (System.IO.File.Exists(rutaCompleta))
                    System.IO.File.Delete(rutaCompleta);

                ModelState.AddModelError(
                    "",
                    "No se pudo guardar el documento.");

                return View();
            }

            TempData["Mensaje"] =
                "Archivo clínico adjuntado correctamente.";

            return RedirectToAction(
                "Details",
                "Consulta",
                new { id = idConsulta });
        }


        // Visualizar PDF o imagen en el navegador
        [HttpGet]
        public IActionResult Ver(int id)
        {
            var historiaClinica =
                _repositorioHClinica.ObtenerPorId(id);

            if (historiaClinica == null)
                return NotFound();

            // Verificar que el usuario tenga acceso a la consulta.
            if (historiaClinica.Consulta == null ||
                !PuedeAdjuntar(historiaClinica.Consulta))
                return Forbid();

            string? ruta = ObtenerRutaArchivo(
                historiaClinica.CarpetaDescarga);

            if (ruta == null || !System.IO.File.Exists(ruta))
                return NotFound();

            string tipoContenido = ObtenerTipoContenido(
                historiaClinica.FormatoArchivo);

            if (tipoContenido == "application/octet-stream")
                return BadRequest("Formato no permitido.");

            Response.Headers["Content-Disposition"] = "inline";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Cache-Control"] = "no-store";

            var stream = new FileStream(
                ruta,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            return File(stream, tipoContenido);
        }


        // Descargar documento clínico
        [HttpGet]
        public IActionResult Descargar(int id)
        {
            var historiaClinica =
                _repositorioHClinica.ObtenerPorId(id);

            if (historiaClinica == null)
                return NotFound();

            // Verificar que el usuario tenga acceso a la consulta.
            if (historiaClinica.Consulta == null ||
                !PuedeAdjuntar(historiaClinica.Consulta))
                return Forbid();

            string? ruta = ObtenerRutaArchivo(
                historiaClinica.CarpetaDescarga);

            if (ruta == null || !System.IO.File.Exists(ruta))
                return NotFound();

            string tipoContenido = ObtenerTipoContenido(
                historiaClinica.FormatoArchivo);

            if (tipoContenido == "application/octet-stream")
                return BadRequest("Formato no permitido.");

            string nombreDescarga = Path.GetFileName(
                historiaClinica.NombreArchivo);

            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return PhysicalFile(
                ruta,
                tipoContenido,
                nombreDescarga);
        }

        // Comprobar permisos de carga
        private bool PuedeAdjuntar(Consulta consulta)
        {
            if (User.IsInRole("Administrador") ||
                User.IsInRole("Empleado"))
                return true;

            if (!User.IsInRole("Veterinario"))
                return false;

            var idClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idUsuario))
                return false;

            return _context.Veterinarios.Any(v =>
                v.IdUsuario == idUsuario &&
                v.Usuario.Activo &&
                v.Usuario.Rol == "Veterinario" &&
                consulta.IdVeterinario == v.IdVeterinario);
        }

        // Verificar firma básica del archivo
        private static async Task<bool> ValidarContenidoArchivo(
            IFormFile archivo)
        {
            string extension = Path.GetExtension(
                archivo.FileName).ToLowerInvariant();

            byte[] firma = new byte[12];

            using var stream = archivo.OpenReadStream();

            int leidos = await stream.ReadAsync(
                firma.AsMemory());

            if (leidos < 12)
                return false;

            return extension switch
            {
                ".pdf" =>
                    firma[0] == 0x25 &&
                    firma[1] == 0x50 &&
                    firma[2] == 0x44 &&
                    firma[3] == 0x46 &&
                    firma[4] == 0x2D,

                ".jpg" or ".jpeg" =>
                    firma[0] == 0xFF &&
                    firma[1] == 0xD8 &&
                    firma[2] == 0xFF,

                ".png" =>
                    firma.Take(8).SequenceEqual(
                        new byte[]
                        {
                            137, 80, 78, 71, 13, 10, 26, 10
                        }),

                _ => false
            };
        }

        // Resolver ubicación física del archivo
        private string? ObtenerRutaArchivo(string nombreGuardado)
        {
            if (string.IsNullOrWhiteSpace(nombreGuardado))
                return null;

            if (nombreGuardado != Path.GetFileName(nombreGuardado)
                || nombreGuardado.Contains('\\')
                || nombreGuardado.Contains('/')
                || nombreGuardado == "."
                || nombreGuardado == "..")
                return null;

            string carpeta = Path.GetFullPath(Path.Combine(
                _environment.ContentRootPath,
                "ArchivosClinicos"));

            string ruta = Path.GetFullPath(
                Path.Combine(carpeta, nombreGuardado));

            if (!Path.GetDirectoryName(ruta)!.Equals(
                    carpeta,
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal))
                return null;

            return ruta;
        }

        // Tipos de archivo permitidos
        private string ObtenerTipoContenido(string? formato)
        {
            return formato?.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
}
