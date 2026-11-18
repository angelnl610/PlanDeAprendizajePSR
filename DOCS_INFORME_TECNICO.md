# Informe Técnico Integrador: Sistema "Pizza Planeta"
## "Tu app pide una pizza... y la API se la entrega"

* **Institución:** Escuela Técnica N.º 12 D.E. 1.º "Lib. Gral. José de San Martín"
* **Espacio Curricular:** Desarrollo de Sistemas / Programación Sobre Redes — 6.º 7.ª
* **Tipo de Trabajo:** Trabajo Práctico Integrador (Duración: 3 Semanas + Defensa Oral)
* **Solución Tecnológica:** Arquitectura Cliente-Servidor en .NET 10 (ASP.NET Core Minimal API REST + Cliente Web ASP.NET Core MVC + Entity Framework Core + MySQL + Autenticación JWT y Cookies + Scalar OpenAPI)

---

## 1. Sinopsis y Objetivos del Proyecto

El presente informe técnico documenta el análisis, diseño mediante modelos UML, arquitectura, implementación, validación y pruebas funcionales del sistema **Pizza Planeta**. El proyecto resuelve de forma integral el ciclo completo de gestión de una pizzería moderna mediante una arquitectura distribuida Cliente-Servidor:

1. **Modelado UML riguroso:** Identificación de actores, casos de uso, especificaciones formales, diagrama de clases de dominio y diagrama de secuencia de extremo a extremo.
2. **Desacoplamiento Cliente-Servidor:** Una **API REST (`PizzaPlanetaApi`)** que concentra toda la lógica de negocio, seguridad JWT y persistencia relacional en MySQL, consumida a través de la red mediante JSON por un **Cliente Web (`PizzaPlanetMVC`)** estructurado con el patrón Modelo-Vista-Controlador.
3. **Simulación autónoma del ciclo de vida del pedido:** Una vez confirmado el pedido por el cliente, el backend gestiona automáticamente su progresión por los estados **Pendiente (Espera de confirmación) -> En Preparación -> En Camino (En viaje) -> Entregado** en intervalos de 5 segundos, actualizándose en tiempo real en la vista del cliente.
4. **Control de acceso por roles (RBAC), Validaciones y Manejo de Errores:** Separación estricta de permisos para **Cliente**, **Cocinero** y **Administrador**, con validaciones multicapa (HTML5, Data Annotations, lógica de negocio y restricciones SQL).

---

## 2. SEMANA 1 — Análisis y Diseño

### 2.1. Actividad 1: Identificación de Actores y Diagrama de Casos de Uso UML

A partir del relevamiento de requerimientos del sistema, se identificaron los siguientes actores que interactúan con la plataforma:

| Actor | Tipo | Responsabilidades y Permisos dentro del Sistema |
|---|---|---|
| **Usuario No Autenticado** | Primario (Humano) | Puede acceder únicamente a los endpoints y vistas públicas de **Registro de Cuenta** (`/auth/registro`) e **Inicio de Sesión** (`/auth/login`). Al registrarse, el sistema le asigna de forma automática e inmutable el rol de `Cliente`. |
| **Cliente** | Primario (Humano) | Usuario autenticado que consulta el catálogo de pizzas activas, selecciona una o múltiples variedades con sus cantidades para **crear un pedido**, visualiza su historial propio de órdenes y realiza el **seguimiento en vivo** de los estados de su pedido hasta la entrega. |
| **Cocinero** | Primario (Humano) | Responsable de la gestión del catálogo gastronómico: puede **dar de alta nuevas pizzas**, **modificar parámetros** (nombre, descripción, precio, imagen) y **dar de baja lógica (Soft Delete)** variedades para que dejen de ofrecerse sin perder el historial de ventas pasadas. |
| **Administrador** | Primario (Humano) | Posee acceso total al sistema, incluyendo el **Panel de Administración de Usuarios y Roles** (`/usuarios`), desde donde visualiza el padrón ordenado por ID y puede promover o cambiar el rol de cualquier usuario registrado (`Cliente`, `Cocinero` o `Admin`). |
| **Temporizador Asíncrono** | Secundario (Sistema) | Proceso autónomo en segundo plano (`Task.Run` + `Task.Delay(5000)` con `IServiceScopeFactory`) que hace avanzar el estado del pedido automáticamente cada 5 segundos hasta completar la entrega. |

#### Diagrama de Casos de Uso UML

![Diagrama de Casos de Uso UML](/home/luka/.gemini/antigravity/brain/cf2419a0-525e-4bc7-bb6d-48ae7f14f9ea/scratch/docs/diagrama-casos-uso.png)

---

### 2.2. Actividad 2: Especificación de Dos Casos de Uso Principales

A continuación se desarrollan en detalle los dos casos de uso nucleares del sistema:

#### Caso de Uso 1: Realizar Pedido de Pizzas Multi-Variedad (CU-04)

