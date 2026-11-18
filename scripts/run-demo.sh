#!/usr/bin/env bash
set -euo pipefail

# ==============================================================================
# PizzaPlaneta - Demo Orchestrator with Cloudflare Quick Tunnels
# Launches MySQL check, .NET 10 API (Scalar) and MVC, and opens public tunnels.
# ==============================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

# Configuración de binarios y puertos
CLOUDFLARED="${CLOUDFLARED_BIN:-/usr/local/bin/cloudflared}"
PORT_API=5272
PORT_MVC=5278

# Directorio temporal para logs de ejecución
TMP_DIR=$(mktemp -d /tmp/pizzaplaneta_demo_XXXXXX)

# Control de procesos
API_PID=""
MVC_PID=""
CF_MVC_PID=""
CF_API_PID=""

cleanup() {
    local original_exit=$?
    # Desactivar traps para evitar recursión
    trap - SIGINT SIGTERM EXIT
    
    echo ""
    echo "[INFO] Deteniendo servicios y liberando túneles de PizzaPlaneta..."
    
    if [[ -n "$CF_MVC_PID" ]] && kill -0 "$CF_MVC_PID" 2>/dev/null; then
        kill "$CF_MVC_PID" 2>/dev/null || true
    fi
    if [[ -n "$CF_API_PID" ]] && kill -0 "$CF_API_PID" 2>/dev/null; then
        kill "$CF_API_PID" 2>/dev/null || true
    fi
    if [[ -n "$MVC_PID" ]] && kill -0 "$MVC_PID" 2>/dev/null; then
        kill "$MVC_PID" 2>/dev/null || true
    fi
    if [[ -n "$API_PID" ]] && kill -0 "$API_PID" 2>/dev/null; then
        kill "$API_PID" 2>/dev/null || true
    fi

    # Asegurar que procesos dotnet hijos se hayan cerrado
    pkill -P $$ 2>/dev/null || true

    # Esperar liberación de sockets
    for i in {1..10}; do
        if ! ss -tulpn 2>/dev/null | grep -q -E ":($PORT_API|$PORT_MVC) "; then
            break
        fi
        sleep 0.3
    done

    # Si aún no liberaron, forzar kill
    if [[ -n "$MVC_PID" ]] && kill -0 "$MVC_PID" 2>/dev/null; then
        kill -9 "$MVC_PID" 2>/dev/null || true
    fi
    if [[ -n "$API_PID" ]] && kill -0 "$API_PID" 2>/dev/null; then
        kill -9 "$API_PID" 2>/dev/null || true
    fi

    rm -rf "$TMP_DIR"
    echo "[INFO] Todos los recursos fueron liberados exitosamente."
    if [[ $original_exit -eq 0 || $original_exit -eq 130 || $original_exit -eq 143 ]]; then
        exit 0
    else
        exit "$original_exit"
    fi
}

trap cleanup SIGINT SIGTERM EXIT

# ------------------------------------------------------------------------------
# 1. Preflight Checks
# ------------------------------------------------------------------------------
echo "==> [1/4] Verificando dependencias previas..."

# Validar MySQL
if [[ "${MOCK_MYSQL_INACTIVE:-0}" == "1" ]] || ! systemctl is-active --quiet mysql; then
    echo "[FAIL] ERR_MYSQL_INACTIVE: MySQL service must be running." >&2
    exit 1
fi
echo "    ✓ MySQL activo localmente."

# Validar binario cloudflared
if [[ ! -x "$CLOUDFLARED" ]]; then
    echo "[FAIL] ERR_CLOUDFLARED_MISSING: cloudflared binary not found." >&2
    exit 1
fi
echo "    ✓ Cloudflare CLI disponible ($CLOUDFLARED)."

# Validar colisión de puertos
if ss -tulpn 2>/dev/null | grep -q ":$PORT_API "; then
    echo "[FAIL] ERR_PORT_OCCUPIED_API: Port $PORT_API is already in use." >&2
    exit 3
fi
if ss -tulpn 2>/dev/null | grep -q ":$PORT_MVC "; then
    echo "[FAIL] ERR_PORT_OCCUPIED_MVC: Port $PORT_MVC is already in use." >&2
    exit 4
fi
echo "    ✓ Puertos $PORT_API y $PORT_MVC disponibles."

# Exportar variable para evitar agotamiento de inotify instances en Linux
export DOTNET_USE_POLLING_FILE_WATCHER=1
export ASPNETCORE_ENVIRONMENT="Development"

# ------------------------------------------------------------------------------
# 2. Verificación de Compilación y Arranque de PizzaPlanetaApi (.NET 10 + Scalar)
# ------------------------------------------------------------------------------
echo "==> [2/4] Verificando binarios e iniciando PizzaPlanetaApi en puerto $PORT_API..."
if [[ ! -f "$ROOT_DIR/PizzaPlanetaApi/bin/Debug/net10.0/PizzaPlanetaApi.dll" ]]; then
    dotnet build "$ROOT_DIR/PizzaPlanetaApi/PizzaPlanetaApi.csproj" -v quiet --nologo
fi

(cd "$ROOT_DIR/PizzaPlanetaApi" && exec dotnet bin/Debug/net10.0/PizzaPlanetaApi.dll --urls "http://localhost:$PORT_API" > "$TMP_DIR/api.log" 2>&1) &
API_PID=$!

