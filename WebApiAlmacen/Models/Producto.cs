﻿using System;
using System.Collections.Generic;

namespace WebApiAlmacen.Models;

public partial class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Precio { get; set; }

    public DateTime? FechaAlta { get; set; }

    public bool Descatalogado { get; set; }

    public string? FotoUrl { get; set; }

    public int FamiliaId { get; set; }

    public virtual Familia Familia { get; set; } = null!;
}
