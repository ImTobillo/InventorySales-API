using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using InventorySalesApi.Application.Interfaces;
using InventorySalesApi.Api.Services;
using InventorySalesApi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InventorySalesApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.NombreUsuario) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Intento de inicio de sesión fallido: solicitud vacía o incompleta.");
            return BadRequest("Nombre de usuario y contraseña son requeridos.");
        }

        var usuario = await _usuarioRepository.ObtenerPorNombreUsuarioAsync(request.NombreUsuario, cancellationToken);
        if (usuario == null || !usuario.Activo)
        {
            _logger.LogWarning("Intento de inicio de sesión fallido para el usuario '{NombreUsuario}': no encontrado o inactivo.", request.NombreUsuario);
            return Unauthorized("Credenciales inválidas o usuario inactivo.");
        }

        var passwordValido = _passwordHasher.VerifyPassword(request.Password, usuario.PasswordHash);
        if (!passwordValido)
        {
            _logger.LogWarning("Intento de inicio de sesión fallido para el usuario '{NombreUsuario}': contraseña incorrecta.", request.NombreUsuario);
            return Unauthorized("Credenciales inválidas.");
        }

        // Map domain role to JWT role (Almacenero -> Operador)
        string roleClaim = usuario.Rol switch
        {
            RolUsuario.Administrador => "Administrador",
            RolUsuario.Vendedor => "Vendedor",
            RolUsuario.Almacenero => "Operador",
            _ => throw new InvalidOperationException("Rol de usuario desconocido.")
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings.GetValue<string>("Secret") ?? "SuperSecretKeyForInventorySalesApiProjectMustBeAtLeast32BytesLong!";
        var key = Encoding.ASCII.GetBytes(secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Role, roleClaim)
            }),
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.GetValue<int>("ExpiryInMinutes", 60)),
            Issuer = jwtSettings.GetValue<string>("Issuer") ?? "InventorySalesApi",
            Audience = jwtSettings.GetValue<string>("Audience") ?? "InventorySalesApi.Users",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation("Usuario '{NombreUsuario}' inició sesión correctamente. Rol asignado: {Rol}.", usuario.NombreUsuario, roleClaim);

        return Ok(new LoginResponse(
            usuario.Id,
            usuario.NombreUsuario,
            usuario.Email.Value,
            roleClaim,
            tokenString
        ));
    }
}

public record LoginRequest(string NombreUsuario, string Password);
public record LoginResponse(Guid Id, string NombreUsuario, string Email, string Rol, string Token);
