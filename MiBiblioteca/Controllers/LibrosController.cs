using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.EntityFrameworkCore;
using MiLibreria.DTOs;
using MiLibreria.Models;
using MiLibreria.Services;

namespace MiLibreria.Controllers;

[Route("api/[controller]")]
[ApiController]
// El filtro de excepción también puede aplicar solo a uno o varios controladores
// La siguiente línea activaría este control de errores solo a este controlador
// Nosotros lo hemos configurado a nivel global en el Program, que sería el sitio idóneo
// para que todos los controladores tuvieran integrado el control de errores
//    [TypeFilter(typeof(FiltroDeExcepcion))]
public class LibrosController : ControllerBase
{
    #region Context
    private readonly MiLibreriaContext _context;
    private readonly OperacionesService _operacionService;
    private readonly IGestorArchivos _gestorArchivoLocal;
   
    #endregion

    #region Constructor
    public LibrosController(MiLibreriaContext context, OperacionesService operacionService, IGestorArchivos gestorArchivoLocal)
    {
        _context = context;
        _operacionService = operacionService;
        _gestorArchivoLocal = gestorArchivoLocal;
    }
    #endregion

    #region Get
    // GET: api/Libros
    [HttpGet("/libros")]
    public async Task<ActionResult<IEnumerable<Libro>>> GetLibros()
    {
        if (_context.Libros == null)
        {
            return NotFound();
        }

        var noTienePermiso = await _operacionService.OperacionPermiso(
                ip: HttpContext.Connection.RemoteIpAddress?.ToString(),
                controller: nameof(GetLibros));

        if (noTienePermiso)
        {
            return BadRequest("Se paciente cabrón no hace ni 30 segundos que me has preguntao lo mismo");
        }
        await _operacionService.AddOperacion(operacion: HttpMethods.Get, controller: nameof(GetLibros));
        return await _context.Libros.ToListAsync();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Libro>>> GetApiLibros()
    {
        if (_context.Libros == null)
        {
            return NotFound();
        }


        var noTienePermiso = await _operacionService.OperacionPermiso(
            ip: HttpContext.Connection.RemoteIpAddress?.ToString(),
            controller: "GetApiLibros");

        if (noTienePermiso)
        {
            return BadRequest("Se paciente cabrón no hace ni 30 segundos que me has preguntao lo mismo");
        }

        await _operacionService.AddOperacion(operacion: HttpMethods.Get, controller: nameof(GetApiLibros));
        return await _context.Libros.ToListAsync();
    }

    // GET: api/Libros/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Libro>> GetLibro(string id)
    {
        if (_context.Libros == null)
        {
            return NotFound();
        }
        var libro = await _context.Libros.FindAsync(id);

        if (libro == null)
        {
            return NotFound();
        }

        return libro;
    }
    [HttpGet("titulo/contiene/{texto}")]
    public async Task<ActionResult<List<Libro>>> GetNombreLibro(string texto)
    {
        if (_context.Libros == null)
        {
            return NotFound();
        }
        var libro = await _context.Libros.Where(l => l.Titulo.Contains(texto)).ToListAsync();

        if (libro.Count() == 0)
        {
            return NotFound();
        }

        return libro;
    }
    [HttpGet("ordenadosportitulo/{direccion}")]
    public async Task<ActionResult<List<Libro>>> GetOrdenadosPorTitulo(string direccion)
    {
        if (_context.Libros == null)
        {
            return NotFound();
        }
        var libro = await _context.Libros.ToListAsync();
        if (direccion == "true")
        {
            return libro.OrderBy(l => l.Titulo).ToList();
        }
        else if (direccion == "false")
        {
            return libro.OrderByDescending(l => l.Titulo).ToList();
        }
        else
        {
            return libro;
        }
    }
    [HttpGet("precio/entre")]
    public async Task<ActionResult<List<Libro>>> GetPrecioEntre([FromQuery] decimal min, [FromQuery] decimal max)
    {
        if (_context.Libros == null)
        {
            return NotFound();
        }
        var resultado = await _context.Libros.Where(libro => libro.Precio >= min && libro.Precio <= max).ToListAsync();
        return Ok(resultado);
    }
    [HttpGet("desdehasta/{desde}/{hasta}")]
    public async Task<ActionResult<List<Libro>>> GetPrecioDesdeHasta(int desde, int hasta)
    {
        if (_context.Libros == null)
        {
            return NotFound();
        }
        var resultado = await _context.Libros.Skip(desde - 1).Take(hasta - desde + 1).ToListAsync();
        return Ok(resultado);
    }
    [HttpGet("venta")]
    public async Task<ActionResult<IEnumerable<DTOVentaLibro>>> GetLibroVenta()
    {
        if (_context.Libros == null)
        {
            return NotFound();
        }
        var resultado = await _context.Libros.Select(libro => new DTOVentaLibro
        {
            TituloLibro = libro.Titulo,
            PrecioLibro = libro.Precio,

        }).ToListAsync();
        return Ok(resultado);
    }
    [HttpGet("/librosAgrupados")]
    public async Task<ActionResult<List<Libro>>> GetLibrosAgrupados()
    {
        var resultado = await _context.Libros.GroupBy(l => l.Descatalogado).Select(group => new
        {
            Descatalogado = group.Key,
            Cantidad = group.Count(),
            Libros = group.ToArray(),

        }).ToListAsync();
        return Ok(resultado);
    }
    [HttpGet("filtrar")]
    public async Task<ActionResult> GetFiltroMultiple([FromQuery] DTOLibrosFiltro filtroLibro)
    {
        var librosQueryable = _context.Libros.AsQueryable();

        if (filtroLibro.Descatalogado is not null)    
            librosQueryable = librosQueryable
                .Where(x => x.Descatalogado == filtroLibro.Descatalogado);

        if (filtroLibro.Precio is not null)      
            librosQueryable = librosQueryable
                .Where(x => x.Precio > filtroLibro.Precio);
        
        var libros = await librosQueryable.ToListAsync();

        return Ok(libros);
    }
    #endregion

    #region Put
    // PUT: api/Libroes/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutLibro(string id, Libro libro)
    {
        if (id != libro.Isbn)
        {
            return BadRequest();
        }

        _context.Entry(libro).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!LibroExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }
    [HttpPut("ModificarLibroControl/{id}")]
    public async Task<IActionResult> PutEditorial(string id, DTOAgregarLibros libro)
    {
        if (id != libro.Isbn) { return BadRequest("Los Id no coinciden"); }
        if (await _context.Editoriales.FindAsync(libro.EditorialId) == null) return BadRequest("La editorial no existe");
        if (await _context.Autores.FindAsync(id) == null) return BadRequest("El autor no existe");
        if (await _context.Libros.FindAsync(id) == null) return BadRequest("El libro no existe");
        else
        {
            Libro edit = new Libro
            {
                Isbn = libro.Isbn,
                Titulo = libro.Titulo,
                Paginas = libro.Paginas,
                FotoPortada = libro.FotoPortada,
                Descatalogado = libro.Descatalogado,
                AutorId = libro.AutorId,
                EditorialId = libro.EditorialId,
                Precio = libro.Precio
            };
            _context.Entry(edit).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
    #endregion

    #region Post
    // POST: api/Libroes
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("Agregar un libro")]
    public async Task<ActionResult<IEnumerable<Libro>>> PostLibros(IEnumerable<Libro> libros)
    {
        _context.Libros.AddRange(libros);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetLibros", libros);
    }
   
    [HttpPost("AgregarVariosLibros")]
    public async Task<ActionResult<Libro>> PostVariosLibros([FromBody] DTOAgregarLibros[] libros)
    {
        if (_context.Libros == null) { return Problem("El conjunto de entidades 'MiLibreriaContext.Libros' es nulo."); }
        List<Libro> _libros = new List<Libro>();
        foreach (var lib in libros)
        {
            if (await _context.Libros.FindAsync(lib.Isbn) == null) _libros.Add(new Libro
            {
                Isbn = lib.Isbn,
                Titulo = lib.Titulo,
                Paginas = lib.Paginas,
                FotoPortada = lib.FotoPortada,
                Descatalogado = lib.Descatalogado,
                AutorId = lib.AutorId,
                EditorialId = lib.EditorialId,
                Precio = lib.Precio
            });
        }

        foreach (var libro in _libros)
        {
            await _context.Libros.AddAsync(libro);
        }

        await _context.SaveChangesAsync();
        return Created("Libro", new { Libros = _libros });
    }
    [HttpPost("AgregarLibroControl")]
    public async Task<ActionResult<Libro>> PostLibrosControl([FromBody] DTOAgregarLibros libro)
    {
        if (_context.Libros == null) { return Problem("Entity set 'MiBibliotecaContext.Libros' is null"); }
        if (await _context.Autores.FindAsync(libro.AutorId) == null) return BadRequest("El Autor no existe");
        if (await _context.Editoriales.FindAsync(libro.EditorialId) == null) return BadRequest("La Editorial no existe");
        if (await _context.Libros.FindAsync(libro.Isbn) == null) await _context.Libros.AddAsync(new Libro
        {
            Isbn = libro.Isbn,
            Titulo = libro.Titulo,
            Paginas = libro.Paginas,
            FotoPortada = libro.FotoPortada,
            Descatalogado = libro.Descatalogado,
            AutorId = libro.AutorId,
            EditorialId = libro.EditorialId,
            Precio = libro.Precio
        });

        await _context.SaveChangesAsync();
        return Created("Libro", new { Libro = libro });
    }
    [HttpPost("ConPortada")]
    public async Task<ActionResult<Libro>> PostLibrosConPortada([FromForm] DTOLibrosValidados libro) 
    {
        if (_context.Libros is null) { return Problem("Entity set 'MiBibliotecaContext.Libros' is null"); }
        if (await _context.Autores.FindAsync(libro.AutorId) is  not null) return BadRequest("El Autor no existe");
        if (await _context.Editoriales.FindAsync(libro.EditorialId) is not null) return BadRequest("La Editorial no existe");
        if (await _context.Libros.FindAsync(libro.Isbn) is not null) await _context.Libros.AddAsync(new Libro
        {
            Isbn = libro.Isbn,
            Titulo = libro.Titulo,
            Paginas = libro.Paginas,
            FotoPortada = libro.FotoPortada,
            Descatalogado = libro.Descatalogado,
            AutorId = libro.AutorId,
            EditorialId = libro.EditorialId,
            Precio = libro.Precio
        });

        await _context.SaveChangesAsync();
        return Created("Libro", new { Libro = libro });
    }

    #endregion

    #region Delete
    // DELETE: api/Libroes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLibro(string id)
    {
        if (_context.Libros == null) return NotFound();
        var libro = await _context.Libros.FindAsync(id);
        if (libro == null)
        {
            return NotFound();
        }

        _context.Libros.Remove(libro);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    private bool LibroExists(string id)
    {
        return (_context.Libros?.Any(e => e.Isbn == id)).GetValueOrDefault();
    }

    #endregion
}

