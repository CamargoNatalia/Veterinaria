using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    [Authorize(Roles = "Administrador,Empleado")]
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

        // Formulario para adjuntar un archivo a una consulta
        [HttpGet]
        public IActionResult Create(int idConsulta)
        {
            var consulta = _context.Consultas.Find(idConsulta);

            if (consulta == null)
                return NotFound();

            ViewBag.IdConsulta = idConsulta;

            return View();
        }

        // Guarda el documento y registra sus datos
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Create(
            int idConsulta,
            IFormFile? archivo,
            string? descripcion)
        {
            if (!_context.Consultas.Any(c =>
                    c.IdConsulta == idConsulta))
                return NotFound();

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
                    Descripcion = descripcion
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

        // Abre el archivo en el navegador
        [HttpGet]
        public IActionResult Ver(int id)
        {
            var historiaClinica =
                _repositorioHClinica.ObtenerPorId(id);

            if (historiaClinica == null)
                return NotFound();

            string? ruta = ObtenerRutaArchivo(
                historiaClinica.CarpetaDescarga);

            if (ruta == null || !System.IO.File.Exists(ruta))
                return NotFound();

            string tipoContenido = ObtenerTipoContenido(
                historiaClinica.FormatoArchivo);

            // Evita que el navegador interprete tipos no permitidos
            if (tipoContenido == "application/octet-stream")
                return BadRequest("Formato no permitido.");

            Response.Headers["Content-Disposition"] = "inline";
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return PhysicalFile(ruta, tipoContenido);
        }

        // Descarga el documento con su nombre original
        [HttpGet]
        public IActionResult Descargar(int id)
        {
            var historiaClinica =
                _repositorioHClinica.ObtenerPorId(id);

            if (historiaClinica == null)
                return NotFound();

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

        // Resuelve la ubicación dentro de ArchivosClinicos
        private string? ObtenerRutaArchivo(string nombreGuardado)
        {
            if (string.IsNullOrWhiteSpace(nombreGuardado))
                return null;

            // Solo admitimos un nombre físico, nunca una ruta externa
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

        // Tipos admitidos para visualización y descarga
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