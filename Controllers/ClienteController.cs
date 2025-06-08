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
using TendalProject.Models;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System.Text.Json;
namespace TendalProject.Controllers
{
    public class ClienteController : Controller
    {

        private readonly BdTendalDefinitivoContext _context;
        private static CarritoViewModel _carrito = new CarritoViewModel();

        public ClienteController(BdTendalDefinitivoContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Cliente")]
        public IActionResult ListaProductos()
        {
            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;

            // Cargar las categorías para el SelectList
            var categorias = _context.Categoria.ToList();
            ViewBag.Categorias = categorias;

            // Cargar solo los productos que están en estado 'ACTIVO'
            var articulos = _context.Productos
                .Where(p => p.Estado == "ACTIVO")
                .ToList();

            return View(articulos);
        }

        public IActionResult BuscarProductos(string query)
        {
            var productos = _context.Productos
                                    .Where(p => p.Nombre.Contains(query) && p.Estado == "ACTIVO")  // Filtramos por nombre de producto y estado activo
                                    .ToList();

            return PartialView("_ListaProductosTable", productos);
        }

        public async Task<IActionResult> FiltrarPorCategoria(int categoria = 0)
        {
            // Filtrar productos que estén en estado 'ACTIVO'
            var productosFiltrados = _context.Productos
                .Where(p => p.Estado == "ACTIVO");

            // Aplicar filtro por categoría si se especifica una categoría
            if (categoria != 0)
            {
                productosFiltrados = productosFiltrados.Where(p => p.IdCategoria == categoria);
            }

            var productos = await productosFiltrados.ToListAsync();
            return PartialView("_ListaProductosTable", productos);
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public IActionResult AgregarAlCarrito(string codigoProducto, int cantidad = 1)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.CodigoProducto == codigoProducto);

            if (producto == null)
            {
                TempData["Error"] = "El producto no existe.";
                return RedirectToAction("ListaProductos");
            }

            if (producto.Stock < cantidad)
            {
                TempData["Error"] = "No hay suficiente stock disponible.";
                return RedirectToAction("ListaProductos");
            }

            var itemExistente = _carrito.Items.FirstOrDefault(i => i.Producto.CodigoProducto == codigoProducto);

            if (itemExistente != null)
            {
                if (itemExistente.Cantidad + cantidad > producto.Stock)
                {
                    TempData["Error"] = "La cantidad solicitada excede el stock disponible.";
                    return RedirectToAction("Carrito");
                }

                itemExistente.Cantidad += cantidad;
            }
            else
            {
                // Asegura que la cantidad inicial siempre sea 1
                _carrito.Items.Add(new CarritoItemViewModel
                {
                    Producto = producto,
                    Cantidad = 1
                });
            }

            // Agregar mensaje de confirmación
            TempData["Success"] = $"El producto '{producto.Nombre}' se ha agregado al carrito.";

            return RedirectToAction("ListaProductos");
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public IActionResult ActualizarCantidad(string codigoProducto, int nuevaCantidad)
        {
            var item = _carrito.Items.FirstOrDefault(i => i.Producto.CodigoProducto == codigoProducto);

            if (item == null)
            {
                TempData["Error"] = "El producto no se encuentra en el carrito.";
                return RedirectToAction("Carrito");
            }

            if (item.Producto.Stock < nuevaCantidad)
            {
                TempData["Error"] = "No hay suficiente stock disponible para la cantidad solicitada.";
                return RedirectToAction("Carrito");
            }

            item.Cantidad = nuevaCantidad;
            return RedirectToAction("Carrito");
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public IActionResult EliminarDelCarrito(string codigoProducto)
        {
            // Busca el item en el carrito
            var itemExistente = _carrito.Items.FirstOrDefault(i => i.Producto.CodigoProducto == codigoProducto);

            if (itemExistente != null)
            {
                // Elimina el item del carrito
                _carrito.Items.Remove(itemExistente);
                TempData["Success"] = "Producto eliminado del carrito.";
            }
            else
            {
                TempData["Error"] = "El producto no se encontró en el carrito.";
            }

            return RedirectToAction("Carrito");
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public IActionResult Pedir()
        {
            // Obtén el id del usuario de la sesión
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            // Obtener el último pedido para generar el nuevo número de pedido
            var ultimoNumeroPedido = _context.Pedidos
                .OrderByDescending(v => v.IdPedido)
                .Select(v => v.NumeroPedido)
                .FirstOrDefault();

            int numeroPedidoActual = 1; // Comenzamos desde 1
            if (!string.IsNullOrEmpty(ultimoNumeroPedido) &&
                ultimoNumeroPedido.StartsWith("PE") &&
                int.TryParse(ultimoNumeroPedido.Substring(2), out int ultimoNumero))
            {
                numeroPedidoActual = ultimoNumero + 1; // Incrementamos el último número
            }

            string nuevoNumeroPedido = $"PE{numeroPedidoActual:D4}"; // Generamos el nuevo número en el formato "PE0001"

            int totalProductosSeleccionados = 0; // Para contar el total de productos únicos seleccionados

            // Recorre los productos en el carrito, crea un pedido por cada uno y actualiza el stock
            foreach (var item in _carrito.Items)
            {
                // Para contar solo los productos únicos seleccionados
                totalProductosSeleccionados++;

                var pedido = new Pedido
                {
                    NumeroPedido = nuevoNumeroPedido,
                    IdUsuario = idUsuario ?? 0,
                    IdProducto = item.Producto.CodigoProducto,
                    Cantidad = item.Cantidad,
                    Importe = item.Total,
                    IdEstado = 1, // Suponiendo que el estado por defecto es "Pendiente". Ajusta según tu lógica.
                    FechaPedido = DateTime.Now
                };

                _context.Pedidos.Add(pedido);

                // Actualiza el stock del producto correspondiente
                var producto = _context.Productos.FirstOrDefault(p => p.CodigoProducto == item.Producto.CodigoProducto);
                if (producto != null)
                {
                    producto.Stock -= item.Cantidad;
                    if (producto.Stock < 0) producto.Stock = 0; // Asegúrate de que el stock no sea negativo
                }
            }

            // Guarda los cambios en la base de datos
            _context.SaveChanges();

            // Almacena el número de pedido y la cantidad total de productos seleccionados en TempData
            TempData["NumeroVenta"] = nuevoNumeroPedido;
            TempData["TotalProductosSeleccionados"] = totalProductosSeleccionados;

            // Limpia el carrito después de realizar el pedido
            LimpiarCarrito();

            // Redirige a una página de confirmación
            return RedirectToAction("Confirmacion");
        }

        [Authorize(Roles = "Cliente")]
        public IActionResult Confirmacion()
        {
            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            return View();
        }

        [Authorize(Roles = "Cliente")]
        public IActionResult Carrito()
        {
            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;

            return View(_carrito);
        }

        public static void LimpiarCarrito()
        {
            _carrito.Items.Clear(); // Limpia los items del carrito
        }

        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisPedidos()
        {
            // Obtener el IdUsuario de la sesión
            int userId = (int)HttpContext.Session.GetInt32("IdUsuario")!;

            // Obtener los pedidos con IdEstado = 1 y el IdUsuario igual al usuario de la sesión
            var pedidos = await _context.Pedidos
                .Where(p => p.IdEstado == 1 && p.IdUsuario == userId)
                .Include(p => p.IdEstadoNavigation)   // Incluir la información del estado
                .Include(p => p.IdProductoNavigation) // Incluir la información del producto
                .Include(p => p.IdUsuarioNavigation)  // Incluir la información del usuario
                .OrderByDescending(p => p.FechaPedido) // Ordenar por fecha de forma descendente
                .ToListAsync();

            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;



            return View(pedidos);  // Pasar los pedidos a la vista
        }

    }
}