#!/usr/bin/env bash
set -euo pipefail

MVC_URL="${MVC_URL:-http://localhost:5278}"
API_URL="${API_URL:-http://localhost:5272}"
TIMESTAMP=$(date +%s)
echo "==> [SVC] Verificando Pedidos Multi-Pizza en MVC ($MVC_URL)"

COOKIE_JAR="/tmp/cookie_multi_order.txt"
rm -f "$COOKIE_JAR"

get_token() {
  local url="$1"
  local jar="$2"
  curl -s -c "$jar" "$url" | grep -o '__RequestVerificationToken.*' | head -n1 | sed -E 's/.*value="([^"]+)".*/\1/'
}

# 1. Registrar y Loguear Cliente
CLIENTE_EMAIL="cliente_multi_${TIMESTAMP}@test.com"
cleanup() {
  mysql -u 5to_agbd -pTrigg3rs! -h localhost -e "USE 5to_PizzaPlanetaDb; DELETE i FROM ItemsPedido i INNER JOIN Pedidos p ON i.PedidoId = p.Id WHERE p.UsuarioId IN (SELECT Id FROM (SELECT Id FROM Usuarios WHERE Email = '$CLIENTE_EMAIL') t); DELETE FROM Pedidos WHERE UsuarioId IN (SELECT Id FROM (SELECT Id FROM Usuarios WHERE Email = '$CLIENTE_EMAIL') t); DELETE FROM Usuarios WHERE Email = '$CLIENTE_EMAIL';" 2>/dev/null || true
  rm -f "$COOKIE_JAR"
}
trap cleanup EXIT
echo "1. Registrando y logueando cliente de prueba ($CLIENTE_EMAIL)..."

REG_TOKEN=$(get_token "$MVC_URL/Cuenta/Registro" "$COOKIE_JAR")
curl -s -b "$COOKIE_JAR" -c "$COOKIE_JAR" -X POST "$MVC_URL/Cuenta/Registro" \
  -d "Nombre=Cliente Multi&Email=$CLIENTE_EMAIL&Password=Cliente123!&Telefono=11223344&Direccion=Calle Falsa 123&__RequestVerificationToken=$REG_TOKEN" > /dev/null

LOGIN_TOKEN=$(get_token "$MVC_URL/Cuenta/Login" "$COOKIE_JAR")
curl -s -b "$COOKIE_JAR" -c "$COOKIE_JAR" -X POST "$MVC_URL/Cuenta/Login" \
  -d "Email=$CLIENTE_EMAIL&Password=Cliente123!&__RequestVerificationToken=$LOGIN_TOKEN" > /dev/null

# 2. Cargar vista GET /Pedidos/Crear
echo "2. Obteniendo formulario de nuevo pedido..."
PAGE_CREAR=$(curl -s -b "$COOKIE_JAR" -c "$COOKIE_JAR" "$MVC_URL/Pedidos/Crear")
CREAR_TOKEN=$(echo "$PAGE_CREAR" | grep -o '__RequestVerificationToken.*' | head -n1 | sed -E 's/.*value="([^"]+)".*/\1/')

if [ -z "$CREAR_TOKEN" ]; then
  echo "FAIL: No se pudo obtener el token antiforgery del formulario de creación"
  exit 1
fi

# 3. Prueba Negativa: Enviar con todas las cantidades en 0
echo "3. Probando rechazo de pedido con todas las cantidades en 0..."
RESP_CERO=$(curl -s -b "$COOKIE_JAR" -c "$COOKIE_JAR" -X POST "$MVC_URL/Pedidos/Crear" \
  -d "Pizzas[0].PizzaId=1&Pizzas[0].Cantidad=0&Pizzas[1].PizzaId=2&Pizzas[1].Cantidad=0&__RequestVerificationToken=$CREAR_TOKEN")

if ! echo "$RESP_CERO" | grep -q "Debe seleccionar al menos una pizza con cantidad mayor a cero"; then
  echo "FAIL: No se mostró el mensaje de error por cantidades en 0"
  exit 1
fi
echo "OK: Rechazo por cantidades en 0 validado"

# 4. Prueba Negativa: Enviar con cantidad negativa
echo "4. Probando rechazo de pedido con cantidades negativas..."
RESP_NEG=$(curl -s -b "$COOKIE_JAR" -c "$COOKIE_JAR" -X POST "$MVC_URL/Pedidos/Crear" \
  -d "Pizzas[0].PizzaId=1&Pizzas[0].Cantidad=-2&Pizzas[1].PizzaId=2&Pizzas[1].Cantidad=1&__RequestVerificationToken=$CREAR_TOKEN")

if ! echo "$RESP_NEG" | grep -q "Las cantidades no pueden ser negativas"; then
  echo "FAIL: No se mostró el mensaje de error por cantidades negativas"
  exit 1
fi
echo "OK: Rechazo por cantidades negativas validado"

# 5. Prueba Positiva: Enviar pedido con 2 variedades distintas (Pizza ID 1: 2 unidades, Pizza ID 2: 1 unidad)
echo "5. Creando pedido multi-variedad (Pizza 1 x2, Pizza 2 x1)..."
REDIRECT_HEADER=$(curl -s -i -b "$COOKIE_JAR" -c "$COOKIE_JAR" -X POST "$MVC_URL/Pedidos/Crear" \
  -d "Pizzas[0].PizzaId=1&Pizzas[0].Cantidad=2&Pizzas[1].PizzaId=2&Pizzas[1].Cantidad=1&__RequestVerificationToken=$CREAR_TOKEN" \
  | grep -i "^location:" || true)

if [ -z "$REDIRECT_HEADER" ]; then
  echo "FAIL: Creación de pedido multi-ítem no devolvió cabecera de redirección Location (302)"
  exit 1
fi

DETALLE_PATH=$(echo "$REDIRECT_HEADER" | awk '{print $2}' | tr -d '\r\n')
echo "OK: Redirigido a $DETALLE_PATH"

# 6. Verificar detalle del pedido
echo "6. Verificando ítems y subtotales en vista de detalle..."
DETALLE_HTML=$(curl -s -b "$COOKIE_JAR" "$MVC_URL$DETALLE_PATH")

if ! echo "$DETALLE_HTML" | grep -q "Muzzarella"; then
  echo "FAIL: La primera pizza no figura en el detalle"
  exit 1
fi

if ! echo "$DETALLE_HTML" | grep -q "Napolitana"; then
  echo "FAIL: La segunda pizza no figura en el detalle"
  exit 1
fi

echo "OK: Ambas pizzas verificadas en el detalle del pedido"
echo "==> [SVC] Verificación de Pedidos Multi-Pizza completada exitosamente (Exit Code 0)"
