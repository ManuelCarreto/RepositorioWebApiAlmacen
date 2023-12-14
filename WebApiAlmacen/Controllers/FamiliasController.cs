using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Drawing;
using System.Net;
using WebApiAlmacen.DTOs;
using WebApiAlmacen.Models;
using WebApiAlmacen.Services;

namespace WebApiAlmacen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamiliasController : ControllerBase
    {
        #region Propiedades
        private readonly MiAlmacenContext _context;

        // Inyección de dependencia en acción. Pasamos al constructor la instancia global de la base de datos
        // al controller. En el constructor, se toma esa instancia global representada por MiAlmacenContext
        // Una vez lo recibe, ese valor se pasa a una propiedad de la clase que es la que vamos a usar en el resto
        // del controller para utilizar la base de datos.
        // Es un estándar llamarle a esa propiedad privada _context
        private readonly OperacionesService _operacionesService;
        // Para emitir comentarios y verlos en la consola debemos inyectar otro servicio. Se llama ILogger
        private readonly ILogger _logger;
        // Tenemos estos 6 niveles de log: Critical, Error, Warning, Information, Debug y Trace
        #endregion

        #region Constructores
        // Inyección de dependencia en acción. Pasamos al constructor la instancia global de la base de datos
        // al controller. En el constructor, se toma esa instancia global representada por MiAlmacenContext
        // Una vez lo recibe, ese valor se pasa a una propiedad de la clase que es la que vamos a usar en el resto
        // del controller para utilizar la base de datos.
        // Es un estándar llamarle a esa propiedad privada _context
        public FamiliasController(MiAlmacenContext context, OperacionesService operacionesService, ILogger logger)
        {
            _context = context;
            _operacionesService = operacionesService;
            _logger = logger;
        }
        #endregion

        #region Get

        // [HttpGet] precederá a todos los métodos de consulta get. Si, por ejemplo, sería post, pondríamos [HttpPost]
        // Los métodos de acceso a una base de datos deben ser siempre asíncronos porque accedemos a un recurso externo que no se controla en la aplicación
        // Cada método asíncrono deberá tener la palabra reservada async. El valor de retorno de los métodos asíncronos será un Task (tarea) que devolverá unos datos
        // de un determinado tipo. En este caso, una lista de familias
        [HttpGet]
        public async Task<List<Familia>> GetFamilias()
        {
            // Accedemos a la base de datos (MiAlmacenContext). Esa representación de la base de datos en .Net se llama context
            // var context = new MiAlmacenContext();

            // Devolvemos de la tabla familias (representada por el modelo Familias) la lista de todas
            // Recordamos que await hace esperar la resolución del método para continuar. En este caso continúa con el return

            // Gracias a la inyección de dependencia, podemos usar el context que traemos del program
            var lista = await _context.Familias.ToListAsync();
            return lista;
        }

        // Si en la ruta ponemos una / con un texto, para ejecutar este endopoint, deberemos poner en la url localhost:xxxxx/familias
        // Si ponemos [HttpGet("familias")] la ruta parte de api/familias, luegeo el endpoint sería ejecutado bajo la url localhost:xxxxx/api/familias
        [HttpGet("/familias")]
        public async Task<List<Familia>> GetListadoFamilias()
        {
            // Accedemos a la base de datos (MiAlmacenContext). Esa representación de la base de datos en .Net se llama context
            // var context = new MiAlmacenContext();

            // Devolvemos de la tabla familias (representada por el modelo Familias) la lista de todas
            // Recordamos que await hace esperar la resolución del método para continuar. En este caso continúa con el return

            // Gracias a la inyección de dependencia, podemos usar el context que traemos del program
            var lista = await _context.Familias.ToListAsync();
            return lista;
        }

        // Si en la ruta ponemos una / con un texto, para ejecutar este endopoint, deberemos poner en la url localhost:xxxxx/familias
        // Si ponemos [HttpGet("familias")] la ruta parte de api/familias, luegeo el endpoint sería ejecutado bajo la url localhost:xxxxx/api/familias
        [HttpGet("sincrono")]
        public List<Familia> GetListadoFamiliasSincrono()
        {
            // Las operaciones contra una base de datos DEBEN SER SIEMPRE ASÍNCRONAS. Para liberar los hilos de ejecución en cada petición, eso no debe hacerse nunca
            var lista = _context.Familias.ToList();
            return lista;
        }

        // Agregar un método get que devuelva las familias ordenadas por nombre
        // api/familias/ordenadas
        [HttpGet("ordenadas")]
        public async Task<List<Familia>> GetFamiliasOrdenadas()
        {
            // var context = new MiAlmacenContext();
            var listaOrdenada = await _context.Familias.OrderBy(x => x.Nombre).ToListAsync();
            return listaOrdenada;
        }

        // ActionResult es lo que debería devolver todo proceso asíncrono. Es lo que nos va a permitir devolver un código de éxito (Ej. 200)
        // o fracaso (Ej. 400).
        // ActionResult puede ir acompañado de el tipo de dato que el endpoint va a devolver. En este caso, devolvemos una lista de familias

        [HttpGet("actionresult")]
        public async Task<ActionResult<List<Familia>>> GetFamiliasActionResult()
        {
            // Un try / catch debe siempre codificarse en una operación asíncrona porque puede dar error. Un error que rompa de forma abrupta
            // la ejecución del servidor. Nosotros haremos un try/catch global por medio de desarrollar un filtro de excepción
            try
            {
                var listaOrdenada = await _context.Familias.ToListAsync();
                // Devolvemos el ActionResult. Es un Ok que por detrás es una especificación del código 200
                // Si no ponemos Ok, por defecto, al resultado que devolvemos, le pone un código 200
                // return Ok(listaOrdenada);
                return Ok(listaOrdenada);
            }
            catch (Exception)
            {
                // En caso de error devolvemos un error. Esos errores están en diferentes objetos. BadRequest devuelve un 400
                // return BadRequest("Error en la operación");
                // Si no recordamos el objeto podemos retornar un StatusCode de la enumeración StatusCodes. En el ejemplo, devolvemos un 500
                // return StatusCode(StatusCodes.Status500InternalServerError);
                // Si además queremos devolver un mensaje personalizado aparte del código de error, podemos crear un objeto ContentResult
                // con las características que queramos. En el ejemplo, un error 500 (InternalServerError) con un mensaje (Content) "Se ha producido un error de acceso a la base de datos"
                return new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Content = "Se ha producido un error de acceso a la base de datos"
                };
            }
        }
        // [HttpGet("{id}")] // api/familias/1 -->Si llamamos a api/familias/moda da 400
        [HttpGet("{id:int}")] // api/familias/1 -->Si llamamos a api/familias/moda da 404 por la restricción
        public async Task<ActionResult<Familia>> GetFamiliaPorId([FromRoute] int id) // este nombre id es muy importante que coincida con {id} [FromRoute] es opcional y es una manera de decir que el parámetro viene de la ruta
        {
            var familia = await _context.Familias.FindAsync(id); // FindAsync busca por campo clave. En este caso, la familia con el id que se le pasa por argumento
                                                                 // var familiaporid = await _context.Familias.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (familia == null)
            {
                return NotFound("La familia " + id + " no existe");
            }
            return Ok(familia);
        }

        // [HttpGet("familiacontiene/{contiene}")]
        //  [HttpGet("{contiene}/{param2?}")] // api/familias/a/b  --> param2 es opcional por el ?
        [HttpGet("{contiene}/{param2=hogar}")] // api/familias/a/b  --> param2 tiene el valor por defecto hogar
        public async Task<ActionResult<Familia>> PrimeraFamiliaPorContiene([FromRoute] string contiene, [FromRoute] string? param2)
        // public async Task<ActionResult<Familia>> GetPrimeraFamiliaPorContiene(string contiene)
        {
            var familia = await _context.Familias.FirstOrDefaultAsync(x => x.Nombre.Contains(contiene));
            if (familia == null)
            {
                return NotFound();
            }
            return Ok(familia);
        }

        // QueryString es una forma de pasar argumentos menos usual. Van después de la ruta con un ? y se pueden encadenar con &
        // /api/productosentreprecios?min=5&max=10 nos serviría para capturar el min y el max con [FromQuery] int 5, [FromQuery] int 10

        [HttpGet("parametrocontienequerystring")] // api/familias/parametrocontienequerystring?contiene=moda
        public async Task<ActionResult<IEnumerable<Familia>>> GetFamiliasContieneQueryString([FromQuery] string contiene)
        {
            var familias = await _context.Familias.Where(x => x.Nombre.Contains(contiene)).ToListAsync();
            return Ok(familias);
        }

        [HttpGet("paginacion")]
        public async Task<ActionResult<List<Familia>>> GetFamiliasPaginacion()
        {
            var familias = await _context.Familias.Take(2).ToListAsync();
            var familias2 = await _context.Familias.Skip(1).Take(2).ToListAsync();
            return Ok(new { take = familias, takeSkip = familias2 });
        }

        // MUY BUEN PUNTO DE REFERENCIA PARA CUANDO QUERAMOS HACER PAGINACIÓN
        [HttpGet("paginacion2/{pagina?}")]
        public async Task<ActionResult<List<Familia>>> GetFamiliasPaginacionPersonalizada(int pagina = 1)
        {
            // Fórmula para hacer paginación automática. Al nº de página restamos 1 para fijar el punto inicial de captura de los registrosPorPagina especificados
            // A partir de ahí, devolvemos esos registros
            int registrosPorPagina = 2;
            var familias = await _context.Familias.Skip((pagina - 1) * registrosPorPagina).Take(registrosPorPagina).ToListAsync();
            return Ok(familias);
        }

        // Objetos DTO (Data Transfer Object)
        // Tienen como objetivo dar tipo a lo que se va a devolver cuando se desea devolver algo personalizado
        // Ejemplo: Queremos ver, por cada familia, su id y su nombre.
        // Pasos: 1 - Crear una clase DTO con las propiedades id y nombre. 2 - Desarrollar un método get que devuelva una lista de esa clase DTO
        [HttpGet("seleccioncamposdto")]
        public async Task<ActionResult<List<DTOFamilia>>> GetFamiliasSeleccionCamposDTO()
        {
            // Dos maneras de hacerlo. Al final retornamos una de las dos
            var familias = await _context.Familias.Select(x => new DTOFamilia { Id = x.Id, Nombre = x.Nombre }).ToListAsync();
            var familias2 = await (from x in _context.Familias
                                   select new DTOFamilia
                                   {
                                       Id = x.Id,
                                       Nombre = x.Nombre
                                   }).ToListAsync();
            return Ok(familias);
        }

        // Familias con productos
        [HttpGet("familiasproductos/{id:int}")]
        public async Task<ActionResult<Familia>> GetFamiliasProductosEager(int id)
        {
            // Familia llama a producto y producto a familia, lo que provoca un ciclo infinito del que informa swagger.
            // Por eso, hay que ir al Program y el la configuración de los controllers determinar que se ignoren los ciclos
            // Con ThenInclude podemos profundizar más en las relaciones
            // var familias = await _context.Familias.Include(x => x.Productos).ToListAsync();
            var familia = await _context.Familias.Include(x => x.Productos).FirstOrDefaultAsync(x => x.Id == id);
            if (familia == null)
            {
                return NotFound();
            }
            return Ok(familia);
        }

        // Familias con productos con una clase a medida con lo que se desea devolver
        [HttpGet("familiasproductosselect/{id:int}")]
        public async Task<ActionResult<Familia>> GetFamiliasProductosSelect(int id)
        {
            // Probar los dos
            //var familia = await _context.Familias
            //    .Select(x=> new DTOFamiliaProducto
            //    {
            //        IdFamilia = x.Id,
            //        Nombre = x.Nombre,
            //        TotalProductos = x.Productos.Count(),
            //        Productos = x.Productos.Select(y => new DTOProductoItem
            //        {
            //                IdProducto=y.Id,
            //                Nombre = y.Nombre
            //        }).ToList(),
            //    })
            //    .FirstOrDefaultAsync(x => x.IdFamilia == id);

            var familia = await (from x in _context.Familias
                                 select new DTOFamiliaProducto
                                 {
                                     IdFamilia = x.Id,
                                     Nombre = x.Nombre,
                                     TotalProductos = x.Productos.Count(),
                                     Productos = x.Productos.Select(y => new DTOProductoItem
                                     {
                                         IdProducto = y.Id,
                                         Nombre = y.Nombre,
                                     }).ToList(),
                                 }).FirstOrDefaultAsync(x => x.IdFamilia == id);

            if (familia == null)
            {
                return NotFound();
            }
            return Ok(familia);
        }

        [HttpGet("sql/{id:int}")]
        public async Task<ActionResult<Familia>> FamiliaPorIdSQL(int id)
        {
            var familia = await _context.Familias
                        .FromSqlInterpolated($"SELECT * FROM Familias WHERE Id = {id}")
                        .FirstOrDefaultAsync();

            if (familia == null)
            {
                return NotFound();
            }
            return Ok(familia);
        }
        #endregion

        #region Put

        [HttpPut("aumentoprecios")]
        public async Task<ActionResult> PutProductosTracking()
        {
            // Como tenemos el tracking deshabilitado en el program, debemos habilitarlo en esta operación porque lo necesitamos para
            // que, una vez tengamos la lista de productos en memoria, podamos cambiar esa lista y actualizarla en la base de datos
            var productos = await _context.Productos.AsTracking().ToListAsync();
            foreach (var producto in productos)
            {
                producto.Precio = producto.Precio + (producto.Precio * 2 / 100);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region Post

        [HttpPost]
        public async Task<ActionResult> PostFamilia([FromBody] DTOFamilia familia)
        {
            var newFamilia = new Familia()
            {
                Nombre = familia.Nombre
            };

            await _context.AddAsync(newFamilia);
            await _context.SaveChangesAsync();

            return Created("Familia", new { familia = newFamilia });
        }

        [HttpPost("sql")]
        public async Task<ActionResult> Post(DTOFamilia familia)
        {
            //Ejemplo de sentencia SQL de inserción	
            await _context.Database.ExecuteSqlInterpolatedAsync($@"INSERT INTO Familias(Nombre) VALUES({familia.Nombre})");

            return Ok();
        }

        // Agregar varias familias
        [HttpPost("varios")]
        public async Task<ActionResult> PostFamilias([FromBody] DTOFamilia[] familias)
        {
            // Método 1. Por cada DTOFamilia creamos un objeto Familia y lo agregamos. Al final, hacemos el SaveChanges
            //foreach (var f in familias)
            //{
            //    var nuevaFamilia = new Familia
            //    {
            //        Nombre = f.Nombre
            //    };

            //    await _context.AddAsync(nuevaFamilia);
            //}

            //await _context.SaveChangesAsync();

            // Método 2. Consturimos una lista de objetos Familia. Por cada DTOFamilia agregamos ese objeto a la lista. Al final, agregamos la lista
            // entera con AddRangeAsync y al final hacemos el SaveChanges

            // List<Familia> variasFamilias = new();
            var variasFamilias = new List<Familia>();
            foreach (var f in familias)
            {
                variasFamilias.Add(new Familia
                {
                    Nombre = f.Nombre
                });
            }
            await _context.AddRangeAsync(variasFamilias);
            await _context.SaveChangesAsync();

            return Ok();
        }
        #endregion

        #region Put

      
        [HttpPut("{id:int}")]
        public async Task<ActionResult> PutFamilia([FromRoute] int id, DTOFamilia familia)
        {
            if (id != familia.Id)
            {
                return BadRequest("Los ids proporcionados son diferentes");
            }
            var familiaUpdate = await _context.Familias.AsTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (familiaUpdate == null)
            {
                return NotFound();
            }
            familiaUpdate.Nombre = familia.Nombre;
            _context.Update(familiaUpdate);

            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region Delete

       
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            //var productos = await _context.Productos.CountAsync(x => x.FamiliaId == id);
            //if (productos != 0)
            //{
            //    return BadRequest("Hay productos relacionados");
            //}

            //var listaProductos = await _context.Productos.Where(x => x.FamiliaId == id).ToListAsync();
            //if (listaProductos.Count() != 0)
            //{
            //    return BadRequest("Hay productos relacionados");
            //}

            var hayProductos = await _context.Productos.AnyAsync(x => x.FamiliaId == id);
            if (hayProductos)
            {
                return BadRequest("Hay productos relacionados");
            }

            var familia = await _context.Familias.FindAsync(id);

            if (familia is null)
            {
                return NotFound("La familia no existe");
            }

            _context.Remove(familia);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("familiayproductos/{id:int}")]
        public async Task<ActionResult> DeleteFamiliaYProductos(int id)
        {
            var familiaConProductos = await _context.Familias.Include(x => x.Productos).FirstOrDefaultAsync(x => x.Id == id);

            if (familiaConProductos is null)
            {
                return NotFound("La familia no existe");
            }

            _context.Productos.RemoveRange(familiaConProductos.Productos);
            _context.Familias.Remove(familiaConProductos);
            await _context.SaveChangesAsync();
            return Ok();
        }
        #endregion
    }
}
