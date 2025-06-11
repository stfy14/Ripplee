// Ripplee.Server/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Ripplee.Server.Models;

namespace Ripplee.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Эта строка говорит EF Core, что у нас есть таблица "Users",
        // которая соответствует модели User.
        public DbSet<User> Users { get; set; }
    }
}