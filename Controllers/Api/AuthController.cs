
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Veterinaria.Interfaces;
using Veterinaria.Models;
using Veterinaria.Models.DTOs;

namespace Veterinaria.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IRepositorioUsuario _repositorio;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public AuthController(
            IRepositorioUsuario repositorio,
            IConfiguration configuration)
        {
            _repositorio = repositorio;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginApiDto datos)
        {
            // Buscar usuario
            var usuario = _repositorio.ObtenerPorEmail(
                datos.Email.Trim()
            );

            if (usuario == null || !usuario.Activo)
            {
                return Unauthorized(new
                {
                    mensaje = "Credenciales incorrectas."
                });
            }

            // Verificar contraseña
            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.Clave,
                datos.Password
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    mensaje = "Credenciales incorrectas."
                });
            }

            // Información del usuario dentro del JWT
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()
                ),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            // Recuperar clave secreta
            var clave = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(clave))
                throw new InvalidOperationException(
                    "No se configuró la clave JWT."
                );

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(clave)
            );

            var credenciales = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            // Vencimiento del token
            var vencimiento = DateTime.UtcNow.AddHours(1);

            // Crear token
            var token = new JwtSecurityToken(
                issuer: "VeterinariaGit",
                audience: "VeterinariaGit.Api",
                claims: claims,
                expires: vencimiento,
                signingCredentials: credenciales
            );

            var tokenGenerado =
                new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenGenerado,
                tipo = "Bearer",
                expiraEn = vencimiento,
                usuario = new
                {
                    usuario.IdUsuario,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.Rol
                }
            });
        }
    }
}
