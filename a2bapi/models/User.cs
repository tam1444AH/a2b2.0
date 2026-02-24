//namespace a2bapi.Models
//{
//    public class User
//    {
//        public int Id { get; set; }
//        public string? Email { get; set; }
//        public string? Password { get; set; }

//    }
//}

using System.ComponentModel.DataAnnotations;

namespace a2bapi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, EmailAddress, MaxLength(256)]
        public string Email {  get; set; }

        [Required]
        public string PasswordHash { get; set; } = "";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}