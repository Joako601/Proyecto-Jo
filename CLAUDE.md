# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Git workflow — never push, hand off the commit instead

Claude must **never** run `git add`, `git commit`, or `git push` in this repository. The user pushes everything by hand from a separate terminal (add → commit → push → branch). This applies regardless of how routine or small the change looks.

At the end of a set of changes, Claude must:

1. Explain what was changed and why it's correct/safe — in plain terms the user can verify before pushing.
2. Provide the exact commit message to use, formatted per [Conventional Commits v1.0.0](https://www.conventionalcommits.org/en/v1.0.0), so the user can paste it straight into `git commit -m "..."`.

Do not run `git commit` "on the user's behalf" even if asked to prepare everything — staging/committing/pushing is the one step that always stays manual here.

## Project overview

Proyecto Jo' is a financial/administrative management system for small businesses, built with **ASP.NET Core (.NET 10)** using **Hexagonal Architecture (Ports & Adapters)**. Domain logic is isolated from frameworks and infrastructure across five independent projects with a single dependency direction: adapters depend on the domain, the domain never depends on adapters. Full history of architectural decisions lives in `/ADRs`; a C4-model overview (context/containers/components) is in `docs/Arquitectura-C4.md`.

## Commands

```bash
# Restore the whole solution
dotnet restore

# Run the web app (admin panel + public site + Cocina/Recepción operational flow)
dotnet run --project ProyectoJo.Web
# → https://localhost:7287 / http://localhost:5207

# Run the REST API (Swagger-documented, currently not consumed by anything — see Known technical debt)
dotnet run --project ProyectoJo.Api
# → https://localhost:63639 / http://localhost:63640 (Swagger UI at the API root)

# Run the full test suite
dotnet test ProyectoJo.Application.Tests

# Run a single test class or method
dotnet test ProyectoJo.Application.Tests --filter "FullyQualifiedName~PedidoUseCaseTests"
dotnet test ProyectoJo.Application.Tests --filter "FullyQualifiedName~PedidoUseCaseTests.MethodName"
```

The admin panel requires the `Auth__AdminUser` and `Auth__AdminPasswordHash` environment variables (or .NET User Secrets) — see `ProyectoJo.Infrastructure/Auth/EnvAuthService`. Never hardcode these or commit real values in `launchSettings.json`.

CI (`.github/workflows/ci.yml`) runs `dotnet restore/build/test` on every push to any branch and on PRs targeting the `deuda-tecnica` branch (not `main`).

## Architecture

Five projects, dependency direction flows inward toward the domain:

- **`ProyectoJo.Domain`** — core entities (`Item`, `Finanza`, `Pedido`, `ItemPedido`, `Promocion`, ...), zero external dependencies.
- **`ProyectoJo.Application`** — use cases and ports. `Ports/In` defines service interfaces (`IProductoService`, `IFinanzaService`, `IPedidoService`, `IPromocionService`, etc.) that adapters call into; `Ports/Out` defines repository/notifier interfaces (`IProductoRepository`, `IPedidoNotificador`, etc.) that infrastructure implements; `UseCases/` holds the actual business logic; `DTOs/` holds cross-boundary data shapes.
- **`ProyectoJo.Infrastructure`** — output adapters. `Persistence/` has one `Json*Repository` per entity, all writing atomically (write to `.tmp`, then move) and using a static `SemaphoreSlim` per repository type for concurrency safety. `Auth/` has `EnvAuthService` (env-var-based credential check).
- **`ProyectoJo.Web`** — primary input adapter (ASP.NET Core MVC). Composes the whole dependency graph itself in `Program.cs` (see below). Contains three cookie-auth schemes, SignalR for real-time order flow, rate limiting on login endpoints, and its own JSON persistence files.
- **`ProyectoJo.Api`** — secondary input adapter (ASP.NET Core Web API + Swagger), documented but **not actively used** by the running system; reserved for future external clients (mobile apps, WhatsApp, Postman). It composes its own (incomplete) dependency graph independently of `Web` — see Known technical debt.
- **`ProyectoJo.Application.Tests`** — xUnit + Moq. `UseCases/` has unit tests mocking the ports; `Infrastructure/` has integration tests instantiating real `Json*Repository` implementations against temp files to exercise atomic-write/concurrency behavior.

### `ProyectoJo.Web` structure

- `Controllers/` — public-facing pages (Home, Menu, Historia, Nosotros, Ubicación).
- `Areas/Admin/` — administrative panel (Finanzas, Productos/Menu, Promociones, Inventario, Insumos, Recetario, Mapa de Calor, Auditoría, Dispositivos, Administradores, Operadores, Opiniones, Cierre de Caja). Access is gated by `RequiereAreaAttribute` (`Authorization/RequiereAreaAttribute.cs`): a user must hold the `Administrador` role and either the `General` area claim or the specific area claim the attribute names; `SuperAdmin` bypasses area checks entirely.
- `Areas/Operaciones/` — Cocina (kitchen) and Recepción (front desk) operational flow, gated by supervisor PIN before every employee login (`Auth/`).
- `Hubs/PedidosHub` + `Realtime/SignalRPedidoNotificador` — real-time order status push implementing `IPedidoNotificador`, consumed by Cocina/Recepción views instead of polling.
- `Middleware/JsonExceptionMiddleware` — converts unhandled exceptions to JSON responses.
- `Persistencia/` — the actual JSON data files (`menu.json`, `finanzas.json`, `promociones.json`, `pedidos.json`, `empleados.json`, `dispositivos.json`, `cierres-caja.json`, `auditoria.json`, `supervisor-clave.json`, `recetas.json`, `opiniones.json`, `insumos.json`, `administradores.json`).

### Authentication schemes (all cookie-based, defined in `ProyectoJo.Web/Program.cs`)

Three independent, non-overlapping cookie schemes — compromising one grants no access to the others:

| Scheme | Cookie | Login path | Expiry |
|---|---|---|---|
| `JoCookieAuth` | `Jo.Admin` | `/Admin/Login` | 45 min sliding |
| `SupervisorAuth` | `Jo.Supervisor` | `/Operaciones/Auth/LoginSupervisor` | 15 min, not sliding |
| `OperacionesCookieAuth` | `Jo.Operaciones` | `/Operaciones/Auth/Login` | 12 h sliding |

Login endpoints for all three are rate-limited (5–8 requests/min per IP via `AddRateLimiter`); a rejected request redirects to the matching login page with `?bloqueado=true`.

### Dependency composition is manual and duplicated per entry point

`ProyectoJo.Web/Program.cs` and `ProyectoJo.Api/Program.cs` each wire up their own DI graph independently — there is no shared composition root. When adding a new use case/repository pair, register it in **both** `Program.cs` files if it needs to be reachable from both adapters (currently the API only wires up `Pedido`, `Producto`, and `Finanza`). See "Known technical debt" below before assuming the API's graph is complete.

## Known technical debt

Documented explicitly in [ADR-08](./ADRs/ADR-08-Joaquin-Uriona.md) rather than left implicit in code:

- `ProyectoJo.Api/Program.cs` builds persistence paths by hand (relative `Path.Combine` into `../ProyectoJo.Web/Persistencia/...`) instead of configuration.
- `JsonPedidoRepository` uses a static per-process `SemaphoreSlim`, not shared between `Web` and `Api` processes — concurrent writes from both are not mutually safe.
- `ProyectoJo.Api/Program.cs` never registers `IPedidoNotificador` or `IPromocionService`, so `PedidosController` fails at runtime resolving `PedidoUseCase`.

Root cause for all three: `Web` and `Api` compose their dependency graphs separately. The proposed fix (per ADR-08) is a shared extension method (`AddProyectoJoServices`) both entry points call into.

## Persistence

Everything currently persists to JSON files with atomic writes (temp file + move), one repository class per entity in `ProyectoJo.Infrastructure/Persistence`. A move to SQL + Entity Framework is planned but not yet started — don't assume a database exists.
