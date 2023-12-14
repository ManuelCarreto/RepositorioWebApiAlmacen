using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApiAlmacen.Classes;
using WebApiAlmacen.DTOs;

namespace WebApiAlmacen.Services
{
    public class HashService
    {
        #region Propiedades
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public HashService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion
        // Un hash es una clave que no se puede revertir. Es lo correcto para contraseñas seguras
        // El hash es lo que se guardará en la tabla de usuarios.
        // Las funciones que generan hash también nos van a servir para contrastarlos
        // Un salt es un valor aleatorio que se anexa al texto plano al que queremos aplicar la función que genera el hash
        // Añade más seguridad porque, uniendo un salt aleatorio al password, los valores siempre serán diferentes
        // Si generamos el password sin salt, basándonos solo en el password (que es solo un texto plano) los hashes generados basados en ese password siempre serán iguales
        // El salt se debe guardar junto al password para contrastar el login

        // Método para generar el salt
        public ResultadoHash Hash(string textoPlano)
        {
            // Generamos el salt aleatorio
            var salt = new byte[16];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt); // Genera un array aleatorio de bytes
            }

            // Llamamos al método ResultadoHash y retornamos el hash con el salt
            return Hash(textoPlano, salt);
        }


        public ResultadoHash Hash(string textoPlano, byte[] salt)
        {
            //Pbkdf2 es un algoritmo de encriptación
            var claveDerivada = KeyDerivation.Pbkdf2(password: textoPlano,
                salt: salt, prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 32);

            var hash = Convert.ToBase64String(claveDerivada);

            return new ResultadoHash()
            {
                Hash = hash,
                Salt = salt
            };
        }
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