| Campo | Descripción Detallada |
|---|---|
| **Nombre** | **CU-04: Crear Pedido Multi-Variedad y Simular Ciclo de Entrega** |
| **Actor Principal** | Cliente (autenticado en el sistema) |
| **Objetivo** | Permitir al cliente seleccionar una o varias pizzas activas del catálogo indicando sus cantidades, registrar la orden a través de la API REST y dar inicio automático al ciclo de preparación y entrega. |
| **Precondiciones** | 1. El usuario debe haber iniciado sesión válidamente y poseer una Cookie activa con un token JWT con rol `Cliente` o `Admin`.<br>2. Debe existir al menos una pizza con estado `Activa = true` en la base de datos. |
| **Flujo Principal (Camino Feliz)** | 1. El Cliente accede a la vista de nuevo pedido (`GET /Pedidos/Crear`), opcionalmente preseleccionando una pizza desde el catálogo.<br>2. El controlador MVC solicita a la API (`GET /pizzas`) el listado de pizzas activas y presenta una tabla con precios unitarios y selectores de cantidad inicializados en `0` (o `1` en la pizza preseleccionada).<br>3. El Cliente incrementa las cantidades deseadas en una o más variedades de pizza (ej. 2 Muzzarellas y 1 Napolitana) observando el total estimado en pantalla, y presiona **"Confirmar Pedido"**.<br>4. El controlador `PedidosController` recibe el formulario (`POST /Pedidos/Crear`), verifica que no existan cantidades negativas, filtra las variedades con `Cantidad > 0` y construye el objeto `PedidoAltaDto`.<br>5. El servicio `PedidoService` adjunta el token JWT en la cabecera `Authorization: Bearer <token>` y envía una petición `POST /pedidos` con el cuerpo serializado en JSON hacia `PizzaPlanetaApi`.<br>6. La API extrae el `UsuarioId` directamente de los claims del token JWT, verifica en la base de datos que todas las pizzas solicitadas existan y estén activas, y calcula el `Total` oficial multiplicando el precio vigente de base de datos por la cantidad de cada ítem.<br>7. La API persiste el registro en la tabla `Pedidos` con estado inicial `Pendiente` (Espera de confirmación) junto con sus renglones en `ItemsPedido`.<br>8. La API lanza en segundo plano la tarea asíncrona (`Task.Run`) que cada 5 segundos actualiza el estado: `Pendiente` -> `EnPreparacion` -> `EnCamino` -> `Entregado`.<br>9. La API responde con código `201 Created` y el JSON de `PedidoDetalleDto`.<br>10. El cliente web redirige (`302 Found`) a la pantalla de seguimiento `/Pedidos/Detalle/{id}`, la cual se refresca automáticamente cada 5 segundos mostrando el avance hasta llegar a `Entregado`. |
| **Flujos Alternativos y Excepciones** | • **4a. Todas las cantidades en cero:** Si el cliente presiona confirmar sin elegir ninguna pizza (`Cantidad == 0` en todas las filas), el controlador MVC detiene el envío, retorna código `200 OK` y muestra la alerta: *"Debe seleccionar al menos una pizza con cantidad mayor a cero."*<br>• **4b. Intento de enviar cantidad negativa:** Los botones `[-]` e inputs HTML (`min="0"`) impiden bajar de 0 en el navegador. Si se adultera la petición enviando un número negativo, el controlador lo rechaza con: *"Las cantidades no pueden ser negativas."*<br>• **6a. Pizza dada de baja simultáneamente:** Si un cocinero desactivó una de las pizzas elegidas justo antes de confirmar, la API devuelve `400 Bad Request` (`ERR_PIZZA_NOT_AVAILABLE`) y la vista informa la indisponibilidad. |
| **Resultado / Postcondición** | El pedido queda registrado en la base de datos asociado al cliente, con sus ítems e importe total calculado de forma segura, y transiciona autónomamente hasta el estado final `Entregado`. |

---

#### Caso de Uso 2: Gestionar Catálogo de Pizzas — Alta y Baja Lógica (CU-06 / CU-07)

| Campo | Descripción Detallada |
|---|---|
| **Nombre** | **CU-06 / CU-07: Alta de Nueva Variedad y Baja Lógica (Soft Delete) de Pizza** |
| **Actor Principal** | Cocinero o Administrador |
| **Objetivo** | Incorporar nuevas variedades de pizza al menú activo o retirar variedades existentes mediante borrado lógico (`Activa = false`) sin romper la integridad referencial de los pedidos históricos. |
| **Precondiciones** | El usuario debe encontrarse autenticado y poseer el rol `Cocinero` o `Admin` en su token JWT. |
| **Flujo Principal (Alta y Baja)** | 1. El Cocinero accede al catálogo (`GET /Pizzas`) donde visualiza los controles exclusivos de gestión (**"+ Nueva Pizza"**, **"Editar"** y **"Baja (Soft Delete)"**).<br>2. Selecciona **"+ Nueva Pizza"** (`GET /Pizzas/Crear`) y completa el formulario con Nombre (máx. 100 caracteres), Descripción (máx. 300 caracteres), Precio unitario (mayor a 0) y URL de imagen opcional.<br>3. Al enviar el formulario (`POST /Pizzas/Crear`), el controlador valida los Data Annotations y delega la petición a `PizzaService.CrearAsync(dto)`.<br>4. El servicio envía `POST /pizzas` en JSON con el Bearer Token hacia la API REST.<br>5. La API verifica el rol del token (`RequireRole("Admin", "Cocinero")`), normaliza los textos, asigna `Activa = true` y persiste la entidad en la tabla `Pizzas`, respondiendo `201 Created`.<br>6. Cuando se desea retirar una pizza del menú, el Cocinero presiona **"Baja (Soft Delete)"**, lo cual dispara `DELETE /pizzas/{id}`.<br>7. La API recupera la pizza, cambia su propiedad `pizza.Activa = false`, guarda los cambios y devuelve `204 No Content`. |
| **Flujos Alternativos y Excepciones** | • **3a. Nombre vacío o precio menor o igual a cero:** Tanto el validador de modelo como el endpoint de la API rechazan la operación con `400 Bad Request` (`ERR_INVALID_PRICE`).<br>• **3b. Descripción vacía:** Si el cocinero omite la descripción, la API convierte el valor nulo en cadena vacía `""` para cumplir con la restricción `NOT NULL` de la base de datos sin producir errores.<br>• **5a. Usuario con rol Cliente intentando operar:** Si un usuario con rol `Cliente` intenta invocar `POST`, `PUT` o `DELETE /pizzas`, el middleware de autorización rechaza la solicitud con `403 Forbidden`. |
| **Resultado / Postcondición** | La nueva pizza aparece inmediatamente disponible para ser pedida por los clientes, mientras que las pizzas dadas de baja lógica son excluidas automáticamente del catálogo activo gracias al filtro global (`HasQueryFilter(p => p.Activa)`) conservando intacto el historial de pedidos anteriores. |

