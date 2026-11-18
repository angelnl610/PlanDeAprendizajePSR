using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PizzaPlanetaBiblioteca.Entidades;
using PizzaPlanetaBiblioteca.Enums;

namespace PizzaPlanetaApi.Datos;

public static class DbSeeder
{
    private static readonly PasswordHasher<Usuario> _hasher = new();

    public static void SeedData(PizzeriaDbContext db)
    {
        db.Database.EnsureCreated();

        // 1. Sembrado de Usuarios iniciales idempotente
        if (!db.Usuarios.Any())
        {
            var admin = new Usuario
            {
                Nombre = "Administrador Central",
                Email = "admin@pizzaplaneta.com",
                Rol = RolUsuario.Admin,
                Telefono = "11-0000-0001",
                Direccion = "Torre de Control Central"
            };
            admin.PasswordHash = _hasher.HashPassword(admin, "Admin123!");

            var cocinero = new Usuario
            {
                Nombre = "Maestro Pizzero",
                Email = "cocinero@pizzaplaneta.com",
                Rol = RolUsuario.Cocinero,
                Telefono = "11-0000-0002",
                Direccion = "Cocina Principal"
            };
            cocinero.PasswordHash = _hasher.HashPassword(cocinero, "Cocinero123!");

            var cliente = new Usuario
            {
                Nombre = "Cliente Inicial",
                Email = "cliente@pizzaplaneta.com",
                Rol = RolUsuario.Cliente,
                Telefono = "11-0000-0003",
                Direccion = "Av. Siempre Viva 742"
            };
            cliente.PasswordHash = _hasher.HashPassword(cliente, "Cliente123!");

            db.Usuarios.AddRange(admin, cocinero, cliente);
            db.SaveChanges();
        }

        // 2. Sembrado de Pizzas activas iniciales
        if (!db.Pizzas.IgnoreQueryFilters().Any())
        {
            db.Pizzas.AddRange(
                new Pizza
                {
                    Nombre = "Muzzarella Clásica",
                    Descripcion = "Salsa de tomate casera, abundante muzzarella fundida y orégano.",
                    Precio = 9500.00m,
                    ImagenUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?auto=format&fit=crop&w=600&q=80",
                    Activa = true
                },
                new Pizza
                {
                    Nombre = "Napolitana con Ajo",
                    Descripcion = "Muzzarella, rodajas finas de tomate natural, ajo picado y hojas de albahaca.",
                    Precio = 11200.00m,
                    ImagenUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=600&q=80",
                    Activa = true
                },
                new Pizza
                {
                    Nombre = "Fugazzeta Especial",
                    Descripcion = "Cebolla caramelizada, doble muzzarella y toque de pimienta negra.",
                    Precio = 12400.00m,
                    ImagenUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?auto=format&fit=crop&w=600&q=80",
                    Activa = true
                },
                new Pizza
                {
                    Nombre = "Calabresa",
                    Descripcion = "Muzzarella, rodajas de longaniza calabresa y aceitunas negras.",
                    Precio = 13500.00m,
                    ImagenUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?auto=format&fit=crop&w=600&q=80",
                    Activa = true
                }
            );
            db.SaveChanges();
        }
    }
}
