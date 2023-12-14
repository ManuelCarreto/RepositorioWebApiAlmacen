using System;
using System.Collections.Generic;

namespace MiFacturacion.Models;

public partial class Factura
{
    public int Nfacturas { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Importe { get; set; }

    public bool Pagada { get; set; }

    public int ClienteId { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;
}
