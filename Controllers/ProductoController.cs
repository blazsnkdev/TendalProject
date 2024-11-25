using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TendalProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using Rotativa.AspNetCore;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;

namespace TendalProject.Controllers
{
    public class ProductoController : Controller
    {


        private readonly BdTendalDefinitivoContext _context;

        public ProductoController(BdTendalDefinitivoContext context)
        {
            _context = context;
        }


        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ListaProductos(int pageNumber = 1, int pageSize = 4, int? categoriaId = null, int? proveedorId = null)
        {
            ViewBag.Categorias = new SelectList(await _context.Categoria
                .Where(c => c.Estado == "ACTIVO")
                .ToListAsync(), "IdCategoria", "NombreCategoria");

            ViewBag.Proveedores = new SelectList(await _context.Proveedors
                .Where(p => p.Estado == "ACTIVO")
                .ToListAsync(), "IdProveedor", "NombreProveedor");

            // Query para filtrar productos activos
            var query = _context.Productos
                .Where(p => p.Estado == "ACTIVO")
                .AsQueryable();

            // Filtrar por categoría y proveedor si se seleccionan
            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.IdCategoria == categoriaId.Value);
            }
            if (proveedorId.HasValue)
            {
                query = query.Where(p => p.IdProveedor == proveedorId.Value);
            }

