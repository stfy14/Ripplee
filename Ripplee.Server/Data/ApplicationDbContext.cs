using Microsoft.EntityFrameworkCore;
using Ripplee.Server.Models;

namespace Ripplee.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
    }
}