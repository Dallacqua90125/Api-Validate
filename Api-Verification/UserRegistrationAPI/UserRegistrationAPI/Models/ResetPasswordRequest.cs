using System.ComponentModel.DataAnnotations;

namespace UserRegistrationAPI.Models
{
    public class ResetPasswordRequest
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
