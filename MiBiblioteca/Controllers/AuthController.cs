using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiLibreria.Classes;
using MiLibreria.DTOs;
using MiLibreria.Models;
using MiLibreria.Services;
using NuGet.Protocol.Plugins;
using Serilog.Context;

namespace MiLibreria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        #region PROPs
        private readonly MiLibreriaContext _context;
        private readonly HashService _hashService;
        private readonly TokenService _tokenService;
        #endregion

        #region CONSTRUCTORs
        public AuthController(MiLibreriaContext context, HashService hashService, TokenService tokenService)
        {
            _context = context;
            _hashService = hashService;
            _tokenService = tokenService;
        }
        #endregion

        #region METHODs
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] DTOUsuario input)
        {
            // Encriptamos la contraseña y recuperamos su hash y su salt
            ResultadoHash hashResult = _hashService.Hash(input.Password);

            // creamos el usuario que vamos a almacenar en BD 
            Usuario nuevoUsuario = new()
            {
                Email = input.Email,
                Password = hashResult.Hash,
                Salt = hashResult.Salt,
            };

            // añadimos el nuevo usuario a BD y guardamos los cambios
            _ = await _context.Usuarios.AddAsync(nuevoUsuario);
            _ = await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DTOUsuario input)
        {
            // Buscamos en BD para comprobar que exista un usuario registrado con el email introducido
            Usuario? usuarioBD = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == input.Email);

            if (usuarioBD is null)
            {
                return NotFound($"El email {input.Email} no está registrado");
            }

            // Hasheamos el pasword proporcionado por el usuario con el salt guardado en BD
            ResultadoHash resultadoHash = _hashService.Hash(input.Password, usuarioBD.Salt);

            // Comporbamos que el resultado del hash coincida con el que ya teniamos almacenado en BD
            if (resultadoHash.Hash != usuarioBD.Password)
            {
                return Unauthorized("Credenciales incorrectas");
            }

            DTOLoginResponse loginResponse = _tokenService.GenerarToken(input);

            return Ok(loginResponse);
        }
        #endregion
    }
}
