using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiFacturacion.DTOs;
using MiFacturacion.Models;
using MiFacturacion.Services;

namespace MiFacturacion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        #region Propiedades
        private readonly MiFacturacionContext _context;
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
        private readonly TokenService _tokenService;

        #endregion

        #region Constructores
        public UsuariosController(MiFacturacionContext context, IConfiguration configuration, 
            IDataProtectionProvider dataProtectionProvider, HashService hashService, IHttpContextAccessor httpContextAccesor, TokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            // Con el dataProtector podemos configurar un gestor de encriptación con esta línea
            // dataProtectionProvider.CreateProtector crea el gestor de encriptación y se apoya en la clave
            // de encriptación que tenemos en el appsettings.Development y que hemos llamado ClaveEncriptacion
            _dataProtector = dataProtectionProvider.CreateProtector(_configuration["ClaveEncriptacion"]);
            _hashService = hashService;
            _httpContextAccesor = httpContextAccesor;
            _tokenService = tokenService;
        }
        #endregion

        #region Get
        

        #endregion

        #region Post
        [HttpPost]
        public async Task<ActionResult<DTOLoginResponse>> Login([FromBody] DTOUsuario input)
        {
            var usuarioBD = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == input.Email);

            if (usuarioBD == null) return NotFound($"No existe ningun usuario con ese {input.Email}");

            // si hemos llegado aqui es que existe un usuario con ese email en la base de datos
            var result = _hashService.Hash(input.Password, usuarioBD.Salt);
            if (result.Hash != usuarioBD.Password)
            {
                return Unauthorized("Contraseña Incorrecta");
            }
            var loginResponse =  _tokenService.GenerarToken(input);
            // si llegamos hasta aqui las credenciales (email y contraseña) del notas son correctas
            
            return Ok(loginResponse);
        }
    
        [HttpPost("hash/registerUsuario")]
        public async Task<ActionResult> RegisterUsuario([FromBody] DTOUsuario usuario)
        {
            var resultadoHash = _hashService.Hash(usuario.Password);
            //Cifra la contraseña que recibe del usuario y llega en texto plano
            var newUsuario = new Usuario
            {
                Email = usuario.Email,
                Password = resultadoHash.Hash,
                Salt = resultadoHash.Salt,
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
        #endregion
    }
}
