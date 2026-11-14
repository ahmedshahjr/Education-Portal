# ADR-006: Cloud-Agnostic Secrets Layer

## Status

Accepted

## Date

2026-11-11

## Context

The portal needs platform secrets (the database connection string, later signing keys) and per-school secrets (for example a school's SMS provider key). Hosting is still undecided (AGENTS.md section 9), so the code must not be tied to one cloud. Local development must work without a cloud account. Before this ADR, `Web` could only add Azure Key Vault as a configuration source (`AddKeyVaultIfConfigured`, `AZURE_KEY_VAULT_ENDPOINT`), which loaded every secret at startup and offered no tenant scoping.

## Decision

- **One secret store per environment, shared by all schools.** It is chosen by configuration only: `Secrets:Provider` = `HashiCorp`, `AzureKeyVault`, `Aws` or `Gcp`. There are no per-school store settings. The `Secrets` section is the app's single store configuration; `AddKeyVaultIfConfigured` and `AZURE_KEY_VAULT_ENDPOINT` are removed.
- **Logical keys.** Application code uses only `SecretKey` (Application layer): `platform/{name}` and `tenants/{schoolId}/{name}`. A name is 1 to 63 lowercase letters, digits and dashes and starts with a letter. Each provider translates keys to its own rules:

  | Provider | `tenants/5/sms-api-key` | `platform/db-connection` |
  |---|---|---|
  | HashiCorp Vault (KV v2) | `tenants/5/sms-api-key` (value in field `value`) | `platform/db-connection` |
  | Azure Key Vault | `tenant-5-sms-api-key` | `platform-db-connection` |
  | AWS Secrets Manager | `tenants/5/sms-api-key` | `platform/db-connection` |
  | Google Secret Manager | `tenant-5-sms-api-key` | `platform-db-connection` |

- **Abstractions (Application):** `ISecretProvider` (Get returns `null` when missing, Set, Delete) and `ITenantSecretProvider`, which builds the key from `ICurrentTenant.SchoolId` (resolved from the host) and never accepts a tenant id. It throws when no tenant is resolved.
- **Implementations (Infrastructure/Secrets):** VaultSharp with token or AppRole auth; Azure SDK with `DefaultAzureCredential`; AWS SDK with the default credential chain; Google client with application default credentials.
- **Decorators:** `CachedSecretProvider` (IMemoryCache, `Secrets:CacheTtlMinutes`, default 10, Set and Delete evict, values loaded on first use, misses not cached) over `ResilientSecretProvider` (Polly: 3 retries with exponential backoff and jitter on transient failures, 10 s per attempt).
- **Startup:** `AddPlatformSecretsAsync` loads the startup secrets (today `platform/db-connection` into `ConnectionStrings:EducationPortalDb`) before the DbContext is registered. A value in the store wins; a missing value or an empty `Secrets:Provider` falls back to normal configuration. Invalid settings stop startup with a message that names the missing key.
- **DI:** `AddSecretsManagement(IServiceCollection, IConfiguration)` registers the selected store with retries and cache as `ISecretProvider`, `ITenantSecretProvider` as scoped, options validation on start, and the `secrets` health check (reads `platform/health-check` past the cache). Without a provider, `ISecretProvider` fails closed with a clear message.
- **Local development:** the Aspire AppHost runs HashiCorp Vault (`hashicorp/vault`, dev mode, port 8200) as a container resource, generates its root token as a secret Aspire parameter kept in the AppHost's user-secrets, and passes `Secrets__*` to the API. `scripts/vault-seed.sh` / `.ps1` write sample values. Containers are always Aspire resources, never docker-compose (AGENTS.md section 6).
- **Never** store secret values in the database, log them or put them in exception messages. `SecretProviderException` names the operation and the logical key only. Credentials for the store come from environment variables or user-secrets, never `appsettings*.json` (SEC-SECRET-01).

## Rationale

- An interface with one small implementation per cloud keeps the switch to configuration only, which is what an undecided hosting choice needs. A configuration-provider approach (load every secret into `IConfiguration`) was rejected: it loads all tenants' secrets at startup, cannot write, and has no tenant scoping.
- Taking the tenant only from `ICurrentTenant` reuses the existing host-based resolution, so tenant secrets follow the same isolation rule as data (SEC-AUTHZ-04).
- The strict name rule makes the dashed translation (Azure, GCP) unambiguous: the tenant id is digits and a name starts with a letter, so two keys never map to one name, and `..` or `/` cannot reach another key.
- Polly comes from `Microsoft.Extensions.Resilience`, the same family and version as the HTTP resilience package ServiceDefaults already uses.
- HashiCorp Vault locally needs no cloud account, and running it through Aspire keeps one way to start everything (ADR-002).

## Consequences

**Easier:**
- Moving between Azure, AWS and Google Cloud is a configuration change.
- School-specific integrations can store keys safely with `ITenantSecretProvider` without new plumbing.

**Harder:**
- Four SDKs are now dependencies and must be kept patched.
- A value changed outside the app is seen after the cache TTL (10 minutes by default). Values changed through the app are evicted at once on that instance; other instances see them after the TTL.
- Delete semantics differ: Vault removes all versions; Azure Key Vault soft-deletes, so the same name can only be set again after purge or recovery; AWS schedules deletion with a 7-day recovery window and a later Set restores the secret; Google deletes the secret and its versions.
- Vault dev mode keeps data in memory: after the container is recreated, run the seed script again.
- Production still needs the store itself, its identity and access policies (least privilege: the app reads and writes only `platform/*` and `tenants/*`) as part of the hosting decision.