# Sondeo de readiness de API (GET a /scalar/v1)
api_ready=0
for i in {1..20}; do
    if curl -s -o /dev/null -w "%{http_code}" "http://localhost:$PORT_API/scalar/v1" | grep -q "200"; then
        api_ready=1
        break
    fi
    sleep 1
done

if [[ $api_ready -ne 1 ]]; then
    echo "[FAIL] ERR_STARTUP_TIMEOUT_API: API did not respond with HTTP 200 within timeout." >&2
    echo "--- LOG DE API ---" >&2
    cat "$TMP_DIR/api.log" >&2
    exit 5
fi
echo "    ✓ PizzaPlanetaApi lista en http://localhost:$PORT_API/scalar/v1"

# ------------------------------------------------------------------------------
# 3. Verificación de Compilación y Arranque de PizzaPlanetMVC (.NET 10 Web App)
# ------------------------------------------------------------------------------
echo "==> [3/4] Verificando binarios e iniciando PizzaPlanetMVC en puerto $PORT_MVC..."
if [[ ! -f "$ROOT_DIR/PizzaPlanetMVC/bin/Debug/net10.0/PizzaPlanetMVC.dll" ]]; then
    dotnet build "$ROOT_DIR/PizzaPlanetMVC/PizzaPlanetMVC.csproj" -v quiet --nologo
fi

(cd "$ROOT_DIR/PizzaPlanetMVC" && exec dotnet bin/Debug/net10.0/PizzaPlanetMVC.dll --urls "http://localhost:$PORT_MVC" > "$TMP_DIR/mvc.log" 2>&1) &
MVC_PID=$!

# Sondeo de readiness de MVC (usa -L para seguir redirect a /Cuenta/Login)
mvc_ready=0
for i in {1..20}; do
    if curl -s -L -o /dev/null -w "%{http_code}" "http://localhost:$PORT_MVC" | grep -q "200"; then
        mvc_ready=1
        break
    fi
    sleep 1
done

if [[ $mvc_ready -ne 1 ]]; then
    echo "[FAIL] ERR_STARTUP_TIMEOUT_MVC: MVC did not respond with HTTP 200 within timeout." >&2
    echo "--- LOG DE MVC INICIO ---" >&2
    cat "$TMP_DIR/mvc.log" >&2
    echo "--- LOG DE MVC FIN ---" >&2
    exit 6
fi
echo "    ✓ PizzaPlanetMVC lista en http://localhost:$PORT_MVC"

# ------------------------------------------------------------------------------
# 4. Establecer Túneles Concurrentes con Cloudflare
# ------------------------------------------------------------------------------
echo "==> [4/4] Estableciendo túneles públicos Cloudflare..."
"$CLOUDFLARED" tunnel --url "http://localhost:$PORT_MVC" > "$TMP_DIR/cf_mvc.log" 2>&1 &
CF_MVC_PID=$!

"$CLOUDFLARED" tunnel --url "http://localhost:$PORT_API" > "$TMP_DIR/cf_api.log" 2>&1 &
CF_API_PID=$!

# Extracción de URLs públicas
url_mvc=""
url_api=""

for i in {1..25}; do
    if [[ -z "$url_mvc" ]] && grep -o 'https://[a-zA-Z0-9.-]*\.trycloudflare\.com' "$TMP_DIR/cf_mvc.log" >/dev/null 2>&1; then
        url_mvc=$(grep -o 'https://[a-zA-Z0-9.-]*\.trycloudflare\.com' "$TMP_DIR/cf_mvc.log" | head -n1)
    fi
    if [[ -z "$url_api" ]] && grep -o 'https://[a-zA-Z0-9.-]*\.trycloudflare\.com' "$TMP_DIR/cf_api.log" >/dev/null 2>&1; then
        url_api=$(grep -o 'https://[a-zA-Z0-9.-]*\.trycloudflare\.com' "$TMP_DIR/cf_api.log" | head -n1)
    fi
    if [[ -n "$url_mvc" && -n "$url_api" ]]; then
        break
    fi
    sleep 1
done

if [[ -z "$url_mvc" ]]; then
    echo "[FAIL] ERR_TUNNEL_MVC_FAILED: Could not establish Cloudflare Tunnel for MVC." >&2
    cat "$TMP_DIR/cf_mvc.log" >&2
    exit 7
fi

if [[ -z "$url_api" ]]; then
    echo "[FAIL] ERR_TUNNEL_SCALAR_FAILED: Could not establish Cloudflare Tunnel for Scalar/API." >&2
    cat "$TMP_DIR/cf_api.log" >&2
    exit 8
fi

echo ""
echo "========================================================================"
echo "🍕 PIZZA PLANETA - DEMO EN VIVO ACTIVA (Cloudflare Quick Tunnels)"
echo "========================================================================"
echo "URL_MVC:    $url_mvc"
echo "URL_SCALAR: $url_api/scalar/v1"
echo "------------------------------------------------------------------------"
echo "🗄️  Base de Datos:  MySQL Local (5to_PizzaPlanetaDb) [OK]"
echo "⚡ Runtime:        .NET 10 SDK (Nativo)"
echo "🔐 Ambiente:       Development (Seeders & Scalar Activos)"
echo "========================================================================"
echo "Presiona Ctrl+C en cualquier momento para detener la demo."
echo ""

# Si estamos en modo test, mantener vivo esperando señal del test runner
if [[ "${DEMO_TEST_MODE:-0}" == "1" ]]; then
    while true; do
        sleep 1
    done
else
    # Modo interactivo para el usuario
    while true; do
        sleep 1
    done
fi
