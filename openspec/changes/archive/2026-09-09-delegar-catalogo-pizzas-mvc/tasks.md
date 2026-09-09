# Tareas de Implementación: Delegar Catálogo de Pizzas y Operaciones Asíncronas

- [x] 1. Limpieza de modelo y eliminación de hardcodeo de imágenes
  - [x] 1.1 Eliminar la propiedad `ImagenUrl` y el método estático `ResolverImagen` en `PizzaPlanetMVC/Models/PizzaItemViewModel.cs`.
  - [x] 1.2 Asegurar que el ViewModel mantenga únicamente las propiedades esenciales del producto (`Id`, `Nombre`, `Descripcion`, `Precio`, `PrecioFormateado`).

- [x] 2. Ampliación del servicio asíncrono hacia la API REST
  - [x] 2.1 Declarar los métodos `CrearPizzaAsync`, `ActualizarPizzaAsync` y `EliminarPizzaAsync` en `PizzaPlanetMVC/Services/IPizzaApiService.cs`.
  - [x] 2.2 Implementar los métodos asíncronos en `PizzaPlanetMVC/Services/PizzaApiService.cs` utilizando `HttpClient.PostAsJsonAsync`, `PutAsJsonAsync` y `DeleteAsync` con manejo de excepciones y cancelación.

- [x] 3. Reestructuración de `HomeController`
  - [x] 3.1 Refactorizar la acción `Index()` para que sea una acción liviana que renderice la portada sin consultar la API.
  - [x] 3.2 Crear la acción asíncrona `Pizzas(string? searchTerm, string? sortOrder)` que consulta la API, aplica filtros/ordenamiento y retorna el modelo a `Views/Home/Pizzas.cshtml`.
  - [x] 3.3 Implementar la acción POST asíncrona `CrearPizza(CrearPizzaDto dto)` con redirección a `Pizzas` y mensajes de estado en `TempData`.
  - [x] 3.4 Implementar la acción POST asíncrona `EditarPizza(int id, ActualizarPizzaDto dto)` con redirección a `Pizzas` y mensajes de estado en `TempData`.
  - [x] 3.5 Implementar la acción POST asíncrona `EliminarPizza(int id)` con redirección a `Pizzas` y mensajes de estado en `TempData`.

- [x] 4. Limpieza de la página principal (`Views/Home/Index.cshtml`)
  - [x] 4.1 Retirar la sección del catálogo de pizzas, el HUD de búsqueda y los contenedores de tarjetas.
  - [x] 4.2 Mantener la sección Hero y la cinta de advertencia `hazard-ribbon`.
  - [x] 4.3 Actualizar el botón de llamada a la acción "VER PIZZAS" para que navegue hacia `asp-controller="Home" asp-action="Pizzas"`.

- [x] 5. Creación de la vista especializada `Views/Home/Pizzas.cshtml`
  - [x] 5.1 Trasladar la estructura del catálogo y el panel HUD de búsqueda y telemetría de costos.
  - [x] 5.2 Incorporar en la parte superior el botón de acción destacado `[ + NUEVA PIZZA EN ÓRBITA ]`.
  - [x] 5.3 Implementar el bloque de alertas informativas (`TempData["MensajeExito"]` / `TempData["MensajeError"]`).
  - [x] 5.4 Crear e integrar los modales Bootstrap con temática arcade para creación, edición y confirmación de baja de pizzas.
  - [x] 5.5 Incorporar script JavaScript para inicializar los datos en el modal de edición y confirmación de eliminación al hacer clic en los ítems del menú contextual.

- [x] 6. Actualización de vistas parciales de tarjetas
  - [x] 6.1 Modificar `Views/Shared/_PizzaCard.cshtml` para suprimir la etiqueta `<img>` y el contenedor de imagen.
  - [x] 6.2 Integrar en `_PizzaCard.cshtml` el botón de menú contextual de tres puntos (`⋮`) con las opciones "Editar Parámetros" y "Dar de Baja".
  - [x] 6.3 Actualizar `Views/Shared/_PizzaSkeletonCard.cshtml` eliminando el bloque placeholder de imagen.

- [x] 7. Actualización de navegación y diseño general
  - [x] 7.1 Actualizar `Views/Shared/_Layout.cshtml` con enlaces directos a `[ INICIO ]` (`Home/Index`) y `[ CATÁLOGO DE PIZZAS ]` (`Home/Pizzas`).
  - [x] 7.2 Añadir estilos CSS arcade en `PizzaPlanetMVC/wwwroot/css/site.css` para el botón de tres puntos, menús desplegables neón, tarjetas sin imagen y modales interactivos.

- [x] 8. Verificación y validación de la solución
  - [x] 8.1 Compilar la solución completa con `dotnet build` para certificar la ausencia de errores.
  - [x] 8.2 Comprobar el flujo de navegación entre la página de inicio y el catálogo de pizzas.
  - [x] 8.3 Verificar la ejecución asíncrona de las operaciones de lectura, alta, edición y baja contra la API REST.
