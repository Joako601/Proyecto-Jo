# Proyecto Jo'

> Sistema de gestión financiera y administrativa para dueños de pequeños y medianos
> negocios, construido con **ASP.NET Core** bajo **Arquitectura Hexagonal (Ports & Adapters)**.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![Arquitectura](https://img.shields.io/badge/Arquitectura-Hexagonal-blue)
![API](https://img.shields.io/badge/API-REST%20%2B%20Swagger-85EA2D)
![Tests](https://img.shields.io/badge/Tests-26%20passing-brightgreen)

---

## Descripción

Proyecto Jo' nació como una aplicación MVC monolítica y migró progresivamente hacia
una **Arquitectura Hexagonal**, separando el dominio de negocio de los frameworks
y la infraestructura, el sistema se compone de cinco proyectos independientes con
fronteras explícitas y una dirección de dependencia única: los adaptadores dependen
del dominio, el dominio nunca depende de ellos.

El sistema expone dos adaptadores de entrada simultáneos:

- **`ProyectoJo.Web`** — panel administrativo y vitrina pública (ASP.NET Core MVC),
  con comunicación en tiempo real vía **SignalR** para las pantallas de Cocina y Recepción
- **`ProyectoJo.Api`** — API REST documentada con Swagger, actualmente desarrollada
  pero sin uso activo en el sistema, reservada para una futura integración con clientes
  externos (apps móviles, WhatsApp, Postman)

El historial completo de decisiones de diseño está documentado en
[`/ADRs`](./ADRs).

---

## Arquitectura

```mermaid
flowchart TD

    subgraph DOMAIN ["ProyectoJo.Domain"]
        ENT["Entities
        Item, Finanza, Pedido, Promocion"]
    end

    subgraph APPLICATION ["ProyectoJo.Application"]
        direction TB
        PIN["Ports/In
        IProductoService, IFinanzaService,
        IPedidoService, IPromocionService"]
        UC["UseCases"]
        POUT["Ports/Out
        IProductoRepository, IFinanzaRepository,
        IPedidoRepository, IPromocionRepository,
        IPedidoNotificador"]
        PIN --> UC --> POUT
    end

    subgraph WEB ["ProyectoJo.Web"]
        WC["Controllers MVC (Razor Views)"]
        HUB["PedidosHub (SignalR)"]
        NOTIF["SignalRPedidoNotificador"]
        MW["JsonExceptionMiddleware"]
    end

    subgraph API ["ProyectoJo.Api"]
        AC["Controllers REST (Swagger)"]
    end

    subgraph INFRA ["ProyectoJo.Infrastructure"]
        PERS["Persistence — JSON (escritura atómica)"]
        AUTH["Auth — IAuthService"]
    end

    subgraph TESTS ["ProyectoJo.Application.Tests"]
        UT["UseCases/ — Tests unitarios (Moq)"]
        IT["Infrastructure/ — Tests de integración (archivo temporal)"]
    end

    WC -->|invoca| PIN
    AC -->|invoca| PIN
    UC -->|usa| ENT
    POUT -->|implementado por| PERS
    POUT -->|implementado por| AUTH
    POUT -->|implementado por| NOTIF
    NOTIF -->|push| HUB
    UT -->|mock de| POUT
    IT -->|instancia real de| PERS
```

Más detalle en las vistas arquitectónicas de cada ADR.

## Módulos implementados

| Módulo | Descripción |
|---|---|
| Finanzas | CRUD de movimientos, dashboard con gráficas, filtros por mes y año |
| Menú | CRUD de platillos con búsqueda y filtros por categoría |
| Inventario | Toggle activo/agotado por platillo |
| Promociones | Banners y descuentos, vista pública en menú |
| Mapa de Calor | Ventas por semana y por mes con navegación por período |
| Cocina / Recepción | Flujo operacional de pedidos con autenticación por rol y PIN, sincronización en tiempo real vía SignalR |

>  En desarrollo activo — Arquitectura Hexagonal implementada, nuevos módulos en camino

---

## Estructura del repositorio

```text
ProyectoJo/
├── ProyectoJo.Domain/            # Núcleo del negocio — sin dependencias externas
│   └── Entities/                 # Item, Finanza, Pedido, ItemPedido, Promocion
│
├── ProyectoJo.Application/       # Casos de uso y puertos
│   ├── Ports/In/                 # IProductoService, IFinanzaService, IPedidoService, IPromocionService
│   ├── Ports/Out/                # IProductoRepository, IFinanzaRepository, IPedidoRepository,
│   │                             # IPromocionRepository, IPedidoNotificador
│   ├── UseCases/                 # Implementación de la lógica de negocio
│   └── DTOs/                     # ResumenFinanciero, ResumenDashboard, ResultadoCrearPedido
│
├── ProyectoJo.Infrastructure/    # Adaptadores de salida
│   ├── Persistence/              # Repositorios JSON con escritura atómica (.tmp + Move)
│   └── Auth/                     # EnvAuthService
│
├── ProyectoJo.Web/               # Adaptador de entrada — ASP.NET Core MVC
│   ├── Controllers/              # Home, Menu, Historia, Nosotros, Ubicación
│   ├── Areas/Admin/              # Panel administrativo (Finanzas, Productos, Promociones)
│   ├── Areas/Operaciones/        # Cocina, Recepción, Auth por PIN
│   ├── Hubs/                     # PedidosHub — canal SignalR en tiempo real
│   ├── Realtime/                 # SignalRPedidoNotificador
│   ├── Middleware/               # JsonExceptionMiddleware
│   ├── Views/
│   ├── Persistencia/             # menu.json, finanzas.json, promociones.json,
│   │                             # pedidos.json, cierres-caja.json, auditoria.json
│   └── ADRs/                     # Historial de decisiones arquitectónicas
│
├── ProyectoJo.Api/               # Adaptador de entrada — ASP.NET Core Web API
│   ├── Controllers/              # PedidosController
│   └── Program.cs                # Composición de dependencias + Swagger
│
└── ProyectoJo.Application.Tests/ # Proyecto de tests — xUnit + Moq
    ├── UseCases/                 # Tests unitarios con mocks (ProductoUseCase,
    │                             # FinanzaUseCase, PromocionUseCase,
    │                             # CierreCajaUseCase, PedidoUseCase)
    └── Infrastructure/           # Tests de integración con repos reales contra
                                  # archivos temporales (concurrencia y escritura atómica)
```
---

## Documentación de Arquitectura (Modelo C4)

La arquitectura del sistema está documentada en tres niveles de detalle bajo el
[Modelo C4](https://c4model.com/) — Contexto, Contenedores y Componentes -

**[Ver documentación completa → `/docs/Arquitectura-C4.md`](./docs/Arquitectura-C4.md)**

| Nivel | Contenido | Audiencia |
|---|---|---|
| 1 — Contexto | Qué es el sistema y quién interactúa con él | General |
| 2 — Contenedores | Procesos desplegables y cómo se comunican | Equipo técnico |
| 3 — Componentes | Clases e interfaces dentro de `ProyectoJo.Web` | Equipo de desarrollo |

> El historial de decisiones que sustenta esta arquitectura está documentado
> por separado en [`/ADRs`](./ADRs).

---

## Tecnologías

| Categoría | Tecnología |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| Patrón arquitectónico | Arquitectura Hexagonal (Ports & Adapters) |
| Web (adaptador de entrada) | ASP.NET Core MVC, Razor Views |
| API (adaptador de entrada) | ASP.NET Core Web API |
| Tiempo real | SignalR |
| Documentación de API | Swagger / OpenAPI (Swashbuckle.AspNetCore) |
| Persistencia actual | Archivos JSON con escritura atómica (planeado: SQL + Entity Framework) |
| Autenticación | Cookie auth  |
| Logging | Serilog (consola + archivo rotativo diario) |
| Tests | xUnit 2.9.2 + Moq 4.20.72 — 26 tests (unitarios + integración con concurrencia real) |
| Despliegue objetivo | AWS EC2 |

---

## Requisitos previos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) o superior
- Un editor compatible (Visual Studio, VS Code con C# Dev Kit, Rider)

---

## Cómo ejecutar el proyecto

El sistema tiene **dos puntos de entrada independientes**: el sitio web (`ProyectoJo.Web`)
y la API (`ProyectoJo.Api`). Cada uno se ejecuta por separado.

```bash
# Restaurar dependencias de toda la solución
dotnet restore

# Levantar el sitio web (panel admin + vitrina pública + Cocina/Recepción)
dotnet run --project ProyectoJo.Web
# → https://localhost:7287  /  http://localhost:5207

# Levantar la API REST (actualmente sin uso activo en el sistema)
dotnet run --project ProyectoJo.Api
# → https://localhost:63639  /  http://localhost:63640

# Correr los tests
dotnet test ProyectoJo.Application.Tests
# → 26 tests, 0 errores
```

### Credenciales del panel administrativo

El panel admin requiere las variables de entorno `Auth__AdminUser` y
`Auth__AdminPasswordHash` (ver `Infrastructure/Auth/EnvAuthService`).
Configúralas en tu entorno local o en los *User Secrets* de .NET —
**nunca las dejes hardcodeadas ni las subas al repositorio** en `launchSettings.json`
con su valor real.

---

## Documentación interactiva (Swagger)

Con `ProyectoJo.Api` corriendo, Swagger UI queda disponible directamente en la raíz
del proyecto:

```
http://localhost:63640/
```

Desde ahí se pueden explorar y probar todos los endpoints sin necesidad de Postman.

---

## Endpoints disponibles

> Esta tabla se mantiene actualizada manualmente como referencia rápida. La fuente
> de verdad siempre es Swagger, generado directamente desde el código.

### Pedidos — `/api/Pedidos`

| Método | Ruta | Tag | Descripción |
|---|---|---|---|
| GET | `/api/Pedidos/recepcion` | Recepción | Lista pedidos para la vista de recepción |
| GET | `/api/Pedidos/{id}` | Recepción | Obtiene un pedido por id |
| POST | `/api/Pedidos` | Recepción | Crea un nuevo pedido |
| PATCH | `/api/Pedidos/{id}/pagar` | Recepción | Marca un pedido como pagado |
| GET | `/api/Pedidos/cocina` | Cocina | Lista pedidos pendientes/preparados para cocina |
| PATCH | `/api/Pedidos/{id}/estado` | Cocina | Cambia el estado de un pedido |

### Hub de tiempo real — SignalR

| Ruta | Descripción |
|---|---|
| `/hubs/pedidos` | Canal SignalR para Cocina y Recepción — push de cambios de estado en tiempo real |

### Próximos módulos a exponer vía API

Productos, Finanzas y Promociones ya tienen sus casos de uso y puertos listos en
`ProyectoJo.Application` (`IProductoService`, `IFinanzaService`, `IPromocionService`),
pero todavía solo se consumen desde `ProyectoJo.Web`. Quedan pendientes sus
respectivos controladores en `ProyectoJo.Api`.

---

## Decisiones de arquitectura (ADRs)

| ADR | Decisión |
|---|---|
| [ADR-01](./ADRs/ADR-01-Joaquin-Uriona.md) | Decisión inicial de stack/arquitectura del MVP |
| [ADR-02](./ADRs/ADR-02-Joaquin-Uriona.md) | MVC puro y sus limitaciones anticipadas |
| [ADR-03](./ADRs/ADR-03-Joaquin-Uriona.md) | Migración hacia Arquitectura Hexagonal |
| [ADR-04](./ADRs/ADR-04-Joaquin-Uriona.md) | Incorporación de una API REST con Swagger |
| [ADR-05](./ADRs/ADR-05-Joaquin-Uriona.md) | Integración de Patrones de Diseño GOF |
| [ADR-06](./ADRs/ADR-06-Joaquin-Uriona.md) | Reemplazo de Polling por SignalR en Cocina/Recepción |
| [ADR-07](./ADRs/ADR-07-Joaquin-Uriona.md) | Introducción de Proyecto de Tests y Estrategia de Cobertura |
| [ADR-08](./ADRs/ADR-08-Joaquin-Uriona.md) | Deuda técnica de `ProyectoJo.Api` |

---

## Deuda técnica conocida

El sistema documenta su deuda técnica de forma explícita en vez de dejarla implícita en el código. El detalle completo — causa, costo de no pagarla y propuesta de solución — está en [ADR-08](./ADRs/ADR-08-Joaquin-Uriona.md).

| Deuda | Tipo | Estado |
|---|---|---|
| `ProyectoJo.Api/Program.cs` arma las rutas de persistencia a mano (`Path.Combine` relativo, no configuración) | Infraestructura | Documentada — pendiente |
| `JsonPedidoRepository` usa un `SemaphoreSlim` estático por proceso, no compartido entre `Web` y `Api` | Infraestructura | Documentada — pendiente |
| `ProyectoJo.Api/Program.cs` no registra `IPedidoNotificador` ni `IPromocionService`, por lo que `PedidosController` falla en runtime al resolver `PedidoUseCase` | Accidental | Documentada — pendiente |

> Ambas deudas comparten causa raíz: `Web` y `Api` componen su grafo de dependencias por separado. La solución propuesta es centralizar el registro en un método de extensión compartido (`AddProyectoJoServices`), detallado en el ADR-08.
---

## Uso de IA

Se utilizó IA para:

- Corregir redacción y ortografía de este documento
- Generar la sintaxis Mermaid del diagrama de arquitectura
- Generar la estructura y el código de los tests unitarios y de integración a partir del código existente en
  `ProyectoJo.Application` y `ProyectoJo.Infrastructure`

No se utilizó para tomar decisiones arquitectónicas ni para diseñar la solución.

## 👨‍💻 Autor

**Joaquin Uriona**
* [LinkedIn](https://www.linkedin.com/in/Joaquin-Uriona)
* [GitHub](https://github.com/Joako601)

## 🔒 Licencia y propiedad intelectual

Copyright (c) 2026 Joaquin Uriona — Todos los derechos reservados.

Este software, su código fuente, arquitectura, diseño y documentación son
propiedad exclusiva de **Joaquin Uriona**. Queda terminantemente prohibido
el uso, copia, modificación, distribución o comercialización sin permiso
expreso y por escrito del autor.

El acceso a este repositorio se otorga únicamente para revisión técnica y
evaluación académica o profesional. Cualquier otro uso queda expresamente
prohibido.

> Este software se proporciona "tal cual", para fines de exhibición de
> portafolio profesional, sin garantías de ningún tipo.