---

### 2.3. Actividad 3: Diagrama de Clases UML

El modelo de dominio fue diseñado en el proyecto compartido `PizzaPlanetBiblioteca`, unificando la representación de usuarios (clientes, cocineros y administradores) en la entidad `Usuario`, vinculada en relación `1 a N` con `Pedido`. A su vez, cada `Pedido` se compone de `1 a N` instancias de `ItemPedido`, permitiendo órdenes con múltiples variedades de `Pizza` y congelando el `PrecioUnitario` histórico al momento de la compra.

El enum `EstadoPedido` modela exactamente los estados requeridos por la consigna:
* `Pendiente = 1` *(Espera de confirmación)*
* `EnPreparacion = 2` *(En preparación)*
* `EnCamino = 3` *(En viaje)*
* `Entregado = 4` *(Entregado)*
* `Cancelado = 5` *(Cancelado)*

![Diagrama de Clases UML](/home/luka/.gemini/antigravity/brain/cf2419a0-525e-4bc7-bb6d-48ae7f14f9ea/scratch/docs/diagrama-clases.png)

---

### 2.4. Actividad 4: Diagrama de Secuencia UML

El siguiente Diagrama de Secuencia UML representa el flujo completo para **Realizar un Pedido**, ilustrando la delegación de responsabilidades desde el navegador del cliente, pasando por el controlador MVC, el servicio HTTP con inyección de token JWT, el endpoint de la API REST, la persistencia transaccional en MySQL y el bucle asíncrono que transiciona los estados cada 5 segundos:

![Diagrama de Secuencia UML](/home/luka/.gemini/antigravity/brain/cf2419a0-525e-4bc7-bb6d-48ae7f14f9ea/scratch/docs/diagrama-secuencia.png)

---

### 2.5. Actividad 5: Arquitectura Cliente-Servidor y Planificación Tecnológica

La solución se estructuró siguiendo principios de **Clean Architecture** y **Separación de Responsabilidades (SoC)** dividida en **4 proyectos independientes** dentro de la solución `PizzaPlaneta.slnx`:

1. **`PizzaPlanetBiblioteca` (Núcleo de Dominio y Contratos):**
   Contiene las Entidades (`Usuario`, `Pizza`, `Pedido`, `ItemPedido`), los Enumerados (`RolUsuario`, `EstadoPedido`), los objetos de transferencia de datos (`AuthDto`, `PizzaDto`, `PedidoDto`, `UsuarioDto`) enriquecidos con *Data Annotations*, y las interfaces de servicio (`IAuthService`, `IPizzaService`, `IPedidoService`, `IUsuarioService`). No posee dependencias hacia frameworks web ni bases de datos.
2. **`PizzaPlanetaApi` (Servidor Backend / API REST — Puerto `5272`):**
   Implementado con ASP.NET Core Minimal APIs. Gestiona la autenticación mediante tokens **JWT Bearer** firmados con HMAC-SHA256, aplica políticas de autorización por rol, ejecuta la simulación asíncrona de pedidos con `IServiceScopeFactory`, expone la documentación interactiva en **Scalar** (`/scalar/v1`) y administra la persistencia con **Entity Framework Core** sobre **MySQL** (con fallback automático a SQLite).
3. **`PizzaPlanetServicios` (Capa de Integración de Red / Clientes HTTP):**
   Implementa las interfaces `IServices` utilizando `HttpClient` tipados. Intercepta el contexto actual mediante `IHttpContextAccessor`, extrae el claim `TokenJwt` almacenado en la sesión del usuario e inyecta automáticamente la cabecera `Authorization: Bearer <token>` en cada llamada hacia la API REST.
4. **`PizzaPlanetMVC` (Aplicación Cliente Web — Puerto `5278`):**
   Frontend construido en ASP.NET Core MVC con vistas Razor y **Bootstrap 5** estándar. Administra la sesión del usuario mediante **Cookies encriptadas**, adapta la barra de navegación y botones según el rol activo (`@if (User.IsInRole("..."))`) y actualiza en vivo el seguimiento de pedidos.

