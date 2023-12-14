using Microsoft.AspNetCore.Mvc.Filters;

namespace MiFacturacion.Filters;

public class FiltroDeExcepcion : ExceptionFilterAttribute
{
    private readonly IWebHostEnvironment _env;

    public FiltroDeExcepcion(IWebHostEnvironment env)
    {
        _env = env;
    }

    public override void OnException(ExceptionContext context)
    {
        var path = $@"{_env.ContentRootPath}\wwwroot\log.txt";
        using (StreamWriter writer = new StreamWriter(path, append: true))
        {
            writer.WriteLine(context.Exception.Source);
            writer.WriteLine(context.Exception.Message);
        }

        base.OnException(context);
    }
}
