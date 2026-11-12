#!/usr/bin/env bash
# Writes sample platform and tenant secrets to the local HashiCorp Vault that Aspire starts (ADR-006).
# Development only. The values are placeholders, not real credentials.
#
#   ./scripts/vault-seed.sh            # run from "Education Portal Full Stack App"
#
# VAULT_ADDR defaults to http://localhost:8200. VAULT_TOKEN defaults to the token Aspire generated and keeps in the
# AppHost's user-secrets (Parameters:vault-dev-token).
set -euo pipefail

VAULT_ADDR="${VAULT_ADDR:-http://localhost:8200}"
MOUNT="${VAULT_MOUNT:-secret}"

if [[ -z "${VAULT_TOKEN:-}" ]]; then
  VAULT_TOKEN="$(dotnet user-secrets list --project src/AppHost 2>/dev/null \
    | sed -n 's/^Parameters:vault-dev-token = //p')"
fi

if [[ -z "${VAULT_TOKEN:-}" ]]; then
  echo "No Vault token. Start the AppHost once (aspire run) so it generates one, or set VAULT_TOKEN." >&2
  exit 1
fi

put() {
  local path="$1" value="$2"
  curl --fail --silent --show-error \
    --header "X-Vault-Token: ${VAULT_TOKEN}" \
    --header "Content-Type: application/json" \
    --request POST \
    --data "{\"data\":{\"value\":\"${value}\"}}" \
    "${VAULT_ADDR}/v1/${MOUNT}/data/${path}" > /dev/null
  echo "Wrote ${path}"
}

# Platform secrets. platform/db-connection is not seeded: Aspire already gives the app its local database.
put "platform/sample-api-key" "local-sample-platform-key"

# Tenant secrets for the seeded schools: 1 = master tenant (localhost), 2 = Demo School (demo.localhost).
put "tenants/1/sms-api-key" "local-sample-sms-key-school-1"
put "tenants/2/sms-api-key" "local-sample-sms-key-school-2"
