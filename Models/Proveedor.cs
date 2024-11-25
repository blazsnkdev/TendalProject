/*using System;
using System.Collections.Generic;

namespace TendalProject.Models;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    public string NombreProveedor { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public DateTime? FechaServicio { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
*/
using System;
using System.Collections.Generic;

namespace TendalProject.Models;

public partial class Proveedor
{
    public int IdProveedor { get; set; }

    public string NombreProveedor { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Direccion { get; set; } = null!;
    public string Estado { get; set; } = "ACTIVO"; // Nueva propiedad añadida

    public DateTime? FechaServicio { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
