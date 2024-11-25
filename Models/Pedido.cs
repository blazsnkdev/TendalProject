using System;
using System.Collections.Generic;

namespace TendalProject.Models;

public partial class Pedido
{
    public long IdPedido { get; set; }

    public string NumeroPedido { get; set; } = null!;

    public int IdUsuario { get; set; }

    public string IdProducto { get; set; } = null!;

    public int Cantidad { get; set; }

    public decimal Importe { get; set; }

    public int IdEstado { get; set; }

    public DateTime? FechaPedido { get; set; }

    public virtual Estado IdEstadoNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<PedidoEntregado> PedidoEntregados { get; set; } = new List<PedidoEntregado>();
}
