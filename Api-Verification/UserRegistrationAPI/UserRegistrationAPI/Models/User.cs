using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserRegistrationAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [JsonIgnore]
        public string? EmailVerificationCode { get; set; }

        [JsonIgnore]
        public bool IsEmailVerified { get; set; } = false;
    }
}