            // Aplicar paginación y ordenar por fecha de creación en orden descendente
            var productosActivos = await query
                .OrderByDescending(p => p.FechaCreacion) // Asegúrate de que este campo existe
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalProductos = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProductos / (double)pageSize);

            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            // Pasar los datos a través de ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.SelectedCategoria = categoriaId;
            ViewBag.SelectedProveedor = proveedorId;

            return View(productosActivos);
        }
        

        public IActionResult ExportProductosXlsx()
        {
            var productos = _context.Productos
                .Include(r => r.IdCategoriaNavigation) // dentro están creadas las relaciones IdEstadoNavigation entre tablas
                .Include(r => r.IdProveedorNavigation)
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Productos");
                var currentRow = 1;

                // Encabezados de las columnas con formato
                worksheet.Cell(currentRow, 1).Value = "Codigo";
                worksheet.Cell(currentRow, 2).Value = "Nombre";
                worksheet.Cell(currentRow, 3).Value = "Descripcion";
                worksheet.Cell(currentRow, 4).Value = "Categoria";
                worksheet.Cell(currentRow, 5).Value = "Precio";
                worksheet.Cell(currentRow, 6).Value = "Stock";
                worksheet.Cell(currentRow, 7).Value = "Proveedor";
                worksheet.Cell(currentRow, 8).Value = "Estado";
                worksheet.Cell(currentRow, 9).Value = "FechaCreacion";


                // Aplicar formato a los encabezados
                worksheet.Range(1, 1, 1, 13).Style.Font.Bold = true;
                worksheet.Range(1, 1, 1, 13).Style.Fill.BackgroundColor = XLColor.Lime;
                worksheet.Range(1, 1, 1, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Ajustar el ancho de las columnas
                worksheet.Columns(1, 13).AdjustToContents();

                // Configurar el ancho manualmente para mayor separación
                worksheet.Column(1).Width = 15;
                worksheet.Column(2).Width = 25;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 10;
                worksheet.Column(6).Width = 10;
                worksheet.Column(7).Width = 25;
                worksheet.Column(8).Width = 15;
                worksheet.Column(9).Width = 15;

                // Datos de los pedidos
                foreach (var producto in productos)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = producto.CodigoProducto;
                    worksheet.Cell(currentRow, 2).Value = producto.Nombre;
                    worksheet.Cell(currentRow, 3).Value = producto.Descripcion;
                    worksheet.Cell(currentRow, 4).Value = producto.IdCategoriaNavigation?.NombreCategoria;
                    worksheet.Cell(currentRow, 5).Value = producto.Precio;
                    worksheet.Cell(currentRow, 6).Value = producto.Stock;
                    worksheet.Cell(currentRow, 7).Value = producto.IdProveedorNavigation?.NombreProveedor;
                    worksheet.Cell(currentRow, 8).Value = producto.Estado;
                    worksheet.Cell(currentRow, 9).Value = producto.FechaCreacion?.ToString("yyyy-MM-dd HH:mm");

                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    // Formato de fecha para el nombre del archivo
                    var fechaDescarga = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                    var fileName = $"Productos_{fechaDescarga}.xlsx";

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }//fin del metodo exportacion


        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RegistrarProducto()
        {
            ViewBag.Categorias = new SelectList(await _context.Categoria
                .Where(c => c.Estado == "ACTIVO")
                .ToListAsync(), "IdCategoria", "NombreCategoria");

            ViewBag.Proveedores = new SelectList(await _context.Proveedors
                .Where(p => p.Estado == "ACTIVO")
                .ToListAsync(), "IdProveedor", "NombreProveedor");

            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RegistrarProducto(ProductoViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Convertir IFormFile a un arreglo de bytes para la imagen
                byte[]? imagenData = null;
                if (model.ImagenFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ImagenFile.CopyToAsync(memoryStream);
                        imagenData = memoryStream.ToArray();
                    }
                }

                // Llamar al procedimiento almacenado para insertar el producto
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
            EXEC InsertarProducto 
                @Nombre = {model.Nombre}, 
                @Descripcion = {model.Descripcion}, 
                @IdCategoria = {model.IdCategoria}, 
                @Precio = {model.Precio}, 
                @Stock = {model.Stock}, 
                @IdProveedor = {model.IdProveedor}, 
                @Imagen = {imagenData}");

                return RedirectToAction(nameof(ListaProductos)); // Redirigir a la página de lista
            }

            // Si llegamos a este punto, algo falló, volver a mostrar el formulario
            ViewBag.Categorias = new SelectList(await _context.Categoria
                .Where(c => c.Estado == "ACTIVO")
                .ToListAsync(), "IdCategoria", "NombreCategoria", model.IdCategoria);

            ViewBag.Proveedores = new SelectList(await _context.Proveedors
                .Where(p => p.Estado == "ACTIVO")
                .ToListAsync(), "IdProveedor", "NombreProveedor", model.IdProveedor);

            return View(model);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarProducto(string codigoProducto)
        {
            if (string.IsNullOrEmpty(codigoProducto))
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdProveedorNavigation)
                .FirstOrDefaultAsync(p => p.CodigoProducto == codigoProducto);

            if (producto == null)
            {
                return NotFound();
            }

            ViewBag.Categorias = new SelectList(await _context.Categoria
                .Where(c => c.Estado == "ACTIVO")
                .ToListAsync(), "IdCategoria", "NombreCategoria", producto.IdCategoria);

            ViewBag.Proveedores = new SelectList(await _context.Proveedors
                .Where(p => p.Estado == "ACTIVO")
                .ToListAsync(), "IdProveedor", "NombreProveedor", producto.IdProveedor);

            // Convertir la imagen a base64 si existe
            string? imagenBase64 = null;
            if (producto.Imagen != null)
            {
                imagenBase64 = Convert.ToBase64String(producto.Imagen);
            }

            var model = new ProductoViewModel
            {
                CodigoProducto = producto.CodigoProducto,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                IdCategoria = producto.IdCategoria,
                Precio = producto.Precio,
                Stock = producto.Stock,
                IdProveedor = producto.IdProveedor,
                ImagenActual = imagenBase64 // Asignar la imagen actual
            };

            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;

            return View(model);
        }

        // POST: Editar Producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarProducto(string codigoProducto, ProductoViewModel model)
        {
            if (codigoProducto != model.CodigoProducto) // Asegúrate de que esta propiedad esté presente
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var producto = await _context.Productos.FindAsync(codigoProducto);

                if (producto == null)
                {
                    return NotFound();
                }

                // Actualizar las propiedades del producto
                producto.Nombre = model.Nombre;
                producto.Descripcion = model.Descripcion;
                producto.IdCategoria = model.IdCategoria;
                producto.Precio = model.Precio;
                producto.Stock = model.Stock;
                producto.IdProveedor = model.IdProveedor;

                // Si se sube una nueva imagen, convertir y asignar
                if (model.ImagenFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ImagenFile.CopyToAsync(memoryStream);
                        producto.Imagen = memoryStream.ToArray();
                    }
                }

                // Guardar los cambios
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListaProductos)); // Redirigir a la lista de productos
            }

            // Si llegamos a este punto, algo falló, volver a mostrar el formulario
            ViewBag.Categorias = new SelectList(await _context.Categoria
                .Where(c => c.Estado == "ACTIVO")
                .ToListAsync(), "IdCategoria", "NombreCategoria", model.IdCategoria);

            ViewBag.Proveedores = new SelectList(await _context.Proveedors
                .Where(p => p.Estado == "ACTIVO")
                .ToListAsync(), "IdProveedor", "NombreProveedor", model.IdProveedor);

            return View(model);
        }

        // POST: Eliminar Producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarProducto(string codigoProducto)
        {
            if (string.IsNullOrEmpty(codigoProducto))
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(codigoProducto);

            if (producto == null)
            {
                return NotFound();
            }

            // Cambiar el estado a "ELIMINADO"
            producto.Estado = "ELIMINADO";

            // Guardar los cambios
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ListaProductos)); // Redirigir a la lista de productos
        }




        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ListaProductosEliminados(int pageNumber = 1, int pageSize = 10, int? categoriaId = null, int? proveedorId = null)
        {
            // Cargar categorías y proveedores activos para los filtros
            ViewBag.Categorias = new SelectList(await _context.Categoria
                .Where(c => c.Estado == "ACTIVO")
                .ToListAsync(), "IdCategoria", "NombreCategoria");

            ViewBag.Proveedores = new SelectList(await _context.Proveedors
                .Where(p => p.Estado == "ACTIVO")
                .ToListAsync(), "IdProveedor", "NombreProveedor");

            // Query para filtrar productos activos
            var query = _context.Productos
                .Where(p => p.Estado == "ELIMINADO")
                .AsQueryable();

            // Filtrar por categoría y proveedor si se seleccionan
            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.IdCategoria == categoriaId.Value);
            }
            if (proveedorId.HasValue)
            {
                query = query.Where(p => p.IdProveedor == proveedorId.Value);
            }

            // Aplicar paginación
            var productosDesactivados = await query
                .OrderBy(p => p.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calcular total de productos y páginas
            var totalProductos = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProductos / (double)pageSize);

            var nombreUsuario = HttpContext.Session.GetString("NombreUsuario");
            ViewBag.NombreUsuario = nombreUsuario;
            // Pasar los datos a través de ViewBag
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.SelectedCategoria = categoriaId;
            ViewBag.SelectedProveedor = proveedorId;

            return View(productosDesactivados);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActivarProducto(string codigoProducto)
        {
            if (string.IsNullOrEmpty(codigoProducto))
            {
                return NotFound();
            }

            // Asegúrate de que el tipo de codigoProducto sea string
            var producto = await _context.Productos.FindAsync(codigoProducto);

            if (producto == null || producto.Estado != "ELIMINADO")
            {
                return NotFound(); // Asegúrate de que el producto existe y está eliminado
            }

            // Cambiar el estado a "ACTIVO"
            producto.Estado = "ACTIVO";

            // Guardar los cambios
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ListaProductos)); // Redirigir a la lista de productos eliminados
        }






    }
}