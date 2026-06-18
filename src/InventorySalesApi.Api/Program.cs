using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using MediatR;
using FluentValidation;
using Asp.Versioning;
using InventorySalesApi.Application.DependencyInjection;
using InventorySalesApi.Infrastructure;
using Infrastructure.Persistence;
using InventorySalesApi.Api.Middleware;
using InventorySalesApi.Api.PipelineBehaviors;
using InventorySalesApi.Api.Services;
using InventorySalesApi.Domain.Entities;
using InventorySalesApi.Domain.ValueObjects;
using InventorySalesApi.Domain.Enums;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure DB Context with SQL Server (Enable Retry on Failure for Docker initialization)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        }));

// Register Application & Infrastructure Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

// Register Password Hasher
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

// Register Validation Pipeline Behavior for MediatR
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Configure API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new QueryStringApiVersionReader("api-version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings.GetValue<string>("Secret") ?? "SuperSecretKeyForInventorySalesApiProjectMustBeAtLeast32BytesLong!";
var key = Encoding.ASCII.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer") ?? "InventorySalesApi",
        ValidateAudience = true,
        ValidAudience = jwtSettings.GetValue<string>("Audience") ?? "InventorySalesApi.Users",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Inventory & Sales API", 
        Version = "v1",
        Description = "Sistema de Gestión de Inventario y Ventas - API REST Presentation Layer"
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduzca 'Bearer' [espacio] y luego su token JWT.\n\nEjemplo: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();

// Enable Swagger UI in both Dev and Prod for testing purposes
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory & Sales API v1");
    c.RoutePrefix = "swagger"; // Expose Swagger UI at root-level /swagger
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database Seeding and Initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        // Ensure database is created
        context.Database.EnsureCreated();

        // Seed Categorías if empty
        if (!context.Categorias.Any())
        {
            var catElectro = new Categoria("Electrodomésticos", "Dispositivos electrónicos del hogar");
            catElectro.RegistrarCreacion("Semilla");
            await context.Categorias.AddAsync(catElectro);

            var catAlimentos = new Categoria("Alimentos", "Productos alimenticios y consumibles");
            catAlimentos.RegistrarCreacion("Semilla");
            await context.Categorias.AddAsync(catAlimentos);

            await context.SaveChangesAsync();
        }

        // Seed Usuarios if empty
        if (!context.Usuarios.Any())
        {
            // Seed Admin User
            var adminUser = new Usuario(
                "admin", 
                new Email("admin@sistema.com"), 
                passwordHasher.HashPassword("AdminPassword123!"), 
                RolUsuario.Administrador
            );
            adminUser.RegistrarCreacion("Semilla");
            await context.Usuarios.AddAsync(adminUser);

            // Seed Vendedor User
            var vendedorUser = new Usuario(
                "vendedor", 
                new Email("vendedor@sistema.com"), 
                passwordHasher.HashPassword("VendedorPassword123!"), 
                RolUsuario.Vendedor
            );
            vendedorUser.RegistrarCreacion("Semilla");
            await context.Usuarios.AddAsync(vendedorUser);

            // Seed Operador User (Almacenero)
            var operadorUser = new Usuario(
                "operador", 
                new Email("operador@sistema.com"), 
                passwordHasher.HashPassword("OperadorPassword123!"), 
                RolUsuario.Almacenero
            );
            operadorUser.RegistrarCreacion("Semilla");
            await context.Usuarios.AddAsync(operadorUser);

            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al migrar o sembrar la base de datos.");
    }
}

app.Run();
