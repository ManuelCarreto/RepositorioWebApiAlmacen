namespace MiLibreria.Services;

public class TestService
{
    public decimal CalcularIva(decimal precio)
    {
        return precio * 0.04m;
    }
    public decimal CalcularDescuento(decimal precio, decimal descuento)
    {
        return precio - (precio * descuento / 100);
    }
}
