using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Marathon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbcontext _db;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<UserAccount> _hasher;

        public AuthController(
            ApplicationDbcontext db,
            IConfiguration config,
            IPasswordHasher<UserAccount> hasher)
        {
            _db = db;
            _config = config;
            _hasher = hasher;
        }

        // ------------------------------------------------------------
        // POST: api/auth/register
        // ------------------------------------------------------------
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO regDto)
        {
            if (await _db.accounts.AnyAsync(u => u.Email == regDto.Email))
                return BadRequest("Email already exists");

            var regUser = new UserAccount
            {
                Email = regDto.Email,
                PasswordHash = null, // will be set below
                Role = "User"   // ignore regDto.Role — prevent self-promotion 
                //Roles — Paticipant, Event Manager or Admin
            };

            regUser.PasswordHash = _hasher.HashPassword(regUser, regDto.Password);

            _db.accounts.Add(regUser);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful",
                userId = regUser.UserAccountId,
                email = regUser.Email
            });
        }

        // ------------------------------------------------------------
        // POST: api/auth/login
        // ------------------------------------------------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO logDto)
        {
            var user = await _db.accounts.FirstOrDefaultAsync(u => u.Email == logDto.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");

            var result = _hasher.VerifyHashedPassword(
                user, user.PasswordHash, logDto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password");

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.UserAccountId,
                    user.Email,
                    user.Role
                }
            });
        }

        // ------------------------------------------------------------
        // GET: api/auth/me   (requires JWT)
        // ------------------------------------------------------------
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(raw, out var userId))
                return Unauthorized();

            var user = await _db.accounts.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.UserAccountId,
                user.Email,
                user.Role,
                IsAdmin = User.IsInRole("Admin")
            });
        }

        // ------------------------------------------------------------
        // Helper — private so Swagger ignores it
        // ------------------------------------------------------------
        private string GenerateJwtToken(UserAccount user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserAccountId.ToString()),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}