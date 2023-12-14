using Microsoft.IdentityModel.Tokens;
using MiLibreria.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiLibreria.Services
{
    public class TokenService
    {
        #region Propiedades
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region GenerarToken
        public DTOLoginResponse GenerarToken(DTOUsuario credencialesUsuario)
        {
            // Los claims construyen la información que va en el payload del token
            var claims = new List<Claim>()
             {
                 new Claim(ClaimTypes.Email, credencialesUsuario.Email),
                 new Claim("lo que yo quiera", "cualquier otro valor")
             };

            // Necesitamos la clave de generación de tokens
            var clave = _configuration["ClaveJWT"];
            // Fabricamos el token
            var claveKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave));
            var signinCredentials = new SigningCredentials(claveKey, SecurityAlgorithms.HmacSha256);
            // Le damos características
            var securityToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: signinCredentials
            );

            // Lo pasamos a string para devolverlo
            var tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new DTOLoginResponse()
            {
                Token = tokenString,
                Email = credencialesUsuario.Email
            };
        }
        #endregion
    }
}
