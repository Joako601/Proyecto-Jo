# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Git workflow — never push, hand off the commit instead

Claude must **never** run `git add`, `git commit`, or `git push` in this repository. The user pushes everything by hand from a separate terminal (add → commit → push → branch). This applies regardless of how routine or small the change looks.

At the end of a set of changes, Claude must:

1. Explain what was changed and why it's correct/safe — in plain terms the user can verify before pushing.
2. Provide the exact commit message to use, formatted per [Conventional Commits v1.0.0](https://www.conventionalcommits.org/en/v1.0.0), so the user can paste it straight into `git commit -m "..."`.

Do not run `git commit` "on the user's behalf" even if asked to prepare everything — staging/committing/pushing is the one step that always stays manual here.

## No code comments

Do not add comments to code in this repository (C#, Razor, JS, CSS, etc.), including XML doc comments — whether writing new code or editing existing files. Write self-explanatory code (clear names, small methods) instead of explaining it with comments. Don't remove pre-existing comments in code you aren't otherwise touching.

## Summaries: only report what was actually applied

When explaining a set of changes (end-of-task summary, commit message, etc.), describe only the real, applied changes to classes/files in that commit. Do not add ADRs, do not describe changes that weren't actually implemented, and do not present hypothetical/future/"could also do X" work as if it were part of what happened. Suggestions for follow-up work belong in a separate, clearly-labeled "next steps" note — never blended into the list of what changed.

## Project overview

Proyecto Jo' is a financial/administrative management system for small businesses, built with **ASP.NET Core (.NET 10)** using **Hexagonal Architecture (Ports & Adapters)**. Domain logic is isolated from frameworks and infrastructure across five independent projects with a single dependency direction: adapters depend on the domain, the domain never depends on adapters. Full history of architectural decisions lives in `/ADRs`; a C4-model overview (context/containers/components) is in `docs/Arquitectura-C4.md`.

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

CI (`.github/workflows/ci.yml`) runs on every push to any branch, on PRs targeting the `deuda-tecnica` branch (not `main`), and manually via `workflow_dispatch`: restore, a check for pending EF Core migrations (`dotnet ef migrations has-pending-model-changes`), a NuGet vulnerable-package audit (`dotnet list package --vulnerable`), build, then test. A second workflow, `.github/workflows/docs.yml`, runs on any push/PR touching `**/*.md` and fails if `lychee` finds a broken link (internal or external) in the repo's Markdown files.

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
- `Areas/Admin/` — administrative panel (Finanzas, Productos/Menu, Promociones, Inventario, Insumos, Recetario, Mapa de Calor, Auditoría, Dispositivos, Administradores, Operadores, Opiniones, Cierre de Caja). Access is gated by `RequiereAreaAttribute` (`Authorization/RequiereAreaAttribute.cs`): a user must hold the `Administrador` role and either the `General` area claim or the specific area claim the attribute names; `SuperAdmin` bypasses area checks entirely.
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

`SupervisorAuth`'s PIN is **not** a single global secret — each `Administrador` has an optional `ClaveSupervisorHash` (PBKDF2, set/changed from Admin → Administradores when creating or editing an admin). `POST /Operaciones/Auth/LoginSupervisor` takes just a PIN (no username) and `SupervisorAuthUseCase.ValidarClaveAsync` accepts it if it matches **any** active administrator's key. There is no dedicated "change supervisor key" screen anymore — Admin → Dispositivos only lists paired devices.

### Dependency composition is manual and duplicated per entry point

`ProyectoJo.Web/Program.cs` and `ProyectoJo.Api/Program.cs` each wire up their own DI graph independently — there is no shared composition root. `Web` registers `AddDbContext<ProyectoJoDbContext>` plus every `Ef*Repository`; `Api` currently registers none. When adding a new use case/repository pair, register it in `Web`'s `Program.cs` — `Api` is out of scope for now (see Known technical debt).

## Known technical debt

- **`ProyectoJo.Api` has no persistence wired up at all.** Its `Program.cs` registers `IPedidoService`/`IProductoService`/`IFinanzaService` but no repositories, so any endpoint that touches the database throws a DI resolution error at runtime. This is a deliberate, temporary state — `Api` is being ignored while `Web`'s PostgreSQL migration is the priority. [ADR-08](./ADRs/ADR-08-Joaquin-Uriona.md) describes an older, now-superseded version of the `Api`/`Web` split (from when both read the same JSON files); a new ADR covering the current state is expected later.

## Persistence

PostgreSQL via EF Core (`Npgsql.EntityFrameworkCore.PostgreSQL` + `EFCore.NamingConventions` for snake_case table/column names). `ProyectoJoDbContext` (in `ProyectoJo.Infrastructure/Persistence/EfCore/`) maps every `Ports/Out` interface to an `Ef*Repository`. `Pedido.Items` and `Receta.Ingredientes` are owned collections in their own tables (`pedido_items`, `receta_ingredientes`) with `ON DELETE CASCADE`. All `DateTime`/`DateTime?` properties are normalized to UTC on write via a global `ValueConverter` in `OnModelCreating` — Postgres `timestamp with time zone` columns reject any other `Kind`, and use cases mix `DateTime.Now`/`DateTime.UtcNow`, so this conversion is load-bearing, not cosmetic.

Operations that need atomicity across a read-validate-write sequence (`Pedido.CambiarEstadoAtomicoAsync`, `Insumo` stock descontar/reponer) use a DB transaction with `SELECT ... FOR UPDATE` instead of the in-process locking the old JSON repositories used.

Postgres does not guarantee row order without an explicit `ORDER BY` — an `UPDATE` can change where a row lands in a plain `SELECT`. `EfProductoRepository.ObtenerTodos()`/`ObtenerMenu()` and `EfInsumoRepository.ObtenerTodos()` order by `Id` for this reason (rows were visibly jumping to the end of the Admin Menu/Inventario lists after toggling `Activo`/`Agotado`). Apply the same `.OrderBy(x => x.Id)` to any new repository list method that feeds an Admin table.

Migrations live in `ProyectoJo.Infrastructure/Persistence/EfCore/Migrations/`. Apply them with `dotnet ef database update --project ProyectoJo.Infrastructure --startup-project ProyectoJo.Web`.

`JsonToPostgresSeeder` (`ProyectoJo.Infrastructure/Persistence/EfCore/JsonToPostgresSeeder.cs`), invoked via `dotnet run --project ProyectoJo.Web -- --seed`, was a one-time tool to import the old JSON files into Postgres. The source JSON files it reads no longer exist in the repo, so `--seed` is currently a no-op (it logs "no existe, se omite" per file and exits). `--reset` (`TRUNCATE ... RESTART IDENTITY CASCADE` across all app tables) still works and is useful to wipe the database clean, e.g. before rehearsing a demo.

## Input validation (Domain entities)

Domain entities carry `System.ComponentModel.DataAnnotations` attributes for business-rule numeric/string validation (`[Range]`, `[Required]`, `[StringLength]`), enforced through the `ModelState.IsValid` check every Admin/Operaciones controller already runs before calling a use case — no separate Create/Edit DTOs, no changes to `Ports/In` signatures. Cross-field rules a single attribute can't express (e.g. `Promocion`'s discount value depending on its discount type, or its `FechaInicio`/`FechaFin` ordering) are implemented via `IValidatableObject.Validate` on the entity instead. When an action bypasses full-entity model binding (e.g. `PromocionesController.ActualizarFecha`, which takes loose `DateTime?` parameters instead of a bound `Promocion`), the equivalent check is duplicated in the use case itself, throwing `InvalidOperationException` — same pattern `CierreCajaUseCase` already used — and the controller surfaces it via `TempData["Error"]`. The date-range comparison itself is centralized in `Promocion.RangoDeFechasEsValido(DateTime?, DateTime?)`, called from both `Promocion.Validate` and `PromocionUseCase.ActualizarFecha` — only the invocation site is duplicated (by necessity), not the comparison logic.

Entities with attributes so far: `Item` (`Precio > 0`, `Platillo`/`Categoria` required), `Finanza` (`Monto > 0`), `Insumo` (`StockActual`/`StockMinimo >= 0`), `IngredienteReceta` (`Cantidad > 0`, `CostoUnitario >= 0`), `Receta` (`Rendimiento >= 1`), `Promocion` (discount value/type consistency + date ordering), `Pedido` (`Mesa` required, max 50 chars).

`ProyectoJo.Application.Tests/Domain/EntityValidationTests.cs` covers all of the above via `Validator.TryValidateObject` (one valid case plus one case per violated rule, per entity), and `PromocionUseCaseTests` covers `ActualizarFecha`'s date-range check and `Agregar`/`Editar`'s filtering of `ItemIds` against real menu items. None of these `DataAnnotations` require an EF Core migration — confirmed via `dotnet ef migrations has-pending-model-changes` — and every numeric range/string length matches the actual Postgres column precision/length (`numeric(18,2)`/`numeric(18,4)`/`character varying(n)`) with no mismatch in either direction.

## Security hardening

- All three auth cookies (`Jo.Admin`, `Jo.Supervisor`, `Jo.Operaciones`) plus the device-pairing cookie (`Jo.DispositivoToken`) are `HttpOnly` + `Secure` + `SameSite=Strict`.
- `ProyectoJo.Web/Middleware/SecurityHeadersMiddleware.cs` (registered early in `Program.cs`, before `UseStaticFiles`) sets `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, and a `Content-Security-Policy` on every response, including static files. **Adding a new external script/style/font CDN, a new embedded iframe, or any inline `<script>` block, requires updating the CSP allowlist in that file.** It currently allows `cdn.jsdelivr.net` (Bootstrap/Chart.js), `fonts.googleapis.com`/`fonts.gstatic.com` (fonts), and `frame-src https://www.google.com` (the Google Maps embed on the public Ubicación page — `default-src 'self'` blocks iframes by default, so this was added after the CSP rollout silently broke that page), with no `'unsafe-inline'` for scripts — there are no inline `<script>` tags or `onclick`/`onsubmit`/`onchange` attributes anywhere in the project; keep it that way or the CSP will need loosening.
- `PromocionesController.SubirImagen` validates the real file signature (magic bytes) of JPEG/PNG/GIF/WEBP, not just the extension, before saving to `wwwroot/uploads/promociones/`. The WEBP check reads its 12-byte header in a loop rather than a single `ReadAsync` call, since `Stream.ReadAsync` isn't guaranteed to fill the buffer in one call even when more data is available.
- The five entity-creation actions that bind the full Domain entity straight from the POST body (`MenuController.Agregar`, `FinanzasController.Registrar`, `InsumosController.Crear`, `PromocionesController.Agregar`, `RecetarioController.Agregar`) call `.DescartarId()` before invoking the use case, discarding any client-supplied primary key. `DescartarId()` is an extension method on `IEntidadConId` (`ProyectoJo.Domain/Entities/IEntidadConId.cs`), implemented by `Item`, `Finanza`, `Insumo`, `Promocion`, and `Receta` — the five entities bound this way — so the reset is a single reusable operation instead of five independent `entity.Id = 0` copies.
- Verified clean as of the last security audit: CSRF (global `AutoValidateAntiforgeryTokenAttribute` + header-based token for `[FromBody]` endpoints), SQL injection (raw SQL is either parameterized `FromSqlInterpolated` or hardcoded table names with zero user input), XSS (`Html.Raw` usages only wrap `System.Text.Json`-serialized data islands), password hashing (PBKDF2 + `CryptographicOperations.FixedTimeEquals`), authorization (`RequiereAreaAttribute` consistent across all Admin controllers; the SignalR hub requires auth and validates group/role match).

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
