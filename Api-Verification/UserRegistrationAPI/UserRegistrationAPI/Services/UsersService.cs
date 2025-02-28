using System.Net.Mail;
using System.Net;
using UserRegistrationAPI.Data;
using UserRegistrationAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.OpenApi.Writers;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace UserRegistrationAPI.Services
{
    public class UsersService : IUsers
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> UpdateUserAsync(int id, User user)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null) return null;

            if (user.Nome == "string")
            {
                user.Nome = existingUser.Nome;
            }
            if (user.Email == "string")
            {
                user.Email = existingUser.Email;
            }
            if (user.Password == "string")
            {
                user.Password = existingUser.Password;
            }
            existingUser.Nome = user.Nome;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task SendEmailAsync(string userEmail, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
            {
                Port = int.Parse(emailSettings["SmtpPort"]),
                Credentials = new NetworkCredential(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["FromEmail"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(userEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendSmsAsync(string userPhoneNumber, string body)
        {
            var smsSettings = _configuration.GetSection("SmsSettings");

            TwilioClient.Init(smsSettings["AccountSid"], smsSettings["AuthToken"]);

            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(userPhoneNumber),
                from: new PhoneNumber(smsSettings["FromPhoneNumber"]),
                body: body
                );
        }
        public async Task<bool> VerifyPhoneNumberAsync(string phoneNumber, string code)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Telefone == phoneNumber);

            if (user == null || user.PhoneVerificationCode != code)
            {
                return false;
            }

            user.PhoneVerificationCode = null;
            user.IsPhoneVerified = true;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return false;
            }

            using (SHA256 sha256Hash = SHA256.Create())
            {
                user.Password = Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(user.Password)));
            }

            user.PhoneVerificationCode = GenerateVerificationCode();
            user.IsPhoneVerified = false;

            string smsBody = $"Seu código de verificação é: {user.PhoneVerificationCode}";
            await SendSmsAsync(user.Telefone, smsBody);

            user.EmailVerificationCode = GenerateVerificationCode();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            

            return true;
        }


        public async Task<bool> VerifyEmail(string email, string code)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.EmailVerificationCode != code)
            {
                return false;
            }

            user.EmailVerificationCode = null;
            user.IsEmailVerified = true;

            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public string GeneratePasswordResetToken(User user)
        { 
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            user.ResetPasswordToken = code;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(30);

            return code;
        }

    }
}
