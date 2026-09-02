## 1. Extracción de Assets del Formato y Preparación de Estilos

- [x] 1.1 Extraer los recursos gráficos de formato desde `Layout.fig` (ilustración del planeta hero e iconos base) hacia `PizzaPlanetMVC/wwwroot/images/` y verificar la existencia de los archivos.
- [x] 1.2 Configurar fuentes Google Fonts (`Unbounded`, `Fira Code`), variables CSS retro-arcade y las animaciones de Skeleton Loading (`@keyframes cosmic-pulse`) en `PizzaPlanetMVC/wwwroot/css/site.css`, verificando que los estilos se apliquen correctamente.

## 2. Servicio de API Asíncrono y ViewModels

- [x] 2.1 Crear la interfaz `IPizzaApiService` y la clase `PizzaApiService` en `PizzaPlanetMVC/Services/` implementando consultas 100% asíncronas (`async Task<IEnumerable<PizzaDto>> ObtenerPizzasAsync(...)`) mediante `IHttpClientFactory` y `GetFromJsonAsync`.
- [x] 2.2 Registrar `PizzaApiService` con `AddHttpClient` en `PizzaPlanetMVC/Program.cs` vinculando la URL base de `PizzaPlanetaApi` desde `appsettings.json`.
- [x] 2.3 Crear los ViewModels (`PizzaCatalogoViewModel`, `PizzaItemViewModel`) en `PizzaPlanetMVC/Models` con soporte para datos dinámicos, términos de búsqueda, ordenamiento y estados de error/conexión.

## 3. Componentes de Vista y Skeleton Loading

- [x] 3.1 Crear la vista parcial `Views/Shared/_PizzaCard.cshtml` fuertemente tipada que renderice los datos de cada pizza provista por la API con el formato exacto de card de Figma (borde neón, tipografía, descripción y badge `COSMIC PRICE`).
- [x] 3.2 Crear el componente o vista parcial `Views/Shared/_PizzaSkeletonCard.cshtml` con tarjetas placeholder animadas que emulen las dimensiones de las cards de pizza.

## 4. Controlador Asíncrono y Vistas Razor

- [x] 4.1 Actualizar `HomeController.cs` implementando la acción asíncrona `[HttpGet] public async Task<IActionResult> Index(...)` que consuma `IPizzaApiService` mediante `await` y retorne el ViewModel a la vista.
- [x] 4.2 Actualizar `Views/Shared/_Layout.cshtml` con la estructura semántica, fuentes, navbar retro-espacial y footer temático con copyright y prompt "INSERT COIN TO PLAY [ 0.25$ ]".
- [x] 4.3 Desarrollar en `Views/Home/Index.cshtml` la sección Hero, la cinta de peligro ("WARNING: DANGER OF DELICIOUSNESS"), el panel HUD con Tag Helpers (`asp-for`, `asp-action`), la sección de Skeleton Loading y la renderización dinámica de pizzas mediante la vista parcial `_PizzaCard`.

## 5. Verificación Integral y Pruebas

- [x] 5.1 Compilar la solución completa con `dotnet build` y verificar cero errores de compilación.
- [x] 5.2 Ejecutar la API y el front MVC de forma simultánea, comprobando en el navegador la carga asíncrona de datos reales de la API, la visualización del skeleton loading y el formato de las cards según Figma.
