// Файл: Ripplee.Server/Program.cs (ФИНАЛЬНАЯ ВЕРСИЯ)

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ripplee.Server.Data;
using Ripplee.Server.Hubs;
using System.IdentityModel.Tokens.Jwt; // Убедитесь, что этот using на месте
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
    };
});

// Регистрируем сервисы Авторизации
builder.Services.AddAuthorization();

// Регистрируем контроллеры
builder.Services.AddControllers();

// Регистрируем сервисы SignalR
builder.Services.AddSignalR();

// Добавляем и настраиваем Swagger (OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // Это поможет Swagger правильно найти свои файлы за Nginx
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ripplee API v1");
    options.RoutePrefix = "swagger"; // Убедитесь, что URL: http://your_ip/swagger
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

// Включаем аутентификацию и авторизацию.
app.UseAuthentication();
app.UseAuthorization();

// Сопоставляем запросы с методами в контроллерах
app.MapControllers();

// ULR к hUB
app.MapHub<MatchmakingHub>("/matchmakingHub");

app.Run();