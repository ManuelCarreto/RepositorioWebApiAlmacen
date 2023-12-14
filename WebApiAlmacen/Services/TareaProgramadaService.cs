using Microsoft.EntityFrameworkCore;
using System.Drawing;
using WebApiAlmacen.Models;

namespace WebApiAlmacen.Services
{
    public class TareaProgramadaService : IHostedService
    {
        #region Propiedades

        private readonly IServiceProvider serviceProvider;
        private readonly IWebHostEnvironment env;
        private readonly string nombreArchivo = "Archivo.txt";
        private Timer timer;
        #endregion

        #region Constructores

        public TareaProgramadaService(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            this.serviceProvider = serviceProvider;
            this.env = env;
        }
        #endregion

        #region Metodos

      
        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(EscribirDatos, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            Escribir("Proceso iniciado");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Dispose();
            Escribir("Proceso finalizado");
            return Task.CompletedTask; // Parar la depuración desde el icono de IIS para que se ejecute el StopAsync
        }

        private async void EscribirDatos(object state)
        {
            using (var scope = serviceProvider.CreateScope()) // Necesitamos delimitar un alcance scoped. Los servicios IHostedService son singleton y el ApplicationDBContext Scoped. A continuación "abrimos" un scoped con using para poder
                                                              // utilizar el ApplicationDbContext en este servicio Singleton
            {
                var context = scope.ServiceProvider.GetRequiredService<MiAlmacenContext>();
                var primerProducto = await context.Productos.Select(x => x.Nombre).FirstAsync();
                Escribir(primerProducto);
            }
        }
        private void Escribir(string mensaje)
        {
            var ruta = $@"{env.ContentRootPath}\wwwroot\{nombreArchivo}";
            using (StreamWriter writer = new StreamWriter(ruta, append: true))
            {
                writer.WriteLine(mensaje);
            }
        }
        #endregion
    }
}
