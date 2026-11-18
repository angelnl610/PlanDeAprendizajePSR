#!/usr/bin/env bash
set -euo pipefail

API_URL="${API_URL:-http://localhost:5272}"
echo "==> [SVC] Verificando Soft Delete y Roles en Pizzas en $API_URL"

# 1. Obtener Token de Cliente
CLIENT_EMAIL="cliente_pizza_roles@test.com"
cleanup() {
  mysql -u 5to_agbd -pTrigg3rs! -h localhost -e "USE 5to_PizzaPlanetaDb; DELETE FROM Usuarios WHERE Email = '$CLIENT_EMAIL';" 2>/dev/null || true
}
trap cleanup EXIT
curl -s -X POST "$API_URL/auth/registro" \
  -H "Content-Type: application/json" \
  -d "{\"nombre\":\"Cliente Roles\",\"email\":\"$CLIENT_EMAIL\",\"password\":\"Clave123!\",\"telefono\":\"123\",\"direccion\":\"Dir 1\"}" > /dev/null || true

LOGIN_CLIENT_RES=$(curl -s -X POST "$API_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$CLIENT_EMAIL\",\"password\":\"Clave123!\"}")

CLIENT_TOKEN=$(echo "$LOGIN_CLIENT_RES" | grep -o '"token":"[^"]*' | grep -o '[^"]*$' || true)

# 2. Cliente intentando crear Pizza -> debe dar 403 Forbidden
echo "1. Probando rechazo de creación de pizza por Cliente (403)..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$API_URL/pizzas" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $CLIENT_TOKEN" \
  -d "{\"nombre\":\"Pizza No Autorizada\",\"descripcion\":\"Test\",\"precio\":1000}")

if [ "$HTTP_CODE" -ne 403 ]; then
  echo "FAIL: Se esperaba 403 Forbidden al crear pizza con rol Cliente, se obtuvo $HTTP_CODE"
  exit 1
fi
echo "OK: Cliente rechazado con 403 Forbidden"

# 3. Login como Cocinero Seed (cocinero@pizzaplaneta.com / Cocinero123!)
echo "2. Autenticando como Cocinero..."
LOGIN_COCINERO_RES=$(curl -s -X POST "$API_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"cocinero@pizzaplaneta.com\",\"password\":\"Cocinero123!\"}")

COCINERO_TOKEN=$(echo "$LOGIN_COCINERO_RES" | grep -o '"token":"[^"]*' | grep -o '[^"]*$' || true)
if [ -z "$COCINERO_TOKEN" ]; then
  echo "FAIL: No se pudo loguear como cocinero@pizzaplaneta.com"
  exit 1
fi

# 4. Cocinero creando Pizza -> debe dar 201 Created
echo "3. Cocinero creando nueva pizza (201)..."
CREAR_PIZZA_RES=$(curl -s -X POST "$API_URL/pizzas" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $COCINERO_TOKEN" \
  -d "{\"nombre\":\"Especial Cocinero\",\"descripcion\":\"Salsa y muzzarella espacial\",\"precio\":14500}")

PIZZA_ID=$(echo "$CREAR_PIZZA_RES" | grep -o '"id":[0-9]*' | head -n 1 | cut -d: -f2 || true)
if [ -z "$PIZZA_ID" ]; then
  echo "FAIL: No se pudo crear la pizza como cocinero: $CREAR_PIZZA_RES"
  exit 1
fi
echo "OK: Pizza creada con ID #$PIZZA_ID (201)"

# 5. Soft Delete de la Pizza
echo "4. Eliminando pizza mediante Soft Delete (DELETE /pizzas/$PIZZA_ID)..."
DELETE_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$API_URL/pizzas/$PIZZA_ID" \
  -H "Authorization: Bearer $COCINERO_TOKEN")

if [ "$DELETE_CODE" -ne 204 ]; then
  echo "FAIL: Se esperaba 204 No Content en DELETE, se obtuvo $DELETE_CODE"
  exit 1
fi
echo "OK: DELETE respondió 204 No Content"

# 6. Verificar que la pizza eliminada YA NO aparece en el catálogo activo (GET /pizzas)
echo "5. Verificando que la pizza inactiva no figura en el catálogo..."
CATALOGO_RES=$(curl -s -X GET "$API_URL/pizzas" -H "Authorization: Bearer $COCINERO_TOKEN")
if echo "$CATALOGO_RES" | grep -q "\"id\":$PIZZA_ID,"; then
  echo "FAIL: La pizza con ID $PIZZA_ID sigue listándose en el catálogo activo tras soft delete"
  exit 1
fi
echo "OK: Pizza excluida correctamente del catálogo activo (Soft Delete validado)"

echo "==> [SVC] Verificación de Soft Delete y Roles de Pizza exitosa (Exit Code 0)"
