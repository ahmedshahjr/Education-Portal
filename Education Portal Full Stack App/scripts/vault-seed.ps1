# Writes sample platform and tenant secrets to the local HashiCorp Vault that Aspire starts (ADR-006).
# Development only. The values are placeholders, not real credentials.
#
#   ./scripts/vault-seed.ps1           # run from "Education Portal Full Stack App"
#
# VAULT_ADDR defaults to http://localhost:8200. VAULT_TOKEN defaults to the token Aspire generated and keeps in the
# AppHost's user-secrets (Parameters:vault-dev-token).
$ErrorActionPreference = 'Stop'

$vaultAddr = if ($env:VAULT_ADDR) { $env:VAULT_ADDR } else { 'http://localhost:8200' }
$mount = if ($env:VAULT_MOUNT) { $env:VAULT_MOUNT } else { 'secret' }
$token = $env:VAULT_TOKEN

if (-not $token) {
    $line = dotnet user-secrets list --project src/AppHost 2>$null | Where-Object { $_ -like 'Parameters:vault-dev-token = *' }
    if ($line) { $token = ($line -split ' = ', 2)[1] }
}

if (-not $token) {
    Write-Error 'No Vault token. Start the AppHost once (aspire run) so it generates one, or set VAULT_TOKEN.'
}

function Set-VaultSecret([string] $Path, [string] $Value) {
    $body = @{ data = @{ value = $Value } } | ConvertTo-Json -Compress
    Invoke-RestMethod -Method Post -Uri "$vaultAddr/v1/$mount/data/$Path" `
        -Headers @{ 'X-Vault-Token' = $token } -ContentType 'application/json' -Body $body | Out-Null
    Write-Host "Wrote $Path"
}

# Platform secrets. platform/db-connection is not seeded: Aspire already gives the app its local database.
Set-VaultSecret 'platform/sample-api-key' 'local-sample-platform-key'

# Tenant secrets for the seeded schools: 1 = master tenant (localhost), 2 = Demo School (demo.localhost).
Set-VaultSecret 'tenants/1/sms-api-key' 'local-sample-sms-key-school-1'
Set-VaultSecret 'tenants/2/sms-api-key' 'local-sample-sms-key-school-2'
