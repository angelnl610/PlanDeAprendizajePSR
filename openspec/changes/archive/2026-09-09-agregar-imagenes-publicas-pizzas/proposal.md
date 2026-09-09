## Why

Actualmente, las pizzas en `PizzaPlaneta` no poseen un campo de imagen en la base de datos ni en los contratos DTO. Anteriormente, la aplicación MVC utilizaba un método con nombres de archivos locales hardcodeados en el frontend (`ResolverImagen`), el cual fue eliminado para desacoplar la capa de presentación de datos ficticios.

Se requiere incorporar las imágenes como un parámetro formal y persistente de cada pizza en la base de datos (`ImagenUrl`), manejando **exclusivamente URLs públicas externas** (hospedadas en internet/CDNs). De este modo, se evita almacenar archivos estáticos pesados en el servidor local (`wwwroot`), manteniendo el backend liviano y permitiendo que administradores y clientes visualicen imágenes reales obtenidas dinámicamente a través de la API REST.

## What Changes

- **Dominio y Contratos (`PizzaPlanetBiblioteca`)**:
  - Incorporar la propiedad `public string? ImagenUrl { get; set; }` en la entidad `Pizza`.
  - Incorporar la propiedad `public string? ImagenUrl { get; set; }` en los DTOs `PizzaDto`, `CrearPizzaDto` y `ActualizarPizzaDto`.

- **Persistencia y Backend (`PizzaPlanetaApi`)**:
  - Configurar en `PizzaConfig.cs` el campo `ImagenUrl` con longitud máxima de 500 caracteres y como campo opcional (`IsRequired(false)`).
  - Crear y aplicar la migración de Entity Framework Core para agregar la columna `ImagenUrl` a la tabla `Pizzas`.
  - Actualizar `DbSeeder.cs` con URLs públicas reales y estables para las pizzas iniciales del menú cósmico.
  - Actualizar los mapeos de `PizzaEndpoints.cs` para leer, crear y actualizar `ImagenUrl` en las rutas `GET /pizzas`, `GET /pizzas/{id}`, `POST /pizzas` y `PUT /pizzas/{id}`.

- **Frontend Web (`PizzaPlanetMVC`)**:
  - Incorporar `public string? ImagenUrl { get; set; }` en `PizzaItemViewModel.cs`.
  - Mapear `ImagenUrl` en la acción `Pizzas()` de `HomeController.cs`.
  - Actualizar el componente de tarjeta `Views/Shared/_PizzaCard.cshtml` para renderizar el bloque de imagen cuando la pizza posea URL, incluyendo protección defensiva (`onerror`) ante enlaces caídos.
  - Modificar los modales de **Crear** y **Editar** pizza en `Views/Home/Pizzas.cshtml` para incluir el campo `URL Pública de la Imagen`.
  - Actualizar el script JavaScript para cargar la URL actual en el modal de edición mediante el atributo `data-imagen-url`.
  - Restablecer los estilos arcade en `wwwroot/css/site.css` para el marco de imagen con efecto neón (`.pizza-card-img-wrap` y `.pizza-card-img`).

- **Documentación del Proyecto (`doc/`)**:
  - Actualizar el Diagrama Entidad-Relación ([doc/DER.md](file:///mnt/Datos/Repos/PlanDeAprendizajePSR/doc/DER.md)).
  - Actualizar el Diagrama de Clases ([doc/Diagrama de clases.md](file:///mnt/Datos/Repos/PlanDeAprendizajePSR/doc/Diagrama%20de%20clases.md)).
  - Actualizar el relevamiento en [doc/Estado_Del_Proyecto.md](file:///mnt/Datos/Repos/PlanDeAprendizajePSR/doc/Estado_Del_Proyecto.md).

## Capabilities

### New Capabilities
- `pizza-public-images`: Soporte completo en base de datos, API y frontend para asignar, persistir y visualizar imágenes de pizzas mediante URLs públicas en internet sin almacenamiento local en el servidor.

### Modified Capabilities
- `pizza-catalog-frontend`: Las tarjetas de pizza vuelven a exhibir imágenes dinámicas, pero ahora provistas 100% desde la base de datos a través de la API REST, con soporte de fallback si el link no está disponible.
- `pizza-management-actions`: Los formularios de creación y edición admiten la carga de la URL pública de la imagen de la variedad.

## Impact

- **Base de Datos**:
  - Se agrega la columna `ImagenUrl VARCHAR(500) NULL` en la tabla `Pizzas`.
- **Biblioteca Compartida (`PizzaPlanetBiblioteca`)**:
  - Modificación de contratos DTO y entidad `Pizza`. Requiere recompilar los proyectos dependientes.
- **Backend (`PizzaPlanetaApi`)**:
  - Manejo transparente del nuevo campo en todos los endpoints CRUD de pizzas.
- **Frontend (`PizzaPlanetMVC`)**:
  - Tarjetas y formularios actualizados para soportar la URL de la imagen sin lógica hardcodeada.
- **Sin Almacenamiento Local**:
  - No se utiliza espacio en disco para archivos binarios (cero consumo en carpetas `wwwroot/uploads` o `wwwroot/images/pizzas/`).
