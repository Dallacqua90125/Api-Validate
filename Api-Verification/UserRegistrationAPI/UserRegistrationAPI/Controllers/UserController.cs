using Azure.Core;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
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
        private readonly UsersService _usersService;
        private readonly IConfiguration _configuration;

        public UserController(ApplicationDbContext context, UsersService usersService, IConfiguration configuration)
        {
            _context = context;
            _usersService = usersService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var users = await _usersService.GetAllUsersAsync();
            if (users == null) return NotFound();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var users = await _usersService.GetUserByIdAsync(id);
            return Ok(users);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var isUserRegistered = await _usersService.RegisterUserAsync(user);

            if (!isUserRegistered)
            {
                return BadRequest("Email already registered.");
            }

            await _usersService.SendEmailAsync(user.Email, "Verify your email", $"Your verification code is: {user.EmailVerificationCode}");

            // Retorna o objeto user completo, incluindo o ID gerado
            return Ok(user);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail(string email, string code)
        {
            var isVerified = await _usersService.VerifyEmail(email, code);

            if (!isVerified)
            {
                return BadRequest("Invalid verification code");
            }

            return Ok(new {message = "Email verified successfully" });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            var updatedUser = await _usersService.UpdateUserAsync(id, user);
            if (updatedUser == null) return NotFound();
            return Ok(updatedUser);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !user.IsEmailVerified)
            {
                return BadRequest("Usuário não encontrado ou e-mail não verificado.");
            }

            var token = _usersService.GeneratePasswordResetToken(user);
            user.ResetPasswordToken = token;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(30);

            await _context.SaveChangesAsync();

            await _usersService.SendEmailAsync(user.Email, "Redefinir senha", $"Token para redefinir senha: {token}");

            return Ok(new { message = "E-mail de redefinição de senha enviado." });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] Models.ResetPasswordRequest request)
        {
            // Buscar o usuário que tem o código de redefinição igual ao enviado
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == request.Token);

            if (user == null || user.ResetPasswordTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest("Código inválido ou expirado.");
            }

            // Atualizar a senha e invalidar o código
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetPasswordToken = null; // Invalidar o código
            user.ResetPasswordTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Senha redefinida com sucesso." });
        }
    }
}
