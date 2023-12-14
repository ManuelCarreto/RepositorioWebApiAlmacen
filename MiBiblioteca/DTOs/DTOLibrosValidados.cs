using MiLibreria.Models;
using MiLibreria.Validators;


namespace MiLibreria.DTOs
{
    public class DTOLibrosValidados
    {
        public string Isbn { get; set; } = null!;

        public string Titulo { get; set; } = null!;
        [PaginaNegativaValidator]
        public int Paginas { get; set; }
        [PesoArchivoValidacion(6)]
        [TipoArchivoValidacion(GrupoTipoArchivo.Imagen)]
        public string? FotoPortada { get; set; }

        public bool Descatalogado { get; set; }

        public int AutorId { get; set; }

        public int EditorialId { get; set; }
        [PrecioValidacion]
        public decimal? Precio { get; set; }
    }
}
