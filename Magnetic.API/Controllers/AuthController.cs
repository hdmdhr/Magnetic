using System.Threading.Tasks;
using Magnetic.API.Data;
using Magnetic.API.Dtos;
using Magnetic.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Magnetic.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;

        public AuthController(IAuthRepository repo)
        {
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
    }
}