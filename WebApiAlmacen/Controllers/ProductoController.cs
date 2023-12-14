using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using WebApiAlmacen.DTOs;
using WebApiAlmacen.Models;
using WebApiAlmacen.Services;

namespace WebApiAlmacen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        #region Propiedades
        private readonly MiAlmacenContext _context;
        private readonly IGestorArchivos _gestorArchivosLocal;
        private readonly OperacionesService _operacionesService;

        #endregion

        #region Constructores
        public ProductosController(MiAlmacenContext context, IGestorArchivos gestorArchivosLocal, OperacionesService operacionesService)
        {
            _context = context;
            _gestorArchivosLocal = gestorArchivosLocal;
            _operacionesService = operacionesService;
        }
        #endregion

        #region Get
        [HttpGet]
        public async Task<ActionResult> GetProductos()
        {
            var productos = await _context.Productos.ToListAsync();
            await _operacionesService.AddOperacion("Get", "Productos");
            return Ok(productos);
        }

        [HttpGet("productosagrupadospordescatalogado")]
        public async Task<ActionResult<List<DTOProductosAgrupadosDescatalogado>>> GetProductosAgrupadosPorDescatalogado()
        {
            // Esta consulta devuelve todos los productos agrupados por descatalogado
            // Me salen 2 arrays. Uno con los descatalogados false y el otro con los descatalogados true
            //  var productos = await _context.Productos.GroupBy(g => g.Descatalogado).ToListAsync();
            // Suponemos que nos piden un agrupamiento a medida. Por ejemplo: 
            // * Los valores de los grupos (en este caso serán true/false)
            // * Cuántos productos hay de cada grupo
            // * La lista de productos por grupo

            // Podemos devolverlo así
            //var productos = await _context.Productos.GroupBy(g => g.Descatalogado)
            //    .Select(x => new
            //    {
            //        Descatalogado = x.Key,
            //        Total = x.Count(),
            //        Productos = x.ToList()
            //    }).ToListAsync();

            // O mejor así, utilizando una clase a medida DTO
            var productos = await _context.Productos.GroupBy(g => g.Descatalogado)
               .Select(x => new DTOProductosAgrupadosDescatalogado
               {
                   Descatalogado = x.Key,
                   Total = x.Count(),
                   Productos = x.ToList()
               }).ToListAsync();
            await _operacionesService.AddOperacion("Get", "Productos");
            return Ok(productos);
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult> GetFiltroMultiple([FromQuery] DTOProductosFiltro filtroProductos)
        {
            // AsQueryable nos permite ir construyendo paso a paso el filtrado y ejecutarlo al final.
            // Si lo convertimos a una lista (toListAsync) el resto de filtros los hacemos en memoria
            // porque toListAsync ya trae a la memoria del servidor los datos desde el servidor de base de datos
            // Hacer los filtros en memoria es menos eficiente que hacerlos en una base de datos.
            // Construimos los filtros de forma dinámica y hasta que no hacemos el ToListAsync no vamos a la base de datos
            // para traer la información

            // Versión poco optimizada (no aconsejada)
            //var productos = await _context.Productos.ToListAsync();
            //if (!string.IsNullOrEmpty(filtroProductos.ContieneEnNombre))
            //{
            //    productos = productos.Where(x => x.Nombre.Contains(filtroProductos.ContieneEnNombre)).ToList();
            //}

            //if (filtroProductos.Descatalogado)
            //{
            //    productos = productos.Where(x => x.Descatalogado).ToList();
            //}

            //if (filtroProductos.FamiliaId != 0)
            //{
            //    productos = productos.Where(x => x.FamiliaId == filtroProductos.FamiliaId).ToList();
            //}

            //return Ok(productos);

            // Versión aconsejada porque hace una única consulta al final
            var productosQueryable = _context.Productos.AsQueryable();

            if (!string.IsNullOrEmpty(filtroProductos.Nombre))
            {
                productosQueryable = productosQueryable.Where(x => x.Nombre.Contains(filtroProductos.Nombre));
            }

            if (filtroProductos.Descatalogado)
            {
                productosQueryable = productosQueryable.Where(x => x.Descatalogado);
            }

            if (filtroProductos.FamiliaId != 0)
            {
                productosQueryable = productosQueryable.Where(x => x.FamiliaId == filtroProductos.FamiliaId);
            }

            var productos = await productosQueryable.ToListAsync();
            await _operacionesService.AddOperacion("Get", "Productos Filtrar");


            return Ok(productos);
        }



        #endregion

        #region Post

      
        [HttpPost]
        public async Task<ActionResult> PostProductos([FromForm] DTOProductoAgregar producto)
        {
            Producto newProducto = new Producto
            {
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descatalogado = false,
                FechaAlta = DateTime.Now,
                FamiliaId = producto.FamiliaId,
                FotoUrl = ""
            };

            if (producto.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Extraemos la imagen de la petición
                    await producto.Foto.CopyToAsync(memoryStream);
                    // La convertimos a un array de bytes que es lo que necesita el método de guardar
                    var contenido = memoryStream.ToArray();
                    // La extensión la necesitamos para guardar el archivo
                    var extension = Path.GetExtension(producto.Foto.FileName);
                    // Recibimos el nombre del archivo
                    // El servicio Transient GestorArchivosLocal instancia el servicio y cuando se deja de usar se destruye
                    newProducto.FotoUrl = await _gestorArchivosLocal.GuardarArchivo(contenido, extension, "imagenes",
                        producto.Foto.ContentType);
                }
            }

            await _context.AddAsync(newProducto);
            await _context.SaveChangesAsync();
            await _operacionesService.AddOperacion("Post", "Productos");

            return Ok(newProducto);
        }
        #endregion

        #region Delete

      
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProductos([FromRoute] int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            await _gestorArchivosLocal.BorrarArchivo(producto.FotoUrl, "imagenes");
            _context.Remove(producto);
            await _context.SaveChangesAsync();
            await _operacionesService.AddOperacion("Delete", "Productos");

            return Ok();
        }
        #endregion
    }
}
