using MiFacturacion.Models;

namespace MiFacturacion.DTOs;

public class DTOClienteCustom
{
    public int IdCliente { get; set; }

    public string Nombre { get; set; }

    public decimal TotalFacturado { get; set; }

    public List<DTOFacturaCustom> Facturas { get; set; }
}

public class DTOFacturaCustom
{
    public int NFactura { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Importe { get; set; }

}

