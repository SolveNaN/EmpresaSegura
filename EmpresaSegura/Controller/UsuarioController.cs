using EmpresaSegura.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NombreProyecto.Server.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace autOficialBlazer.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UsuarioController(ApplicationDbContext context)
        {

            _context = context;
        }

        [HttpGet("ConexionServidor"),Authorize(Roles ="Usuario")]
        public async Task<ActionResult<string>> GetEjemplo()
        {
            return "Autorizado para usar endpoint";
        }




        [HttpGet("Datos")]
        public async Task<ActionResult<List<Usuario>>> GetCuenta()
        {
            var lista = await _context.Usuarios.ToListAsync();
            return Ok(lista);
        }

        public static Usuario usuario = new Usuario();
        [HttpPost("Registrar")]
        public async Task<ActionResult<string>> CreateCuenta(UsuarioDTO objeto)
        {
            try
            {
                CreatePasswordHash(objeto.Password, out byte[] passwordHash, out byte[] passwordSalt);
                usuario.NombreUsuario = objeto.NombreUsuario;
                usuario.Correo = objeto.Correo;
                usuario.PasswordHash = passwordHash;
                usuario.PasswordSalt = passwordSalt;

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                var respuesta = "Registrado Con Exito";

                return respuesta;
            }
            catch (Exception ex)
            {
                return BadRequest("Error dutante el registro");
            }

        }
        [HttpPost("Login")]
        public async Task<ActionResult<string>> InicioSesion(UsuarioDTO objeto)
        {
            var cuanta = await _context.Usuarios.Where(x => x.Correo == objeto.Correo).FirstOrDefaultAsync();
            if (cuanta == null)
            {
                return BadRequest("Usuario no encontrado");
            }
            if (!VerifyPasswordHash(objeto.Password, cuanta.PasswordHash, cuanta.PasswordSalt))
            {
                return BadRequest("Contraseña incorrecta");
            }
            string token = CreateToken(cuanta);
            return Ok(token);

        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }


        private string CreateToken(Usuario user)
        {
            List<Claim> claims = new List<Claim>
     {
         new Claim(ClaimTypes.Name, user.Correo),
         new Claim(ClaimTypes.Role,user.Rol)
     };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                "PROYECTO CONTROL USUARUIS EN BLAZOR WEB WASM_ DAGO PARA BLAZOR  PARA APPS"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }



    }
}

