using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Veterinaria.Interfaces;
using Veterinaria.Data;
using Veterinaria.Models;

namespace Veterinaria.Controllers
{
    public class MascotaController : Controller
    {
        private readonly IRepositorioMascota _repositorioMascota;
        private readonly IRepositorioRaza _repositorioRaza;
        private readonly VeterinariaContext _context;


        public MascotaController(
            IRepositorioMascota repositorioMascota,
            IRepositorioRaza repositorioRaza,
            VeterinariaContext context)
        {
            _repositorioMascota = repositorioMascota;
            _repositorioRaza = repositorioRaza;
            _context = context;
        }


        public IActionResult Index()
        {
            var mascotas = _repositorioMascota.ObtenerTodos();

            return View(mascotas);
        }


        public IActionResult Details(int id)
        {
            var mascota = _repositorioMascota.ObtenerPorId(id);

            if (mascota == null)
            {
                return NotFound();
            }

            return View(mascota);
        }

        public IActionResult Create()
        {
            CargarListas();

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Mascota mascota)
        {
            if (ModelState.IsValid)
            {
                _repositorioMascota.Crear(mascota);

                return RedirectToAction(nameof(Index));
            }

            CargarListas(mascota);

            return View(mascota);
        }



        [HttpGet]
        public IActionResult ObtenerRazasPorEspecie(int idEspecie)
        {
            var razas = _repositorioRaza
                .ObtenerPorEspecie(idEspecie)
                .Select(r => new
                {
                    idRaza = r.IdRaza,
                    nombre = r.Nombre
                })
                .ToList();

            return Json(razas);
        }

        public IActionResult Edit(int id)
        {
            var mascota = _repositorioMascota.ObtenerPorId(id);

            if (mascota == null)
            {
                return NotFound();
            }

            CargarListas(mascota);

            return View(mascota);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Mascota mascota)
        {
            if (id != mascota.IdMascota)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _repositorioMascota.Editar(mascota);

                return RedirectToAction(nameof(Index));
            }

            CargarListas(mascota);

            return View(mascota);
        }


        public IActionResult Delete(int id)
        {
            var mascota = _repositorioMascota.ObtenerPorId(id);

            if (mascota == null)
            {
                return NotFound();
            }

            return View(mascota);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioMascota.Eliminar(id);

            return RedirectToAction(nameof(Index));
        }


        private void CargarListas(Mascota? mascota = null)
        {

            ViewBag.Clientes = new SelectList(
                _context.Clientes
                    .Where(c => c.Activo)
                    .Select(c => new
                    {
                        IdCliente = c.IdCliente,
                        NombreCompleto = c.Nombre + " " + c.Apellido
                    }),
                "IdCliente",
                "NombreCompleto",
                mascota?.IdCliente
            );



            ViewBag.Especies = new SelectList(
                _context.Especies
                    .Where(e => e.Activo),
                "IdEspecie",
                "Nombre",
                mascota?.IdEspecie
            );


            if (mascota != null && mascota.IdEspecie > 0)
            {
                var razas = _repositorioRaza
                    .ObtenerPorEspecie(mascota.IdEspecie);

                ViewBag.Razas = new SelectList(
                    razas,
                    "IdRaza",
                    "Nombre",
                    mascota.IdRaza
                );
            }
            else
            {

                ViewBag.Razas = new SelectList(
                    Enumerable.Empty<Raza>(),
                    "IdRaza",
                    "Nombre"
                );
            }
        }
    }
}