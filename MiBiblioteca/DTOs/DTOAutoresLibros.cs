

namespace MiLibreria.DTOs;

public class DTOAutoresLibros
{
    public int IdAutor { get; set; }
    public string? Nombre { get; set; }
    public int CantidadLibros { get; set; }
    public decimal? PromedioPrecio { get; set; }
    public IEnumerable<DTOLibros>? Libros { get; set; }
}

