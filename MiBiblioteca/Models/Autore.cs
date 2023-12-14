using System;
using System.Collections.Generic;

namespace MiLibreria.Models;

public partial class Autore
{
    public int IdAutor { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Libro> Libros { get; set; } = new List<Libro>();
}
