## Why

Actualmente, toda la interfaz y la lógica de presentación del frontend se encuentran concentradas en `Views/Home/Index.cshtml`, mezclando la portada (Hero Section) con el catálogo y controles de búsqueda de pizzas, lo que desvirtúa la separación de responsabilidades del patrón MVC. Además, el modelo `PizzaItemViewModel.cs` contenía lógica y rutas de imágenes hardcodeadas (`ResolverImagen`) que añadían complejidad innecesaria sin corresponderse con el modelo de datos de la base de datos ni con los DTOs de `PizzaPlanetaBiblioteca`.

Para resolver estas inconsistencias y alinear la aplicación con los principios de desarrollo MVC y arquitectura cliente/servidor:
1. Se delega el catálogo completo de pizzas a una vista dedicada (`Views/Home/Pizzas.cshtml`) atendida por la acción asíncrona `Pizzas()` en `HomeController`.
2. Se eliminan las imágenes de las pizzas y el código hardcodeado en `PizzaItemViewModel`, dejando tarjetas limpias centradas en la información real del producto (nombre, descripción, precio cósmico).
3. Se garantiza el consumo 100% asíncrono de la base de datos a través de la API REST (`PizzaPlanetaApi`).
4. Se incorporan capacidades de gestión interactiva: un botón superior para crear nuevas pizzas y un menú contextual de "tres puntitos" (`⋮`) en cada tarjeta para editar parámetros o dar de baja el producto directamente contra los endpoints de la API.

## What Changes

- **Depuración de `PizzaItemViewModel` (Eliminación de Hardcodeo)**:
  - Eliminación de la propiedad `ImagenUrl` y del método estático `ResolverImagen(string nombre)`.
  - El modelo refleja fielmente los datos provenientes de la API/Base de datos: `Id`, `Nombre`, `Descripcion`, `Precio` y `PrecioFormateado`.

- **Extensión del Servicio Asíncrono (`IPizzaApiService` y `PizzaApiService`)**:
  - Incorporación de métodos asíncronos para operaciones completas de pizzas contra la API:
    - `Task<bool> CrearPizzaAsync(CrearPizzaDto dto, CancellationToken cancellationToken = default);`
    - `Task<bool> ActualizarPizzaAsync(int id, ActualizarPizzaDto dto, CancellationToken cancellationToken = default);`
    - `Task<bool> EliminarPizzaAsync(int id, CancellationToken cancellationToken = default);`
  - Manejo de excepciones y logging no bloqueante.

- **Reestructuración de `HomeController`**:
  - `Index()`: Se transforma en una acción liviana que renderiza exclusivamente la portada / landing page, sin sobrecargar la página inicial con consultas a la API.
  - `Pizzas([FromQuery] string? searchTerm, [FromQuery] string? sortOrder)`: Acción asíncrona dedicada que consulta la API, aplica filtros y devuelve el ViewModel a `Views/Home/Pizzas.cshtml`.
  - Acciones POST asíncronas para atender la creación (`CrearPizza`), edición (`EditarPizza`) y eliminación (`EliminarPizza`).

- **Rediseño y Separación de Vistas**:
  - `Views/Home/Index.cshtml`: Portada estelar con Hero Section, cintillo de advertencia arcade y botones de acción rápida con redirección hacia `Home/Pizzas` ("VER CATÁLOGO").
  - `Views/Home/Pizzas.cshtml`: Vista dedicada del catálogo con:
    - Cabecera con botón de acción destacado `[ + NUEVA PIZZA ]`.
    - Panel de telemetría y controles HUD (buscador, ordenamiento por precio ascendente/descendente y reset).
    - Grilla responsiva de tarjetas y skeleton loading asíncrono.
    - Modales interactivos con diseño retro-arcade para el alta, modificación y confirmación de baja de pizzas.
  - `Views/Shared/_PizzaCard.cshtml`: Actualización del componente de tarjeta eliminando el bloque `<img>` y añadiendo un menú contextual desplegable (`⋮`) con opciones "Editar Parámetros" y "Dar de Baja".
  - `Views/Shared/_PizzaSkeletonCard.cshtml`: Adaptación del skeleton loading para coincidir con la nueva tarjeta sin imagen.
  - `Views/Shared/_Layout.cshtml`: Actualización de la barra de navegación para incluir enlaces a `[ INICIO ]` y `[ CATÁLOGO DE PIZZAS ]`.

- **Estilos CSS Retro-Arcade en `site.css`**:
  - Clases para el menú contextual de tres puntitos (`.btn-card-menu`, `.dropdown-arcade`).
  - Formato y proporciones de las tarjetas optimizadas para la tipografía `Unbounded` y precios cósmicos sin contenedor de imagen.
  - Estilizado de los modales con estética neón cyberpunk/arcade (`.modal-arcade`).

## Capabilities

### New Capabilities
- `pizza-catalog-delegated-view`: Vista independiente y especializada para el catálogo de pizzas en `Views/Home/Pizzas.cshtml` bajo la ruta `/Home/Pizzas`.
- `pizza-management-actions`: Creación, edición y baja de variedades de pizzas desde la interfaz web consumiendo los endpoints REST de `PizzaPlanetaApi` de forma asíncrona.
- `pizza-card-context-menu`: Menú interactivo de opciones contextuales de tres puntos en cada tarjeta de pizza.

### Modified Capabilities
- `pizza-catalog-frontend`: Se remueve la sección de catálogo de la página principal (`Views/Home/Index.cshtml`) y se retira el renderizado y hardcodeo de imágenes en el frontend y en los ViewModels.

## Impact

- **Código Frontend (`PizzaPlanetMVC`)**:
  - `Models/PizzaItemViewModel.cs`: Limpieza de métodos y propiedades de imágenes.
  - `Services/IPizzaApiService.cs` y `PizzaApiService.cs`: Nuevas operaciones asíncronas HTTP (`POST`, `PUT`, `DELETE`).
  - `Controllers/HomeController.cs`: Desacoplamiento de `Index()` y adición de acciones `Pizzas()`, `CrearPizza()`, `EditarPizza()`, `EliminarPizza()`.
  - `Views/Home/Index.cshtml` y `Views/Home/Pizzas.cshtml`: Separación de responsabilidades.
  - `Views/Shared/_PizzaCard.cshtml` y `_PizzaSkeletonCard.cshtml`: Nueva estructura de tarjetas.
  - `Views/Shared/_Layout.cshtml`: Ajuste de navegación.
  - `wwwroot/css/site.css`: Ajustes visuales de menú contextual y modales.
- **Backend y Base de Datos**:
  - No requiere alterar `PizzaPlanetaApi` ni `PizzaPlanetBiblioteca` puesto que los endpoints (`GET`, `POST`, `PUT`, `DELETE` en `/pizzas`) y los DTOs ya existen y están disponibles.
