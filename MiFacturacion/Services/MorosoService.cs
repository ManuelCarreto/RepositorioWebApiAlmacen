using Microsoft.EntityFrameworkCore;
using MiFacturacion.Models;

namespace MiFacturacion.Services;

public class MorosoService
{
    #region PROPIEDADES
    private readonly MiFacturacionContext _context;

    #endregion

    #region CONSTRUCTOR
    public MorosoService(MiFacturacionContext context)
    {
        _context = context;
    }
    #endregion

    #region METODO
    public async Task<bool?> EsMoroso(int id)
    {
        bool esMoroso = await _context.Facturas.AnyAsync(f => f.ClienteId == id && f.Pagada == false);
        return esMoroso;
    }
    #endregion
}
