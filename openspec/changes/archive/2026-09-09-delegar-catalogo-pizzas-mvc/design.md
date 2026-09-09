## Context

El proyecto `PizzaPlanetMVC` implementa el frontend web para la pizzería intergaláctica. Inicialmente, la vista principal `Views/Home/Index.cshtml` combinaba la presentación de la marca con el catálogo interactivo, y el ViewModel `PizzaItemViewModel.cs` contenía un método de resolución de imágenes (`ResolverImagen`) que asignaba nombres de archivo basándose en cadenas hardcodeadas. 

El objetivo de este cambio es ordenar la arquitectura de acuerdo al patrón MVC, delegando el catálogo a una vista especializada (`Views/Home/Pizzas.cshtml`), suprimiendo completamente las imágenes para eliminar complejidad innecesaria y hardcodeos, garantizando el consumo asíncrono de los datos de la base de datos a través de `PizzaPlanetaApi`, e incorporando controles de gestión de pizzas (alta mediante botón superior y menú contextual de tres puntos para edición y eliminación).

## Goals / Non-Goals

**Goals:**
- Separar responsabilidades en MVC: `Home/Index` como portada (Landing) y `Home/Pizzas` como catálogo de productos.
- Eliminar el hardcodeo de imágenes en `PizzaItemViewModel.cs` y en las tarjetas Razor (`_PizzaCard.cshtml` y `_PizzaSkeletonCard.cshtml`).
- Implementar el consumo 100% asíncrono de la base de datos a través de `PizzaPlanetaApi` utilizando `HttpClient` y métodos no bloqueantes (`GetFromJsonAsync`, `PostAsJsonAsync`, `PutAsJsonAsync`, `DeleteAsync`).
- Incorporar en la vista `Pizzas.cshtml` un botón superior para la creación de nuevas pizzas mediante un modal accesible.
- Incorporar en cada tarjeta de pizza un botón desplegable de tres puntos (`⋮`) que permita editar sus parámetros (nombre, descripción, precio) o darla de baja con confirmación previa.
- Actualizar la barra de navegación de `_Layout.cshtml` para enlazar tanto a la portada como al catálogo de pizzas.

**Non-Goals:**
- No conectar el proyecto MVC directamente a la base de datos (`PizzeriaDbContext`), preservando la arquitectura distribuida donde el backend API gestiona la persistencia.
- No mantener campos ni rutas de imágenes en el modelo ni en las vistas de pizzas.
- No alterar las firmas de endpoints existentes en `PizzaPlanetaApi`, aprovechando los endpoints ya implementados (`POST /pizzas`, `PUT /pizzas/{id}`, `DELETE /pizzas/{id}`).

## Decisions

### 1. Delegación del Catálogo a `Views/Home/Pizzas.cshtml`
- **Decisión**: Conservar `HomeController` y trasladar la lógica del catálogo a una nueva acción `[HttpGet] public async Task<IActionResult> Pizzas([FromQuery] string? searchTerm, [FromQuery] string? sortOrder)` con su correspondiente vista `Views/Home/Pizzas.cshtml`.
- **Justificación**: Mantiene una estructura de controladores simple y unificada en esta fase del proyecto, permitiendo que la ruta `/Home/Pizzas` responda con el catálogo completo y `/Home/Index` actúe como página de bienvenida limpia con botones de llamada a la acción (CTA).

### 2. Eliminación Total de Imágenes de Pizzas
- **Decisión**: Eliminar la propiedad `ImagenUrl` y el método `ResolverImagen` de `PizzaItemViewModel.cs`.
- **Estructura resultante de `PizzaItemViewModel`**:
  ```csharp
  public class PizzaItemViewModel
  {
      public int Id { get; set; }
      public string Nombre { get; set; } = string.Empty;
      public string Descripcion { get; set; } = string.Empty;
      public decimal Precio { get; set; }
      public string PrecioFormateado => $"${Precio:0.00}";
  }
  ```
