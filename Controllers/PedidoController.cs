using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient; // Asegúrate de usar esto
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TendalProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace TendalProject.Controllers
{

    public class PedidoController : Controller
    {

        private readonly BdTendalDefinitivoContext _context;

        public PedidoController(BdTendalDefinitivoContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult ListaPedidos(int page = 1, int pageSize = 10, string searchNumeroPedido = "", string searchNombreUsuario = "")
        {
            var skip = (page - 1) * pageSize;


            var pedidosQuery = _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdEstadoNavigation)
                .Where(p => p.IdEstado == 1);

            if (!string.IsNullOrEmpty(searchNumeroPedido))
            {
                pedidosQuery = pedidosQuery.Where(p => p.NumeroPedido.Contains(searchNumeroPedido));
            }

            if (!string.IsNullOrEmpty(searchNombreUsuario))
            {
                pedidosQuery = pedidosQuery.Where(p => p.IdUsuarioNavigation.NombreUsuario.Contains(searchNombreUsuario));
            }

            var pedidos = pedidosQuery
                .OrderByDescending(p => p.FechaPedido)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            var metodosPago = _context.MetodoPagos.ToList();
            ViewBag.MetodosPago = metodosPago;


            var totalPedidos = pedidosQuery.Count();


            var totalPages = (int)Math.Ceiling(totalPedidos / (double)pageSize);


            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pedidos);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult EntregarPedido(string numeroPedido, int idMetodoPago)
        {
            if (string.IsNullOrEmpty(numeroPedido) || idMetodoPago <= 0)
            {
                return BadRequest("Número de pedido o ID del método de pago no válidos.");
            }

            try
            {
                // Llamar al procedimiento almacenado
                _context.Database.ExecuteSqlRaw("EXEC EntregarPedido @NumeroPedido, @IdMetodoPago",
                    new SqlParameter("@NumeroPedido", numeroPedido),
                    new SqlParameter("@IdMetodoPago", idMetodoPago));

                TempData["SuccessMessage"] = "Pedido entregado exitosamente.";
                return RedirectToAction("ListaPedidos");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Error al entregar el pedido: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inesperado: {ex.Message}");
            }
        }

        public IActionResult ExportPedidosXlsx()
        {
            var pedidos = _context.Pedidos
                .Where(p => p.IdEstado == 1) // Filtrar por IdEstado = 1
                .Include(r => r.IdEstadoNavigation) // Incluye la relación IdEstadoNavigation
                .Include(r => r.IdUsuarioNavigation) // Incluye la relación IdUsuarioNavigation
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Pedidos");
                var currentRow = 1;

                // Encabezados de las columnas con formato
                worksheet.Cell(currentRow, 1).Value = "Número de Pedido";
                worksheet.Cell(currentRow, 2).Value = "Cliente";
                worksheet.Cell(currentRow, 3).Value = "Producto";
                worksheet.Cell(currentRow, 4).Value = "Cantidad";
                worksheet.Cell(currentRow, 5).Value = "Precio";
                worksheet.Cell(currentRow, 6).Value = "Estado";
                worksheet.Cell(currentRow, 7).Value = "Fecha de Pedido";

                // Aplicar formato a los encabezados
                worksheet.Range(1, 1, 1, 7).Style.Font.Bold = true;
                worksheet.Range(1, 1, 1, 7).Style.Fill.BackgroundColor = XLColor.Lime;
                worksheet.Range(1, 1, 1, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Ajustar el ancho de las columnas
                worksheet.Columns(1, 13).AdjustToContents();

                // Configurar el ancho manualmente para mayor separación
                worksheet.Column(1).Width = 30;
                worksheet.Column(2).Width = 25;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 10;
                worksheet.Column(6).Width = 15;
                worksheet.Column(7).Width = 30;

                // Datos de los pedidos
                foreach (var pedido in pedidos)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = pedido.NumeroPedido;
                    worksheet.Cell(currentRow, 2).Value = pedido.IdUsuarioNavigation?.NombreUsuario;
                    worksheet.Cell(currentRow, 3).Value = pedido.IdProducto;
                    worksheet.Cell(currentRow, 4).Value = pedido.Cantidad;
                    worksheet.Cell(currentRow, 5).Value = pedido.Importe;
                    worksheet.Cell(currentRow, 6).Value = pedido.IdEstadoNavigation?.NombreEstado;
                    worksheet.Cell(currentRow, 7).Value = pedido.FechaPedido?.ToString("yyyy-MM-dd HH:mm");
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    // Formato de fecha para el nombre del archivo
                    var fechaDescarga = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                    var fileName = $"Pedidos_{fechaDescarga}.xlsx";

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult ListaPedidosEntregados(int pagina = 1, int paginasTotales = 10, string buscarNumeroPedido = "", string buscarNombreCliente = "")
        {
            var skip = (pagina - 1) * paginasTotales;

            // Obtener todos los registros de PedidoEntregado con paginación y filtros
            var pedidosEntregadosQuery = _context.PedidoEntregados
                .Include(pe => pe.IdUsuarioNavigation)
                .Include(pe => pe.IdProductoNavigation)
                .Include(pe => pe.IdMetodoPagoNavigation)
                .AsQueryable(); // Cambia a IQueryable para permitir paginación eficiente

            if (!string.IsNullOrEmpty(buscarNumeroPedido))
            {
                pedidosEntregadosQuery = pedidosEntregadosQuery.Where(p => p.NumeroPedido.Contains(buscarNumeroPedido));
            }

            if (!string.IsNullOrEmpty(buscarNombreCliente))
            {
                pedidosEntregadosQuery = pedidosEntregadosQuery.Where(p => p.IdUsuarioNavigation.NombreUsuario.Contains(buscarNombreCliente));
            }

            var totalPedidoEntregados = pedidosEntregadosQuery.Count(); // Contar los pedidos entregados después de aplicar filtros

            var totalPaginas = (int)Math.Ceiling(totalPedidoEntregados / (double)paginasTotales);

            var pedidosEntregados = pedidosEntregadosQuery
                .OrderByDescending(p => p.FechaEntrega)
                .Skip(skip)
                .Take(paginasTotales)
                .ToList();



            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;

            ViewBag.CurrentPage = pagina;
            ViewBag.TotalPages = totalPaginas;

            return View(pedidosEntregados);
        }


        public IActionResult ExportPedidosEntregadosXlsx()
        {
            var pedidosEntregados = _context.PedidoEntregados
                .Include(pe => pe.IdUsuarioNavigation)  // Incluye la relación IdUsuarioNavigation
                .Include(pe => pe.IdProductoNavigation) // Incluye la relación IdProductoNavigation
                .Include(pe => pe.IdMetodoPagoNavigation) // Incluye la relación IdMetodoPagoNavigation
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Pedidos Entregados");
                var currentRow = 1;

                // Encabezados de las columnas con formato
                worksheet.Cell(currentRow, 1).Value = "Número de Pedido";
                worksheet.Cell(currentRow, 2).Value = "Cliente";
                worksheet.Cell(currentRow, 3).Value = "Producto";
                worksheet.Cell(currentRow, 4).Value = "Cantidad";
                worksheet.Cell(currentRow, 5).Value = "Precio";
                worksheet.Cell(currentRow, 6).Value = "Estado";
                worksheet.Cell(currentRow, 7).Value = "Método de Pago";
                worksheet.Cell(currentRow, 8).Value = "Fecha de Pedido";
                worksheet.Cell(currentRow, 9).Value = "Fecha de Entrega";

                // Aplicar formato a los encabezados
                worksheet.Range(1, 1, 1, 9).Style.Font.Bold = true;
                worksheet.Range(1, 1, 1, 9).Style.Fill.BackgroundColor = XLColor.Lime;
                worksheet.Range(1, 1, 1, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Ajustar el ancho de las columnas
                worksheet.Columns(1, 7).AdjustToContents();

                // Configurar el ancho manualmente para mayor separación
                worksheet.Column(1).Width = 30;
                worksheet.Column(2).Width = 25;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 10;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 30;
                worksheet.Column(8).Width = 30;
                worksheet.Column(9).Width = 30;

                // Datos de los pedidos entregados
                foreach (var pedidoEntregado in pedidosEntregados)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = pedidoEntregado.NumeroPedido;
                    worksheet.Cell(currentRow, 2).Value = pedidoEntregado.IdUsuarioNavigation?.NombreUsuario;
                    worksheet.Cell(currentRow, 3).Value = pedidoEntregado.IdProductoNavigation?.Nombre;
                    worksheet.Cell(currentRow, 4).Value = pedidoEntregado.Cantidad;
                    worksheet.Cell(currentRow, 5).Value = pedidoEntregado.Importe;
                    worksheet.Cell(currentRow, 6).Value = pedidoEntregado.Estado;
                    worksheet.Cell(currentRow, 7).Value = pedidoEntregado.IdMetodoPagoNavigation?.Metodo;
                    worksheet.Cell(currentRow, 8).Value = pedidoEntregado.FechaPedido?.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(currentRow, 9).Value = pedidoEntregado.FechaEntrega?.ToString("yyyy-MM-dd HH:mm");
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    // Formato de fecha para el nombre del archivo
                    var fechaDescarga = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                    var fileName = $"Pedidos_Entregados_{fechaDescarga}.xlsx";

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

    }
}