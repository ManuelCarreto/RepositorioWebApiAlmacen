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

namespace MiLibreria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // El filtro de excepción también puede aplicar solo a uno o varios controladores
    // La siguiente línea activaría este control de errores solo a este controlador
    // Nosotros lo hemos configurado a nivel global en el Program, que sería el sitio idóneo
    // para que todos los controladores tuvieran integrado el control de errores
    //    [TypeFilter(typeof(FiltroDeExcepcion))]
    public class EditorialesController : ControllerBase
    {
        #region Context
        private readonly MiLibreriaContext _context;
        #endregion

        #region Constructor
        public EditorialesController(MiLibreriaContext context)
        {
            _context = context;
        }
        #endregion

        #region Get

        // GET: api/Editoriales
        [HttpGet("/editoriales")]
        public async Task<ActionResult<IEnumerable<Editoriale>>> GetEditoriales()
        {
          if (_context.Editoriales == null)
          {
              return NotFound();
          }
            return await _context.Editoriales.ToListAsync();
        }
       
        [HttpGet("{id}")]
        public async Task<ActionResult<Editoriale>> GetEditoriales(int id)
        {
          if (_context.Editoriales == null)
          {
              return NotFound();
          }
            var editoriales = await _context.Editoriales.FindAsync(id);

            if (editoriales == null)
            {
                return NotFound();
            }

            return editoriales;
        }
        [HttpGet("EditorialesConLibros")]
        public async Task<ActionResult<IEnumerable<Editoriale>>> GetEditorialesConLibrosFull()
        {
            if (_context.Editoriales is null)
                return NotFound();

            Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Editoriale, ICollection<Libro>> result = _context.Editoriales.Include(l => l.Libros);

            return Ok(await result.ToArrayAsync());
        }
       
        #endregion

        #region Put
        // PUT: api/Editoriales/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("editoriales/{id}")]
        public async Task<IActionResult> ModificaEditorial(int id, Editoriale editorial)
        {
            if (id != editorial.IdEditorial)
            {
                return BadRequest();
            }

            _context.Entry(editorial).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EditorialExists(id))
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

        private bool EditorialExists(int id)
        {
            return _context.Editoriales.Any(e => e.IdEditorial == id);
        }
        [HttpPut("ModificarEditorial/{id}")]
        public async Task<IActionResult> PutEditorial(int id, DTOAgregarEditorial editorial)
        {
            if (id != editorial.IdEditorial) { return BadRequest("Los Id no coinciden"); }
            if (await _context.Editoriales.FindAsync(id) == null) return BadRequest("La editorial no existe");
            else
            {
                Editoriale edit = new Editoriale { IdEditorial = id, Nombre = editorial.Nombre };
                _context.Entry(edit).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return NoContent();
            }
        }

        /// <summary>
        /// Actualización de una editorial usando una query de sql
        /// </summary>
        /// <param name="id">identificador (primary key) de la editorial que vamos a modificar</param>
        /// <param name="editorial">nuevos datos con los que vamos a actualizar la editorial</param>
        /// <returns>ok 200</returns>
        [HttpPut("sql/{id:int}")]
        public async Task<ActionResult<Editoriale>> EditorialPorIdSQL(int id, DTOAgregarEditorial editorial)
        {
            if (_context.Editoriales == null)
                return NotFound("No se ha encontrado la tabla de editoriales");

            if (await _context.Editoriales.FindAsync(id) == null)
                return NotFound("No se encuentra ninguna editorial con ese id");

            if (id != editorial.IdEditorial) 
                return BadRequest("No se puede modificar el id");
            
            if (string.IsNullOrEmpty(editorial.Nombre)) 
                return BadRequest("Debes proporcionar un nombre válido");

          
            await _context.Database.ExecuteSqlInterpolatedAsync($@"UPDATE Editoriales SET Nombre = {editorial.Nombre} 
        WHERE IdEditorial = {editorial.IdEditorial}");

            return Ok();
        }
       

        #endregion

        #region Post


        // POST: api/Editoriales
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754


        [HttpPost("Agregar una Editorial")]
        public async Task<ActionResult<Editoriale>> PostEditorial(DTOAgregarEditorial editorial)
        {
            var newEditorial = new Editoriale { Nombre = editorial.Nombre };

            _context.Editoriales.Add(newEditorial);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEditorial", new { id = editorial.IdEditorial }, editorial);
        }
        #endregion

        #region Delete


        // DELETE: api/Editoriales/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEditorial(int id)
        {
            if (_context.Editoriales is null)
                return NotFound("No existe la tabla");

            Editoriale? result = await _context.Editoriales
                .Include(e => e.Libros)
                .FirstOrDefaultAsync(e => e.IdEditorial == id);

            if (result is null)
                return NotFound($"No existe una editorial con el ID:{id}");

            if (result.Libros.Count > 0)
                return BadRequest("No se puede eliminar una editorial que contiene libros");

            _ = _context.Editoriales.Remove(result);
            _ = await _context.SaveChangesAsync();

            return NoContent();
        }

        #endregion
    }
}
