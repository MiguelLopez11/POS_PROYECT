using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using POS.Data;
using POS.Services;
using System;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using POS.Middlewares;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtKey = builder.Configuration["Jwt:Key"];
//var jwtSettings = builder.Configuration.GetSection("Jwt");
//var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);
//Add secret key management solution for production use 
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? "")),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "POS Proyect API",
        Version = "v1",
        Description = "API con autenticación JWT"
    });

    
    // Agregar seguridad JWT a Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
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
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Rate Limiting CORREGIDO
builder.Services.AddRateLimiter(options =>
{
    // Política estricta SIN queue (para ver el 429 inmediatamente)
    options.AddFixedWindowLimiter("StrictPolicy", policyOptions =>
    {
        policyOptions.PermitLimit = 3;  // Solo 3 requests
        policyOptions.Window = TimeSpan.FromSeconds(30); // Ventana de 30 segundos para testing
        policyOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        policyOptions.QueueLimit = 0; // CERO - sin cola, rechazo inmediato
    });

    // Política general
    options.AddFixedWindowLimiter("GeneralPolicy", policyOptions =>
    {
        policyOptions.PermitLimit = 50;
        policyOptions.Window = TimeSpan.FromMinutes(1);
        policyOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        policyOptions.QueueLimit = 0; // CERO - sin cola
    });

    // Sliding Window - alternativa que puede ser más efectiva
    options.AddSlidingWindowLimiter("SlidingPolicy", policyOptions =>
    {
        policyOptions.PermitLimit = 5;
        policyOptions.Window = TimeSpan.FromSeconds(30);
        policyOptions.SegmentsPerWindow = 3;
        policyOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        policyOptions.QueueLimit = 0;
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;

        // Agregar headers informativos
        context.HttpContext.Response.Headers["Retry-After"] = "30";

        await context.HttpContext.Response.WriteAsync(
            "?? Too many requests. Please try again in 30 seconds.", token);
    };
});

builder.Services.AddDbContext<POSDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registrar servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "POS Proyect API v1");
        options.RoutePrefix = string.Empty; // Para que Swagger esté en la raíz
        options.EnablePersistAuthorization(); // Persistir el token entre recargas
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}
app.UseRateLimiter();
app.UseSecurityHeaders();
app.UseUserMiddleware();
app.UseHttpsRedirection();
// Asegúrate de habilitar autenticación antes de autorización
app.UseAuthentication();

app.UseAuthorization();


// Endpoint de error para producción
app.MapControllers();
app.Map("/error", () => Results.Problem("An error occurred.", statusCode: 500));

app.Run();

