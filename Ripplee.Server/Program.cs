// Файл: Ripplee.Server/Program.cs (ФИНАЛЬНАЯ ИСПРАВЛЕННАЯ ВЕРСИЯ)

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ripplee.Server.Data;
using Ripplee.Server.Hubs;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Ripplee.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. РЕГИСТРАЦИЯ СЕРВИСОВ В DI-КОНТЕЙНЕРЕ ---

// Добавляем DbContext для работы с базой данных SQLite.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


// --- НАЧАЛО БЛОКА НАСТРОЙКИ АУТЕНТИФИКАЦИИ (ИСПРАВЛЕННЫЙ СПОСОБ) ---

// Шаг 1: Добавляем сервисы аутентификации и указываем схему по умолчанию.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Добавляем обработчик JwtBearer без немедленной настройки.
.AddJwtBearer();

// Шаг 2: Регистрируем отдельный конфигуратор для JwtBearerOptions.
// Этот подход гарантирует, что IConfiguration будет полностью загружена
// из всех источников (файлы, User Secrets, переменные окружения) к моменту настройки.
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, configuration) =>
    {
        var jwtKey = configuration["Jwt:Key"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// --- КОНЕЦ БЛОКА НАСТРОЙКИ АУТЕНТИФИКАЦИИ ---



builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddSingleton<IMatchmakingService, MatchmakingService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Настройка для использования JWT-токенов в интерфейсе Swagger.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token. Example: \"Bearer {token}\"",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// --- 2. ПОСТРОЕНИЕ ПРИЛОЖЕНИЯ ---

var app = builder.Build();


// --- 3. НАСТРОЙКА КОНВЕЙЕРА ОБРАБОТКИ HTTP-ЗАПРОСОВ (MIDDLEWARE) ---

// Этот блок автоматически создает и применяет миграции базы данных при запуске.
// Это самый простой способ поддерживать БД в актуальном состоянии при развертывании.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database migration check completed. Database is up to date.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");
        // В случае ошибки при миграции, лучше остановить приложение, чтобы не работать с некорректной БД.
        throw;
    }
}


// Включаем Swagger в любом окружении для простоты отладки на сервере.
// В реальном продакшене можно обернуть в if (app.Environment.IsDevelopment()).
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // Это поможет Swagger правильно найти свои файлы за Nginx
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ripplee API v1");
    // Указываем, что Swagger UI будет доступен по корневому пути /swagger
    options.RoutePrefix = "swagger";
});

// Middleware для корректной работы за прокси-сервером (Nginx).
// Он считывает заголовки X-Forwarded-For и X-Forwarded-Proto.
// ВАЖНО: Должен быть одним из первых в конвейере.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Убираем app.UseHttpsRedirection(), так как терминированием SSL занимается Nginx.
// Если оставить, может вызывать проблемы с редиректами за прокси.
// app.UseHttpsRedirection(); 

// Включаем аутентификацию и авторизацию.
// ВАЖНО: UseAuthentication() всегда должен идти перед UseAuthorization().
app.UseAuthentication();
app.UseAuthorization();

// Сопоставляем запросы с методами в контроллерах.
app.MapControllers();

// Сопоставляем URL для хаба SignalR.
app.MapHub<MatchmakingHub>("/matchmakingHub");

// --- 4. ЗАПУСК ПРИЛОЖЕНИЯ ---

app.Run();