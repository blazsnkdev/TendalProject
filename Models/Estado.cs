using System;
using System.Collections.Generic;

namespace TendalProject.Models;

public partial class Estado
{
    public int IdEstado { get; set; }

    public string NombreEstado { get; set; } = null!;

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
