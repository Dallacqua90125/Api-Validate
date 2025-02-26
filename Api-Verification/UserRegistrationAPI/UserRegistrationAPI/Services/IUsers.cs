using UserRegistrationAPI.Models;

namespace UserRegistrationAPI.Services
{
    public interface IUsers
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<User> UpdateUserAsync(int id, User user);
        Task SendEmailAsync(string userEmail, string subject, string body);
        Task<bool> VerifyEmail(string email, string code);
        Task<bool> RegisterUserAsync(User user);
    }
}
