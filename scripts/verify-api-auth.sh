#!/usr/bin/env bash
set -euo pipefail

API_URL="${API_URL:-http://localhost:5272}"
echo "==> [SVC] Verificando Autenticación y Autorización en $API_URL"

TIMESTAMP=$(date +%s)
TEST_EMAIL="test_${TIMESTAMP}@cliente.com"

cleanup() {
  mysql -u 5to_agbd -pTrigg3rs! -h localhost -e "USE 5to_PizzaPlanetaDb; DELETE FROM Usuarios WHERE Email = '$TEST_EMAIL';" 2>/dev/null || true
  rm -f /tmp/reg_res.json /tmp/dup_res.json /tmp/login_res.json
}
trap cleanup EXIT

# 1. Registro Exitoso
echo "1. Probando Registro exitoso (POST /auth/registro)..."
HTTP_CODE=$(curl -s -o /tmp/reg_res.json -w "%{http_code}" -X POST "$API_URL/auth/registro" \
  -H "Content-Type: application/json" \
  -d "{\"nombre\":\"Test User\",\"email\":\"$TEST_EMAIL\",\"password\":\"Clave123!\",\"telefono\":\"12345\",\"direccion\":\"Calle Falsa 123\"}")

if [ "$HTTP_CODE" -ne 201 ]; then
  echo "FAIL: Se esperaba 201 en registro, se obtuvo $HTTP_CODE"
  cat /tmp/reg_res.json
  exit 1
fi
echo "OK: Registro exitoso (201)"

# 2. Rechazo Correo Duplicado
echo "2. Probando Rechazo de correo duplicado (POST /auth/registro)..."
HTTP_CODE=$(curl -s -o /tmp/dup_res.json -w "%{http_code}" -X POST "$API_URL/auth/registro" \
  -H "Content-Type: application/json" \
  -d "{\"nombre\":\"Test User Duplicado\",\"email\":\"$TEST_EMAIL\",\"password\":\"Clave123!\",\"telefono\":\"12345\",\"direccion\":\"Calle Falsa 123\"}")

if [ "$HTTP_CODE" -ne 400 ]; then
  echo "FAIL: Se esperaba 400 en correo duplicado, se obtuvo $HTTP_CODE"
  exit 1
fi
echo "OK: Correo duplicado rechazado (400)"

# 3. Login Exitoso
echo "3. Probando Login exitoso (POST /auth/login)..."
HTTP_CODE=$(curl -s -o /tmp/login_res.json -w "%{http_code}" -X POST "$API_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"Clave123!\"}")

if [ "$HTTP_CODE" -ne 200 ]; then
  echo "FAIL: Se esperaba 200 en login, se obtuvo $HTTP_CODE"
  cat /tmp/login_res.json
  exit 1
fi

TOKEN=$(grep -o '"token":"[^"]*' /tmp/login_res.json | grep -o '[^"]*$' || true)
if [ -z "$TOKEN" ]; then
  echo "FAIL: No se extrajo token JWT de la respuesta"
  exit 1
fi
echo "OK: Login exitoso y Token JWT emitido (200)"

# 4. Login Fallido
echo "4. Probando Login con clave errónea (POST /auth/login)..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$API_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$TEST_EMAIL\",\"password\":\"ClaveErronea\"}")

if [ "$HTTP_CODE" -ne 401 ]; then
  echo "FAIL: Se esperaba 401 en credenciales inválidas, se obtuvo $HTTP_CODE"
  exit 1
fi
echo "OK: Login con clave errónea rechazado (401)"

# 5. Petición Anónima a recurso protegido
echo "5. Probando rechazo de petición anónima (GET /usuarios)..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -X GET "$API_URL/usuarios")
if [ "$HTTP_CODE" -ne 401 ]; then
  echo "FAIL: Se esperaba 401 en petición anónima, se obtuvo $HTTP_CODE"
  exit 1
fi
echo "OK: Petición anónima rechazada con 401"

echo "==> [SVC] Todos los tests de Autenticación pasaron exitosamente (Exit Code 0)"
