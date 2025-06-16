using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ripplee.Server.Data;
using Ripplee.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ripplee.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment; // Для доступа к wwwroot

        public UsersController(ApplicationDbContext context, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (userExists)
            {
                return BadRequest(new { Message = "Username is already taken." });
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHash,
                MyGender = string.Empty, 
                MyCity = string.Empty   
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { Message = "Invalid credentials." });
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        [HttpPost("criteria")]
        [Authorize]
        public async Task<IActionResult> UpdateCriteria([FromBody] UpdateUserCriteriaDto dto)
        {
            var username = User.Identity?.Name; // Получаем имя пользователя из текущего токена
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (userFromDb == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            userFromDb.MyGender = dto.MyGender;
            userFromDb.MyCity = dto.MyCity;

            await _context.SaveChangesAsync();

            // Генерируем новый токен с обновленными данными
            var newToken = GenerateJwtToken(userFromDb);

            return Ok(new { Message = "Criteria updated successfully.", Token = newToken }); // Возвращаем новый токен
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { Message = "Could not find username in token." });
            }

            var userFromDb = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            if (userFromDb == null)
            {
                return NotFound(new { Message = "User not found in database." });
            }

            return Ok(new
            {
                userFromDb.Id,
                userFromDb.Username,
                userFromDb.MyGender,
                userFromDb.MyCity,
                userFromDb.AvatarUrl
            });
        }

        [HttpGet("exists/{username}")]
        public async Task<IActionResult> CheckUsernameExists(string username)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
            return Ok(new { exists = userExists });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { Message = "User identity not found in token." });
            }

            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (userFromDb == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, userFromDb.PasswordHash))
            {
                return BadRequest(new { Message = "Invalid old password." });
            }

            userFromDb.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Password changed successfully." });
        }

        [HttpPost("change-username")]
        [Authorize]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDto dto)
        {
            var currentUsernameFromToken = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUsernameFromToken))
            {
                return Unauthorized(new { Message = "User identity not found in token." });
            }

            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUsernameFromToken);
            if (userFromDb == null)
            {
                return NotFound(new { Message = "Current user not found." });
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, userFromDb.PasswordHash))
            {
                return BadRequest(new { Message = "Invalid current password." });
            }

            if (string.IsNullOrWhiteSpace(dto.NewUsername) || dto.NewUsername.Length < 3)
            {
                return BadRequest(new { Message = "New username is invalid (minimum 3 characters)." });
            }

            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == dto.NewUsername.ToLower() && u.Id != userFromDb.Id))
            {
                return BadRequest(new { Message = "New username is already taken." });
            }

            userFromDb.Username = dto.NewUsername;
            await _context.SaveChangesAsync();

            var newToken = GenerateJwtToken(userFromDb);
            return Ok(new { Token = newToken, Message = "Username changed successfully." });
        }

        [HttpPost("avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { Message = "User identity not found in token." });
            }

            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (userFromDb == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "No file uploaded." });
            }

            if (file.Length > 5 * 1024 * 1024) 
            {
                return BadRequest(new { Message = "File size exceeds limit (5MB)." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                return BadRequest(new { Message = "Invalid file type. Only JPG, JPEG, PNG are allowed." });
            }

            // Используем для получения пути к wwwroot
            var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "avatars");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            if (!string.IsNullOrEmpty(userFromDb.AvatarUrl))
            {
                var oldFileName = Path.GetFileName(userFromDb.AvatarUrl);
                if (!string.IsNullOrEmpty(oldFileName))
                {
                    var oldFilePath = Path.Combine(uploadsFolderPath, oldFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try { System.IO.File.Delete(oldFilePath); }
                        catch (IOException ex) { Console.WriteLine($"Error deleting old avatar for user {userFromDb.Username}: {ex.Message}"); }
                    }
                }
            }

            var uniqueFileName = $"{userFromDb.Id}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarRelativeUrl = $"/avatars/{uniqueFileName}";
            userFromDb.AvatarUrl = avatarRelativeUrl;
            await _context.SaveChangesAsync();

            return Ok(new { AvatarUrl = avatarRelativeUrl });
        }

        [HttpPost("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto) 
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { Message = "User identity not found in token." });
            }

            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (userFromDb == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, userFromDb.PasswordHash))
            {
                return BadRequest(new { Message = "Invalid password. Account deletion denied." });
            }

            if (!string.IsNullOrEmpty(userFromDb.AvatarUrl))
            {
                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "avatars");
                var fileName = Path.GetFileName(userFromDb.AvatarUrl);
                if (!string.IsNullOrEmpty(fileName))
                {
                    var filePath = Path.Combine(uploadsFolderPath, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        try { System.IO.File.Delete(filePath); }
                        catch (IOException ex) { Console.WriteLine($"Error deleting avatar for user {userFromDb.Username} during account deletion: {ex.Message}"); }
                    }
                }
            }

            _context.Users.Remove(userFromDb);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Account deleted successfully." });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Gender, user.MyGender ?? string.Empty),
                new Claim("city", user.MyCity ?? string.Empty),
                new Claim("avatar_url", user.AvatarUrl ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30), 
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}