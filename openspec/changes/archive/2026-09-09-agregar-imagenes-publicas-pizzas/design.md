## Context

En fases anteriores del proyecto se exploró el desacoplamiento del catálogo y se eliminó el hardcodeo de imágenes locales en el ViewModel para garantizar que ningún dato dependiera de lógica inventada en el cliente web. Con la arquitectura MVC establecida y la API REST operando de forma 100% asíncrona, el siguiente paso es incorporar el atributo `ImagenUrl` al modelo relacional y a los contratos de la API, usando exclusivamente enlaces públicos directos hacia imágenes en la web.

## Goals / Non-Goals

**Goals:**
- Agregar la columna `ImagenUrl` a la entidad `Pizza` en la base de datos (longitud 500 caracteres, opcional/nullable).
- Actualizar los DTOs de `PizzaPlanetBiblioteca` (`PizzaDto`, `CrearPizzaDto`, `ActualizarPizzaDto`) para transmitir la URL.
- Actualizar los endpoints de `PizzaPlanetaApi` para permitir la consulta, creación y actualización de dicha URL.
- Proveer URLs públicas reales y estables en `DbSeeder.cs` para las pizzas precargadas en el sistema.
- Permitir al usuario ingresar y modificar la URL pública desde los modales de creación y edición en `Views/Home/Pizzas.cshtml`.
- Diseñar la tarjeta `_PizzaCard.cshtml` para que renderice la imagen con un marco neón cuando exista `ImagenUrl`, con protección contra enlaces caídos mediante JavaScript (`onerror`).

**Non-Goals:**
- No implementar subida de archivos físicos (upload de `.jpg`/`.png` con `IFormFile`) ni almacenamiento en disco local del servidor.
- No hacer que `ImagenUrl` sea obligatoria; si una pizza no cuenta con imagen, la tarjeta debe mantener un diseño limpio y presentable.
- No volver a hardcodear nombres de archivo o mapeos locales en `PizzaItemViewModel`.

## Decisions

### 1. Tipo de Dato y Longitud de `ImagenUrl`
- **Decisión**: Utilizar `string?` en C# y `varchar(500)` en el mapeo de Entity Framework Core (`PizzaConfig.cs`).
- **Justificación**: Las URLs públicas de servicios como Unsplash, Cloudinary, Imgur o repositorios suelen rondar entre 80 y 250 caracteres. 500 caracteres ofrece suficiente margen sin penalizar el rendimiento ni desperdiciar almacenamiento en MySQL o SQLite.
- **Opcionalidad**: `IsRequired(false)`. Permite dar de alta pizzas rápidamente incluso si no se dispone de una imagen al momento del registro.

### 2. Contratos DTO y API REST
- **Decisión**: Agregar `string? ImagenUrl` en `CrearPizzaDto`, `ActualizarPizzaDto` y `PizzaDto`.
- **Validación**:
  - En DTOs: Decorar opcionalmente con `[Url(ErrorMessage = "La URL de la imagen debe ser un enlace web válido.")]`.
  - En HTML5: Atributo `type="url"` en los inputs de los modales.

### 3. Visualización Defensiva en la Tarjeta de Pizza (`_PizzaCard.cshtml`)
- **Decisión**: Mostrar el bloque de imagen antes del cuerpo de la tarjeta, únicamente cuando `!string.IsNullOrWhiteSpace(Model.ImagenUrl)`.
- **Tolerancia a fallos**:
  ```html
  @if (!string.IsNullOrWhiteSpace(Model.ImagenUrl))
  {
      <div class="pizza-card-img-wrap">
          <img src="@Model.ImagenUrl" 
               alt="@Model.Nombre" 
               class="pizza-card-img" 
               loading="lazy" 
               onerror="this.closest('.pizza-card-img-wrap').style.display='none';" />
      </div>
  }
  ```
  Si la URL externa arroja 404, tiempo de espera o bloqueo CORS, el evento `onerror` oculta suavemente el contenedor de la imagen sin romper la estructura de la tarjeta ni mostrar el ícono de imagen rota del navegador.

### 4. Modales de Creación y Edición
- **Decisión**: Incorporar un campo de texto con validación URL en ambos modales:
  - En el modal de creación: campo vacío con placeholder temático.
  - En el modal de edición: precargar el valor existente desde el atributo `data-imagen-url` del botón de tres puntos `⋮`.

## Risks / Trade-offs

- **[Riesgo] Enlaces externos rotos o lentos**: Un servidor externo puede tener latencia o eliminar la imagen con el tiempo.
  - *Mitigación*: Uso del atributo `loading="lazy"` para no retrasar la carga del documento HTML y el manejador `onerror` para ocultar automáticamente la imagen rota.
- **[Riesgo] Migración en base de datos existente**:
  - *Mitigación*: La columna se crea como `NULLABLE`, por lo que todos los registros de pizzas preexistentes conservan su integridad sin necesidad de scripts manuales de migración de datos.
