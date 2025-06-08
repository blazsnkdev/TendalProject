using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TendalProject.Models;
using TendalProject.Service;
namespace TendalProject.Controllers
{
    public class UsuarioController : Controller
    {

        private readonly BdTendalDefinitivoContext _context;
        private readonly UsuarioService _usuarioService; 
        public UsuarioController(BdTendalDefinitivoContext context, UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string clave)
        {
           
            var usuario = await _usuarioService.ValidarUsuarioAsync(correo, clave);

            if (usuario != null)
            {
            
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim("Correo", usuario.Correo),
               new Claim(ClaimTypes.Role, usuario.IdRolNavigation.NombreRol) 
            };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            
                HttpContext.Session.SetString("NombreUsuario", usuario.NombreUsuario);
    
                HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);


        
                if (usuario.IdRol == 1) 
                {
                    return RedirectToAction("Dashboard", "Administrador");
                }
                else if (usuario.IdRol == 2) 
                {
                    return RedirectToAction("ListaProductos", "Cliente");
                }
            }

            ModelState.AddModelError("", "Correo o clave incorrectos.");
            return View();
        }

        
        public async Task<IActionResult> Logout()
        {

            ClienteController.LimpiarCarrito();
        
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); 
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarUsuarioLogin(string nombreUsuario, string apellidoPaterno, string apellidoMaterno, string celular, string direccion, string correo, string clave, int idRol)
        {
           
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
            {
                ModelState.AddModelError(string.Empty, "Los campos Nombre de Usuario, Correo y Clave son obligatorios.");
                return View("Login"); 
            }

          
            var resultado = await _usuarioService.InsertarUsuarioLogin(nombreUsuario, apellidoPaterno, apellidoMaterno, celular, direccion, correo, clave);

        
            if (resultado.Contains("exitosamente"))
            {
                TempData["Success"] = $"Se a registrado correctamente.";
                return RedirectToAction("Login");
            }
            else if (resultado.Contains("ya existe"))
            {
                TempData["Success"] = $"Se a registrado correctamente.";
                return View("Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado); 
                return View("Login");
            }
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ListaUsuarios(int? roleFilter = null, int pageNumber = 1, int pageSize = 4)
        {
            
            var usuariosQuery = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .Where(u => u.Estado == "ACTIVO") 
                .AsQueryable();

            if (roleFilter.HasValue)
            {
                usuariosQuery = usuariosQuery.Where(u => u.IdRol == roleFilter.Value);
            }

         
            var totalUsers = await usuariosQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            var usuarios = await usuariosQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.RoleFilter = roleFilter;

            return View(usuarios);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult FormularioUsuario()
        {

            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            
            ViewData["RolId"] = new SelectList(_context.Rols, "IdRol", "NombreRol");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RegistrarUsuario(string nombreUsuario, string apellidoPaterno, string apellidoMaterno, string celular, string direccion, string correo, string clave, int idRol)
        {
            
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
            {
                ModelState.AddModelError(string.Empty, "Los campos Nombre de Usuario, Correo y Clave son obligatorios.");
                
                ViewData["RolId"] = new SelectList(_context.Rols, "IdRol", "NombreRol", idRol);
                return View("FormularioUsuario"); 
            }


         
            var resultado = await _usuarioService.InsertarUsuarioAsync(nombreUsuario, apellidoPaterno, apellidoMaterno, celular, direccion, correo, clave, idRol);

           
            if (resultado.Contains("exitosamente"))
            {
                return RedirectToAction("ListaUsuarios");
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado); 
             
                ViewData["RolId"] = new SelectList(_context.Rols, "IdRol", "NombreRol", idRol);
                return View("FormularioUsuario"); 
            }
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarUsuario(int id)
        {
            
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }
             var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
           
            ViewData["RolId"] = new SelectList(_context.Rols, "IdRol", "NombreRol", usuario.IdRol);
            return View(usuario); 
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarUsuario(int id, string nombreUsuario, string apellidoPaterno, string apellidoMaterno, string celular, string direccion, string correo, string clave, int idRol)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

           
            usuario.NombreUsuario = nombreUsuario;
            usuario.ApellidoPaterno = apellidoPaterno;
            usuario.ApellidoMaterno = apellidoMaterno;
            usuario.Celular = celular;
            usuario.Direccion = direccion;
            usuario.Correo = correo;
            usuario.IdRol = idRol;

            

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("ListaUsuarios");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar el usuario.");
                return View(usuario);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult EliminarUsuario(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Estado = "ELIMINADO";

            _context.Update(usuario);
            _context.SaveChanges();

            return RedirectToAction(nameof(ListaUsuarios));
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ListaUsuariosEliminados(int? roleFilter = null, int pageNumber = 1, int pageSize = 8)
        {
            
            var usuariosQuery = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .Where(u => u.Estado == "ELIMINADO")
                .AsQueryable();

            if (roleFilter.HasValue)
            {
                usuariosQuery = usuariosQuery.Where(u => u.IdRol == roleFilter.Value);
            }

            // Paginación
            var totalUsers = await usuariosQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);
            var usuarios = await usuariosQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.RoleFilter = roleFilter;

            return View(usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult ActivarUsuario(int id)
        {
            var usuario = _context.Usuarios.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }

      
            usuario.Estado = "ACTIVO";

            _context.Update(usuario);
            _context.SaveChanges();

            return RedirectToAction(nameof(ListaUsuarios)); 
        }

    }
}