#### Esquema de Arquitectura Propuesta

![Esquema de Arquitectura Cliente-Servidor](/home/luka/.gemini/antigravity/brain/cf2419a0-525e-4bc7-bb6d-48ae7f14f9ea/scratch/docs/diagrama-arquitectura.png)

#### Justificación de Lenguaje y Herramientas Elegidas

| Tecnología / Herramienta | Justificación Técnica en el Proyecto |
|---|---|
| **C# 13 y .NET 10** | Lenguaje fuertemente tipado, moderno y de alto rendimiento, con soporte nativo para programación asíncrona (`async/await`) indispensable para simular el ciclo de vida de los pedidos sin bloquear hilos. |
| **ASP.NET Core Minimal APIs** | Permite definir endpoints RESTful (`/auth`, `/pizzas`, `/pedidos`, `/usuarios`) con baja sobrecarga, alta legibilidad y serialización JSON nativa de alto desempeño (`System.Text.Json`). |
| **Entity Framework Core + MySQL** | ORM industrial que facilita el mapeo Objeto-Relacional, migraciones versionadas (`ModelosUnificados`), filtros globales para Soft Delete (`HasQueryFilter`) y prevención total contra inyección SQL. |
| **JWT Bearer + Cookies MVC** | Combina la naturaleza *stateless* estándar de una API REST (protegida por JWT) con la ergonomía de una aplicación web tradicional (sesión mantenida por Cookie HTTP-Only en el MVC). |
| **Scalar (`Scalar.AspNetCore`)** | Herramienta visual moderna que reemplaza a Swagger UI, ofreciendo un cliente interactivo para probar endpoints manualmente, configurar tokens Bearer y generar código cliente automáticamente. |
| **Bootstrap 5 + Razor** | Permite construir una interfaz limpia, responsiva y accesible sin depender de hojas de estilo complejas ni frameworks JavaScript pesados, respetando el principio *YAGNI*. |

---

## 3. SEMANA 2 — Producción e Implementación

### 3.1. Actividad 1: Implementación de la Gestión de Pedidos

La gestión de pedidos se materializó en el archivo `PizzaPlanetaApi/Endpoints/PedidoEndpoints.cs` y en su contraparte cliente `PizzaPlanetMVC/Controllers/PedidosController.cs`, cubriendo las cuatro operaciones fundamentales:

1. **Crear un pedido (`POST /pedidos`):**
   * Recibe un `PedidoAltaDto` con una lista de ítems (`PizzaId` y `Cantidad`).
   * Extrae el identificador del cliente directamente del token JWT (`ClaimTypes.NameIdentifier`), evitando que un usuario suplante la identidad de otro.
   * Verifica que todas las variedades solicitadas existan y estén activas en la base de datos.
   * Calcula el subtotal de cada renglón y el total general leyendo el precio oficial desde la tabla `Pizzas` en MySQL.
   * Guarda el pedido con estado `Pendiente` (`1`).
2. **Consultar todos los pedidos (`GET /pedidos`):**
   * Si el usuario autenticado posee rol `Cliente`, filtra automáticamente las órdenes con `.Where(p => p.UsuarioId == userId)` para que solo vea sus propios pedidos.
   * Si posee rol `Admin` o `Cocinero`, retorna la totalidad de los pedidos registrados ordenados por fecha descendente.
3. **Consultar un pedido determinado (`GET /pedidos/{id}`):**
   * Recupera el pedido junto con los datos del usuario y la colección de `ItemsPedido` incluyendo el nombre de cada pizza.
   * Valida que, si quien consulta es un `Cliente`, el pedido le pertenezca (`pedido.UsuarioId == userId`); de lo contrario responde `403 Forbidden`.
4. **Modificar y transicionar su estado (`PUT /pedidos/{id}/estado` y Simulación Automática):**
   * Además del endpoint manual `PUT /pedidos/{id}/estado` para cambiar el estado o cancelar la orden, al crearse el pedido se inicia una tarea asíncrona en segundo plano que utiliza `IServiceScopeFactory` para instanciar un `PizzeriaDbContext` fresco cada 5 segundos y avanzar el estado:
     * t = 0 s: `Pendiente` (Espera de confirmación)
     * t = 5 s: `EnPreparacion` (En preparación)
     * t = 10 s: `EnCamino` (En viaje)
     * t = 15 s: `Entregado` (Estado terminal)

---

### 3.2. Actividad 2: API REST y Catálogo Completo de Endpoints

La API REST implementa los verbos estándar HTTP (`GET`, `POST`, `PUT`, `DELETE`) organizados en 4 grupos modulares:

