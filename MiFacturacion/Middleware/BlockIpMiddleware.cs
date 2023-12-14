namespace MiFacturacion.Middleware;

public class BlockIpMiddleware
{
    #region Propiedades
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;
    #endregion

    #region Constructores

    public BlockIpMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _env = env;
    }
    #endregion

    #region Metodos

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var IP = httpContext.Connection.RemoteIpAddress?.ToString();
        //var ruta = httpContext.Request.Path.ToString();
        //var metodo = httpContext.Request.Method;
        //var path = $@"{_env.ContentRootPath}\wwwroot\log.txt";
        var peticionRechazada = IP == "::1";
        //var permiso = peticionRechazada ? "rechazada" : "aceptada";

        //using (StreamWriter writer = new StreamWriter(path, append: true))
        //{
        //    writer.WriteLine($@"{IP} - {metodo} - {ruta} - {permiso}- {DateTime.Now}");
        //}

        if (peticionRechazada) // Bloquearía las peticiones de una IP
        {

            httpContext.Response.StatusCode = 400;
            httpContext.Response.ContentType = "text/plain";
            await httpContext.Response.WriteAsync("Tu IP no tiene permisos para realizar ninguna petición");

            return;
        }

        await _next(httpContext);
    }
    #endregion
}
