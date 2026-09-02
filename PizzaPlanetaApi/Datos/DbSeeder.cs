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
                    Precio = 14.50m
                },
                new Pizza
                {
                    Nombre = "Meteorito BBQ",
                    Descripcion = "Trocitos de pollo ahumado al carbón estelar, cebolla morada crujiente y una capa densa de BBQ planetario.",
                    Precio = 16.00m
                },
                new Pizza
                {
                    Nombre = "Planeta Vegano",
                    Descripcion = "Champiñones galácticos, pimientos verdes del cuadrante 4 y queso vegano derretido a temperatura de órbita.",
                    Precio = 15.00m
                },
                new Pizza
                {
                    Nombre = "Hula Hula Anillo",
                    Descripcion = "Piñas caramelizadas al sol de Andrómeda y jamón cósmico curado en gravedad cero. Un clásico tropical estelar.",
                    Precio = 15.50m
                },
                new Pizza
                {
                    Nombre = "Cuatro Quesos Nebula",
                    Descripcion = "Una fusión termonuclear de mozzarella, provolone estelar, parmesano espacial y queso azul de la constelación de Orión.",
                    Precio = 17.20m
                },
                new Pizza
                {
                    Nombre = "Fuego Intergaláctico",
                    Descripcion = "Jalapeños radioactivos, carne picante del espacio profundo y un toque de salsa de chile fantasma estelar.",
                    Precio = 18.00m
                }
            );
            db.SaveChanges();
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
