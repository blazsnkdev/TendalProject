using System;
using System.Collections.Generic;

namespace TendalProject.Models;

public partial class MetodoPago
{
    public int IdMetodoPago { get; set; }

    public string Metodo { get; set; } = null!;

    public virtual ICollection<PedidoEntregado> PedidoEntregados { get; set; } = new List<PedidoEntregado>();
}
