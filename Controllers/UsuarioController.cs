
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Veterinaria.Interfaces;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IRepositorioUsuario _repositorioUsuario;
        private readonly PasswordHasher<Usuario> _passwordHasher;
        private readonly IWebHostEnvironment _environment;

        public UsuarioController(
            IRepositorioUsuario repositorioUsuario,
            IWebHostEnvironment environment)
        {
            _repositorioUsuario = repositorioUsuario;
            _environment = environment;
            _passwordHasher = new PasswordHasher<Usuario>();
        }

        // LOGIN - GET
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN - POST
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Ingrese email y contraseña.";
                return View();
            }

            var usuario = _repositorioUsuario.ObtenerPorEmail(email);

            if (usuario == null)
            {
                ViewBag.Error = "Email o contraseña incorrectos.";
                return View();
            }

            if (!usuario.Activo)
            {
                ViewBag.Error = "El usuario se encuentra dado de baja.";
                return View();
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.Clave,
                password
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Email o contraseña incorrectos.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()),

                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("Avatar", usuario.Avatar ?? "")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return RedirectToAction("Index", "Home");
        }

        // LOGOUT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Index", "Home");
        }

        // PERFIL
        [HttpGet]
        [Authorize]
        public IActionResult Perfil()
        {
            var idClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idUsuario))
                return Unauthorized();

            var usuario = _repositorioUsuario.ObtenerPorId(idUsuario);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // EDITAR PERFIL - GET
        [HttpGet]
        [Authorize]
        public IActionResult EditarPerfil()
        {
            var idClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idUsuario))
                return Unauthorized();

            var usuario = _repositorioUsuario.ObtenerPorId(idUsuario);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // EDITAR PERFIL - POST
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(
            string nombre, string email)
        {
            var idClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idUsuario))
                return Unauthorized();

            var usuario = _repositorioUsuario.ObtenerPorId(idUsuario);

            if (usuario == null)
                return NotFound();

            nombre = nombre?.Trim() ?? "";
            email = email?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombre) ||
                nombre.Length > 100)
            {
                ModelState.AddModelError(
                    "Nombre", "Ingrese un nombre válido.");
            }

            if (string.IsNullOrWhiteSpace(email) ||
                email.Length > 150 ||
                !new System.ComponentModel.DataAnnotations
                    .EmailAddressAttribute().IsValid(email))
            {
                ModelState.AddModelError(
                    "Email", "Ingrese un email válido.");
            }

            var otroUsuario = _repositorioUsuario.ObtenerPorEmail(email);

            if (otroUsuario != null &&
                otroUsuario.IdUsuario != idUsuario)
            {
                ModelState.AddModelError(
                    "Email", "El email ya está registrado.");
            }

            if (!ModelState.IsValid)
            {
                usuario.Nombre = nombre;
                usuario.Email = email;
                return View(usuario);
            }

            var actualizado = _repositorioUsuario.ActualizarPerfil(
                idUsuario, nombre, email);

            if (!actualizado)
            {
                TempData["Error"] =
                    "No se pudo actualizar el perfil.";

                return RedirectToAction(nameof(Perfil));
            }

            var usuarioActualizado =
                _repositorioUsuario.ObtenerPorId(idUsuario);

            if (usuarioActualizado == null)
                return NotFound();

            await RenovarClaims(usuarioActualizado);

            TempData["Mensaje"] =
                "Perfil actualizado correctamente.";

            return RedirectToAction(nameof(Perfil));
        }

        // CAMBIAR CONTRASEÑA - GET
        [HttpGet]
        [Authorize]
        public IActionResult CambiarPassword()
        {
            return View();
        }

        // CAMBIAR CONTRASEÑA - POST
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarPassword(
            string passwordActual,
            string passwordNueva,
            string confirmarPassword)
        {
            var idClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idUsuario))
                return Unauthorized();

            var usuario = _repositorioUsuario.ObtenerPorId(idUsuario);

            if (usuario == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(passwordActual) ||
                string.IsNullOrWhiteSpace(passwordNueva))
            {
                ViewBag.Error = "Complete todos los campos.";
                return View();
            }

            if (passwordNueva.Length < 8)
            {
                ViewBag.Error =
                    "La contraseña debe tener al menos 8 caracteres.";

                return View();
            }

            if (passwordNueva != confirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario, usuario.Clave, passwordActual);

            if (resultado == PasswordVerificationResult.Failed)
            {
                ViewBag.Error =
                    "La contraseña actual es incorrecta.";

                return View();
            }

            var mismaClave = _passwordHasher.VerifyHashedPassword(
                usuario, usuario.Clave, passwordNueva);

            if (mismaClave != PasswordVerificationResult.Failed)
            {
                ViewBag.Error =
                    "La nueva contraseña debe ser diferente.";

                return View();
            }

            var nuevoHash = _passwordHasher.HashPassword(
                usuario, passwordNueva);

            var actualizado = _repositorioUsuario.ActualizarPassword(
                idUsuario, nuevoHash);

            if (!actualizado)
            {
                ViewBag.Error =
                    "No se pudo cambiar la contraseña.";

                return View();
            }

            TempData["Mensaje"] =
                "Contraseña actualizada correctamente.";

            return RedirectToAction(nameof(Perfil));
        }

        // ACTUALIZAR AVATAR
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> ActualizarAvatar(
            IFormFile? avatar)
        {
            var idClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idUsuario))
                return Unauthorized();

            var usuario = _repositorioUsuario.ObtenerPorId(idUsuario);

            if (usuario == null)
                return NotFound();

            if (avatar == null || avatar.Length == 0)
            {
                TempData["Error"] = "Seleccione una imagen.";
                return RedirectToAction(nameof(Perfil));
            }

            if (avatar.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] =
                    "La imagen no puede superar los 5 MB.";

                return RedirectToAction(nameof(Perfil));
            }

            var extension = Path.GetExtension(
                avatar.FileName).ToLowerInvariant();

            var tiposPermitidos = new Dictionary<string, string>
            {
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".webp", "image/webp" }
            };

            if (!tiposPermitidos.TryGetValue(
                    extension, out var tipo) ||
                !string.Equals(
                    avatar.ContentType,
                    tipo,
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] =
                    "Formato de imagen no permitido.";

                return RedirectToAction(nameof(Perfil));
            }

            // Validar firma del archivo
            var firma = new byte[12];

            using (var lectura = avatar.OpenReadStream())
            {
                var leidos = await lectura.ReadAsync(
                    firma.AsMemory());

                if (leidos < 12)
                {
                    TempData["Error"] =
                        "Archivo de imagen inválido.";

                    return RedirectToAction(nameof(Perfil));
                }
            }

            bool esImagen = extension switch
            {
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

                ".webp" =>
                    System.Text.Encoding.ASCII
                        .GetString(firma, 0, 4) == "RIFF" &&
                    System.Text.Encoding.ASCII
                        .GetString(firma, 8, 4) == "WEBP",

                _ => false
            };

            if (!esImagen)
            {
                TempData["Error"] =
                    "El archivo no es una imagen válida.";

                return RedirectToAction(nameof(Perfil));
            }

            var carpeta = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "usuarios"
            );

            Directory.CreateDirectory(carpeta);

            var nombreArchivo =
                $"usuario_{idUsuario}_{Guid.NewGuid():N}{extension}";

            var rutaFisica = Path.Combine(
                carpeta, nombreArchivo);

            var rutaAvatar =
                $"/uploads/usuarios/{nombreArchivo}";

            using (var stream = new FileStream(
                rutaFisica, FileMode.CreateNew))
            {
                await avatar.CopyToAsync(stream);
            }

            var avatarAnterior = usuario.Avatar;
            bool actualizado;

            try
            {
                actualizado = _repositorioUsuario.ActualizarAvatar(
                    idUsuario, rutaAvatar);
            }
            catch
            {
                System.IO.File.Delete(rutaFisica);
                throw;
            }

            if (!actualizado)
            {
                System.IO.File.Delete(rutaFisica);

                TempData["Error"] =
                    "No se pudo actualizar el avatar.";

                return RedirectToAction(nameof(Perfil));
            }

            // Eliminar avatar anterior
            if (!string.IsNullOrWhiteSpace(avatarAnterior) &&
                avatarAnterior.StartsWith(
                    "/uploads/usuarios/",
                    StringComparison.Ordinal) &&
                Path.GetFileName(avatarAnterior) ==
                    avatarAnterior.Substring(
                        "/uploads/usuarios/".Length))
            {
                var rutaAnterior = Path.Combine(
                    carpeta,
                    Path.GetFileName(avatarAnterior));

                if (System.IO.File.Exists(rutaAnterior))
                    System.IO.File.Delete(rutaAnterior);
            }

            var usuarioActualizado =
                _repositorioUsuario.ObtenerPorId(idUsuario);

            if (usuarioActualizado == null)
                return NotFound();

            await RenovarClaims(usuarioActualizado);

            TempData["Mensaje"] =
                "Avatar actualizado correctamente.";

            return RedirectToAction(nameof(Perfil));
        }

        // RENOVAR CLAIMS DE LA SESIÓN
        private async Task RenovarClaims(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()),

                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim("Avatar", usuario.Avatar ?? "")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );
        }

        // LISTADO DE USUARIOS CON PAGINACIÓN
        [Authorize(Roles = "Administrador")]
        public IActionResult Index(int pagina = 1)
        {
            const int cantidadPorPagina = 3;

            pagina = Math.Max(1, pagina);

            var resultado = _repositorioUsuario.ObtenerPaginados(
                pagina, cantidadPorPagina);

            int totalPaginas = (int)Math.Ceiling(
                (double)resultado.Total / cantidadPorPagina);

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;

                resultado = _repositorioUsuario.ObtenerPaginados(
                    pagina, cantidadPorPagina);
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = resultado.Total;

            return View(resultado.Usuarios);
        }

        // DETALLES
        [Authorize(Roles = "Administrador")]
        public IActionResult Details(int id)
        {
            var usuario = _repositorioUsuario.ObtenerPorId(id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // CREAR USUARIO - GET
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        // CREAR USUARIO - POST
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                usuario.Clave = _passwordHasher.HashPassword(
                    usuario,
                    usuario.Clave
                );

                _repositorioUsuario.Crear(usuario);

                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        // EDITAR USUARIO - GET
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id)
        {
            var usuario = _repositorioUsuario.ObtenerPorId(id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // EDITAR USUARIO - POST
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario)
                return NotFound();

            var usuarioActual =
                _repositorioUsuario.ObtenerPorId(id);

            if (usuarioActual == null)
                return NotFound();

            usuario.Nombre = usuario.Nombre?.Trim() ?? "";
            usuario.Email = usuario.Email?.Trim() ?? "";

            ModelState.Remove(nameof(Usuario.Clave));
            ModelState.Remove(nameof(Usuario.Rol));
            ModelState.Remove(nameof(Usuario.Veterinario));
            ModelState.Remove(nameof(Usuario.Turnos));
            ModelState.Remove(nameof(Usuario.Consultas));

            if (string.IsNullOrWhiteSpace(usuario.Nombre) ||
                usuario.Nombre.Length > 100)
            {
                ModelState.AddModelError(
                    nameof(Usuario.Nombre),
                    "Ingrese un nombre válido.");
            }

            if (string.IsNullOrWhiteSpace(usuario.Email) ||
                usuario.Email.Length > 150 ||
                !new System.ComponentModel.DataAnnotations
                    .EmailAddressAttribute().IsValid(usuario.Email))
            {
                ModelState.AddModelError(
                    nameof(Usuario.Email),
                    "Ingrese un correo electrónico válido.");
            }

            if (!string.IsNullOrWhiteSpace(usuario.Email))
            {
                var otroUsuario =
                    _repositorioUsuario.ObtenerPorEmail(usuario.Email);

                if (otroUsuario != null &&
                    otroUsuario.IdUsuario != id)
                {
                    ModelState.AddModelError(
                        nameof(Usuario.Email),
                        "El correo electrónico ya está registrado.");
                }
            }

            if (ModelState.IsValid)
            {
                usuarioActual.Nombre = usuario.Nombre;
                usuarioActual.Email = usuario.Email;
                usuarioActual.Activo = usuario.Activo;

                _repositorioUsuario.Editar(usuarioActual);

                TempData["Mensaje"] =
                    "Usuario actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }

            usuario.Rol = usuarioActual.Rol;

            return View(usuario);
        }

        // CAMBIAR ROL
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarRol(
            int id, string nuevoRol)
        {
            var usuario = _repositorioUsuario.ObtenerPorId(id);

            if (usuario == null)
                return NotFound();

            var idClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int idActual))
                return Unauthorized();

            if (id == idActual)
            {
                TempData["Error"] =
                    "No podés modificar tu propio rol.";

                return RedirectToAction(nameof(Index));
            }

            if (!usuario.Activo)
            {
                TempData["Error"] =
                    "No se puede cambiar el rol de un usuario inactivo.";

                return RedirectToAction(nameof(Index));
            }

            string[] rolesPermitidos =
            {
                "Administrador",
                "Empleado",
                "Veterinario"
            };

            if (string.IsNullOrWhiteSpace(nuevoRol) ||
                !rolesPermitidos.Contains(nuevoRol))
            {
                TempData["Error"] =
                    "Seleccione un rol válido.";

                return RedirectToAction(nameof(Index));
            }

            bool tieneFicha = _repositorioUsuario
                .TieneFichaVeterinario(id);

            if (tieneFicha && nuevoRol != "Veterinario")
            {
                TempData["Error"] =
                    "El usuario tiene una ficha profesional asociada. " +
                    "No se puede cambiar su rol directamente.";

                return RedirectToAction(nameof(Index));
            }

            if (usuario.Rol == nuevoRol)
            {
                TempData["Mensaje"] =
                    "El usuario ya tiene ese rol.";

                return RedirectToAction(nameof(Index));
            }

            bool actualizado = _repositorioUsuario
                .CambiarRol(id, nuevoRol);

            if (!actualizado)
            {
                TempData["Error"] =
                    "No se pudo cambiar el rol. " +
                    "Verifique que no sea el último administrador activo.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] =
                $"El rol de {usuario.Nombre} se actualizó a {nuevoRol}.";

            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR USUARIO - GET
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var usuario = _repositorioUsuario.ObtenerPorId(id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // ELIMINAR USUARIO - POST
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioUsuario.Eliminar(id);

            return RedirectToAction(nameof(Index));
        }
    }
}
