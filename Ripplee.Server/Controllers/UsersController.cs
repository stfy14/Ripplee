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
                return BadRequest(new { Message = "Это имя уже занято" });
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

            return Ok(new { Message = "Пользователь успешно зарегистрировался!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { Message = "Пользователя не сузществует" });
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
                return NotFound(new { Message = "Пользователь не найден" });
            }

            userFromDb.MyGender = dto.MyGender;
            userFromDb.MyCity = dto.MyCity;

            await _context.SaveChangesAsync();

            // Генерируем новый токен с обновленными данными
            var newToken = GenerateJwtToken(userFromDb);

            return Ok(new { Message = "Критерии успешно обновлены", Token = newToken }); // Возвращаем новый токен
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { Message = "Не удается найти токен пользователя" });
            }

            var userFromDb = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            if (userFromDb == null)
            {
                return NotFound(new { Message = "Пользователь не найден в базе данных" });
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
                return Unauthorized(new { Message = "Пользователь не идентифицирован" });
            }

            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (userFromDb == null)
            {
                return NotFound(new { Message = "Пользователь не найден" });
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, userFromDb.PasswordHash))
            {
                return BadRequest(new { Message = "Некорректный старый пароль" });
            }

            userFromDb.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Пароль успешно изменен!" });
        }

        [HttpPost("change-username")]
        [Authorize]
        public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameDto dto)
        {
            var currentUsernameFromToken = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUsernameFromToken))
            {
                return Unauthorized(new { Message = "Пользователь не идентифицирован" });
            }

            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUsernameFromToken);
            if (userFromDb == null)
            {
                return NotFound(new { Message = "Пользователь не найден" });
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, userFromDb.PasswordHash))
            {
                return BadRequest(new { Message = "Некорректный текущий пароль" });
            }

            if (string.IsNullOrWhiteSpace(dto.NewUsername) || dto.NewUsername.Length < 3)
            {
                return BadRequest(new { Message = "Новый логин должен содержать не менее 3 символов" });
            }

            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == dto.NewUsername.ToLower() && u.Id != userFromDb.Id))
            {
                return BadRequest(new { Message = "Этот логин уже занято" });
            }

            userFromDb.Username = dto.NewUsername;
            await _context.SaveChangesAsync();

            var newToken = GenerateJwtToken(userFromDb);
            return Ok(new { Token = newToken, Message = "Логин успешно изменен" });
        }

        [HttpPost("avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { Message = "Пользователь не идентифицирован" });
            }

            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (userFromDb == null)
            {
                return NotFound(new { Message = "Пользователь не найден" });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "Аватар не удалось обновить" });
            }

            if (file.Length > 5 * 1024 * 1024) 
            {
                return BadRequest(new { Message = "Файл превышает лимит в 5МБ" });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                return BadRequest(new { Message = "Некорректный тип файла. Только JPG, JPEG, PNG форматы поддерживаются." });
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
                        catch (IOException ex) { Console.WriteLine($"Ошибка при удалении старого аватара {userFromDb.Username}: {ex.Message}"); }
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
                return Unauthorized(new { Message = "Пользователь не идентифицирован" });
            }

            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (userFromDb == null)
            {
                return NotFound(new { Message = "Пользователь не найден" });
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, userFromDb.PasswordHash))
            {
                return BadRequest(new { Message = "Некорректный пароль. В удалении отказано" });
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
                        catch (IOException ex) { Console.WriteLine($"Ошибка удаления аватара {userFromDb.Username} при удалении аккаунта: {ex.Message}"); }
                    }
                }
            }

            _context.Users.Remove(userFromDb);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Аккаунт успешно удален!" });
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