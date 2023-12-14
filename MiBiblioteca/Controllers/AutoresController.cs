using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiLibreria.DTOs;
using MiLibreria.Models;

namespace MiLibreria.Controllers;
[Route("api/[controller]")]
[ApiController]
// El filtro de excepción también puede aplicar solo a uno o varios controladores
// La siguiente línea activaría este control de errores solo a este controlador
// Nosotros lo hemos configurado a nivel global en el Program, que sería el sitio idóneo
// para que todos los controladores tuvieran integrado el control de errores
//    [TypeFilter(typeof(FiltroDeExcepcion))]
public class AutoresController : ControllerBase
{
    #region Context
    private readonly MiLibreriaContext _context;
    #endregion

    #region Constructor
    public AutoresController(MiLibreriaContext context)
    {
        _context = context;
    }
    #endregion

    #region Get
    // GET: api/Autores
    [HttpGet("/autores")]
    public async Task<ActionResult<IEnumerable<Autore>>> GetAutores()
    {
        if (_context.Autores == null)
        {
            return NotFound();
        }
        return await _context.Autores.ToListAsync();
    }

    // GET: api/Autores/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Autore>> GetAutore(int id)
    {
        if (_context.Autores == null)
        {
            return NotFound();
        }
        var autore = await _context.Autores.FindAsync(id);

        if (autore == null)
        {
            return NotFound();
        }

        return autore;
    }

    // PUT: api/Autores/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
   
    [HttpGet("autoresLibros")]
    public async Task<ActionResult<IEnumerable<DTOAutoresLibros>>> GetAutoresDTO()
    {
        var autores = await _context.Autores.Select(a => new DTOAutoresLibros
        {
            IdAutor = a.IdAutor,
            Nombre = a.Nombre,
            CantidadLibros = a.Libros.Count,
            PromedioPrecio = a.Libros.Average(l => l.Precio),
            Libros = a.Libros.Select(l => new DTOLibros
            {
                ISBN = l.Isbn,
                Titulo = l.Titulo,
                Precio = l.Precio
            }).ToList()
        }).ToListAsync();

        return autores;
    }
    [HttpGet("autores/{id}")]
    public async Task<ActionResult<DTOAutoresLibros>> GetAutor(int id)
    {
        var autor = await _context.Autores.Where(a => a.IdAutor == id).Select(a => new DTOAutoresLibros
        {
            IdAutor = a.IdAutor,
            Nombre = a.Nombre,
            CantidadLibros = a.Libros.Count,
            PromedioPrecio = a.Libros.Average(l => l.Precio),
            Libros = a.Libros.Select(l => new DTOLibros
            {
                ISBN = l.Isbn,
                Titulo = l.Titulo,
                Precio = l.Precio
            }).ToList()
        }).FirstOrDefaultAsync();

        if (autor == null)
        {
            return NotFound();
        }

        return autor;
    }
    [HttpGet("libros/descatalogados")]
    public async Task<ActionResult<IEnumerable<object>>> GetLibrosDescatalogados()
    {
        var libros = await _context.Libros.GroupBy(l => l.Descatalogado)
            .Select(g => new
            {
                Descatalogado = g.Key,
                Cantidad = g.Count()
            }).ToListAsync();

        return libros;
    }
    #endregion

    #region Post

    // POST: api/Autores
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost(" Agregar un Autor")]
    public async Task<ActionResult<Autore>> PostAutore(Autore autore)
    {
        if (_context.Autores == null)
        {
            return Problem("Entity set 'MiLibreriaContext.Autores'  is null.");
        }
        _context.Autores.Add(autore);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetAutore", new { id = autore.IdAutor }, autore);
    }


    #endregion

    #region Put

    [HttpPut("autores/{id}")]
    public async Task<IActionResult> ModificaAutor(int id, Autore autor)
    {
        if (id != autor.IdAutor)
        {
            return BadRequest("No puedes modificar la ID");
        }

        _context.Entry(autor).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AutorExists(id))
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

    private bool AutorExists(int id)
    {
        return _context.Autores.Any(a => a.IdAutor == id);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAutore(int id, Autore autore)
    {
        if (id != autore.IdAutor)
        {
            return BadRequest();
        }

        _context.Entry(autore).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AutoreExists(id))
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
    [HttpPut("David/{id}")]
    public async Task<IActionResult> PutAutor(int id, DTOAgregarAutor autor)
    {
        if (id != autor.IdAutor)
            return BadRequest("NO puedes modificar la ID");

        if (await _context.Autores.FindAsync(id) is null)
            return BadRequest($"El autor con ID:{id} no existe");

        Autore updatedData = new()
        {
            IdAutor = autor.IdAutor,
            Nombre = autor.Nombre
        };

        _context.Entry(updatedData).State = EntityState.Modified;

        _ = await _context.SaveChangesAsync();
        return NoContent();
    }
    #endregion

    #region Delete
    // DELETE: api/Autores/5
    [HttpDelete("EliminarAutor/{id}")]
    public async Task<IActionResult> DeleteAutor(int id)
    {
        if (_context.Autores == null) { return NotFound(); }
        var autor = await _context.Autores.Include(e => e.Libros).FirstOrDefaultAsync(e => e.IdAutor == id);
        if (autor == null) return NotFound();
        if (autor.Libros.Count() > 0)
        {
            return BadRequest("Este autor tiene libros asociados en la base de datos");
        }
        else
        {
            _context.Autores.Remove(autor);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
    [HttpDelete("EliminarAutorSQL/{id:int}")]
    public async Task<IActionResult> DeleteAutorSQL(int id)
    {
        if (_context.Autores == null)
            return NotFound();
        
        var autor = await _context.Autores
            .Include(e => e.Libros)
            .FirstOrDefaultAsync(e => e.IdAutor == id);
        
        if (autor == null) 
            return NotFound();

        if (autor.Libros.Count() > 0)
            return BadRequest("Este autor tiene libros asociados en la base de datos");

        await _context.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM Autores WHERE IdAutor={id}");

        return NoContent();       
    }

    private bool AutoreExists(int id)
    {
        return (_context.Autores?.Any(e => e.IdAutor == id)).GetValueOrDefault();
    }
    #endregion
}
