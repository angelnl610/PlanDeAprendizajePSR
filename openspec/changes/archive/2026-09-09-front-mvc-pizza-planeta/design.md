## Context

See `proposal.md` for background and motivation.

El repositorio cuenta con `PizzaPlanetMVC` (un proyecto ASP.NET Core 10 MVC con Bootstrap 5.3.3 preinstalado en `wwwroot/lib/bootstrap`), `PizzaPlanetaBiblioteca` (con entidades y `PizzaDto`) y `PizzaPlanetaApi` (Minimal API con el endpoint `/pizzas`). En lugar de utilizar datos estáticos o hardcodeados de pizzas, se adopta únicamente el formato visual y diseño retro-arcade de `Layout.fig`, consumiendo los datos de las pizzas dinámicamente desde la API mediante llamadas asíncronas (`async/await`), con componentes reutilizables de tarjetas y skeleton loading.

## Goals / Non-Goals

**Goals:**
- Implementar una interfaz web responsiva de alta fidelidad basada en el formato visual de `Layout.fig` (hero, marquesina de advertencia, HUD de filtros, tarjetas de producto y footer).
- Desacoplar los datos: no usar pizzas fijas de Figma, sino consumir dinámicamente las pizzas reales desde `PizzaPlanetaApi` (`GET /pizzas`).
- Implementar todas las operaciones de consulta a la API de forma 100% asíncrona (`async/await`) utilizando `IHttpClientFactory`.
- Diseñar un componente reutilizable de tarjeta de pizza (`_PizzaCard.cshtml`) que aplique el formato visual de las cards de Figma (borde neón, tipografía, descripción y badge `COSMIC PRICE`) a los datos de la API.
- Implementar un sistema de Skeleton Loading con estética retro-arcade y animación pulsante para mostrar mientras se cargan los datos.
- Mantener la conclusión de utilizar el paquete Bootstrap 5.3.3 integrado como base estructural para la grilla responsiva, complementado con CSS personalizado en `site.css`.
- Seguir estrictamente las recomendaciones de `02 ASP.NET Core MVC - Desarrollo.pptx` (controladores con acciones asíncronas, ViewModels tipados, Tag Helpers y directivas Razor).

**Non-Goals:**
- No hardcodear datos de pizzas en archivos estáticos ni en el frontend.
- No alterar los endpoints ni los contratos existentes en `PizzaPlanetaApi`.
- No incluir librerías de componentes externas pesadas cuando CSS nativo y Bootstrap cubren el requerimiento de skeleton loading.

## Decisions

### 1. Consumo Dinámico Asíncrono de la API
- **Decisión**: Crear el servicio `IPizzaApiService` e implementarlo en `PizzaApiService` dentro de `PizzaPlanetMVC/Services/`, registrándolo mediante `AddHttpClient<IPizzaApiService, PizzaApiService>` en `Program.cs`.
- **Implementación asíncrona**: Todos los métodos de consulta serán estrictamente asíncronos:
  ```csharp
  public async Task<IEnumerable<PizzaDto>> ObtenerPizzasAsync(CancellationToken cancellationToken = default);
  ```
  Utilizará `HttpClient.GetFromJsonAsync<IEnumerable<PizzaDto>>` y `await`, garantizando que ningún hilo del servidor quede bloqueado durante la I/O de red.
- **Alternativas descartadas**:
  - *Llamadas síncronas (`.Result` o `.Wait()`)*: Provoca bloqueo de hilos (thread starvation) y viola las buenas prácticas de ASP.NET Core.
  - *Hardcodear pizzas de Figma*: Descartado por requerimiento explícito del usuario.

### 2. Componente Reutilizable de Tarjeta de Pizza (`_PizzaCard.cshtml`)
- **Decisión**: Crear una vista parcial `Views/Shared/_PizzaCard.cshtml` fuertemente tipada con `PizzaDto` (o `PizzaItemViewModel`).
- **Formato visual**:
  - Aplica la geometría, proporciones y estilo de `Layout.fig`:
    - Marco contenedor con borde neón verde (`#00FF66`).
    - Zona de imagen / avatar cósmico con soporte de imagen o icono estelar por defecto si la API no provee URL.
    - Título en mayúsculas con tipografía `Unbounded` y color brillante.
    - Descripción del producto.
    - Fila inferior con badge `COSMIC PRICE` en rojo/naranja retro (`#FF3366`) y el precio formateado en verde neón.
- **Justificación**: Permite reutilizar el formato de card tanto en el catálogo principal como en futuras vistas de detalle o recomendaciones sin duplicar código HTML.

### 3. Skeleton Loading con Identidad Retro-Arcade
- **Decisión**: Implementar componentes o plantillas de Skeleton Cards (`_PizzaSkeletonCard.cshtml` o contenedor con clases `.skeleton-card`) que repliquen exactamente la estructura de la tarjeta:
  - Bloque placeholder de imagen con animación pulsante (`@keyframes cosmic-pulse`).
  - Líneas placeholder de título y descripción con fondo translúcido y barrido de luz neón.
  - Bloque placeholder de precio en el pie de la tarjeta.
- **Comportamiento**: En el frontend se presentará el skeleton loading de manera fluida mientras se espera la resolución de las consultas asíncronas o como transición visual.

### 4. Integración y Conclusión sobre Bootstrap 5
- **Decisión**: Ratificar el uso de Bootstrap 5.3.3 integrado en `PizzaPlanetMVC/wwwroot/lib/bootstrap/` como base de la grilla (`container`, `row`, `col-12 col-md-6 col-lg-4`) y flexbox, complementándolo con `site.css` para la estética arcade (fondo `#080816`, fuentes Google Fonts `Unbounded` y `Fira Code`, y botones arcade).

## Risks / Trade-offs

- **[Riesgo] La API no está corriendo en el puerto esperado durante el desarrollo**: Si `PizzaPlanetaApi` no está encendida, la llamada asíncrona lanzará una excepción o timeout.
  - *Mitigación*: Implementar manejo de excepciones robusto en `PizzaApiService` con `try-catch`, loguear el error y retornar un estado controlado en el ViewModel (por ejemplo `IsApiUnavailable = true`), mostrando un mensaje retro-terminal ("ERROR DE CONEXIÓN CON LA ESTACIÓN CENTRAL - INSERT COIN TO RETRY") en lugar de una pantalla de error no controlada.
- **[Riesgo] Ausencia de campo de imagen en `PizzaDto` de la biblioteca**: `PizzaDto` solo expone `Id`, `Nombre`, `Descripcion`, `Precio`.
  - *Mitigación*: El componente `_PizzaCard` mapeará dinámicamente imágenes temáticas del espacio según el nombre/ID o un asset planetario por defecto para mantener la fidelidad visual de Figma sin alterar el modelo de base de datos de la API.
