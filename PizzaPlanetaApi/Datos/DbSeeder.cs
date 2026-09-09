using PizzaPlanetaBiblioteca.Entidades;

namespace PizzaPlanetaApi.Datos;

public static class DbSeeder
{
    public static void SeedData(PizzeriaDbContext db)
    {
        db.Database.EnsureCreated();

        if (!db.Pizzas.Any())
        {
            db.Pizzas.AddRange(
                new Pizza
                {
                    Nombre = "Supernova Pepperoni",
                    Descripcion = "Cráteres de pepperoni crujiente, queso fundido interestelar y nuestra legendaria salsa cósmica picante.",
                    Precio = 14.50m,
                    ImagenUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?auto=format&fit=crop&w=600&q=80"
                },
                new Pizza
                {
                    Nombre = "Meteorito BBQ",
                    Descripcion = "Trocitos de pollo ahumado al carbón estelar, cebolla morada crujiente y una capa densa de BBQ planetario.",
                    Precio = 16.00m,
                    ImagenUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=600&q=80"
                },
                new Pizza
                {
                    Nombre = "Planeta Vegano",
                    Descripcion = "Champiñones galácticos, pimientos verdes del cuadrante 4 y queso vegano derretido a temperatura de órbita.",
                    Precio = 15.00m,
                    ImagenUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?auto=format&fit=crop&w=600&q=80"
                },
                new Pizza
                {
                    Nombre = "Hula Hula Anillo",
                    Descripcion = "Piñas caramelizadas al sol de Andrómeda y jamón cósmico curado en gravedad cero. Un clásico tropical estelar.",
                    Precio = 15.50m,
                    ImagenUrl = "https://images.unsplash.com/photo-1593560708920-61dd98c46a4e?auto=format&fit=crop&w=600&q=80"
                },
                new Pizza
                {
                    Nombre = "Cuatro Quesos Nebula",
                    Descripcion = "Una fusión termonuclear de mozzarella, provolone estelar, parmesano espacial y queso azul de la constelación de Orión.",
                    Precio = 17.20m,
                    ImagenUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?auto=format&fit=crop&w=600&q=80"
                },
                new Pizza
                {
                    Nombre = "Fuego Intergaláctico",
                    Descripcion = "Jalapeños radioactivos, carne picante del espacio profundo y un toque de salsa de chile fantasma estelar.",
                    Precio = 18.00m,
                    ImagenUrl = "https://images.unsplash.com/photo-1594007654729-407eedc4be65?auto=format&fit=crop&w=600&q=80"
                }
            );
            db.SaveChanges();
        }
        else
        {
            var pizzasSinImagen = db.Pizzas.Where(p => string.IsNullOrEmpty(p.ImagenUrl)).ToList();
            if (pizzasSinImagen.Any())
            {
                foreach (var p in pizzasSinImagen)
                {
                    var n = p.Nombre.ToLowerInvariant();
                    if (n.Contains("pepperoni") || n.Contains("supernova"))
                        p.ImagenUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?auto=format&fit=crop&w=600&q=80";
                    else if (n.Contains("bbq") || n.Contains("meteorito"))
                        p.ImagenUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?auto=format&fit=crop&w=600&q=80";
                    else if (n.Contains("vegano") || n.Contains("vegetariana"))
                        p.ImagenUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?auto=format&fit=crop&w=600&q=80";
                    else if (n.Contains("hula") || n.Contains("anillo") || n.Contains("anana") || n.Contains("piña"))
                        p.ImagenUrl = "https://images.unsplash.com/photo-1593560708920-61dd98c46a4e?auto=format&fit=crop&w=600&q=80";
                    else if (n.Contains("queso") || n.Contains("nebula"))
                        p.ImagenUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?auto=format&fit=crop&w=600&q=80";
                    else if (n.Contains("fuego") || n.Contains("picante"))
                        p.ImagenUrl = "https://images.unsplash.com/photo-1594007654729-407eedc4be65?auto=format&fit=crop&w=600&q=80";
                    else
                        p.ImagenUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?auto=format&fit=crop&w=600&q=80";
                }
                db.SaveChanges();
            }
        }

        if (!db.Clientes.Any())
        {
            db.Clientes.AddRange(
                new Cliente { Nombre = "Comandante Shepard", Telefono = "11-9988-7766", Direccion = "Estación Citadel Nivel 28" },
                new Cliente { Nombre = "Luke Skywalker", Telefono = "11-5544-3322", Direccion = "Sector Tatooine Granja de Humedad" },
                new Cliente { Nombre = "Ellen Ripley", Telefono = "11-2233-4455", Direccion = "Nave Nostromo Muelle 7" }
            );
            db.SaveChanges();
        }
    }
}
