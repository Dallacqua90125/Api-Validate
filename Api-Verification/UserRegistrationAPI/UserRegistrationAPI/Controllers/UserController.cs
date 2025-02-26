using Azure.Core;
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
        private readonly UsersService _usersService;

        public UserController(ApplicationDbContext context, UsersService usersService)
        {
            _context = context;
            _usersService = usersService;
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
        public async Task<IActionResult> Register(User user)
        {
            var isUserRegistered = await _usersService.RegisterUserAsync(user);

            if (!isUserRegistered)
            {
                return BadRequest("Email already registered.");
            }

            await _usersService.SendEmailAsync(user.Email, "Verify your email", $"Your verification code is: {user.EmailVerificationCode}");

            return Ok(new { message = "User registered. Please check your email for the verification code." });
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


    }
}
