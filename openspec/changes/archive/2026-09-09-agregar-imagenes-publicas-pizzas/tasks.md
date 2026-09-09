# Tareas de Implementación: Incorporación de Imágenes Públicas en Pizzas

- [x] 1. Modificación de Dominio y DTOs (`PizzaPlanetBiblioteca`)
  - [x] 1.1 Agregar la propiedad `public string? ImagenUrl { get; set; }` a la entidad `Pizza.cs`.
  - [x] 1.2 Agregar `public string? ImagenUrl { get; set; }` a `PizzaDto.cs`.
  - [x] 1.3 Agregar `public string? ImagenUrl { get; set; }` a `CrearPizzaDto.cs`.
  - [x] 1.4 Agregar `public string? ImagenUrl { get; set; }` a `ActualizarPizzaDto.cs`.

- [x] 2. Configuración y Migración de Base de Datos (`PizzaPlanetaApi`)
  - [x] 2.1 Mapear `ImagenUrl` en `PizzaConfig.cs` con longitud máxima 500 caracteres y como opcional.
  - [x] 2.2 Generar la migración de Entity Framework Core (`AgregarImagenUrlAPizza`).
  - [x] 2.3 Actualizar `DbSeeder.cs` con URLs públicas válidas para las pizzas del catálogo inicial.

- [x] 3. Actualización de Endpoints en el Backend (`PizzaPlanetaApi`)
  - [x] 3.1 Proyectar `ImagenUrl` en `GET /pizzas` y `GET /pizzas/{id}` en `PizzaEndpoints.cs`.
  - [x] 3.2 Asignar `ImagenUrl` desde el DTO en `POST /pizzas` y `PUT /pizzas/{id}`.

- [x] 4. Actualización del ViewModel y Controlador (`PizzaPlanetMVC`)
  - [x] 4.1 Incorporar `public string? ImagenUrl { get; set; }` en `PizzaItemViewModel.cs`.
  - [x] 4.2 Proyectar `ImagenUrl` en la acción `Pizzas()` de `HomeController.cs`.

- [x] 5. Integración de Imagen en Tarjetas, Modales y Estilos (`PizzaPlanetMVC`)
  - [x] 5.1 Actualizar `Views/Shared/_PizzaCard.cshtml` para renderizar el bloque de imagen cuando `ImagenUrl` tenga valor, con manejador `onerror` defensivo.
  - [x] 5.2 Incorporar el atributo `data-imagen-url` en el botón de tres puntos `⋮`.
  - [x] 5.3 Agregar el campo de entrada `URL Pública de la Imagen` en el modal `#modalCrearPizza` de `Views/Home/Pizzas.cshtml`.
  - [x] 5.4 Agregar el campo correspondiente en `#modalEditarPizza` de `Views/Home/Pizzas.cshtml` y enlazarlo en el script de apertura.
  - [x] 5.5 Restablecer y ajustar las clases `.pizza-card-img-wrap` y `.pizza-card-img` en `site.css`.

- [x] 6. Actualización de Documentación (`doc/`)
  - [x] 6.1 Actualizar `doc/DER.md` agregando `varchar(500) ImagenUrl` a la tabla `PIZZA`.
  - [x] 6.2 Actualizar `doc/Diagrama de clases.md` agregando `+ImagenUrl : string?` a la clase `Pizza`.
  - [x] 6.3 Actualizar `doc/Estado_Del_Proyecto.md` con los detalles del nuevo campo.

- [x] 7. Compilación y Validación
  - [x] 7.1 Compilar la solución completa con `dotnet build PizzaPlaneta.slnx`.
  - [x] 7.2 Verificar que las pizzas del seed expongan sus imágenes públicas en el catálogo.
  - [x] 7.3 Verificar que crear y editar pizzas persista la URL pública correctamente en la BD.
