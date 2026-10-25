
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Veterinaria.Interfaces;

namespace Veterinaria.Controllers.Api
{
    [ApiController]
    [Route("api/mascotas")]
    [Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Administrador,Empleado,Veterinario"
    )]
    public class MascotasApiController : ControllerBase
    {
        private readonly IRepositorioMascota _repositorio;

        public MascotasApiController(IRepositorioMascota repositorio)
        {
            _repositorio = repositorio;
        }

        // GET: api/mascotas
        [HttpGet]
        public IActionResult ObtenerTodos()
        {
            var mascotas = _repositorio.ObtenerTodos()
                .Select(m => new
                {
                    m.IdMascota,
                    m.Nombre,
                    m.FechaNacimiento,
                    m.Sexo,
                    m.Peso,
                    m.Observaciones,
                    m.Activo,
                    m.IdCliente,
                    m.IdEspecie,
                    Especie = m.Especie?.Nombre,
                    m.IdRaza,
                    Raza = m.Raza?.Nombre
                })
                .ToList();

            return Ok(mascotas);
        }

        // GET: api/mascotas/1
        [HttpGet("{id:int}")]
        public IActionResult ObtenerPorId(int id)
        {
            var mascota = _repositorio.ObtenerPorId(id);

            if (mascota == null)
            {
                return NotFound(new
                {
                    mensaje = "Mascota no encontrada"
                });
            }

            return Ok(new
            {
                mascota.IdMascota,
                mascota.Nombre,
                mascota.FechaNacimiento,
                mascota.Sexo,
                mascota.Peso,
                mascota.Observaciones,
                mascota.Activo,
                mascota.IdCliente,
                mascota.IdEspecie,
                Especie = mascota.Especie?.Nombre,
                mascota.IdRaza,
                Raza = mascota.Raza?.Nombre
            });
        }
    }
}
