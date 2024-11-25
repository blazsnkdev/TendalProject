/*using System;
using System.Collections.Generic;

namespace TendalProject.Models;

public partial class Producto
{
    public string CodigoProducto { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int IdCategoria { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public int IdProveedor { get; set; }

    public byte[]? Imagen { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Categorium IdCategoriaNavigation { get; set; } = null!;

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    public virtual ICollection<PedidoCancelado> PedidoCancelados { get; set; } = new List<PedidoCancelado>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
*/


using System;
using System.Collections.Generic;

namespace TendalProject.Models;

public partial class Producto
{
    public string CodigoProducto { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int IdCategoria { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public int IdProveedor { get; set; }

    public byte[]? Imagen { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public string Estado { get; set; } = "ACTIVO"; // Nueva propiedad añadida

    public virtual Categorium IdCategoriaNavigation { get; set; } = null!;

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;

    public virtual ICollection<PedidoEntregado> PedidoEntregados { get; set; } = new List<PedidoEntregado>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}