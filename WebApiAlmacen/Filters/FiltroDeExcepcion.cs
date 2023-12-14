using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAlmacen.Filters
{
    public class FiltroDeExcepcion : ExceptionFilterAttribute
    {
        private readonly IWebHostEnvironment _env;

        public FiltroDeExcepcion(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Cuando haya errores inesperados en un controller vendrá siempre al método OnException
        // para dar respuesta al error. Nuestro trabajo pasa por integrar estoy en el proyecto,
        // en el Program, línea de AddControllers incluir la configuración de este filtro de excepción
        // ExceptionContext encapsula toda la información del error
        // En el constructor debemos inyectar otras dependencias que debemos usar, en este caso IWebHostEnvironment
        // porque vamos a registrar el error en 
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
}
