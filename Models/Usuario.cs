using System;
using System.Collections.Generic;

namespace TendalProject.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string ApellidoPaterno { get; set; } = null!;

    public string ApellidoMaterno { get; set; } = null!;

    public string Celular { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public byte[] Clave { get; set; } = null!;

    public int IdRol { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime? FechaRegistro { get; set; }

    public virtual Rol IdRolNavigation { get; set; } = null!;

    public virtual ICollection<PedidoEntregado> PedidoEntregados { get; set; } = new List<PedidoEntregado>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
