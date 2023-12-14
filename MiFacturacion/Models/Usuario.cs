using System;
using System.Collections.Generic;

namespace MiFacturacion.Models;

public partial class Usuario
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public byte[]? Salt { get; set; }
}
