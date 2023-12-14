namespace MiFacturacion.DTOs;

public class DTOFacturasAgregar
{
    public DateTime? Fecha { get; set; }

    public decimal Importe { get; set; }

    public bool Pagada { get; set; }

    public int ClienteId { get; set; }
}
public class DTOFacturasModificar
{
    public decimal Importe { get; set; }

    public bool Pagada { get; set; }

    public int ClienteId { get; set; }
}