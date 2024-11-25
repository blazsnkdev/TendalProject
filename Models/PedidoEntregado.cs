using System;
using System.Collections.Generic;

namespace TendalProject.Models;

public partial class PedidoEntregado
{
    public int IdPedidoEntregado { get; set; }

    public long IdPedido { get; set; }

    public string NumeroPedido { get; set; } = null!;

    public int IdUsuario { get; set; }

    public string IdProducto { get; set; } = null!;

    public int Cantidad { get; set; }

    public decimal Importe { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaPedido { get; set; }

    public int IdMetodoPago { get; set; }

    public DateTime? FechaEntrega { get; set; }

    public virtual MetodoPago IdMetodoPagoNavigation { get; set; } = null!;

    public virtual Pedido IdPedidoNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
