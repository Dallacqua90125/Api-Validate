using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRegistrationAPI.Data;
using UserRegistrationAPI.Models;
using UserRegistrationAPI.Services;

namespace UserRegistrationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public UserController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // Método de registro de usuário
        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Email already registered.");
            }

            // Criptografando a senha do usuário
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // Gerando o código de verificação
            user.EmailVerificationCode = GenerateVerificationCode();

            // Adicionando o usuário no banco de dados
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Enviando o código de verificação por e-mail
            await _emailService.SendEmailAsync(user.Email, "Verify your email", $"Your verification code is: {user.EmailVerificationCode}");

            return Ok("User registered. Please check your email for the verification code.");
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail(string email, string code)
        {
            // Procurando o usuário no banco de dados
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.EmailVerificationCode != code)
            {
                return BadRequest("Invalid verification code.");
            }

            // Limpando o código de verificação após a confirmação
            user.EmailVerificationCode = null;

            // Marcando o email como verificado
            user.IsEmailVerified = true;

            await _context.SaveChangesAsync();

            return Ok("Email verified successfully.");
        }


        // Método para gerar o código de verificação
        private string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}
