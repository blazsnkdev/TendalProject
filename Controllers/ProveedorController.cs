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

    public class ProveedorController : Controller
    {

        private readonly BdTendalDefinitivoContext _context; // Reemplaza con tu contexto real

        public ProveedorController(BdTendalDefinitivoContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Administrador")]
        public IActionResult ListaProveedores(int pageNumber = 1, int pageSize = 7)
        {
            // Filtrar proveedores activos
            var proveedores = _context.Proveedors
                .Where(p => p.Estado == "ACTIVO") // Filtra por estado "ACTIVO"
                .OrderBy(p => p.IdProveedor); // Ordena por IdProveedor

            // Obtener el total de registros
            var totalCount = proveedores.Count();

            // Aplicar paginación
            var proveedoresPaginados = proveedores
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Calcular total de páginas
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            // Pasar datos a la vista
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(proveedoresPaginados);

        }



        [Authorize(Roles = "Administrador")]
        public IActionResult RegistrarProveedor()
        {
            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            return View(); // Devuelve la vista para registrar un nuevo proveedor
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult RegistrarProveedor(Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                // Asignar estado por defecto
                proveedor.Estado = "ACTIVO";

                // Agregar el proveedor al contexto
                _context.Proveedors.Add(proveedor);
                _context.SaveChanges(); // Guarda los cambios en la base de datos

                return RedirectToAction("ListaProveedores"); // Redirige a la lista de proveedores
            }
            return View(proveedor); // Si hay errores, vuelve a mostrar el formulario
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult ActualizarProveedor(int id)
        {
            var proveedor = _context.Proveedors.Find(id); // Busca el proveedor por Id
            if (proveedor == null)
            {
                return NotFound(); // Devuelve un error 404 si no se encuentra
            }
            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            return View(proveedor); // Devuelve la vista para actualizar el proveedor
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult ActualizarProveedor(int id, Proveedor proveedorActualizada)
        {
            if (id != proveedorActualizada.IdProveedor)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Recuperar la categoría existente de la base de datos
                var proveedorExistente = _context.Proveedors.Find(id);
                if (proveedorExistente == null)
                {
                    return NotFound();
                }

                // Actualizar solo los campos que se pueden modificar
                proveedorExistente.NombreProveedor = proveedorActualizada.NombreProveedor;
                proveedorExistente.Telefono = proveedorActualizada.Telefono;
                proveedorExistente.Direccion = proveedorActualizada.Direccion;
                // No actualizamos FechaServicio

                _context.Update(proveedorExistente);
                _context.SaveChanges();

                return RedirectToAction(nameof(ListaProveedores)); // Redirige a la lista de proveedores
            }

            return View(proveedorActualizada); // Si hay errores, vuelve a mostrar el formulario
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult EliminarProveedor(int id)
        {
            var proveedor = _context.Proveedors.Find(id);
            if (proveedor == null)
            {
                return NotFound();
            }

            proveedor.Estado = "ELIMINADO";

            _context.Update(proveedor);
            _context.SaveChanges();

            return RedirectToAction(nameof(ListaProveedores));
        }


        [Authorize(Roles = "Administrador")]
        public IActionResult ListaProveedoresDesactivados(int pageNumber = 1, int pageSize = 5)
        {
            var totalProveedores = _context.Proveedors.Count(c => c.Estado == "ACTIVO");
            var proveedoresEliminados = _context.Proveedors
                .Where(c => c.Estado == "ELIMINADO")
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            ViewBag.TotalPages = (int)Math.Ceiling(totalProveedores / (double)pageSize);
            ViewBag.CurrentPage = pageNumber;

            return View(proveedoresEliminados);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult ActivarProveedor(int id)
        {
            var proveedor = _context.Proveedors.Find(id);
            if (proveedor == null)
            {
                return NotFound();
            }

            // Cambiar el estado a "ACTIVO"
            proveedor.Estado = "ACTIVO";

            _context.Update(proveedor);
            _context.SaveChanges();

            return RedirectToAction(nameof(ListaProveedores)); // Redirige a la lista de categorías activas
        }




    }
}