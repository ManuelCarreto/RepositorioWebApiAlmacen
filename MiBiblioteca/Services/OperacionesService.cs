using Microsoft.EntityFrameworkCore;
using MiLibreria.Models;

namespace MiLibreria.Services
{
    public class OperacionesService
    {
        private readonly MiLibreriaContext _context;
        private readonly IHttpContextAccessor _accessor;

        public OperacionesService(MiLibreriaContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }

        public async Task AddOperacion(string operacion, string controller)
        {
            Operacione nuevaOperacion = new Operacione()
            {
                FechaAccion = DateTime.Now,
                Operacion = operacion,
                Controller = controller,
                Ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString()
            };

            await _context.Operaciones.AddAsync(nuevaOperacion);
            await _context.SaveChangesAsync();

            Task.FromResult(0);
        }

        public async Task<bool> OperacionPermiso(string? ip, string controller) 
        {
            var treintaSegundos = new TimeSpan(0, 0, 30);
            var hace30segundos = DateTime.Now - treintaSegundos;

            return await _context.Operaciones.AnyAsync(x =>
                 x.Ip == ip &&
                 //x.FechaAccion.AddSeconds(30) < DateTime.Now &&
                 x.FechaAccion > hace30segundos &&
                 x.Controller == controller); 
        }
    }
}
