using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ripplee.Server.Data
{
    // Этот класс используется только инструментами командной строки EF Core (для миграций)
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Нам нужно вручную создать объект настроек (Options) для нашего DbContext.
            // Строка подключения должна быть точно такой же, как в appsettings.json.
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite("Data Source=ripplee.db");

            // Создаем и возвращаем экземпляр нашего DbContext
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}