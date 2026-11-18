<h1 align="center">E.T. Nº12 D.E. 1º "Libertador Gral. José de San Martín"</h1>

<p align="center">
  <img src="https://et12.edu.ar/imgs/computacion/VamoAProgramaBanner.png" alt="Banner ET12">
</p>

# Pizza Planeta 🍕🚀
### "Tu app pide una pizza... y la API se la entrega"

Sistema distribuido full-stack Cliente-Servidor para la gestión integral de una pizzería moderna, desarrollado con **.NET 10**, arquitectura en capas, autenticación híbrida (JWT Bearer + Cookies), control de acceso basado en roles (RBAC) y simulación automatizada del ciclo de vida del pedido.

---

## 🏗️ Arquitectura de la Solución

El repositorio está organizado como una solución de múltiples proyectos desacoplados:

```text
PlanDeAprendizajePSR/
├── PizzaPlanetBiblioteca/      # Capa de Dominio Compartida (Entidades, DTOs, Enums, Contratos IServices)
├── PizzaPlanetaApi/            # Servidor Backend: Minimal API REST (.NET 10, EF Core, MySQL, JWT, Scalar)
├── PizzaPlanetServicios/       # Capa de Integración HTTP (Typed HttpClient + propagación de JWT)
├── PizzaPlanetMVC/             # Cliente Frontend: ASP.NET Core MVC (Razor, Cookies, Bootstrap 5)
├── scripts/                    # Harnesses de verificación E2E y orquestador demo con Cloudflare
├── doc/                        # Guías de despliegue y documentación de túneles
└── DOCS_INFORME_TECNICO.md     # Informe Técnico Completo (UML, DER, Casos de Uso, Secuencias)
```

---

## ✨ Características Principales

1. **Arquitectura Cliente-Servidor Desacoplada:**
   - La API (`PizzaPlanetaApi`) expone endpoints REST puros con autenticación **JWT Bearer** y documentación interactiva vía **Scalar OpenAPI**.
   - El cliente Web (`PizzaPlanetMVC`) implementa el patrón MVC tradicional, gestionando sesiones de usuario mediante **Cookies de Autenticación** y delegando la lógica a través de `PizzaPlanetServicios`.

2. **Control de Acceso Basado en Roles (RBAC):**
   - **Cliente:** Consulta de catálogo interactivo, creación de pedidos con carrito dinámico, visualización de pedidos propios y seguimiento de estado en tiempo real.
   - **Cocinero:** Tablero de pedidos de la cocina, actualización de estados del pedido.
   - **Administrador:** Panel de administración global, CRUD completo de pizzas y gestión de usuarios/roles.

3. **Ciclo de Vida de Pedido Autónomo:**
   - Progresión de estados: `Pendiente` (espera de confirmación) ➔ `EnPreparacion` ➔ `EnCamino` (en viaje) ➔ `Entregado` (o `Cancelado`).
   - Tarea en segundo plano con progresión temporal automatizada en intervalos de 5 segundos para demostraciones en vivo.

4. **Validaciones Multicapa:**
   - Validaciones en frontend (HTML5/JS), Data Annotations en DTOs y ViewModels, reglas de negocio en servicios y restricciones de integridad relacional en MySQL.

---

## 🛠️ Tecnologías y Herramientas

