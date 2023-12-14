using WebApiAlmacen.Models;

namespace WebApiAlmacen.DTOs
{
    public class DTOProductosAgrupadosDescatalogado
    {
        public bool Descatalogado { get; set; }
        public int Total { get; set; }
        public List<Producto> Productos { get; set; }
    }
}
