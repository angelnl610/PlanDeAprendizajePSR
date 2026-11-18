using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PizzaPlanetaApi.Datos;
using PizzaPlanetaBiblioteca.DTOs;
using PizzaPlanetaBiblioteca.Entidades;
using PizzaPlanetaBiblioteca.Enums;

namespace PizzaPlanetaApi.Endpoints;

public static class AuthEndpoints
{
    private static readonly PasswordHasher<Usuario> _passwordHasher = new();

    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth");

        group.MapPost("/registro", async (RegistroDto dto, PizzeriaDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return Results.BadRequest(new { error = "ERR_INVALID_INPUT", mensaje = "Nombre, email y contraseña son obligatorios." });
            }

            var existe = await db.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower().Trim());
            if (existe)
            {
                return Results.BadRequest(new { error = "ERR_EMAIL_DUPLICATED", mensaje = "El correo electrónico ya está registrado." });
            }

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Email = dto.Email.ToLower().Trim(),
                Rol = RolUsuario.Cliente,
                Telefono = dto.Telefono?.Trim() ?? string.Empty,
                Direccion = dto.Direccion?.Trim() ?? string.Empty
            };

            usuario.PasswordHash = _passwordHasher.HashPassword(usuario, dto.Password);

            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();

            return Results.Created($"/usuarios/{usuario.Id}", new { mensaje = "Usuario registrado exitosamente.", id = usuario.Id });
        });

        group.MapPost("/login", async (LoginDto dto, PizzeriaDbContext db, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return Results.Unauthorized();
            }

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower().Trim());
            if (usuario is null)
            {
                return Results.Json(new { error = "ERR_INVALID_CREDENTIALS", mensaje = "Credenciales inválidas." }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var resultadoHash = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, dto.Password);
            if (resultadoHash == PasswordVerificationResult.Failed)
            {
                return Results.Json(new { error = "ERR_INVALID_CREDENTIALS", mensaje = "Credenciales inválidas." }, statusCode: StatusCodes.Status401Unauthorized);
            }

            // Generar JWT Token
            var jwtKey = config["Jwt:Key"] ?? "ClaveSecretaSuperSeguraPizzaPlaneta2026!Min32Bytes";
            var issuer = config["Jwt:Issuer"] ?? "PizzaPlanetaApi";
            var audience = config["Jwt:Audience"] ?? "PizzaPlanetMVC";

            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var respuesta = new AuthRespuestaDto
            {
                Token = tokenString,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                Rol = usuario.Rol
            };

            return Results.Ok(respuesta);
        });
    }
}
