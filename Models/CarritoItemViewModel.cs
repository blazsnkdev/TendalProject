using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TendalProject.Models
{
    public class CarritoItemViewModel
    {
        public Producto Producto { get; set; } // El producto que se agrega al carrito
        public int Cantidad { get; set; } // La cantidad del producto en el carrito

        // Propiedad calculada para obtener el total por cada producto en el carrito
        public decimal Total => Producto.Precio * Cantidad;
        
    }
}