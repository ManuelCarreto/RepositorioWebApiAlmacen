namespace WebApiAlmacen.Middlewares
{
    public class LogFileBodyHttpResponseMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IWebHostEnvironment env;

        public LogFileBodyHttpResponseMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            this.next = next; // Para invocar otros Middlewares
            this.env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            // Utilizamos un MemoryStream para guardar en memoria el body de la petición http porque se encuentra en un buffer
            using (var ms = new MemoryStream())
            {
                var bodyOriginalRespuesta = httpContext.Response.Body;
                httpContext.Response.Body = ms;

                // await next(httpContext) hace continuar la ejecución de Middlewares
                await next(httpContext);
                // A partir de aquí la ejecución se producirá cuando se produzca el camino inverso de los Middlewares

                ms.Seek(0, SeekOrigin.Begin); //Vamos al inicio del stream
                string respuesta = new StreamReader(ms).ReadToEnd(); // Lo leemos. Coincidirá con la respuesta que los controllers envíen
                ms.Seek(0, SeekOrigin.Begin); //Colocamos el stream en la posición inicial para poder enviarlo al cliente

                await ms.CopyToAsync(bodyOriginalRespuesta); // Lo dejamos como estaba
                httpContext.Response.Body = bodyOriginalRespuesta; // Y finalmente lo pasamos para que la respuesta se envíe

                var path = $@"{env.ContentRootPath}\wwwroot\log.txt";
                using (StreamWriter writer = new StreamWriter(path, append: true))
                {
                    writer.WriteLine(respuesta);
                }
            }
        }
    }
}
