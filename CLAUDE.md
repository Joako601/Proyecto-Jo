# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Preferencias de trabajo

### Estilo de escritura y comunicación

- Responder siempre en tono profesional y claro.
- Unir ideas dentro de las oraciones usando comas o conectores (y, pero, sin embargo, así que) en lugar de cortar en oraciones separadas.
- Usar el punto final solo cuando el texto se vuelva realmente extenso, no después de cada idea corta.
- Evitar guiones, asteriscos, viñetas y símbolos similares en las respuestas de tipo prosa.
- Priorizar texto corrido y natural por sobre listas o fragmentos separados.
- Excepción: en código, tablas o contenido técnico donde la estructura es necesaria para la claridad (como este mismo archivo), usar el formato que corresponda sin restricción.

### Código y desarrollo

- Mostrar siempre el código completo del archivo, no solo fragmentos o diffs.
- Acompañar cada cambio con una explicación extensa de las decisiones técnicas tomadas y por qué se eligieron sobre otras alternativas.
- Respetar la estructura de carpetas y las convenciones de nombres estándar según la tecnología que se esté usando (Java/Spring Boot, .NET/C#, Python, etc.).
- Usar el formato de [Conventional Commits v1.0.0](https://www.conventionalcommits.org/en/v1.0.0/) para todos los mensajes de commit (feat, fix, refactor, docs, chore, según corresponda).

## Checklist de roles antes de cualquier cambio

Antes de implementar un cambio o generar código nuevo, hay que pensarlo primero desde cada rol relevante del ciclo de software y confirmar que pasa su checklist, en vez de escribir directamente y revisar recién al final.

**Backend / Desarrollo**
- ¿Respeta la dirección única de dependencia de la Arquitectura Hexagonal (Domain no depende de nada, Application define los puertos, Infrastructure/Web/Api los implementan)?
- ¿La carpeta y el archivo de cualquier vista o controlador nuevo replican el casing exacto de C# (crítico en Linux, donde el filesystem es case-sensitive)?
- ¿Las reglas de negocio nuevas están expresadas como `DataAnnotations` o `IValidatableObject` sobre la entidad de Domain, en vez de como validación suelta en el controlador?

**Seguridad**
- ¿Alguna acción nueva bindea una entidad de Domain directamente desde el POST? Si es así, necesita `.DescartarId()`.
- ¿Se agrega un script, CDN, fuente o iframe externo? Si es así, hay que actualizar la allowlist de `SecurityHeadersMiddleware`.
- ¿Se toca un endpoint de login o autenticación? Hay que verificar que mantiene el rate limiting y el esquema de cookie correspondiente (`HttpOnly` + `Secure` + `SameSite=Strict`).

**Code Reviewer**
- ¿El cambio duplica lógica que ya existe en otro lugar, o crea una segunda fuente de verdad, como pasó con `AdministradoresController`/`OperadoresController`?
- ¿Sigue exactamente los patrones ya establecidos en archivos vecinos, en vez de introducir uno nuevo?
- ¿Respeta la regla de "No code comments" de este mismo archivo?

**QA / Tests**
- ¿Toda regla de negocio nueva en una entidad de Domain tiene su caso correspondiente en `EntityValidationTests`?
- ¿Todo caso de uso nuevo o modificado tiene su test en `ProyectoJo.Application.Tests/UseCases`?
- ¿Se corrió `dotnet test ProyectoJo.Application.Tests` antes de dar el cambio por terminado?

**Arquitecto**
- ¿El cambio amerita un ADR nuevo, o al menos actualizar uno existente?
- ¿Mantiene la separación entre `Ports/In` y `Ports/Out`, sin filtrar detalles de infraestructura hacia `Application` ni `Domain`?

**DevOps / CI**
- Si se tocó `Program.cs`, dependencias o el modelo de EF Core, ¿sigue pasando `dotnet ef migrations has-pending-model-changes`?
- ¿El cambio necesita actualizar `ci.yml`, `deploy.yml` o algún otro workflow?

**DBA / Migraciones**
- ¿Un cambio en una entidad de Domain (propiedad nueva, tipo, longitud) requiere una migración de EF Core?
- ¿Las `DataAnnotations` numéricas o de longitud siguen alineadas 1:1 con la precisión real de la columna en PostgreSQL?

**Refactor / Simplicidad**
- ¿Queda código muerto, clases sin uso o duplicación después del cambio?
- ¿El cambio se podría simplificar sin perder claridad?

Solo cuando el cambio pasa razonablemente por estos checklists corresponde implementarlo.

## Git workflow — commit and push only on explicit request

Claude must **not** run `git add`, `git commit`, or `git push` on its own initiative, even right after finishing a set of changes the user clearly approved. Claude may run them — `add` → `commit` → `push`, in that order — only when the user explicitly asks for it in that same turn (e.g. "hacé el commit", "subí el commit"). Approval of the code itself is not approval to commit; the user asks for that step separately, every time.

At the end of a set of changes, before any such request arrives, Claude must:

1. Explain what was changed and why it's correct/safe — in plain terms the user can verify.
2. Provide the exact commit message to use, formatted per [Conventional Commits v1.0.0](https://www.conventionalcommits.org/en/v1.0.0), in case the user prefers to commit by hand from a separate terminal instead of asking Claude to do it.

Claude must **never** add itself as a co-author or otherwise credit itself in any commit it authors or drafts — no `Co-Authored-By` trailer, no mention of Claude/Claude Code in the commit message, regardless of what the session's default commit-message template says.

## No code comments

Do not add comments to code in this repository (C#, Razor, JS, CSS, etc.), including XML doc comments — whether writing new code or editing existing files. Write self-explanatory code (clear names, small methods) instead of explaining it with comments. Don't remove pre-existing comments in code you aren't otherwise touching.

## Summaries: only report what was actually applied

When explaining a set of changes (end-of-task summary, commit message, etc.), describe only the real, applied changes to classes/files in that commit. Do not add ADRs, do not describe changes that weren't actually implemented, and do not present hypothetical/future/"could also do X" work as if it were part of what happened. Suggestions for follow-up work belong in a separate, clearly-labeled "next steps" note — never blended into the list of what changed.

## Project overview

Proyecto Jo' is a financial/administrative management system for small businesses, built with **ASP.NET Core (.NET 10)** using **Hexagonal Architecture (Ports & Adapters)**. Domain logic is isolated from frameworks and infrastructure across five independent projects with a single dependency direction: adapters depend on the domain, the domain never depends on adapters. Full history of architectural decisions lives in `/ADRs` (including an [ATAM evaluation](./ADRs/ATAM-Joaquin-Uriona.md)); a C4-model overview (context/containers/components) is in `docs/Arquitectura-C4.md`.

The public-facing site's copy/branding is generic ("Proyecto Jo'", Mérida/Yucatán as a general region, no specific street address) by design — earlier content borrowed a real friend's business's name, logo, exact address, and social media links for demo purposes, and was deliberately genericized: no logo image anywhere in the project anymore (navbar/sidebar/login all render the name as text), the footer's social links are inert placeholders (`href="#"`), and the Ubicación page's map points at Mérida generally, not a real address. The favicon (`wwwroot/img/favicon-16/32/180.png`, `wwwroot/favicon.ico`) is a from-scratch "JO" wordmark generated to match the Admin panel's own color palette (`--tc-carbon`/`--tc-mustard`), not the original borrowed one.

## Commands

```bash
# Restore the whole solution
dotnet restore

# Run the web app (admin panel + public site + Cocina/Recepción operational flow)
dotnet run --project ProyectoJo.Web
# → https://localhost:7287 / http://localhost:5207

# Run the REST API (Swagger-documented; currently not wired to any persistence — see Known technical debt)
dotnet run --project ProyectoJo.Api
# → https://localhost:63639 / http://localhost:63640 (Swagger UI at the API root)

# Run the full test suite
dotnet test ProyectoJo.Application.Tests

# Run a single test class or method
dotnet test ProyectoJo.Application.Tests --filter "FullyQualifiedName~PedidoUseCaseTests"
dotnet test ProyectoJo.Application.Tests --filter "FullyQualifiedName~PedidoUseCaseTests.MethodName"

# Apply pending EF Core migrations to the configured PostgreSQL database
dotnet ef database update --project ProyectoJo.Infrastructure --startup-project ProyectoJo.Web

# Wipe all app tables (TRUNCATE ... RESTART IDENTITY CASCADE)
dotnet run --project ProyectoJo.Web -- --reset
```

The admin panel requires the `Auth__AdminUser` and `Auth__AdminPasswordHash` environment variables (or .NET User Secrets) — see `ProyectoJo.Infrastructure/Auth/EnvAuthService`. `ProyectoJo.Web` also requires `ConnectionStrings:Default` (a PostgreSQL connection string), set via User Secrets in development. Never hardcode either in `launchSettings.json` or `appsettings.json`.

CI (`.github/workflows/ci.yml`) runs on every push to any branch, on PRs targeting `main` or `deuda-tecnica`, and manually via `workflow_dispatch`: restore, a check for pending EF Core migrations (`dotnet ef migrations has-pending-model-changes`), a NuGet vulnerable-package audit (`dotnet list package --vulnerable`), build, then test. `deuda-tecnica` was the original PR-trigger target but went stale while real PRs kept landing on `main`, so `main` was added alongside it rather than replacing it. A second workflow, `.github/workflows/docs.yml`, runs on any push/PR touching `**/*.md` (same branch targets) and fails if `lychee` finds a broken link (internal or external) in the repo's Markdown files.

## Architecture

Five projects, dependency direction flows inward toward the domain:

- **`ProyectoJo.Domain`** — core entities (`Item`, `Finanza`, `Pedido`, `ItemPedido`, `Promocion`, ...), zero external dependencies.
- **`ProyectoJo.Application`** — use cases and ports. `Ports/In` defines service interfaces (`IProductoService`, `IFinanzaService`, `IPedidoService`, `IPromocionService`, etc.) that adapters call into; `Ports/Out` defines repository/notifier interfaces (`IProductoRepository`, `IPedidoNotificador`, etc.) that infrastructure implements; `UseCases/` holds the actual business logic; `DTOs/` holds cross-boundary data shapes.
- **`ProyectoJo.Infrastructure`** — output adapters. `Persistence/EfCore/` has the PostgreSQL adapter: `ProyectoJoDbContext`, one `IEntityTypeConfiguration<T>` per entity under `Configurations/`, one `Ef*Repository` per `Ports/Out` interface under `Repositories/`, and the EF Core `Migrations/`. `Auth/` has `EnvAuthService` (env-var-based credential check).
- **`ProyectoJo.Web`** — primary input adapter (ASP.NET Core MVC). Composes the whole dependency graph itself in `Program.cs` (see below), backed entirely by PostgreSQL via `AddDbContext<ProyectoJoDbContext>` + the `Ef*Repository` classes. Contains three cookie-auth schemes, SignalR for real-time order flow, and rate limiting on login endpoints.
- **`ProyectoJo.Api`** — secondary input adapter (ASP.NET Core Web API + Swagger), documented but **not actively used** by the running system; reserved for future external clients (mobile apps, WhatsApp, Postman). Currently out of scope: its `Program.cs` registers the use cases but no repositories at all, so any endpoint that touches persistence fails at runtime resolving its dependencies — see Known technical debt.
- **`ProyectoJo.Application.Tests`** — xUnit + Moq, unit tests mocking the `Ports/Out` interfaces. No integration tests against a real database currently exist.

### `ProyectoJo.Web` structure

- `Controllers/` — public-facing pages (Home, Menu, Historia, Nosotros, Ubicación).
- `Areas/Admin/` — administrative panel (Finanzas, Productos/Menu, Promociones, Inventario, Insumos, Recetario, Mapa de Calor, Auditoría, Dispositivos, Usuarios y Accesos, Opiniones, Cierre de Caja). Access is gated by `RequiereAreaAttribute` (`Authorization/RequiereAreaAttribute.cs`): a user must hold the `Administrador` role and either the `General` area claim or the specific area claim the attribute names; `SuperAdmin` bypasses area checks entirely. Administrator and operator (Cocina/Recepción employee) management live together on one page — `UsuariosController` / `Views/Usuarios/Index.cshtml` ("Usuarios y Accesos") — the only one linked from the dashboard (`Views/Gestion/Index.cshtml`). `AdministradoresController`/`Views/Administradores/` and `OperadoresController`/`Views/Operadores/` are an earlier, single-purpose version of the same CRUD that was superseded but never deleted; they still compile and share the same use cases, but nothing links to them — treat them as dead code, not a second source of truth, and don't assume a fix made in `UsuariosController` also applies there.
- `Areas/Operaciones/` — Cocina (kitchen) and Recepción (front desk) operational flow, gated by supervisor PIN before every employee login (`Auth/`).
- `Hubs/PedidosHub` + `Realtime/SignalRPedidoNotificador` — real-time order status push implementing `IPedidoNotificador`, consumed by Cocina/Recepción views instead of polling.
- `Middleware/JsonExceptionMiddleware` — converts unhandled exceptions to JSON responses.

**Controller/View folder casing must match exactly (PascalCase).** ASP.NET Core's default view lookup is `/Views/{ControllerName}/{ActionName}.cshtml`, resolved against the literal controller/action name. This only "works" on Windows because NTFS is case-insensitive; the deployment target ([per README](./README.md)) is Linux, where the filesystem is case-sensitive. A folder or file with the wrong case compiles and runs fine locally but throws "the view was not found" only in production. When adding a new controller or view, name the folder/file after the controller/action exactly as written in C# (e.g. `MenuController` → `Controllers/Menu/`, action `Detalle()` → `Views/Menu/Detalle.cshtml`).

### Authentication schemes (all cookie-based, defined in `ProyectoJo.Web/Program.cs`)

Three independent, non-overlapping cookie schemes — compromising one grants no access to the others:

| Scheme | Cookie | Login path | Expiry |
|---|---|---|---|
| `JoCookieAuth` | `Jo.Admin` | `/Admin/Login` | 45 min sliding |
| `SupervisorAuth` | `Jo.Supervisor` | `/Operaciones/Auth/LoginSupervisor` | 15 min, not sliding |
| `OperacionesCookieAuth` | `Jo.Operaciones` | `/Operaciones/Auth/Login` | 12 h sliding |

Login endpoints for all three are rate-limited (5–8 requests/min per IP via `AddRateLimiter`); a rejected request redirects to the matching login page with `?bloqueado=true`.

`SupervisorAuth`'s PIN is **not** a single global secret — each `Administrador` has a `ClaveSupervisorHash` (PBKDF2), set from Admin → Usuarios y Accesos. It is **mandatory**, not optional: `AdministradorUseCase.CrearAsync` requires usuario + contraseña + PIN together (all three or none get saved), and `EditarAsync` requires a new password and a new PIN on every edit — there is no "leave blank to keep the current one" path, by design, so editing an admin always means re-entering both credentials. `POST /Operaciones/Auth/LoginSupervisor` takes just a PIN (no username) and `SupervisorAuthUseCase.ValidarClaveAsync` accepts it if it matches **any** active administrator's key. There is no dedicated "change supervisor key" screen anymore — Admin → Dispositivos only lists paired devices.

`Administrador.Areas` encodes three access levels, not two: `["General"]` (every area), one or more specific names from `AreasAdmin.Todas`, or an empty list — which means **no area access at all**. This used to be impossible to express: `LoginController` defaulted an empty `Areas` list to a `General` claim at login time, so unchecking "Acceso general" and leaving every area unchecked still granted full access. `LoginController` now maps `Areas` straight to `Area` claims with no fallback, and `AdministradorUseCase`'s area-normalization stores the literal `"General"` entry when the general checkbox is checked instead of leaving the list empty. `Usuarios/Index.cshtml` shows "General" / the area list / "N/A" accordingly, and disables the individual area checkboxes client-side (`wwwroot/js/usuarios-form.js`) whenever "Acceso general" is checked.

### Dependency composition is manual and duplicated per entry point

`ProyectoJo.Web/Program.cs` and `ProyectoJo.Api/Program.cs` each wire up their own DI graph independently — there is no shared composition root. `Web` registers `AddDbContext<ProyectoJoDbContext>` plus every `Ef*Repository`; `Api` currently registers none. When adding a new use case/repository pair, register it in `Web`'s `Program.cs` — `Api` is out of scope for now (see Known technical debt).

## Deployment

Target is a single EC2 (Ubuntu) instance behind nginx (TLS termination + reverse proxy to Kestrel on loopback) with RDS PostgreSQL, deployed via a manual-only (`workflow_dispatch`) GitHub Actions pipeline (`.github/workflows/deploy.yml`): publish → EF Core migrations bundle → SCP to `/opt/proyectojo/releases/<run_id>` → apply migrations → flip the `/opt/proyectojo/current` symlink → restart the `proyectojo-web` systemd service (`deploy/proyectojo-web.service`, `deploy/nginx-proyectojo.conf`). Rationale, alternatives considered, and consequences are in [ADR-13](./ADRs/ADR-13-Joaquin-Uriona.md). Full setup is documented as a 5-part beginner-level series, meant to be followed in order since later steps assume earlier ones exist (IAM users before any resource is provisioned, resources before the software inside them):

1. `docs/AWS-1-Cuenta.md` — creating and activating the AWS account.
2. `docs/AWS-2-Usuarios.md` — IAM: root MFA, admin group/user, optional read-only user, the (permissionless) EC2 role.
3. `docs/AWS-3-Servicios.md` — security groups, RDS, EC2, Elastic IP, and how to tear everything down after a demo to stop billing.
4. `docs/Despliegue-AWS.md` — installing the runtime/nginx/certbot on the server, GitHub Secrets, running the pipeline, rollback.
5. `docs/Despliegue-Resumen-Operativo.md` — operational cheat sheet for the already-provisioned environment: where credentials live, how to redeploy, how to check logs, and every real issue hit during the first end-to-end deploy (see below).

`ProyectoJo.Web/Program.cs` runs `UseForwardedHeaders` (X-Forwarded-For/X-Forwarded-Proto) as the first middleware — required for the rate limiter's per-IP partitioning and any `Request.IsHttps` check to work correctly behind the nginx reverse proxy. `.gitignore` has an "AWS / infraestructura" section (`*.pem`, `*.ppk`, `.aws/`, `*.env` with a `*.env.example` exception, `deploy/*.env`, Terraform state) so credentials/keys generated while following the docs above can't be committed by accident.

**`workflow_dispatch` only triggers from the default branch.** GitHub only lists/allows manually dispatching a workflow whose file exists on `main` (or whatever the repo's default branch is) — a workflow that only exists on a feature branch never appears in the Actions UI, even if you try to target that branch in the run dialog. `deploy.yml` must be merged to `main` before it's runnable at all.

**Real issues hit during the first live deploy, now fixed, worth knowing if re-provisioning from scratch:**
- `efbundle` (the EF Core migrations bundle) can't construct `ProyectoJoDbContext` through the app's own service provider when it's registered via `AddDbContextPool` (a documented EF Core tooling limitation, not specific to this app) — it falls back to booting the full `Program.cs` host, which fails outside a real ASP.NET Core runtime. Fixed with an explicit `ProyectoJo.Infrastructure/Persistence/EfCore/ProyectoJoDbContextFactory.cs` (`IDesignTimeDbContextFactory<ProyectoJoDbContext>`), reading the connection string from `ConnectionStrings__Default`. `AddDbContextPool` in `Program.cs` is unchanged — only design-time tooling needed the extra factory.
- RDS PostgreSQL rejects unencrypted connections by default — the connection string needs `SSL Mode=Require;Trust Server Certificate=true` appended, in both `RDS_CONNECTION_STRING` (GitHub secret) and `ConnectionStrings__Default` (server-side `.env`).
- `deploy/proyectojo-web.service` uses `Type=simple`, not `Type=notify` — ASP.NET Core doesn't implement the `sd_notify` readiness protocol, so `Type=notify` made systemd wait 90s for a signal that never arrives and kill an otherwise-healthy process.
- `ProyectoJo.Web.csproj` has a `<Content Include="Areas\Admin\wwwroot\**" CopyToPublishDirectory="PreserveNewest" .../>` item. `Areas/Admin/wwwroot/` (all Admin-panel CSS/JS) is merged into `WebRootFileProvider` at runtime via a `CompositeFileProvider` in `Program.cs`, but `dotnet publish` never copies that folder on its own — without the explicit `Content` item, production both crashed on startup (`PhysicalFileProvider` throwing on the missing directory) and, if it hadn't, would have served the Admin panel with no styling at all.
- Data Protection keys are ephemeral — see "Security hardening" below. Same-session symptom to know about: a form loaded before a restart/redeploy gets a `400` on submit; the fix is just reloading the page, not a bug to chase.

## Known technical debt

- **`ProyectoJo.Api` has no persistence wired up at all.** Its `Program.cs` registers `IPedidoService`/`IProductoService`/`IFinanzaService` but no repositories, so any endpoint that touches the database throws a DI resolution error at runtime. This is a deliberate, temporary state — `Api` was left out of scope of the PostgreSQL migration described in [ADR-10](./ADRs/ADR-10-Joaquin-Uriona.md). [ADR-08](./ADRs/ADR-08-Joaquin-Uriona.md) describes an older, now-superseded version of the `Api`/`Web` split (from when both read the same JSON files) — the underlying problem (no shared composition root) is unchanged, only the shape of `Web`'s own persistence changed.

## Persistence

PostgreSQL via EF Core (`Npgsql.EntityFrameworkCore.PostgreSQL` + `EFCore.NamingConventions` for snake_case table/column names) — migrated from a JSON-file-based persistence layer, see [ADR-10](./ADRs/ADR-10-Joaquin-Uriona.md) for the rationale and alternatives considered. `ProyectoJoDbContext` (in `ProyectoJo.Infrastructure/Persistence/EfCore/`) maps every `Ports/Out` interface to an `Ef*Repository`. `ProyectoJoDbContextFactory` (`IDesignTimeDbContextFactory<ProyectoJoDbContext>`, same folder) exists solely for EF Core design-time tooling (`dotnet ef`, migration bundles) — `ProyectoJo.Web/Program.cs` registers the context via `AddDbContextPool` (a deliberate performance choice, see [ADR-12](./ADRs/ADR-12-Joaquin-Uriona.md)), and EF Core's tooling cannot construct a pooled context through the app's own service provider, so it needs this separate factory to build one directly. Don't remove it assuming it's unused; it's not referenced by the running app, only by `dotnet ef` commands and CI's migration-drift check. `Pedido.Items` and `Receta.Ingredientes` are owned collections in their own tables (`pedido_items`, `receta_ingredientes`) with `ON DELETE CASCADE`. All `DateTime`/`DateTime?` properties are normalized to UTC on write via a global `ValueConverter` in `OnModelCreating` — Postgres `timestamp with time zone` columns reject any other `Kind`, and use cases mix `DateTime.Now`/`DateTime.UtcNow`, so this conversion is load-bearing, not cosmetic.

Operations that need atomicity across a read-validate-write sequence (`Pedido.CambiarEstadoAtomicoAsync`, `Insumo` stock descontar/reponer) use a DB transaction with `SELECT ... FOR UPDATE` instead of the in-process locking the old JSON repositories used.

Postgres does not guarantee row order without an explicit `ORDER BY` — an `UPDATE` can change where a row lands in a plain `SELECT`. `EfProductoRepository.ObtenerTodos()`/`ObtenerMenu()` and `EfInsumoRepository.ObtenerTodos()` order by `Id` for this reason (rows were visibly jumping to the end of the Admin Menu/Inventario lists after toggling `Activo`/`Agotado`). Apply the same `.OrderBy(x => x.Id)` to any new repository list method that feeds an Admin table.

Migrations live in `ProyectoJo.Infrastructure/Persistence/EfCore/Migrations/`. Apply them with `dotnet ef database update --project ProyectoJo.Infrastructure --startup-project ProyectoJo.Web`.

`JsonToPostgresSeeder` (`ProyectoJo.Infrastructure/Persistence/EfCore/JsonToPostgresSeeder.cs`), invoked via `dotnet run --project ProyectoJo.Web -- --seed`, was a one-time tool to import the old JSON files into Postgres. The source JSON files it reads no longer exist in the repo, so `--seed` is currently a no-op (it logs "no existe, se omite" per file and exits). `--reset` (`TRUNCATE ... RESTART IDENTITY CASCADE` across all app tables) still works and is useful to wipe the database clean, e.g. before rehearsing a demo.

## Input validation (Domain entities)

Domain entities carry `System.ComponentModel.DataAnnotations` attributes for business-rule numeric/string validation (`[Range]`, `[Required]`, `[StringLength]`), enforced through the `ModelState.IsValid` check every Admin/Operaciones controller already runs before calling a use case — no separate Create/Edit DTOs, no changes to `Ports/In` signatures. Cross-field rules a single attribute can't express (e.g. `Promocion`'s discount value depending on its discount type, or its `FechaInicio`/`FechaFin` ordering) are implemented via `IValidatableObject.Validate` on the entity instead. When an action bypasses full-entity model binding (e.g. `PromocionesController.ActualizarFecha`, which takes loose `DateTime?` parameters instead of a bound `Promocion`), the equivalent check is duplicated in the use case itself, throwing `InvalidOperationException` — same pattern `CierreCajaUseCase` already used — and the controller surfaces it via `TempData["Error"]`. The date-range comparison itself is centralized in `Promocion.RangoDeFechasEsValido(DateTime?, DateTime?)`, called from both `Promocion.Validate` and `PromocionUseCase.ActualizarFecha` — only the invocation site is duplicated (by necessity), not the comparison logic.

Entities with attributes so far: `Item` (`Precio > 0`, `Platillo`/`Categoria` required), `Finanza` (`Monto > 0`), `Insumo` (`StockActual`/`StockMinimo >= 0`), `IngredienteReceta` (`Cantidad > 0`, `CostoUnitario >= 0`), `Receta` (`Rendimiento >= 1`), `Promocion` (discount value/type consistency + date ordering), `Pedido` (`Mesa` required, max 50 chars).

`ProyectoJo.Application.Tests/Domain/EntityValidationTests.cs` covers all of the above via `Validator.TryValidateObject` (one valid case plus one case per violated rule, per entity), and `PromocionUseCaseTests` covers `ActualizarFecha`'s date-range check and `Agregar`/`Editar`'s filtering of `ItemIds` against real menu items. None of these `DataAnnotations` require an EF Core migration — confirmed via `dotnet ef migrations has-pending-model-changes` — and every numeric range/string length matches the actual Postgres column precision/length (`numeric(18,2)`/`numeric(18,4)`/`character varying(n)`) with no mismatch in either direction.

## Test coverage (`ProyectoJo.Application.Tests`)

Every `UseCases/` class has a matching test file, but file-level coverage didn't mean method-level coverage — several high-risk methods had zero tests despite that convention. A pass closed the highest-risk gaps: `PromocionUseCase.CalcularPrecioFinal`/`EstaVigente` (untested despite a real prior bug where a negative percentage discount raised the price instead of lowering it), `PedidoUseCase.CrearAsync` (the most branch-heavy method in the app — line discarding, stock adjustment, promo pricing), `CierreCajaUseCase.CerrarCaja`/`ObtenerVistaPreviaCierre` (only `AbrirCaja` had tests before), and `FinanzaUseCase.ObtenerDashboard`/`RegistrarMovimiento` (the heaviest date-grouping computation in the backend). 121 tests grew to 158.

Known gaps left open, not oversights: several read-only pass-through methods across use cases (`ObtenerTodos`/`ObtenerPorId`/etc.) still have no test, since they're one-line repository delegations with low risk. `RecetaUseCase.Editar` only tests the "not found" branch, missing a happy-path test that every sibling use case's `Editar` has. `EmpleadoUseCase.CrearAsync`/`EditarAsync` are missing a happy-path test too, unlike `AdministradorUseCase`, its closest sibling. `OpinionUseCaseTests` asserts an auto-assigned `DateTime.Now` against a 5-second tolerance instead of an injectable clock — low flakiness risk, but the only test in the suite that depends on real wall-clock time.

Integration tests against a real PostgreSQL database (e.g. via Testcontainers) still don't exist — see `ProyectoJo.Application.Tests` in Architecture above. This was deliberately deferred as a separate, larger effort (new test project, Docker dependency in CI) rather than folded into the mocked-unit-test pass described here.

## Security hardening

Rationale, alternatives considered, and consequences for the decisions below are in [ADR-11](./ADRs/ADR-11-Joaquin-Uriona.md).

- All three auth cookies (`Jo.Admin`, `Jo.Supervisor`, `Jo.Operaciones`) plus the device-pairing cookie (`Jo.DispositivoToken`) are `HttpOnly` + `Secure` + `SameSite=Strict`.
- **Data Protection keys are ephemeral** — `Program.cs` doesn't configure a persisted key ring (`PersistKeysToFileSystem` or similar), so every process restart generates a new one. This invalidates every active auth cookie and any already-rendered antiforgery token: a form loaded before a restart gets a `400` on submit (not a bug — just reload the page), and every logged-in user across all three schemes gets silently signed out at once. Documented as a known risk, not yet fixed, in the [ATAM evaluation](./ADRs/ATAM-Joaquin-Uriona.md).
- `ProyectoJo.Web/Middleware/SecurityHeadersMiddleware.cs` (registered early in `Program.cs`, before `UseStaticFiles`) sets `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, and a `Content-Security-Policy` on every response, including static files. **Adding a new external script/style/font CDN, a new embedded iframe, or any inline `<script>` block, requires updating the CSP allowlist in that file.** It currently allows `cdn.jsdelivr.net` (Bootstrap/Chart.js), `fonts.googleapis.com`/`fonts.gstatic.com` (fonts), and `frame-src https://www.google.com` (the Google Maps embed on the public Ubicación page — `default-src 'self'` blocks iframes by default, so this was added after the CSP rollout silently broke that page), with no `'unsafe-inline'` for scripts — there are no inline `<script>` tags or `onclick`/`onsubmit`/`onchange` attributes anywhere in the project; keep it that way or the CSP will need loosening.
- `PromocionesController.SubirImagen` validates the real file signature (magic bytes) of JPEG/PNG/GIF/WEBP, not just the extension, before saving to `wwwroot/uploads/promociones/`. The WEBP check reads its 12-byte header in a loop rather than a single `ReadAsync` call, since `Stream.ReadAsync` isn't guaranteed to fill the buffer in one call even when more data is available.
- The five entity-creation actions that bind the full Domain entity straight from the POST body (`MenuController.Agregar`, `FinanzasController.Registrar`, `InsumosController.Crear`, `PromocionesController.Agregar`, `RecetarioController.Agregar`) call `.DescartarId()` before invoking the use case, discarding any client-supplied primary key. `DescartarId()` is an extension method on `IEntidadConId` (`ProyectoJo.Domain/Entities/IEntidadConId.cs`), implemented by `Item`, `Finanza`, `Insumo`, `Promocion`, and `Receta` — the five entities bound this way — so the reset is a single reusable operation instead of five independent `entity.Id = 0` copies.
- Verified clean as of the last security audit: CSRF (global `AutoValidateAntiforgeryTokenAttribute` + header-based token for `[FromBody]` endpoints), SQL injection (raw SQL is either parameterized `FromSqlInterpolated` or hardcoded table names with zero user input), XSS (`Html.Raw` usages only wrap `System.Text.Json`-serialized data islands), password hashing (PBKDF2 + `CryptographicOperations.FixedTimeEquals`), authorization (`RequiereAreaAttribute` consistent across all Admin controllers; the SignalR hub requires auth and validates group/role match).

## Screenshots (docs/screenshot/)

Used by README.md's "Capturas de pantalla" section. Three subfolders
(`admin/`, `operation/`, `public/`) matching the README's module grouping.
Filenames may contain spaces — referenced in README.md via raw HTML `<img>`
tags with `%20`-encoded `src` paths, not Markdown `![]()` syntax.

## Front-end inline-style/script cleanup (in progress)

Ongoing effort to pull `style=""` inline CSS and inline JS event handlers (`onclick`/`onsubmit`/`onchange`) out of `.cshtml` views into page-scoped CSS files and small delegated-event JS files, one page at a time, each verified by running `ProyectoJo.Web` locally and checking in-browser before moving to the next.

**Conventions established so far:**
- Each public view gets its own CSS file under `wwwroot/css/<page>/`, matching the existing per-page convention. If a view has none yet (e.g. `Ubicacion.cshtml` had none), create one and load it via `@section Styles`.
- `animation-delay` inline styles on `.fade-in-up` / `.card-animate` elements are left inline on purpose — there's an explicit comment in `wwwroot/css/layout/layout.css` documenting this as intentional.
- Genuinely dynamic per-row inline styles (`style="width:@porcentaje%"` progress bars, `animation-delay: @((delay % 4) * 0.08)s`) are left alone — they can't be static CSS classes.
- Admin `onsubmit="return confirm(...)"` / `onchange="this.form.submit()"` were replaced with `data-confirm-delete` / `data-autosubmit` attributes, handled by `wwwroot/js/admin-confirm-delete.js` / `admin-autosubmit.js`, both wired once in `Areas/Admin/Views/Shared/_Layout.cshtml`.
- Inline `<script>` blocks in `MapaCalor/Index.cshtml` and the Admin `_Layout.cshtml` were extracted to `wwwroot/js/mapa-calor.js` and `admin-layout.js`; server data is passed to JS via `<script type="application/json">` "data islands" (same pattern `Finanzas/Dashboard.cshtml` already used), not interpolated directly into the script.
- Some `divider-gold` / `proximamente-placeholder` usages relied entirely on inline styles because the shared class had no matching CSS actually loaded on that page (e.g. `Historia.cshtml`, `Ubicacion.cshtml`) — check this before assuming the shared class alone is enough.

**Done:** `Historia.cshtml`, `Home/Privacy.cshtml`, `Menu/Detalle.cshtml`, `Ubicacion/Ubicacion.cshtml`, `Nosotros/Index.cshtml`, `Menu/Index.cshtml` (this last one also had its "Ver Experiencia" hover-underline effect removed entirely after extensive rendering inconsistencies that couldn't be pinned down — it's now static text with a color-only hover, no underline).

**Remaining public pages:** `Home/Index.cshtml` (18 inline styles), `Views/Shared/_Layout.cshtml` (3).

**Remaining Admin/Operaciones views (lower priority, not started):** `Insumos/Index`, `Insumos/Editar`, `Menu/Index` (Admin), `Menu/Editar`, `Menu/Agregar`, `Recetario/Index`, `Promociones/Editar`, `Promociones/Agregar`, `Promociones/_TablaPromociones`, `Promociones/_FilaPromocion`, `Opiniones/Index`, `Operadores/Index`, `CierreCaja/Cerrar`, `Finanzas/Registrar`, `Operaciones/Auth/Login`, `Operaciones/Auth/LoginSupervisor`, `Operaciones/Auth/Emparejar`, `Operaciones/Recepcion/Index`.

**Also noted, not fixed (out of scope):** `Menu/Index.cshtml`'s `menu.css` has several classes (`.item-module`, `.item-title`, `.menu-title`, `.btn-add-platillo`, `.btn-ver-experiencia`, etc.) that don't match any markup in the current view — looks like dead CSS from an older layout version.

## Operaciones mobile/visual redesign (in progress)

- **`.btn-primary` gotcha:** the shared class (`finanzas-shared.css`) carries `margin-left: 14px`, meant for a button sitting next to a header/filter-bar title — not for a `.form-actions`/flex-column container that already spaces its children with `gap`. Dropping `.btn-primary` into one of those without an explicit `.form-actions .btn-primary { margin-left: 0; }` (or equivalent) override visibly misaligns it from the fields above. Already fixed this way in Promociones, Finanzas `Registrar`/`Editar`, and Usuarios' "Crear administrador"/"Crear operador" buttons — apply the same override to any new form that reuses `.btn-primary`.
- **Recepción mobile flow (`Areas/Operaciones/Views/Recepcion/Index.cshtml`, `recepcion.js`, `recepcion.css`, `≤960px`):** reworked from one long scroll (menu + cart + pedidos-activos/pagados list all stacked) into a kiosk-style flow. A "Pedir"/"Pedidos" switcher splits order-taking from the pedidos list; "Pedir" is itself two steps — big category tiles, then the item grid for that category, with a back button. Typing in the search box while on the category step auto-advances to the item step. Desktop (`>960px`) layout is untouched; the mobile-only behavior is driven by `body.paso-menu`/`body.vista-pedidos` classes toggled from JS state (`pasoPedir`, `vistaPrincipal` in `recepcion.js`) that the desktop CSS simply ignores.
- **Category tile colors:** alternate only `--tc-mustard`/`--tc-carbon`, not `--tc-teal`/`--tc-brick` — those two already carry fixed meaning elsewhere on the same screen (`teal` = order ready/success, `brick` = validation error), so reusing them decoratively for categories would clash with that convention. Carbon/mustard is also the pairing the Admin panel's own identity is built on (see the favicon rationale in "Project overview" above).
- **No emoji icons in Operaciones:** decorative emoji (header icons, search glass, Pedir/Pedidos switch labels, connection-status and order-validation messages) were removed from both Recepción and Cocina to match the project's plain, text-based visual style — there are no logos/icons anywhere else in the app. Plain `✓`/`✕` glyphs used as functional status/close markers (e.g. `pedido-card__pagado`) were kept; they're an existing convention, not decorative pictographs.
- **Cocina (`cocina.css`) palette + bugfix:** previously used a one-off hex palette (`#ea580c`, `#f59e0b`, `#3b82f6`) and `system-ui` with no relation to Admin/Recepción. Rewritten to share the same `--tc-*` tokens and fonts (added the missing Google Fonts `<link>` to `Cocina/Index.cshtml`, which never loaded them). While doing this, found and fixed a real bug: `.top-bar__dispositivo` was defined twice — once malformed, nested inside the `.top-bar` rule (and referencing `--tc-*` variables that weren't even declared in this file, which had no `:root` block), once again at the bottom of the file as a normal rule. The bottom one was silently winning; the nested one never rendered correctly.
- **Usuarios y Accesos:** removed the `TempData["ExitoAdmin"]`/`TempData["ExitoOperador"]` success banners ("Administrador/Operador creado correctamente.") from `UsuariosController` and `Views/Usuarios/Index.cshtml` — the new row appearing in the table already confirms creation. Error banners (`ErrorAdmin`/`ErrorOperador`) are untouched.
- **Queued next (not started):** a further visual pass on Cocina itself. Candidates raised so far, none actioned yet: text/cards are sized for phone-in-hand reading rather than a kitchen screen viewed from a distance; no visual urgency escalation as a ticket ages; the "En preparación / Listos" column title implies a status (`en preparación`) that doesn't actually exist in the data model (only `Pendiente`/`Preparado`); item modifiers (`sin cebolla`, etc.) render as plain text with no visual emphasis despite being operationally important; `Preparado` tickets stay visible indefinitely until Recepción marks them paid, cluttering screen space no longer actionable by kitchen staff.
