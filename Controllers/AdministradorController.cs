using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TendalProject.Models;

namespace TendalProject.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdministradorController : Controller
    {
        private readonly BdTendalDefinitivoContext _context;


        public AdministradorController(BdTendalDefinitivoContext context)
        {
            _context = context;
        }

        
        public IActionResult Dashboard(){
            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;

            int totalCategoriasActivas = _context.Categoria.Count(c => c.Estado == "ACTIVO");
            int totalProductosActivos = _context.Productos.Count(p => p.Estado == "ACTIVO");
            int totalProveedoresActivos = _context.Proveedors.Count(pr => pr.Estado == "ACTIVO");
            int totalPedidosActivos = _context.Pedidos.Count(p => p.IdEstadoNavigation.NombreEstado == "PROCESO");
            int totalPedidosEntregados = _context.PedidoEntregados.Count(p => p.Estado == "ENTREGADO");


            int totalAdministradores = _context.Usuarios.Count(u => u.IdRol == 1 && u.Estado == "ACTIVO");
            int totalClientes = _context.Usuarios.Count(u=>u.IdRol == 2 && u.Estado == "ACTIVO");


                        // En tu controlador
            var hoy = DateTime.Today;
            var totalImporteHoy = _context.PedidoEntregados
                .Where(p => p.FechaEntrega.HasValue && p.FechaEntrega.Value.Date == hoy)
                .Sum(p => p.Importe);

            ViewBag.TotalImporteHoy = totalImporteHoy;


                var totalPedidosHoy = _context.PedidoEntregados
                    .Where(p => p.FechaEntrega.HasValue && p.FechaEntrega.Value.Date == hoy)
                    .Count();

                ViewBag.TotalPedidosHoy = totalPedidosHoy;

                        
                            
                var totalStockActivo = _context.Productos
                    .Where(p => p.Estado == "ACTIVO")
                    .Sum(p => p.Stock);
                ViewBag.TotalStockActivo = totalStockActivo;




            var productoMasVendido = _context.Pedidos
                    .GroupBy(p => p.IdProducto)
                    .Select(g => new
                    {
                        ProductoId = g.Key,
                        Cantidad = g.Count()
                    })
                    .OrderByDescending(g => g.Cantidad)
                    .FirstOrDefault();

                    if (productoMasVendido != null)
                    {
                        var producto = _context.Productos.Find(productoMasVendido.ProductoId);
                        ViewBag.ProductoMasVendidoNombre = producto?.Nombre ?? "Desconocido";
                        ViewBag.ProductoMasVendidoCantidad = productoMasVendido.Cantidad;
                    }
                    else
                    {
                        ViewBag.ProductoMasVendidoNombre = "No disponible";
                        ViewBag.ProductoMasVendidoCantidad = 0;
                    }


                    // Obtener el cliente con más pedidos
                    var clienteConMasPedidos = _context.Pedidos
                        .GroupBy(p => p.IdUsuario)
                        .OrderByDescending(g => g.Count())
                        .Select(g => new
                        {
                            NombreCliente = g.FirstOrDefault().IdUsuarioNavigation.NombreUsuario, // Asegúrate de que el nombre del usuario esté disponible
                            TotalPedidos = g.Count()
                        })
                        .FirstOrDefault();

                    ViewBag.ClienteConMasPedidosNombre = clienteConMasPedidos?.NombreCliente ?? "No disponible";
                    ViewBag.ClienteConMasPedidosCantidad = clienteConMasPedidos?.TotalPedidos ?? 0;





            ViewBag.TotalAdministradores = totalAdministradores;
            ViewBag.TotalClientes = totalClientes;
            ViewBag.TotalPedidosActivos = totalPedidosActivos;
            ViewBag.TotalPedidosEntregados = totalPedidosEntregados;

            ViewBag.TotalCategoriasActivas = totalCategoriasActivas;
            ViewBag.TotalProductosActivos = totalProductosActivos;
            ViewBag.TotalProveedoresActivos = totalProveedoresActivos;


            return View();
        }





    }
}
