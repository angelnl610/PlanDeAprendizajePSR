#!/usr/bin/env bash
set -euo pipefail

MVC_URL="${MVC_URL:-http://localhost:5278}"
TIMESTAMP=$(date +%s)
echo "==> [SVC] Verificando Frontend MVC y Cookies en $MVC_URL"

# Helper para extraer el __RequestVerificationToken
get_token() {
  local url="$1"
  local cookie_jar="$2"
  curl -s -c "$cookie_jar" "$url" | grep -o '__RequestVerificationToken.*' | head -n1 | sed -E 's/.*value="([^"]+)".*/\1/'
}

COOKIE_ADMIN="/tmp/cookie_admin.txt"
COOKIE_COCINERO="/tmp/cookie_cocinero.txt"
COOKIE_CLIENTE="/tmp/cookie_cliente.txt"
rm -f "$COOKIE_ADMIN" "$COOKIE_COCINERO" "$COOKIE_CLIENTE"

CLIENTE_TEST_EMAIL="cliente_mvc_${TIMESTAMP:-123}@test.com"

cleanup() {
  mysql -u 5to_agbd -pTrigg3rs! -h localhost -e "USE 5to_PizzaPlanetaDb; DELETE i FROM ItemsPedido i INNER JOIN Pedidos p ON i.PedidoId = p.Id WHERE p.UsuarioId IN (SELECT Id FROM (SELECT Id FROM Usuarios WHERE Email = '$CLIENTE_TEST_EMAIL') t); DELETE FROM Pedidos WHERE UsuarioId IN (SELECT Id FROM (SELECT Id FROM Usuarios WHERE Email = '$CLIENTE_TEST_EMAIL') t); DELETE FROM Usuarios WHERE Email = '$CLIENTE_TEST_EMAIL';" 2>/dev/null || true
  rm -f "$COOKIE_ADMIN" "$COOKIE_COCINERO" "$COOKIE_CLIENTE"
}
trap cleanup EXIT

# 1. Login como ADMIN
echo "1. Probando Login de ADMIN en MVC..."
TOKEN_REQ=$(get_token "$MVC_URL/Cuenta/Login" "$COOKIE_ADMIN")
HTTP_CODE=$(curl -s -b "$COOKIE_ADMIN" -c "$COOKIE_ADMIN" -o /dev/null -w "%{http_code}" -X POST "$MVC_URL/Cuenta/Login" \
  -d "Email=admin@pizzaplaneta.com&Password=Admin123!&__RequestVerificationToken=$TOKEN_REQ")

if [ "$HTTP_CODE" -ne 302 ]; then
  echo "FAIL: Login Admin debió responder 302 Redirect, se obtuvo $HTTP_CODE"
  exit 1
fi

PAGE_USUARIOS=$(curl -s -b "$COOKIE_ADMIN" "$MVC_URL/Usuarios")
if ! echo "$PAGE_USUARIOS" | grep -q "Administración de Usuarios y Roles"; then
  echo "FAIL: Vista de Usuarios no cargó correctamente para Admin"
  exit 1
fi
echo "OK: Admin autenticado y vista /Usuarios verificada"

# 2. Login como COCINERO
echo "2. Probando Login de COCINERO en MVC..."
TOKEN_REQ=$(get_token "$MVC_URL/Cuenta/Login" "$COOKIE_COCINERO")
HTTP_CODE=$(curl -s -b "$COOKIE_COCINERO" -c "$COOKIE_COCINERO" -o /dev/null -w "%{http_code}" -X POST "$MVC_URL/Cuenta/Login" \
  -d "Email=cocinero@pizzaplaneta.com&Password=Cocinero123!&__RequestVerificationToken=$TOKEN_REQ")

PAGE_PIZZAS=$(curl -s -b "$COOKIE_COCINERO" "$MVC_URL/Pizzas")
if ! echo "$PAGE_PIZZAS" | grep -q "+ Nueva Pizza"; then
  echo "FAIL: Vista de Pizzas no mostró botón '+ Nueva Pizza' para Cocinero"
  exit 1
fi

HTTP_CODE_USUARIOS=$(curl -s -b "$COOKIE_COCINERO" -o /dev/null -w "%{http_code}" "$MVC_URL/Usuarios")
if [ "$HTTP_CODE_USUARIOS" -ne 302 ]; then
  echo "FAIL: Cocinero accediendo a /Usuarios debió ser redirigido a Acceso Denegado (302), se obtuvo $HTTP_CODE_USUARIOS"
  exit 1
fi
echo "OK: Cocinero autenticado, catálogo accesible y /Usuarios denegado"

# 3. Login como CLIENTE
echo "3. Probando Login de CLIENTE en MVC..."
CLIENTE_TEST_EMAIL="cliente_mvc_${TIMESTAMP:-123}@test.com"
TOKEN_REQ_REG=$(get_token "$MVC_URL/Cuenta/Registro" "$COOKIE_CLIENTE")
curl -s -b "$COOKIE_CLIENTE" -c "$COOKIE_CLIENTE" -X POST "$MVC_URL/Cuenta/Registro" \
  -d "Nombre=Cliente MVC&Email=$CLIENTE_TEST_EMAIL&Password=Cliente123!&Telefono=123&Direccion=Dir&__RequestVerificationToken=$TOKEN_REQ_REG" > /dev/null

TOKEN_REQ=$(get_token "$MVC_URL/Cuenta/Login" "$COOKIE_CLIENTE")
HTTP_CODE=$(curl -s -b "$COOKIE_CLIENTE" -c "$COOKIE_CLIENTE" -o /dev/null -w "%{http_code}" -X POST "$MVC_URL/Cuenta/Login" \
  -d "Email=$CLIENTE_TEST_EMAIL&Password=Cliente123!&__RequestVerificationToken=$TOKEN_REQ")

PAGE_CLIENTE=$(curl -s -b "$COOKIE_CLIENTE" "$MVC_URL/Pizzas")
if echo "$PAGE_CLIENTE" | grep -q "+ Nueva Pizza"; then
  echo "FAIL: Cliente no debe ver el botón '+ Nueva Pizza'"
  exit 1
fi

if ! echo "$PAGE_CLIENTE" | grep -q "Pedir esta Pizza"; then
  echo "FAIL: Cliente debe ver botón 'Pedir esta Pizza'"
  exit 1
fi

PAGE_PEDIR=$(curl -s -b "$COOKIE_CLIENTE" "$MVC_URL/Pedidos/Crear")
if ! echo "$PAGE_PEDIR" | grep -q "Solicitar Pedido"; then
  echo "FAIL: Cliente no pudo acceder a la vista /Pedidos/Crear"
  exit 1
fi
echo "OK: Cliente autenticado, catálogo con botón Pedir y formulario de pedido accesible"

echo "==> [SVC] Verificación de Navegación y Vistas MVC completada con éxito (Exit Code 0)"
