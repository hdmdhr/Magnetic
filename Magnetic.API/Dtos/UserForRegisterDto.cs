using System.ComponentModel.DataAnnotations;

namespace Magnetic.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "pin has to be between 4 to 8 characters.")]
        public string Password { get; set; }
    }
}