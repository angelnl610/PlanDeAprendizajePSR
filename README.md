<h1 align="center">E.T. Nº12 D.E. 1º "Libertador Gral. José de San Martín"</h1>

<p align="center">
  <img src="https://et12.edu.ar/imgs/computacion/VamoAProgramaBanner.png">
</p>

# PizzaPlaneta API 🍕

API REST desarrollada para la gestión de una pizzería.

El sistema permite administrar clientes, pizzas y pedidos, utilizando una arquitectura basada en ASP.NET Core Minimal API, Entity Framework Core y MySQL.

## Comenzando 🚀

Para utilizar este proyecto se debe clonar el repositorio y configurar las dependencias necesarias.

Clonar el repositorio desde Github Desktop o ejecutar en la terminal:

```bash
git clone URL_DEL_REPOSITORIO
```

Ingresar al directorio del proyecto:

```bash
cd PizzaPlanetaApi
```

---

# Relevamiento 👓

PizzaPlaneta es una API REST orientada a la gestión de una pizzería.

Permite realizar operaciones sobre:

- Clientes.
- Pizzas.
- Pedidos.

Los pedidos pertenecen a un cliente y están formados por uno o más productos mediante la entidad `ItemPedido`.

Cada elemento del pedido almacena la cantidad solicitada y el precio de la pizza al momento de realizar la compra.

El estado del pedido permite controlar su ciclo de vida:

- Pendiente.
- En preparación.
- En camino.
- Entregado.
- Cancelado.

---

# Pre-requisitos 📋

Antes de ejecutar el proyecto se necesita tener instalado:

- .NET SDK 10.
- MySQL Server 8.0 o superior.

Para comprobar la instalación de .NET:

```bash
dotnet --version
```

---

# Tecnologías utilizadas 🛠️

- ASP.NET Core Minimal API
- Entity Framework Core
- MySQL
- Pomelo.EntityFrameworkCore.MySql
- Swagger / OpenAPI

---

# Instalación 🔧

## Configuración de Base de Datos 🐬

La aplicación utiliza **MySQL** como sistema gestor de base de datos y **Entity Framework Core** para administrar la estructura de la misma mediante migraciones.

Antes de ejecutar el proyecto se debe configurar la cadena de conexión ubicada en:

```
appsettings.json
```

Modificar los valores según la configuración local de MySQL:

```json
"ConnectionStrings": {
    "PizzeriaDb": "server=localhost;port=3306;database=PizzaPlanetaDb;user=ElUsuario;password=LaContraseña;"
}
```

## Migraciones Entity Framework Core 🔄

La base de datos y sus tablas se generan automáticamente utilizando migraciones de Entity Framework Core.

Desde la terminal, ubicado en el directorio raíz del proyecto, ejecutar:

```bash
dotnet ef database update
```

Este comando creará la base de datos configurada y las tablas necesarias:

- Clientes
- Pizzas
- Pedidos
- ItemsPedido

En caso de ser necesario generar una nueva migración:

```bash
dotnet ef migrations add InitialCreate
```

## Instalación de dependencias

Desde la carpeta raíz del proyecto ejecutar:

```bash
dotnet restore
```

---

# Despliegue 📦

Para ejecutar la API:

Compilar el proyecto:

```bash
dotnet build
```

Ejecutar:

```bash
dotnet run
```

La API quedará disponible en la dirección indicada por la consola.

Ejemplo:

```
http://localhost:5000
```

---

# Swagger 📚

La API incluye documentación interactiva mediante Swagger.

Luego de iniciar el proyecto acceder desde el navegador:

```
http://localhost:PUERTO/swagger
```

Desde Swagger se pueden probar todos los endpoints disponibles.

---

# Endpoints 🌐

## Clientes

| Método | Endpoint | Descripción |
|---|---|---|
| GET | /clientes | Obtener todos los clientes |
| GET | /clientes/{id} | Obtener cliente por ID |
| POST | /clientes | Crear cliente |
| PUT | /clientes/{id} | Actualizar cliente |
| DELETE | /clientes/{id} | Eliminar cliente |

---

## Pizzas

| Método | Endpoint | Descripción |
|---|---|---|
| GET | /pizzas | Obtener todas las pizzas |
| GET | /pizzas/{id} | Obtener pizza por ID |
| POST | /pizzas | Crear pizza |
| PUT | /pizzas/{id} | Actualizar pizza |
| DELETE | /pizzas/{id} | Eliminar pizza |

---

## Pedidos

| Método | Endpoint | Descripción |
|---|---|---|
| GET | /pedidos | Obtener todos los pedidos |
| GET | /pedidos/{id} | Obtener pedido por ID |
| POST | /pedidos | Crear pedido |
| PUT | /pedidos/{id}/estado | Actualizar estado del pedido |
| DELETE | /pedidos/{id} | Eliminar pedido |

---

# Reglas de negocio 📌

- Un pedido debe pertenecer a un cliente existente.
- Un pedido debe contener al menos una pizza.
- No se pueden agregar pizzas inexistentes a un pedido.
- El total del pedido se calcula automáticamente.
- Los pedidos comienzan con estado `Pendiente`.
- El estado se almacena como texto en la base de datos.

---

# Construido con 🛠️

- [Visual Studio Code](https://code.visualstudio.com/)
- ASP.NET Core
- Entity Framework Core
- MySQL

---

# Versionado 📌

Se utiliza Git para el control de versiones del proyecto.

---

# Autores ✒️

- **Luka Pasandi Dabek y Angel Nahuel Lopez -insert usuarios de github-** - Desarrollo de la API.

---

# Licencia 📄

Proyecto realizado con fines educativos para la materia Programacion sobre redes.

E.T. Nº12 D.E. 1º "Libertador Gral. José de San Martín"