| Recurso | Método HTTP | Ruta del Endpoint | Acceso / Rol Requerido | Descripción Funcional | Códigos de Respuesta |
|---|:---:|---|---|---|---|
| **Auth** | `POST` | `/auth/registro` | Público (Anónimo) | Registra un nuevo usuario asignándole automáticamente el rol `Cliente` y hasheando su contraseña. | `201 Created`<br>`400 Bad Request` |
| **Auth** | `POST` | `/auth/login` | Público (Anónimo) | Valida credenciales contra el hash almacenado y emite el token JWT firmado junto con el rol. | `200 OK`<br>`401 Unauthorized` |
| **Pizzas** | `GET` | `/pizzas` | Autenticado (Todos) | Lista todas las pizzas activas (`Activa == true`) disponibles en el catálogo. | `200 OK`<br>`401 Unauthorized` |
| **Pizzas** | `GET` | `/pizzas/{id}` | Autenticado (Todos) | Obtiene el detalle de una pizza específica por su identificador. | `200 OK`<br>`404 Not Found` |
| **Pizzas** | `POST` | `/pizzas` | `Admin`, `Cocinero` | Crea una nueva variedad de pizza en el catálogo activo. | `201 Created`<br>`400 Bad Request`<br>`403 Forbidden` |
| **Pizzas** | `PUT` | `/pizzas/{id}` | `Admin`, `Cocinero` | Actualiza nombre, descripción, precio o imagen de una pizza existente. | `204 No Content`<br>`400 Bad Request`<br>`404 Not Found` |
| **Pizzas** | `DELETE` | `/pizzas/{id}` | `Admin`, `Cocinero` | Ejecuta la **Baja Lógica (Soft Delete)** marcando `Activa = false`. | `204 No Content`<br>`404 Not Found` |
| **Pedidos** | `GET` | `/pedidos` | Autenticado (Todos) | Lista todos los pedidos (o solo los propios si el rol es `Cliente`). | `200 OK`<br>`401 Unauthorized` |
| **Pedidos** | `GET` | `/pedidos/{id}` | Autenticado (Dueño/Staff) | Obtiene el detalle completo de un pedido, su estado actual y el desglose de ítems. | `200 OK`<br>`403 Forbidden`<br>`404 Not Found` |
| **Pedidos** | `POST` | `/pedidos` | Autenticado (`Cliente`/`Admin`) | Crea un pedido con 1 o más pizzas e inicia el temporizador automático de estados. | `201 Created`<br>`400 Bad Request` |
| **Pedidos** | `PUT` | `/pedidos/{id}/estado` | `Admin`, `Cocinero` | Permite modificar manualmente el estado de un pedido determinado. | `204 No Content`<br>`404 Not Found` |
| **Pedidos** | `DELETE` | `/pedidos/{id}` | Autenticado (Dueño/Admin) | Cancela un pedido siempre que aún se encuentre en estado `Pendiente`. | `204 No Content`<br>`400 Bad Request` |
| **Usuarios** | `GET` | `/usuarios` | Exclusivo `Admin` | Obtiene el padrón completo de usuarios registrados ordenados ascendentemente por `Id`. | `200 OK`<br>`403 Forbidden` |
| **Usuarios** | `PUT` | `/usuarios/{id}/rol` | Exclusivo `Admin` | Reasigna el rol de un usuario (`Admin`, `Cocinero` o `Cliente`). | `204 No Content`<br>`404 Not Found` |

---

### 3.3. Actividad 3: Intercambio de Datos mediante JSON (Ejemplos Reales)

Todos los datos intercambiados entre `PizzaPlanetMVC` (o clientes externos) y `PizzaPlanetaApi` viajan estructurados en formato **JSON**. A continuación se presentan capturas reales de solicitudes y respuestas obtenidas del sistema en ejecución:

#### 1. Inicio de Sesión y Emisión de Token JWT (`POST /auth/login`)

**Solicitud (Request Body):**
```json
{
  "email": "cliente@pizzaplaneta.com",
  "password": "Cliente123!"
}
```

