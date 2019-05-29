using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Magnetic.API.Data;
using Magnetic.API.Dtos;
using Magnetic.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Magnetic.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController] // 1. ↑ force attribute routing 2. validate request
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            this._config = config;
            this._repo = repo;
        }

        // POST
        [HttpPost("register")]
        public async Task<IActionResult> Register(/*[FromBody] */UserForRegisterDto userDto)
        {
            // todo: validate request
            userDto.Username = userDto.Username.ToLower();
            if (await _repo.UserExists(userDto.Username))
                return BadRequest("Username already in use.");

            var userToCreate = new User
            {
                Username = userDto.Username
            };

            var userCreated = await _repo.Register(userToCreate, userDto.Password);
            // todo: return CreateAtRoute
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto u)
        {
            var userFromRepo = await _repo.Login(u.Username.ToLower(), u.Password);
            if (userFromRepo == null)
                return Unauthorized();

            // Login success, Issue a JWT to client
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };
                // sign the token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}