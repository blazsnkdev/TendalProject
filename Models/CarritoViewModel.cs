using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TendalProject.Models
{
    public class CarritoViewModel
    {
        public long ClienteId { get; set; } public List<CarritoItemViewModel> Items { get; set; } = new List<CarritoItemViewModel>();

        // Propiedad calculada para obtener el total del carrito
        public decimal Total => Items.Sum(item => item.Total);


    }
}