**Respuesta Exitosa (`200 OK`):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIzIiwibmFtZSI6IkNsaWVudGUgSW5pY2lhbCIsImVtYWlsIjoiY2xpZW50ZUBwaXp6YXBsYW5ldGEuY29tIiwicm9sZSI6IkNsaWVudGUiLCJleHAiOjE3OTUwNDg0MDB9...",
  "email": "cliente@pizzaplaneta.com",
  "nombre": "Cliente Inicial",
  "rol": 3
}
```

#### 2. Creación de un Pedido Multi-Pizza (`POST /pedidos`)

**Solicitud (Request Body con Header `Authorization: Bearer <token>`):**
```json
{
  "items": [
    {
      "pizzaId": 1,
      "cantidad": 2
    },
    {
      "pizzaId": 2,
      "cantidad": 1
    }
  ]
}
```

**Respuesta Exitosa (`201 Created`):**
```json
{
  "id": 17,
  "fecha": "2026-11-18T13:33:52.418291",
  "estado": 1,
  "total": 30200.00,
  "usuarioId": 3,
  "usuarioNombre": "Cliente Inicial",
  "items": [
    {
      "pizzaId": 1,
      "pizzaNombre": "Muzzarella Clásica",
      "cantidad": 2,
      "precioUnitario": 9500.00,
      "subtotal": 19000.00
    },
    {
      "pizzaId": 2,
      "pizzaNombre": "Napolitana con Ajo",
      "cantidad": 1,
      "precioUnitario": 11200.00,
      "subtotal": 11200.00
    }
  ]
}
```

#### 3. Consulta del Pedido tras 5 Segundos (`GET /pedidos/17`)

**Respuesta Exitosa (`200 OK` — Transición automática a `EnPreparacion` = 2):**
```json
{
  "id": 17,
  "fecha": "2026-11-18T13:33:52.418291",
  "estado": 2,
  "total": 30200.00,
  "usuarioId": 3,
  "usuarioNombre": "Cliente Inicial",
  "items": [
    {
      "pizzaId": 1,
      "pizzaNombre": "Muzzarella Clásica",
      "cantidad": 2,
      "precioUnitario": 9500.00,
      "subtotal": 19000.00
    },
    {
      "pizzaId": 2,
      "pizzaNombre": "Napolitana con Ajo",
      "cantidad": 1,
      "precioUnitario": 11200.00,
      "subtotal": 11200.00
    }
  ]
}
```

#### 4. Ejemplo de Respuesta Controlada de Error (`400 Bad Request`)

**Solicitud Inválida (`POST /pedidos` con lista vacía o cantidades en 0):**
```json
{
  "items": []
}
```

**Respuesta de Error (`400 Bad Request`):**
```json
{
  "error": "ERR_EMPTY_ORDER",
  "mensaje": "Debe agregar al menos una pizza con cantidad positiva."
}
```

---

### 3.4. Actividad 4: Relación entre Diseño UML e Implementación

Para garantizar la coherencia total entre los modelos de la Semana 1 y el código fuente producido en la Semana 2, la siguiente matriz traza cada elemento del diseño hacia su archivo físico en la solución:

| Elemento en los Modelos UML | Materialización en el Código Fuente | Responsabilidad Implementada |
|---|---|---|
| **Clase `Usuario` (Cliente / Staff)** | `PizzaPlanetBiblioteca/Entidades/Usuario.cs`<br>`PizzaPlanetaApi/Datos/Config/UsuarioConfig.cs` | Entidad persistida en la tabla `Usuarios` con propiedades `Id`, `Nombre`, `Email` (índice único), `PasswordHash`, `Rol`, `Telefono`, `Direccion` y colección `Pedidos`. |
| **Clase `Pizza`** | `PizzaPlanetBiblioteca/Entidades/Pizza.cs`<br>`PizzaPlanetaApi/Datos/Config/PizzaConfig.cs` | Entidad persistida en la tabla `Pizzas` con `Activa = true` por defecto y filtro global `HasQueryFilter(p => p.Activa)` para Soft Delete. |
| **Clase `Pedido` e `ItemPedido`** | `PizzaPlanetBiblioteca/Entidades/Pedido.cs`<br>`PizzaPlanetBiblioteca/Entidades/ItemPedido.cs`<br>`PizzaPlanetaApi/Datos/Config/PedidoConfig.cs` | Cabecera y detalle del pedido. Congela el `PrecioUnitario` y calcula el `Subtotal` por renglón y el `Total` de la orden. |
| **Enum `EstadoPedido`** | `PizzaPlanetBiblioteca/Enums/EstadoPedido.cs` | Define los estados `Pendiente = 1`, `EnPreparacion = 2`, `EnCamino = 3`, `Entregado = 4`, `Cancelado = 5`. |
| **Enum `RolUsuario`** | `PizzaPlanetBiblioteca/Enums/RolUsuario.cs` | Define los roles `Admin = 1`, `Cocinero = 2`, `Cliente = 3`. |
| **Participante `PedidosController` (Secuencia)** | `PizzaPlanetMVC/Controllers/PedidosController.cs`<br>`PizzaPlanetMVC/Views/Pedidos/Crear.cshtml`<br>`PizzaPlanetMVC/Views/Pedidos/Detalle.cshtml` | Recibe la selección multi-pizza del cliente, valida que no haya negativos ni pedidos vacíos y presenta el seguimiento con `<meta http-equiv="refresh" content="5">`. |
| **Participante `PedidoService` (Secuencia)** | `PizzaPlanetServicios/Services/PedidoService.cs` | Cliente HTTP que serializa `PedidoAltaDto` a JSON e inyecta el token JWT en cada petición hacia la API. |
| **Participante `PedidoEndpoints` y Timer** | `PizzaPlanetaApi/Endpoints/PedidoEndpoints.cs` | Procesa `POST /pedidos`, guarda en MySQL y ejecuta el hilo `Task.Run` con `Task.Delay(5000)` e `IServiceScopeFactory`. |

---

### 3.5. Actividad 5: Pruebas Iniciales y Herramientas Utilizadas

Durante el desarrollo se utilizaron tres mecanismos complementarios de prueba:
1. **Scalar UI (`http://localhost:5272/scalar/v1`):** Interfaz visual interactiva integrada en `PizzaPlanetaApi` mediante el paquete `Scalar.AspNetCore`. Permite inspeccionar todos los endpoints, ver los esquemas de los DTOs, cargar el token JWT Bearer y ejecutar llamadas manuales `GET`, `POST`, `PUT` y `DELETE` desde el navegador.
2. **Aplicación Cliente Web MVC (`http://localhost:5278`):** Permite probar la experiencia de usuario completa iniciando sesión con distintos roles (`admin@pizzaplaneta.com`, `cocinero@pizzaplaneta.com`, `cliente@pizzaplaneta.com`).
3. **Suite de Scripts Automatizados (`scripts/verify-*.sh`):** Cinco scripts en Bash que ejecutan pruebas con `curl` contra la API y el MVC verificando códigos HTTP y respuestas JSON.

---

## 4. SEMANA 3 — Pruebas, Validaciones, Manejo de Errores e Integración Final