- **Diseño de Tarjeta**: En `_PizzaCard.cshtml`, se elimina el contenedor de imagen. La tarjeta se optimiza verticalmente, destacando el encabezado con el título y el menú contextual, el cuerpo con la descripción temática y el pie de tarjeta con el badge neón de precio cósmico.

### 3. Ampliación Asíncrona de `IPizzaApiService`
- **Decisión**: Extender `IPizzaApiService` e implementarlo en `PizzaApiService` con los siguientes métodos asíncronos:
  ```csharp
  public interface IPizzaApiService
  {
      Task<IEnumerable<PizzaDto>> ObtenerPizzasAsync(CancellationToken cancellationToken = default);
      Task<PizzaDto?> ObtenerPizzaPorIdAsync(int id, CancellationToken cancellationToken = default);
      Task<bool> CrearPizzaAsync(CrearPizzaDto dto, CancellationToken cancellationToken = default);
      Task<bool> ActualizarPizzaAsync(int id, ActualizarPizzaDto dto, CancellationToken cancellationToken = default);
      Task<bool> EliminarPizzaAsync(int id, CancellationToken cancellationToken = default);
  }
  ```
- **Implementación**:
  - `CrearPizzaAsync`: Ejecuta `await _httpClient.PostAsJsonAsync("/pizzas", dto, cancellationToken)` y valida `response.IsSuccessStatusCode`.
  - `ActualizarPizzaAsync`: Ejecuta `await _httpClient.PutAsJsonAsync($"/pizzas/{id}", dto, cancellationToken)`.
  - `EliminarPizzaAsync`: Ejecuta `await _httpClient.DeleteAsync($"/pizzas/{id}", cancellationToken)`.

### 4. Menú de Tres Puntitos (`⋮`) y Modales Arcade
- **Decisión**: Integrar en cada tarjeta de pizza un botón dropdown de Bootstrap 5 con el ícono `⋮` (`&vellip;` o SVG minimalista) estilizado con colores arcade.
- **Acciones del Menú**:
  - *Editar Parámetros*: Abre un modal (`#modalEditarPizza`) cargando los atributos de la pizza (`data-id`, `data-nombre`, `data-descripcion`, `data-precio`) en los campos del formulario.
  - *Dar de Baja*: Abre un modal de confirmación (`#modalEliminarPizza`) con advertencia en rojo neón antes de despachar la petición de baja.
- **Botón Superior**: En la parte superior de `Views/Home/Pizzas.cshtml` se ubica el botón `[ + AGREGAR PIZZA AL CATÁLOGO ]`, el cual despliega el modal `#modalCrearPizza`.

### 5. Flujo de Controladores para Mutaciones CRUD
- **Decisión**: En `HomeController`, implementar las acciones:
  - `[HttpPost] public async Task<IActionResult> CrearPizza(CrearPizzaDto dto)`
  - `[HttpPost] public async Task<IActionResult> EditarPizza(int id, ActualizarPizzaDto dto)`
  - `[HttpPost] public async Task<IActionResult> EliminarPizza(int id)`
  Tras ejecutar la llamada asíncrona al servicio, redirigen a `RedirectToAction(nameof(Pizzas))` o retornan respuesta adecuada, manteniendo el flujo estándar de ASP.NET Core MVC (patrón Post-Redirect-Get).

## Risks / Trade-offs

- **[Riesgo] Conectividad con la API**: Si `PizzaPlanetaApi` no está activa, las operaciones CRUD fallarán.
  - *Mitigación*: Mostrar alertas amigables en el panel HUD utilizando `TempData` (ej. `TempData["MensajeExito"]` y `TempData["MensajeError"]`) con la estética arcade existente.
- **[Riesgo] Validación de Datos de Entrada**: Precios negativos o nombres vacíos enviados en los formularios.
  - *Mitigación*: Validación en el cliente (atributos HTML5 `required`, `min="0.01"`, `step="0.01"`) y validación en el backend antes de despachar hacia la API.
