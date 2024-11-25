using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TendalProject.Models;


namespace TendalProject.Controllers
{

    public class CategoriaController : Controller
    {

        private readonly BdTendalDefinitivoContext _context;

        public CategoriaController(BdTendalDefinitivoContext context)
        {
            _context = context;
        }

        
        [Authorize(Roles = "Administrador")]
        public IActionResult ListaCategorias(int pageNumber = 1, int pageSize = 7)
        {
            var totalCategorias = _context.Categoria.Count(c => c.Estado == "ACTIVO");
            var categoriasActivas = _context.Categoria
                .Where(c => c.Estado == "ACTIVO")
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCategorias / (double)pageSize);
            ViewBag.CurrentPage = pageNumber;

            return View(categoriasActivas);
        }

        // GET: Categorium/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult RegistrarCategoria()
        {
            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            return View();
        }

        // POST: Categorium/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult RegistrarCategoria(Categorium categoria)
        {
            if (ModelState.IsValid)
            {
                categoria.FechaRegistro = DateTime.Now; // Establece la fecha de registro
                categoria.Estado = "ACTIVO"; // Establece el estado por defecto

                _context.Categoria.Add(categoria);
                _context.SaveChanges();

                return RedirectToAction(nameof(ListaCategorias)); // Redirige a la lista de categorías activas
            }

            return View(categoria); // Si hay errores, vuelve a mostrar el formulario
        }

        // GET: Categorium/Edit/5
        [Authorize(Roles = "Administrador")]
        public IActionResult ActualizarCategoria(int id)
        {
            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            var categoria = _context.Categoria.Find(id);
            if (categoria == null)
            {
                return NotFound();
            }
            return View(categoria);
        }

        // POST: Categorium/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult ActualizarCategoria(int id, Categorium categoriaActualizada)
        {
            if (id != categoriaActualizada.IdCategoria)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Recuperar la categoría existente de la base de datos
                var categoriaExistente = _context.Categoria.Find(id);
                if (categoriaExistente == null)
                {
                    return NotFound();
                }

                // Actualizar solo los campos que se pueden modificar
                categoriaExistente.NombreCategoria = categoriaActualizada.NombreCategoria;
                // No actualizamos FechaRegistro

                _context.Update(categoriaExistente);
                _context.SaveChanges();

                return RedirectToAction(nameof(ListaCategorias)); // Redirige a la lista de categorías activas
            }

            return View(categoriaActualizada); // Si hay errores, vuelve a mostrar el formulario
        }

        // POST: Categorium/Desactivar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult EliminarCategoria(int id)
        {
            var categoria = _context.Categoria.Find(id);
            if (categoria == null)
            {
                return NotFound();
            }

            categoria.Estado = "ELIMINADO";

            _context.Update(categoria);
            _context.SaveChanges();

            return RedirectToAction(nameof(ListaCategorias));
        }


        [Authorize(Roles = "Administrador")]
        public IActionResult ListaCategoriasDesactivadas(int pageNumber = 1, int pageSize = 5)
        {
            var totalCategorias = _context.Categoria.Count(c => c.Estado == "ACTIVO");
            var categoriasEliminadas = _context.Categoria
                .Where(c => c.Estado == "ELIMINADO")
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCategorias / (double)pageSize);
            ViewBag.CurrentPage = pageNumber;

            return View(categoriasEliminadas);
        }

        // POST: Categorium/Activar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult ActivarCategoria(int id)
        {
            var categoria = _context.Categoria.Find(id);
            if (categoria == null)
            {
                return NotFound();
            }

            // Cambiar el estado a "ACTIVO"
            categoria.Estado = "ACTIVO";

            _context.Update(categoria);
            _context.SaveChanges();

            return RedirectToAction(nameof(ListaCategorias)); // Redirige a la lista de categorías activas
        }
    }
}