### 4.1. Actividad 1: Validaciones Incorporadas

El sistema implementa una estrategia de **validación en defensa en profundidad** a través de cuatro anillos:

1. **Anillo 1 — Interfaz de Usuario (HTML5 y Razor):**
   * Atributos `required`, `type="email"`, `type="password"`, `type="url"` y `maxlength` generados automáticamente por los Tag Helpers de ASP.NET Core.
   * En el formulario de pedidos (`/Pedidos/Crear`), cada variedad tiene `min="0" max="50"` y botones `[-]` / `[+]` programados para impedir que el contador descienda por debajo de `0`.
2. **Anillo 2 — Contratos DTO con Data Annotations (`PizzaPlanetBiblioteca/DTOs/`):**
   * `PizzaAltaDto` y `PizzaActualizarDto`: `[Required]`, `[StringLength(100)]` en `Nombre`, `[StringLength(300)]` en `Descripcion`, `[Range(0.01, 10000000)]` en `Precio`, `[StringLength(500)]` en `ImagenUrl`.
   * `RegistroDto` y `LoginDto`: `[Required]`, `[EmailAddress]`, `[StringLength(150)]` en `Email`, `[StringLength(100, MinimumLength = 6)]` en `Password`.
   * `ItemPedidoAltaDto`: `[Range(1, 50)]` en `Cantidad`.
3. **Anillo 3 — Lógica de Controladores MVC y Endpoints de la API:**
   * Rechazo de pedidos vacíos (todas las cantidades en `0`) o con cantidades negativas.
   * Verificación de existencia y vigencia (`Activa == true`) de cada pizza incluida en un pedido.
   * Rechazo de registros con correo electrónico duplicado (`ERR_EMAIL_DUPLICATED`).
   * Impedimento de cancelación de pedidos que ya superaron el estado `Pendiente` (`ERR_INVALID_STATE`).
4. **Anillo 4 — Base de Datos Relacional (EF Core Fluent API y MySQL):**
   * Restricciones `NOT NULL`, `VARCHAR(100)`, `DECIMAL(10,2)` e índice `UNIQUE` en `Usuarios.Email`.

---

### 4.2. Actividad 2: Manejo de Errores

El sistema trata tanto errores de validación como situaciones anómalas de forma controlada, sin exponer trazas internas ni bloquear la aplicación:

| Situación de Error Real o Simulada | Detección y Tratamiento en la API REST | Respuesta en el Cliente Web MVC |
|---|---|---|
| **Intento de pedir sin seleccionar pizzas (todas en 0)** | Si se invoca directo a la API: devuelve `400 Bad Request` con `{"error": "ERR_EMPTY_ORDER"}`. | `PedidosController` intercepta la condición antes de llamar a la API y muestra un cartel de alerta rojo (`alert-danger`): *"Debe seleccionar al menos una pizza con cantidad mayor a cero."* |
| **Intento de ingresar cantidades negativas (ej. `-2`)** | Rechazado con `400 Bad Request` (`ERR_EMPTY_ORDER`). | El botón `[-]` bloquea en `0`. Si se envía por red un valor negativo, `PedidosController` devuelve: *"Las cantidades no pueden ser negativas."* |
| **Consulta de un pedido o pizza inexistente (`Id = 9999`)** | El endpoint verifica `if (pedido is null) return Results.NotFound();` devolviendo `404 Not Found`. | El servicio devuelve `null` y el controlador MVC retorna la respuesta controlada `NotFound()` (`404`). |
| **Cliente intentando acceder a un pedido ajeno o crear una pizza** | El middleware JWT y la validación de propiedad devuelven `403 Forbidden`. | El middleware de Cookies redirige automáticamente a la vista controlada `/Cuenta/AccesoDenegado`. |
| **Descripción nula al dar de alta una pizza** | `PizzaEndpoints` normaliza con `dto.Descripcion?.Trim() ?? string.Empty` evitando fallos de restricción `NOT NULL` en MySQL. | La pizza se registra exitosamente mostrando el cartel verde de confirmación. |
| **Base de datos MySQL no disponible en el entorno** | En `Program.cs`, un bloque `try/catch` detecta la falla de conexión a MySQL y conmuta automáticamente al motor local **SQLite** (`pizzaplaneta.db`), garantizando que la aplicación siempre pueda iniciar. | Transparente para el usuario final. |

---

### 4.3. Actividad 3: Pruebas Funcionales y Registro de Evidencias

Se diseñó y ejecutó una suite de **5 arneses de prueba automatizados** (ubicados en la carpeta `scripts/`) que validan de punta a punta tanto los casos de éxito como las situaciones de error. Todos los scripts limpian automáticamente sus datos temporales al finalizar (`trap cleanup EXIT`) para mantener intacto el padrón de la base de datos.

#### Evidencia 1: Autenticación, Registro y Rechazo de Errores (`scripts/verify-api-auth.sh`)
```text
==> [SVC] Verificando Autenticación y Autorización en http://localhost:5272
1. Probando Registro exitoso (POST /auth/registro)...
OK: Registro exitoso (201)
2. Probando Rechazo de correo duplicado (POST /auth/registro)...
OK: Correo duplicado rechazado (400)
3. Probando Login exitoso (POST /auth/login)...
OK: Login exitoso y Token JWT emitido (200)
4. Probando Login con clave errónea (POST /auth/login)...
OK: Login con clave errónea rechazado (401)
5. Probando rechazo de petición anónima (GET /usuarios)...
OK: Petición anónima rechazada con 401
==> [SVC] Todos los tests de Autenticación pasaron exitosamente (Exit Code 0)
```