- **Framework:** .NET 10 SDK (C# 13)
- **Backend API:** ASP.NET Core Minimal API
- **Frontend MVC:** ASP.NET Core MVC, Razor Views, Bootstrap 5, Bootstrap Icons
- **Persistencia & ORM:** Entity Framework Core 9 / Pomelo MySQL Provider
- **Base de Datos:** MySQL Server 8.0+
- **Seguridad:** JWT (JSON Web Tokens) con BCrypt / Cookie Authentication
- **Documentación API:** Scalar OpenAPI (`/scalar/v1`)
- **Túneles & Exposición:** Cloudflare Quick Tunnels (`scripts/run-demo.sh`)

---

## 🚀 Puesta en Marcha Rápida

### Pre-requisitos
- [.NET SDK 10](https://dotnet.microsoft.com/download)
- MySQL Server 8.0 o superior en ejecución (`localhost:3306`)

### 1. Clonar el repositorio
```bash
git clone https://github.com/angelnl610/PlanDeAprendizajePSR.git
cd PlanDeAprendizajePSR
```

### 2. Configurar Base de Datos
En `PizzaPlanetaApi/appsettings.json`, verificar o actualizar la cadena de conexión:
```json
"ConnectionStrings": {
  "PizzeriaDb": "server=localhost;port=3306;database=PizzaPlanetaDb;user=root;password=tu_contraseña;"
}
```

### 3. Aplicar Migraciones y Carga Inicial (Seeder)
```bash
dotnet ef database update --project PizzaPlanetaApi
```
> **Nota:** Al iniciar la API, el `DbSeeder` creará automáticamente el catálogo inicial de pizzas y los usuarios de prueba si la base de datos está vacía.

### 4. Credenciales de Prueba por Rol

| Rol | Email | Contraseña | Permisos |
|---|---|---|---|
| **Administrador** | `admin@pizzaplaneta.com` | `Admin123!` | CRUD Pizzas, Usuarios, Pedidos |
| **Cocinero** | `cocinero@pizzaplaneta.com` | `Cocinero123!` | Ver y actualizar estado de pedidos |
| **Cliente** | `cliente@pizzaplaneta.com` | `Cliente123!` | Crear pedidos, ver mis pedidos |

---

## 📦 Ejecución de la Solución

### Opción A: Ejecución Manual en Terminales Separadas

**Terminal 1 — API Backend:**
```bash
cd PizzaPlanetaApi
dotnet run
# Escucha en: http://localhost:5272
# Documentación Scalar: http://localhost:5272/scalar/v1
```

**Terminal 2 — Cliente MVC:**
```bash
cd PizzaPlanetMVC
dotnet run
# Escucha en: http://localhost:5278
```

### Opción B: Orquestador Automático con Cloudflare (Demo Pública)
Si dispones de `cloudflared` instalado:
```bash
./scripts/run-demo.sh
```
Iniciará automáticamente la API, el MVC y generará URLs públicas seguras HTTPS sin costos de infraestructura. Consulta la [Guía de Despliegue](doc/GUIA_DEPLOYMENT.md).

---

## 🧪 Pruebas Automatizadas y Scripts de Verificación

En la carpeta `scripts/` se incluyen suites completas de verificación E2E sin intervención manual:
- `scripts/verify-api-auth.sh`: Pruebas de registro, login JWT y permisos RBAC en la API.
- `scripts/verify-order-lifecycle.sh`: Validación de la progresión temporal de pedidos.
- `scripts/verify-pizza-roles.sh`: Verificación de protección de endpoints CRUD de pizzas.
- `scripts/verify-mvc-ui.sh`: Flujos de autenticación e interfaz web por cookies.
- `scripts/verify-mvc-multi-order.sh`: Pruebas de concurrencia y múltiples pedidos.

Para ejecutarlos:
```bash
chmod +x scripts/*.sh
./scripts/verify-api-auth.sh
```

---

## 🌐 Endpoints Principales de la API

### Autenticación (`/auth`)
- `POST /auth/login`: Autentica credenciales y genera token JWT.
- `POST /auth/registro`: Registra un nuevo usuario con rol Cliente.

### Usuarios (`/usuarios`) — [Admin]
- `GET /usuarios`: Listar todos los usuarios.
- `PUT /usuarios/{id}/rol`: Modificar el rol de un usuario.

### Pizzas (`/pizzas`)
- `GET /pizzas`: Catálogo público de pizzas.
- `GET /pizzas/{id}`: Detalle de una pizza.
- `POST /pizzas`: Crear pizza `[Admin]`.
- `PUT /pizzas/{id}`: Actualizar pizza `[Admin]`.
- `DELETE /pizzas/{id}`: Eliminar pizza `[Admin]`.

### Pedidos (`/pedidos`)
- `GET /pedidos`: Listar pedidos (filtrados por cliente autenticado o todos para `[Cocinero/Admin]`).
- `GET /pedidos/{id}`: Detalle completo de un pedido con ítems.
- `POST /pedidos`: Crear nuevo pedido `[Cliente]`.
- `PUT /pedidos/{id}/estado`: Transición de estado `[Cocinero/Admin]`.

---

## 📚 Documentación Técnica Detallada

Para consultar el relevamiento formal, especificación de requisitos, diagramas de clases UML, diagramas de casos de uso y diagramas de secuencia completos, consultar:
👉 **[DOCS_INFORME_TECNICO.md](DOCS_INFORME_TECNICO.md)**

---

## ✒️ Autores

* **Luka Pasandi Dabek** — [@PasandiLuka](https://github.com/PasandiLuka)
* **Angel Nahuel Lopez** — [@angelnl610](https://github.com/angelnl610)

*Escuela Técnica N.º 12 D.E. 1.º "Libertador Gral. José de San Martín" — Computación*
