#!/usr/bin/env bash
set -euo pipefail

# ==============================================================================
# PizzaPlaneta - Verification Harness for Demo Orchestrator
# Tests deterministic preflight checks, tunnel execution and clean shutdown.
# ==============================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
ORCHESTRATOR="$SCRIPT_DIR/run-demo.sh"

RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

pass() { echo -e "${GREEN}[PASS]${NC} $1"; }
fail() { echo -e "${RED}[FAIL]${NC} $1"; exit 1; }
info() { echo -e "${BLUE}[INFO]${NC} $1"; }

test_preflight_failures() {
    info "Running preflight negative assertions..."

    # Test 1: Simular falla de MySQL mediante variable mock
    local output_mysql
    set +e
    output_mysql=$(MOCK_MYSQL_INACTIVE=1 bash "$ORCHESTRATOR" 2>&1)
    local code_mysql=$?
    set -e

    if [[ $code_mysql -ne 1 ]]; then
        fail "Expected exit code 1 when MySQL is inactive, got: $code_mysql"
    fi
    if ! echo "$output_mysql" | grep -q "ERR_MYSQL_INACTIVE"; then
        fail "Expected token ERR_MYSQL_INACTIVE in output. Got: $output_mysql"
    fi
    pass "ERR_MYSQL_INACTIVE detected deterministically on MySQL failure."

    # Test 2: Simular binario cloudflared ausente
    local output_cf
    set +e
    output_cf=$(CLOUDFLARED_BIN="/nonexistent/cloudflared" bash "$ORCHESTRATOR" 2>&1)
    local code_cf=$?
    set -e

    if [[ $code_cf -ne 1 ]]; then
        fail "Expected exit code 1 when cloudflared is missing, got: $code_cf"
    fi
    if ! echo "$output_cf" | grep -q "ERR_CLOUDFLARED_MISSING"; then
        fail "Expected token ERR_CLOUDFLARED_MISSING in output. Got: $output_cf"
    fi
    pass "ERR_CLOUDFLARED_MISSING detected deterministically when binary missing."
}

test_full_lifecycle() {
    info "Running full lifecycle test (boot, tunnel check, shutdown)..."

    # Lanzar el orquestador en modo background y capturar PID
    local log_file
    log_file=$(mktemp /tmp/pizzaplaneta_test_XXXXXX.log)

    info "Starting demo orchestrator (test timeout 30s)..."
    DEMO_TEST_MODE=1 bash "$ORCHESTRATOR" > "$log_file" 2>&1 &
    local orch_pid=$!

    # Esperar a que se emitan las URLs
    local wait_sec=0
    local url_mvc=""
    local url_scalar=""
    local max_wait=60

    while [[ $wait_sec -lt $max_wait ]]; do
        if grep -q "URL_MVC:" "$log_file" 2>/dev/null && grep -q "URL_SCALAR:" "$log_file" 2>/dev/null; then
            url_mvc=$(grep "URL_MVC:" "$log_file" | head -n1 | awk '{print $2}')
            url_scalar=$(grep "URL_SCALAR:" "$log_file" | head -n1 | awk '{print $2}')
            break
        fi
        sleep 1
        wait_sec=$((wait_sec + 1))
    done

    if [[ -z "$url_mvc" || -z "$url_scalar" ]]; then
        cat "$log_file"
        kill -SIGINT "$orch_pid" 2>/dev/null || true
        fail "Timeout waiting for Cloudflare Tunnel URLs after ${max_wait}s."
    fi

    pass "Extracted MVC Tunnel URL: $url_mvc"
    pass "Extracted Scalar Tunnel URL: $url_scalar"

    # Verificar endpoints locales directamente (-L para seguir redirect en MVC)
    local mvc_local_code
    mvc_local_code=$(curl -s -L -o /dev/null -w "%{http_code}" http://localhost:5278 || echo "000")
    if [[ "$mvc_local_code" -ne 200 ]]; then
        kill -SIGINT "$orch_pid" 2>/dev/null || true
        fail "Local MVC endpoint returned HTTP $mvc_local_code (expected 200)"
    fi
    pass "Local MVC endpoint (localhost:5278) verified HTTP 200."

    local scalar_local_code
    scalar_local_code=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5272/scalar/v1 || echo "000")
    if [[ "$scalar_local_code" -ne 200 ]]; then
        kill -SIGINT "$orch_pid" 2>/dev/null || true
        fail "Local Scalar endpoint (localhost:5272/scalar/v1) returned HTTP $scalar_local_code (expected 200)"
    fi
    pass "Local Scalar endpoint (localhost:5272/scalar/v1) verified HTTP 200."

    # Detener mediante señal SIGTERM
    info "Sending SIGTERM to orchestrator (PID $orch_pid)..."
    kill -TERM "$orch_pid" 2>/dev/null || true
    wait "$orch_pid" 2>/dev/null || true

    # Esperar liberación de sockets (hasta 5 segundos)
    local ports_freed=0
    for i in {1..10}; do
        if ! ss -tulpn 2>/dev/null | grep -q -E ':(5272|5278) '; then
            ports_freed=1
            break
        fi
        sleep 0.5
    done

    if [[ $ports_freed -ne 1 ]]; then
        fail "Ports 5272 or 5278 are still occupied after shutdown!"
    fi
    pass "Ports 5272 and 5278 successfully released after shutdown."

    rm -f "$log_file"
}

case "${1:-all}" in
    --test-preflight-failures)
        test_preflight_failures
        ;;
    --test-full)
        test_full_lifecycle
        ;;
    all)
        test_preflight_failures
        test_full_lifecycle
        info "All assertions passed successfully."
        ;;
    *)
        echo "Usage: $0 [--test-preflight-failures|--test-full|all]"
        exit 1
        ;;
esac
