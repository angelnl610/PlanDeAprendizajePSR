## Why

Actualmente, el proyecto cuenta con el backend (`PizzaPlanetaApi`), la biblioteca de clases (`PizzaPlanetBiblioteca`) y un proyecto base `PizzaPlanetMVC` que solo posee una plantilla inicial por defecto con vistas de ejemplo. Se requiere dotar a la aplicación de un frontend interactivo, profesional y dinámico que adopte el formato visual y la estética retro-arcade de `Layout.fig`, pero consumiendo los datos reales de las pizzas desde la API (`PizzaPlanetaApi`) mediante llamadas exclusivamente asíncronas (`async/await`), incorporando componentes reutilizables para las cards de pizzas y skeleton loading para una experiencia de usuario fluida, respetando la arquitectura ASP.NET Core MVC y las recomendaciones de `02 ASP.NET Core MVC - Desarrollo.pptx`.

## What Changes

- **Adopción del Formato de `Layout.fig` (sin pizzas hardcodeadas)**: Se toma exclusivamente el formato visual, layout, paleta de colores y componentes estilizados de `Layout.fig` (hero section, cinta de advertencia `WARNING: DANGER OF DELICIOUSNESS`, panel HUD y footer arcade), desacoplando los datos de las pizzas para que provengan dinámicamente de la API (`PizzaPlanetaApi`).
- **Componente de Tarjeta de Pizza**: Creación de una vista parcial o componente de vista reutilizable (`_PizzaCard.cshtml` / `PizzaCardViewComponent`) que renderiza cada pizza provista por la API con el formato y estilo visual exacto de las cards de Figma (marco con borde neón, tipografía, descripción y badge de precio `COSMIC PRICE`).
- **Skeleton Loading**: Implementación de tarjetas placeholder con animación de carga tipo skeleton (efecto pulsante con brillo cósmico/neón) que se muestran mientras se resuelven las peticiones asíncronas de pizzas.
- **Consultas a la API 100% Asíncronas**: Todos los métodos de consulta hacia la API en el servicio y en el controlador se implementan con llamadas `async/await` no bloqueantes (usando `IHttpClientFactory` y `HttpClient.GetFromJsonAsync`), siguiendo las mejores prácticas de .NET y MVC.
- **Integración con Bootstrap 5**: Utilización del paquete Bootstrap 5.3.3 integrado en el proyecto para la grilla responsiva (`container`, `row`, `col-*`) y flexbox, complementado con CSS retro-arcade en `site.css`.
- **Patrón MVC y Prácticas de la Presentación**: Controlador MVC con acciones `async Task<IActionResult>`, ViewModels fuertemente tipados (`PizzaCatalogoViewModel`), inyección de dependencias de servicios HTTP y uso de Tag Helpers Razor.

## Capabilities

### New Capabilities
- `pizza-catalog-frontend`: Provisión de la interfaz web ASP.NET Core MVC basada en el formato de diseño de `Layout.fig`, con consumo dinámico y asíncrono de la API de pizzas, componente de card reutilizable, skeleton loading interactivo, filtros HUD y estilos responsivos basados en Bootstrap 5.

### Modified Capabilities

## Impact

- **Código Frontend (`PizzaPlanetMVC`)**:
  - `Services/`: Creación del servicio cliente asíncrono (`IPizzaApiService` / `PizzaApiService`) configurado con `HttpClient` en `Program.cs`.
  - `Models/`: ViewModels para el catálogo, estado de carga y elementos de pizza.
  - `Views/Shared/`: Creación de la vista parcial `_PizzaCard.cshtml` y del partial o plantilla de skeleton loading `_PizzaSkeletonCard.cshtml`.
  - `Views/Home/Index.cshtml`: Implementación de la vista principal con hero, hazard bar, HUD y renderizado de cards y skeletons.
  - `Controllers/HomeController.cs`: Acción `Index` convertida en método asíncrono `async Task<IActionResult>`.
  - `wwwroot/css/site.css`: Estilos del formato de Figma, variables neón, botones arcade y animaciones de skeleton loading.
- **Dependencias y APIs**:
  - Consumo directo del endpoint `/pizzas` de `PizzaPlanetaApi` mediante `HttpClient`.
  - No genera breaking changes en la base de datos ni en la biblioteca de clases.
