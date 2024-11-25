namespace TendalProject.Models;

public class ProductoViewModel
{
    public string? CodigoProducto { get; set; } // Asegúrate de que esta propiedad esté presente
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int IdCategoria { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int IdProveedor { get; set; }
    public IFormFile? ImagenFile { get; set; } // For file upload



    // Propiedad para mostrar la imagen actual
    public string? ImagenActual { get; set; }
}