using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiLibreria.DTOs;
using MiLibreria.Models;
using MiLibreria.Services;

namespace MiLibreria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        #region Propiedades
        private readonly MiLibreriaContext _context;
        // Para acceder a la clave de encriptación, que está registrada en el appsettings.Development.json
        // necesitamos una dependencia más que llama IConfiguration. Esa configuración en un servicio
        // que tenemos que inyectar en el constructor
        private readonly IConfiguration _configuration;
        // Para encriptar, debemos incorporar otra dependencia más. Se llama IDataProtector. De nuevo, en un servicio
        // que tenemos que inyectar en el constructor
        private readonly IDataProtector _dataProtector;
        // El IDataProtector, para que funcione, lo debemos registrar en el program
        // Mirar en el program la línea: builder.Services.AddDataProtection();
        private readonly HashService _hashService;
        private readonly IHttpContextAccessor _httpContextAccesor;

        #endregion

        #region Constructores
        public UsuariosController(MiLibreriaContext context, IConfiguration configuration, 
            IDataProtectionProvider dataProtectionProvider, HashService hashService, IHttpContextAccessor httpContextAccesor)
        {
            _context = context;
            _configuration = configuration;
            // Con el dataProtector podemos configurar un gestor de encriptación con esta línea
            // dataProtectionProvider.CreateProtector crea el gestor de encriptación y se apoya en la clave
            // de encriptación que tenemos en el appsettings.Development y que hemos llamado ClaveEncriptacion
            _dataProtector = dataProtectionProvider.CreateProtector(_configuration["ClaveEncriptacion"]);
            _hashService = hashService;
            _httpContextAccesor = httpContextAccesor;
        }
        #endregion

        #region Get
        [HttpGet("/changepassword/{textoEnlace}")]
        public async Task<ActionResult> LinkChangePasswordHash(string textoEnlace)
        {
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.EnlaceCambioPass == textoEnlace);
            if (usuarioDB == null)
            {
                return Unauthorized("Operación no autorizada");
            }

            var fechaCaducidad = usuarioDB.FechaEnvioEnlace.Value.AddMinutes(3);

            if (fechaCaducidad < DateTime.Now)
            {
                return Unauthorized("Operación no autorizada");
            }

            return Ok("Enlace correcto");
        }
        #endregion

        #region Post
        [HttpPost("encriptar/nuevousuario")]
        public async Task<ActionResult> PostNuevoUsuario([FromBody] DTOUsuario usuario)
        {
            //Encriptamos el password
            var passEncriptado = _dataProtector.Protect(usuario.Password);
            var newUsuario = new Usuario
            {
                Email = usuario.Email,
                Password = passEncriptado
            };
            await _context.Usuarios.AddAsync(newUsuario);
            await _context.SaveChangesAsync();

            return Ok(newUsuario);
        }

        [HttpPost("encriptar/checkusuario")]
        public async Task<ActionResult> PostCheckUserPassEncriptado([FromBody] DTOUsuario usuario)
        {
            //Esto haría un login con nuestro sistema de encriptación
            //Buscamos si existe el usuario
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized(); //Si el usuario no existe, devolvemos un 401
            }
            //Desencriptamos el password
            var passDesencriptado = _dataProtector.Unprotect(usuarioDB.Password);
            //Y ahora miramos aver si el password de la base de datos que ya hemos encriptado cuando hemos creado el usuario
            //coincide con el que viene en la petición 
            if (usuario.Password == passDesencriptado)
            {
                return Ok(); //Devolvemos un ok si coinciden
            }
            else
            {
                return Unauthorized();//Devolvemos un 401 si no coinciden
            }
        }
        
        [HttpPost("hash/nuevousuario")]
        public async Task<ActionResult> PostNuevoUsuarioHash([FromBody] DTOUsuario usuario)
        {
            var resultadoHash = _hashService.Hash(usuario.Password);
            var newUsuario = new Usuario
            {
                Email = usuario.Email,
                Password = resultadoHash.Hash,
                Salt = resultadoHash.Salt,
                EnlaceCambioPass=null,
               FechaEnvioEnlace=null,
            };

            await _context.Usuarios.AddAsync(newUsuario);
            await _context.SaveChangesAsync();

            return Ok(newUsuario);
        }

        [HttpPost("hash/checkusuario")]
        public async Task<ActionResult> CheckUsuarioHash([FromBody] DTOUsuario usuario)
        {
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized();
            }

            var resultadoHash = _hashService.Hash(usuario.Password, usuarioDB.Salt!);
            if (usuarioDB.Password == resultadoHash.Hash)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }
        
        [HttpPost("hash/changepassword")]
        public async Task<ActionResult> ChangePassword([FromBody] DTOChangePassword changePassword)
        {
            var usuarioDB = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == changePassword.Email);
            if (usuarioDB == null)
            {
                return Unauthorized("El usuario no existe");
            }

            var resultadoHash = _hashService.Hash(changePassword.PassActual, usuarioDB.Salt!);
            if (usuarioDB.Password != resultadoHash.Hash)
            {
                return Unauthorized("La contraseña actua no es correcta");
            }

            var nuevoHashResultado = _hashService.Hash(changePassword.PassNueva);
            usuarioDB.Password = nuevoHashResultado.Hash;
            usuarioDB.Salt = nuevoHashResultado.Salt;
            usuarioDB.FechaEnvioEnlace = null;
            _context.Usuarios.Update(usuarioDB);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("hash/linkchangepassword")]
        public async Task<ActionResult> LinkChangePasswordHash([FromBody] DTOUsuarioLinkChangePassword usuario)
        {
            var usuarioDB = await _context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized("Usuario no registrado");
            }

            // Creamos un string aleatorio 
            Guid miGuid = Guid.NewGuid();
            string textoEnlace = Convert.ToBase64String(miGuid.ToByteArray());
            // Eliminar caracteres que pueden causar problemas
            textoEnlace = textoEnlace.Replace("=", "").Replace("+", "").Replace("/", "").Replace("?", "").Replace("&", "").Replace("!", "").Replace("¡", "");
            usuarioDB.EnlaceCambioPass = textoEnlace;
            usuarioDB.FechaEnvioEnlace = DateTime.Now;
            await _context.SaveChangesAsync();
            var ruta = $"{_httpContextAccesor.HttpContext.Request.Scheme}://{_httpContextAccesor.HttpContext.Request.Host}/changepassword/{textoEnlace}";
            return Ok(ruta);
        }

        [HttpPost("usuarios/changepassword")]
        public async Task<ActionResult> LinkChangePasswordHash([FromBody] DTOUsuarioChangePassword infoUsuario)
        {
            var usuarioDB = await _context.Usuarios.AsTracking().FirstOrDefaultAsync(x => x.Email == infoUsuario.Email && x.EnlaceCambioPass == infoUsuario.Enlace);
            if (usuarioDB == null)
            {
                return Unauthorized("Operación no autorizada");
            }

            if (usuarioDB.FechaEnvioEnlace.Value.AddMinutes(3) < DateTime.Now)
            {
                return Unauthorized("Operación no autorizada");
            }

            var resultadoHash = _hashService.Hash(infoUsuario.Password);
            usuarioDB.Password = resultadoHash.Hash;
            usuarioDB.Salt = resultadoHash.Salt;
            usuarioDB.EnlaceCambioPass = null;
            usuarioDB.FechaEnvioEnlace = null;

            await _context.SaveChangesAsync();

            return Ok("Password cambiado con exito");
        }

        #endregion
    }
}
