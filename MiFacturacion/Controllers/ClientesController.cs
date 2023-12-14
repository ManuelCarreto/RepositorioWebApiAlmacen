using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiFacturacion.DTOs;
using MiFacturacion.Models;
using MiFacturacion.Services;

namespace MiFacturacion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientesController : ControllerBase
{
    #region Propiedades
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly MiFacturacionContext _context;
    private readonly MorosoService _morosoService;
    #endregion

    #region Constructor
    public ClientesController(IHttpContextAccessor contextAccessor, MiFacturacionContext context, MorosoService morosoService)
    {
        _contextAccessor = contextAccessor;
        _context = context;
        _morosoService = morosoService;
    }
    #endregion

    #region Metodos
    #region GET

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetTodosLosCLientes()
    {
        var result = await _context.Clientes.ToListAsync();
        return Ok(result);
    }

    [HttpGet("porCiudad/{ciudad}")]
    public async Task<ActionResult<IEnumerable<Cliente>>> DevolverClientePorCiudad([FromRoute] string ciudad)
    {
        var resultadoCiudad = await _context.Clientes.Where((x) => x.Ciudad == ciudad).ToListAsync();
        return Ok(resultadoCiudad);
    }

    [HttpGet("ListadoClientes")]
    public async Task<ActionResult<IEnumerable<DTOClienteCustom>>> GetTodosLosCLientesCustom()
    {
        var result = await _context.Clientes.Select((c) => new DTOClienteCustom()
        {
            Nombre = c.Nombre,
            IdCliente = c.IdCliente,
            TotalFacturado = c.Facturas.Sum((f) => f.Importe),
            Facturas= c.Facturas.Select(f => new DTOFacturaCustom() 
            { 
                Fecha= f.Fecha,
                Importe= f.Importe,
                NFactura= f.Nfacturas
            
            }).ToList()
        }).ToListAsync();

        return Ok(result);
    }

    [HttpGet("esMoroso/{id}")]
    public async Task<ActionResult<bool>> GetMoroso([FromRoute] int id)
    {
        return Ok(await _morosoService.EsMoroso(id));
    }
    #endregion

    #region POST
    [HttpPost]
    public async Task<ActionResult<Cliente>> AgregarCliente([FromBody] DTOClientesAgregar nuevoCliente)
    {
        var cliente = new Cliente
        {
            Nombre = nuevoCliente.Nombre,
            Ciudad = nuevoCliente.Ciudad
        };
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();

        return Ok(cliente);
    }
    #endregion

    #region PUT
    [HttpPut("{id}")]
    public async Task<ActionResult<Cliente>> ModificarCliente([FromRoute] int id, DTOClientesAgregar nuevoCliente)
    {
        var clienteDB = await _context.Clientes.FirstOrDefaultAsync(x=>x.IdCliente==id);
        if (clienteDB == null) return NotFound($"No existe ningun cliente con el ID {id}");
        
        
            clienteDB.Nombre = nuevoCliente.Nombre;
            clienteDB.Ciudad = nuevoCliente.Ciudad;
        
        _context.Clientes.Update(clienteDB);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    #endregion

    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarCliente([FromRoute] int id)
    {
        var clienteDB = await _context.Clientes.FirstOrDefaultAsync((c)=> c.IdCliente == id);
        if (clienteDB == null) return NotFound($"No se ha encontrado ningun cliente con el ID {id}");
        if (clienteDB.Facturas.Count>0)
        {
            return BadRequest("No se puede eliminar a un cliente con facturas");
        }
        _context.Clientes.Remove(clienteDB);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    #endregion

    #endregion
}
