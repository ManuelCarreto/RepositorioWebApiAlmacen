using System;
using System.Collections.Generic;

namespace MiLibreria.Models;

public partial class Editoriale
{
    public int IdEditorial { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Libro> Libros { get; set; } = new List<Libro>();
}
