using System.ComponentModel.DataAnnotations;

namespace a2bapi.Dtos.Auth
{
    public class LoginRequest
    {

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(8)]
        public string Password { get; set; } = "";
    }
}