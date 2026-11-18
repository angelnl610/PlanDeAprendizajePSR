# Guía Completa de Despliegue y Orquestación - Pizza Planeta 🍕

Esta guía describe cómo desplegar y exponer públicamente el proyecto completo de **Pizza Planeta** (.NET 10 + MySQL + MVC + API Scalar) a costo cero (\$0) mediante túneles seguros de **Cloudflare**.

---

## 1. Arquitectura de Despliegue

El proyecto se compone de 3 capas locales orquestadas y expuestas al exterior:

```text
  [ Evaluador / Cliente Externo ]
               |
               | HTTPS (Tráfico Público Cifrado)
               v
  +-----------------------------------------------------------+
  |              Cloudflare Global Edge Network               |
  |  - Web MVC:    https://<subdominio-mvc>.trycloudflare.com |
  |  - API Scalar: https://<subdominio-api>.trycloudflare.com |
  +-----------------------------------------------------------+
               |
               | Túneles Salientes (Quick Tunnels / cloudflared)
               v
  +-----------------------------------------------------------+
  |                   SERVIDOR / HOST LOCAL                   |
  |                                                           |
  |  [ PizzaPlanetMVC ]  --- Server-side BFF ---> [ API ]     |
  |  (Puerto 5278)                                (Port 5272) |
  |                                                    |      |
  |                                                    v      |
  |                                             [ MySQL 3306 ]|
  +-----------------------------------------------------------+
```

---

## 2. Requisitos Previos en el Servidor / Host

1. **.NET SDK 10**: Versión `10.0.x` instalada (`dotnet --version`).
2. **MySQL Server**: Servicio activo en el puerto 3306 (`systemctl status mysql`).
3. **Cloudflare CLI (`cloudflared`)**: Binario instalado en `/usr/local/bin/cloudflared` (o en el `$PATH`).

---

## 3. Gestión y Reinicio de la Base de Datos

La aplicación utiliza la base de datos `5to_PizzaPlanetaDb`.

### Reset Completo (Drop & Recreate)
Para limpiar todos los datos y reiniciar el esquema desde cero, ejecutar:

```bash
mysql -u 5to_agbd -p'Trigg3rs!' -e "DROP DATABASE IF EXISTS \`5to_PizzaPlanetaDb\`; CREATE DATABASE \`5to_PizzaPlanetaDb\`;"
```

### Sembrado Automático (Seeding Idempotente)
Al arrancar `PizzaPlanetaApi`, el seeder (`DbSeeder.cs`) detecta que las tablas no existen, ejecuta `db.Database.EnsureCreated()` y crea automáticamente las tablas y los usuarios iniciales:

| Rol | Correo | Contraseña | Propósito |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@pizzaplaneta.com` | `Admin123!` | Gestión de usuarios, roles, catálogo y pedidos |
| **Cocinero** | `cocinero@pizzaplaneta.com` | `Cocinero123!` | Tablero de cocina y cambio de estados |
| **Cliente** | `cliente@pizzaplaneta.com` | `Cliente123!` | Catálogo, armado de carritos y mis pedidos |

---

## 4. Despliegue Rápido Automatizado (Recomendado)

El repositorio incluye un script orquestador determinista que gestiona el ciclo de vida completo:

```bash
./scripts/run-demo.sh
```

### ¿Qué hace el orquestador automáticamente?
1. **Preflight Check**: Comprueba que MySQL esté activo y que `cloudflared` esté disponible.
2. **Compilación y Arranque de API**: Compila y levanta `PizzaPlanetaApi` en el puerto `5272` (con entorno `Development` para habilitar Scalar).
3. **Arranque de MVC**: Compila y levanta `PizzaPlanetMVC` en el puerto `5278`.
4. **Apertura de Túneles Cloudflare**: Abre dos túneles concurrentes y extrae las URLs públicas.
5. **Banner de Consola**: Imprime en terminal las dos URLs listas para ser visitadas.
6. **Graceful Shutdown**: Al pulsar `Ctrl+C`, apaga en cascada los túneles y los procesos .NET, liberando los puertos.

---

## 5. Despliegue Manual Paso a Paso (Alternativa)

Si prefieres ejecutar cada proceso manualmente en terminales independientes:

### Terminal 1: Compilar la Solución
```bash
dotnet build PizzaPlanetaApi/PizzaPlanetaApi.csproj -v quiet --nologo
dotnet build PizzaPlanetMVC/PizzaPlanetMVC.csproj -v quiet --nologo
```

### Terminal 2: Iniciar la API
```bash
cd PizzaPlanetaApi
export DOTNET_USE_POLLING_FILE_WATCHER=1
export ASPNETCORE_ENVIRONMENT=Development
dotnet bin/Debug/net10.0/PizzaPlanetaApi.dll --urls "http://localhost:5272"
```

### Terminal 3: Iniciar el Frontend MVC
```bash
cd PizzaPlanetMVC
export DOTNET_USE_POLLING_FILE_WATCHER=1
export ASPNETCORE_ENVIRONMENT=Development
dotnet bin/Debug/net10.0/PizzaPlanetMVC.dll --urls "http://localhost:5278"
```

### Terminal 4: Iniciar Túnel de la Web MVC
```bash
cloudflared tunnel --url http://localhost:5278
```

### Terminal 5: Iniciar Túnel de Scalar (Documentación API)
```bash
cloudflared tunnel --url http://localhost:5272
```

---

## 6. Verificación Determinista Automatizada

Para validar que todo el flujo funciona de punta a punta (healthchecks negativos, arranque, sondeo de endpoints locales y apagado limpio), ejecutar el test harness:

```bash
./scripts/verify-demo-orchestrator.sh all
```

El script finalizará con `[INFO] All assertions passed successfully.` y código de salida `0`.
