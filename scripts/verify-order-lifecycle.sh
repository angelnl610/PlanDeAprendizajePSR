#!/usr/bin/env bash
set -euo pipefail

API_URL="${API_URL:-http://localhost:5272}"
echo "==> [SVC] Verificando Ciclo de Vida del Pedido en $API_URL"

# 1. Login como Cliente Seed o Crear uno
TIMESTAMP=$(date +%s)
CLIENT_EMAIL="client_order_${TIMESTAMP}@cliente.com"
cleanup() {
  mysql -u 5to_agbd -pTrigg3rs! -h localhost -e "USE 5to_PizzaPlanetaDb; DELETE i FROM ItemsPedido i INNER JOIN Pedidos p ON i.PedidoId = p.Id WHERE p.UsuarioId IN (SELECT Id FROM (SELECT Id FROM Usuarios WHERE Email = '$CLIENT_EMAIL') t); DELETE FROM Pedidos WHERE UsuarioId IN (SELECT Id FROM (SELECT Id FROM Usuarios WHERE Email = '$CLIENT_EMAIL') t); DELETE FROM Usuarios WHERE Email = '$CLIENT_EMAIL';" 2>/dev/null || true
}
trap cleanup EXIT

curl -s -X POST "$API_URL/auth/registro" \
  -H "Content-Type: application/json" \
  -d "{\"nombre\":\"Cliente Pedidos\",\"email\":\"$CLIENT_EMAIL\",\"password\":\"Clave123!\",\"telefono\":\"445566\",\"direccion\":\"Calle Pedido 100\"}" > /dev/null

LOGIN_RES=$(curl -s -X POST "$API_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$CLIENT_EMAIL\",\"password\":\"Clave123!\"}")

CLIENT_TOKEN=$(echo "$LOGIN_RES" | grep -o '"token":"[^"]*' | grep -o '[^"]*$' || true)
if [ -z "$CLIENT_TOKEN" ]; then
  echo "FAIL: No se pudo autenticar al cliente para prueba de pedidos"
  exit 1
fi

# 2. Obtener una pizza disponible para pedir
PIZZAS_RES=$(curl -s -X GET "$API_URL/pizzas" -H "Authorization: Bearer $CLIENT_TOKEN")
PIZZA_ID=$(echo "$PIZZAS_RES" | grep -o '"id":[0-9]*' | head -n 1 | cut -d: -f2 || true)

if [ -z "$PIZZA_ID" ]; then
  echo "FAIL: No hay pizzas activas en el catálogo para generar pedido"
  exit 1
fi

echo "Pizza encontrada: ID $PIZZA_ID"

# 3. Crear Pedido
echo "Creando Pedido..."
CREAR_RES=$(curl -s -X POST "$API_URL/pedidos" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $CLIENT_TOKEN" \
  -d "{\"items\":[{\"pizzaId\":$PIZZA_ID,\"cantidad\":2}]}")

PEDIDO_ID=$(echo "$CREAR_RES" | grep -o '"id":[0-9]*' | head -n 1 | cut -d: -f2 || true)
if [ -z "$PEDIDO_ID" ]; then
  echo "FAIL: No se pudo crear el pedido. Respuesta: $CREAR_RES"
  exit 1
fi
echo "Pedido creado con ID #$PEDIDO_ID"

# 4. Verificar Estado Inicial: Pendiente (1)
CONSULTA_RES=$(curl -s -X GET "$API_URL/pedidos/$PEDIDO_ID" -H "Authorization: Bearer $CLIENT_TOKEN")
echo "Estado inicial: $CONSULTA_RES"
if ! echo "$CONSULTA_RES" | grep -qE '("estado":1|"estado":"Pendiente")'; then
  echo "FAIL: El estado inicial debe ser Pendiente (1)"
  exit 1
fi
echo "OK: Estado inicial es Pendiente"

# 5. Esperar 5.5 segundos para la simulación
echo "Esperando 5.5 segundos para la transición automática a EnPreparacion..."
sleep 5.5

CONSULTA_RES2=$(curl -s -X GET "$API_URL/pedidos/$PEDIDO_ID" -H "Authorization: Bearer $CLIENT_TOKEN")
echo "Estado tras 5s: $CONSULTA_RES2"
if ! echo "$CONSULTA_RES2" | grep -qE '("estado":2|"estado":"EnPreparacion")'; then
  echo "FAIL: Tras 5s el estado debió transicionar a EnPreparacion (2)"
  exit 1
fi
echo "OK: Transición exitosa a EnPreparacion (Exit Code 0)"
