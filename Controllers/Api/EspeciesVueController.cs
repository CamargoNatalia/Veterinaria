
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Veterinaria.Data;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers.Api
{
    public class EspeciePeticion
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }

    [ApiController]
    [Route("api/vue/especies")]
    [Authorize(
        AuthenticationSchemes =
            CookieAuthenticationDefaults.AuthenticationScheme,
        Roles = "Administrador,Empleado"
    )]
    public class EspeciesVueController : ControllerBase
    {
        private readonly IRepositorioEspecie _repositorio;
        private readonly VeterinariaContext _context;

        public EspeciesVueController(
            IRepositorioEspecie repositorio,
            VeterinariaContext context)
        {
            _repositorio = repositorio;
            _context = context;
        }

        // GET: Listar especies
        [HttpGet]
        public IActionResult ObtenerTodos()
        {
            var especies = _repositorio.ObtenerTodos()
                .Select(e => new
                {
                    e.IdEspecie,
                    e.Nombre,
                    e.Activo
                })
                .ToList();

            return Ok(especies);
        }

        // GET: Obtener especie por ID
        [HttpGet("{id:int}")]
        public IActionResult ObtenerPorId(int id)
        {
            var especie = _repositorio.ObtenerPorId(id);

            if (especie == null)
                return NotFound(new
                {
                    mensaje = "Especie no encontrada."
                });

            return Ok(new
            {
                especie.IdEspecie,
                especie.Nombre,
                especie.Activo
            });
        }

        // POST: Crear especie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(
            [FromBody] EspeciePeticion datos)
        {
            string nombre = datos.Nombre.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest(new
                {
                    mensaje = "Ingrese un nombre."
                });

            bool existe = _context.Especies.Any(e =>
                e.Nombre.ToLower() == nombre.ToLower());

            if (existe)
                return Conflict(new
                {
                    mensaje = "La especie ya existe."
                });

            var especie = new Especie
            {
                Nombre = nombre,
                Activo = datos.Activo
            };

            _repositorio.Crear(especie);

            return Ok(new
            {
                mensaje = "Especie registrada correctamente."
            });
        }

        // PUT: Editar especie
        [HttpPut("{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(
            int id, [FromBody] EspeciePeticion datos)
        {
            var especie = _repositorio.ObtenerPorId(id);

            if (especie == null)
                return NotFound(new
                {
                    mensaje = "Especie no encontrada."
                });

            string nombre = datos.Nombre.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest(new
                {
                    mensaje = "Ingrese un nombre."
                });

            bool existe = _context.Especies.Any(e =>
                e.IdEspecie != id &&
                e.Nombre.ToLower() == nombre.ToLower());

            if (existe)
                return Conflict(new
                {
                    mensaje = "Ya existe otra especie con ese nombre."
                });

            especie.Nombre = nombre;
            especie.Activo = datos.Activo;

            _repositorio.Editar(especie);

            return Ok(new
            {
                mensaje = "Especie actualizada correctamente."
            });
        }

        // DELETE: Eliminar especie
        [HttpDelete("{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var especie = _repositorio.ObtenerPorId(id);

            if (especie == null)
                return NotFound(new
                {
                    mensaje = "Especie no encontrada."
                });

            bool tieneRelaciones =
                _context.Razas.Any(r => r.IdEspecie == id) ||
                _context.Mascotas.Any(m => m.IdEspecie == id);

            if (tieneRelaciones)
                return Conflict(new
                {
                    mensaje = "No se puede eliminar: " +
                              "la especie tiene razas o mascotas asociadas."
                });

            try
            {
                _repositorio.Eliminar(id);
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    mensaje = "No se puede eliminar porque " +
                              "existen registros relacionados."
                });
            }

            return Ok(new
            {
                mensaje = "Especie eliminada correctamente."
            });
        }
    }
}
