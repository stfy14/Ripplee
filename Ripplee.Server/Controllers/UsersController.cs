// Ripplee.Server/Controllers/UsersController.cs (ФИНАЛЬНАЯ ИСПРАВЛЕННАЯ ВЕРСИЯ)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ripplee.Server.Data;
using Ripplee.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class UpdateUserCriteriaDto
{
    public required string MyGender { get; set; }
    public required string MyCity { get; set; }
}

namespace Ripplee.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST /api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (userExists)
            {
                return BadRequest("Username is already taken.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully!" });
        }

        // POST /api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("criteria")]
        [Authorize]
        public async Task<IActionResult> UpdateCriteria([FromBody] UpdateUserCriteriaDto dto)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (userFromDb == null)
            {
                return NotFound("User not found.");
            }

            // Обновляем данные
            userFromDb.MyGender = dto.MyGender;
            userFromDb.MyCity = dto.MyCity;

            // Сохраняем изменения в БД
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Criteria updated successfully." });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            //var username = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized("Could not find username in token.");
            }

            var userFromDb = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            if (userFromDb == null)
            {
                return NotFound("User not found in database.");
            }

            // --- ДОБАВЛЯЕМ НОВЫЕ ПОЛЯ В ОТВЕТ ---
            return Ok(new
            {
                userFromDb.Id,
                userFromDb.Username,
                userFromDb.MyGender, // <--- новое поле
                userFromDb.MyCity    // <--- новое поле
            });
        }

        // GET /api/users/exists/{username}
        [HttpGet("exists/{username}")]
        public async Task<IActionResult> CheckUsernameExists(string username)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
            // Возвращаем простой JSON: { "exists": true } или { "exists": false }
            return Ok(new { exists = userExists });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(ClaimTypes.Name, user.Username), // Этот клейм нужен для User.Identity.Name
                new Claim("gender", user.MyGender ?? "Не указан"), // Сразу добавим пол, пригодится в хабе!
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}