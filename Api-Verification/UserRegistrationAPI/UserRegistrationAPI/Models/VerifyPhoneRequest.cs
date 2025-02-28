namespace UserRegistrationAPI.Models
{
    public class VerifyPhoneRequest
    {
        public string PhoneNumber { get; set; }
        public string VerificationCode { get; set; }
    }
}