#### Evidencia 2: Permisos por Rol y Baja Lógica Soft Delete (`scripts/verify-pizza-roles.sh`)
```text
==> [SVC] Verificando Soft Delete y Roles en Pizzas en http://localhost:5272
1. Probando rechazo de creación de pizza por Cliente (403)...
OK: Cliente rechazado con 403 Forbidden
2. Autenticando como Cocinero...
3. Cocinero creando nueva pizza (201)...
OK: Pizza creada con ID #17 (201)
4. Eliminando pizza mediante Soft Delete (DELETE /pizzas/17)...
OK: DELETE respondió 204 No Content
5. Verificando que la pizza inactiva no figura en el catálogo...
OK: Pizza excluida correctamente del catálogo activo (Soft Delete validado)
==> [SVC] Verificación de Soft Delete y Roles de Pizza exitosa (Exit Code 0)
```

#### Evidencia 3: Recorrido Completo del Pedido y Transición de Estados (`scripts/verify-order-lifecycle.sh`)
```text
==> [SVC] Verificando Ciclo de Vida del Pedido en http://localhost:5272
Pizza encontrada: ID 1
Creando Pedido...
Pedido creado con ID #16
Estado inicial: {"id":16,"fecha":"2026-11-18T13:33:46.730842","estado":1,"total":19000.00,"usuarioId":37,"usuarioNombre":"Cliente Pedidos","items":[{"pizzaId":1,"pizzaNombre":"Muzzarella Clásica","cantidad":2,"precioUnitario":9500.00,"subtotal":19000.00}]}
OK: Estado inicial es Pendiente
Esperando 5.5 segundos para la transición automática a EnPreparacion...
Estado tras 5s: {"id":16,"fecha":"2026-11-18T13:33:46.730842","estado":2,"total":19000.00,"usuarioId":37,"usuarioNombre":"Cliente Pedidos","items":[{"pizzaId":1,"pizzaNombre":"Muzzarella Clásica","cantidad":2,"precioUnitario":9500.00,"subtotal":19000.00}]}
OK: Transición exitosa a EnPreparacion (Exit Code 0)
```

#### Evidencia 4: Pedidos Multi-Pizza y Validaciones en MVC (`scripts/verify-mvc-multi-order.sh`)
```text
==> [SVC] Verificando Pedidos Multi-Pizza en MVC (http://localhost:5278)
1. Registrando y logueando cliente de prueba...
2. Obteniendo formulario de nuevo pedido...
3. Probando rechazo de pedido con todas las cantidades en 0...
OK: Rechazo por cantidades en 0 validado
4. Probando rechazo de pedido con cantidades negativas...
OK: Rechazo por cantidades negativas validado
5. Creando pedido multi-variedad (Pizza 1 x2, Pizza 2 x1)...
OK: Redirigido a /Pedidos/Detalle/17
6. Verificando ítems y subtotales en vista de detalle...
OK: Ambas pizzas verificadas en el detalle del pedido
==> [SVC] Verificación de Pedidos Multi-Pizza completada exitosamente (Exit Code 0)
```

---

## 5. Evaluación y Guía para la Defensa Oral

### 5.1. Credenciales Sembradas para la Demostración en Vivo

Al iniciar la API, la clase `DbSeeder.cs` garantiza la existencia de tres cuentas listas para demostrar cada rol durante la defensa oral:

| Rol | Correo Electrónico | Contraseña | Permisos Demostrables en Defensa |
|---|---|---|---|
| **Administrador** | `admin@pizzaplaneta.com` | `Admin123!` | Acceso total, vista `/Usuarios` ordenada por ID y cambio de roles en vivo. |
| **Cocinero** | `cocinero@pizzaplaneta.com` | `Cocinero123!` | Alta, edición y Baja Lógica (Soft Delete) de pizzas en `/Pizzas`. |
| **Cliente** | `cliente@pizzaplaneta.com` | `Cliente123!` | Creación de pedido con múltiples pizzas en `/Pedidos/Crear` y seguimiento automático cada 5 segundos. |

### 5.2. Comandos de Ejecución y Defensa

1. **Iniciar la API REST (`PizzaPlanetaApi`):**
   ```bash
   dotnet run --project PizzaPlanetaApi/PizzaPlanetaApi.csproj --launch-profile http
   ```
   * Documentación interactiva Scalar: `http://localhost:5272/scalar`
2. **Iniciar el Cliente Web MVC (`PizzaPlanetMVC`):**
   ```bash
   dotnet run --project PizzaPlanetMVC/PizzaPlanetMVC.csproj --launch-profile http
   ```
   * Interfaz Web: `http://localhost:5278`
3. **Ejecutar la Suite Automatizada de Pruebas:**
   ```bash
   ./scripts/verify-api-auth.sh && ./scripts/verify-pizza-roles.sh && ./scripts/verify-order-lifecycle.sh && ./scripts/verify-mvc-ui.sh && ./scripts/verify-mvc-multi-order.sh
   ```
