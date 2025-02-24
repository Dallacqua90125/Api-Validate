using System.ComponentModel.DataAnnotations;

namespace UserRegistrationAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public bool IsEmailVerified { get; set; }

        public string EmailVerificationCode { get; set; }
    }
}
