using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiFacturacion.DTOs;
using MiFacturacion.Models;

namespace MiFacturacion.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FacturasController : ControllerBase
{
    #region PROPIEDADES
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly MiFacturacionContext _context;

    #endregion

    #region CONTRUCTOR
    public FacturasController(IHttpContextAccessor contextAccessor, MiFacturacionContext context)
    {
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    #endregion

    #region Metodos
    #region GET

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Factura>>> GetTodosLasFacturas()
    {
        var result = await _context.Facturas.ToListAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Factura>> DevolverFacturaPorId([FromRoute] int id)
    {
        var resultadoFactura = await _context.Facturas.FirstOrDefaultAsync((x) => x.Nfacturas == id);
        if (resultadoFactura == null) 
        { 
            return NotFound($"No se ha encontrado ninguna Factura con el ID {id}");
        }

        return Ok(resultadoFactura);
    }

    [HttpGet("importe/{importe}")]
    public async Task<ActionResult<IEnumerable<Factura>>> DevolverFacturaPorImporte([FromRoute] decimal importe)
    {
        var resultadoFactura = await _context.Facturas.Where((x) => x.Importe > importe).ToArrayAsync();
        if (resultadoFactura.Length == 0)
        {
            return NotFound($"No se ha encontrado ninguna Factura con ese importe {importe}");
        }

        return Ok(resultadoFactura);
    }

    [HttpGet("Pagadas")]
    public async Task<ActionResult<IEnumerable<Factura>>> DevolverFacturasPagadas()
    {
        var resultadoFactura = await _context.Facturas.Where((x) => x.Pagada).ToArrayAsync();
        if (resultadoFactura.Length == 0)
        {
            return NotFound("No se ha encontrado ninguna Factura pagada");
        }

        return Ok(resultadoFactura);
    }

    [HttpGet("cliente/{idCliente}")]
    public async Task<ActionResult<IEnumerable<Factura>>> DevolverFacturaCliente([FromRoute] int idCliente)
    {
        var resultadoFactura = await _context.Facturas.Where((x) => x.ClienteId > idCliente).ToArrayAsync();
        if (resultadoFactura.Length == 0)
        {
            return NotFound($"No se ha encontrado ninguna Factura asociada al cliente con ID {idCliente}");
        }

        return Ok(resultadoFactura);
    }

    #endregion

    #region POST
    [HttpPost]
    public async Task<ActionResult<Factura>> AgregarFacturas([FromBody] DTOFacturasAgregar factura)
    {
        if (factura is null)
        {
            return BadRequest("Faltan datos");
        }
        var clienteDB= await _context.Clientes.FindAsync(factura.ClienteId);
        if (clienteDB is null) 
        {
            return NotFound($" No existe ningun cliente con el ID{factura.ClienteId}");
        }
        var nuevaFactura = new Factura
        {
            Fecha = factura.Fecha?? DateTime.Now,
            Importe = factura.Importe,
            Pagada = factura.Pagada,
            ClienteId = factura.ClienteId
            
        };
        await _context.Facturas.AddAsync(nuevaFactura);
        await _context.SaveChangesAsync();

        return Ok(nuevaFactura);
    }
    #endregion

    #region PUT
    [HttpPut("{id}")]
    public async Task<ActionResult<Factura>> ModificarFactura([FromRoute] int id,[FromBody] DTOFacturasModificar factura)
    {
        var facturaDB = await _context.Facturas.FindAsync(id);
        if (facturaDB == null) return NotFound($"No existe ningun cliente con el ID {id}");

        var clienteDB = await _context.Clientes.FindAsync(factura.ClienteId);
        if (clienteDB is null) return NotFound($"No existe ningun cliente con el ID {factura.ClienteId}");


        facturaDB.Importe = factura.Importe;
        facturaDB.Pagada = factura.Pagada;
        facturaDB.ClienteId = factura.ClienteId;
       
        _context.Facturas.Update(facturaDB);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    #endregion

    #region DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarFactura([FromRoute] int id)
    {
        var facturaDB = await _context.Facturas.FindAsync(id);
        if (facturaDB == null) return NotFound($"No se ha encontrado ningun cliente con el ID {id}");
        
        _context.Facturas.Remove(facturaDB);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    #endregion
    #endregion

}
