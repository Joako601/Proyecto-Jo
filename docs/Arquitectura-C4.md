# Arquitectura de Proyecto Jo' — Modelo C4



## Nivel 1 — Contexto

**Para quién es:** cualquier persona ajena al código. <br>
**Qué responde:** ¿qué es Proyecto Jo' y quién lo usa?

```mermaid
flowchart TD
    ADMIN["Dueño del negocio\n(login por credenciales de entorno)"]
    CLIENTE["Cliente del restaurante\n(navegador, sin login)"]
    COCINERO["Empleado de Cocina\n(login por PIN + dispositivo emparejado)"]
    RECEP["Empleado de Recepción\n(login por PIN + dispositivo emparejado)"]

    SISTEMA["Proyecto Jo'\nSistema de gestión financiera y administrativa"]

    ADMIN -->|"gestiona menú, finanzas, promociones,\ninventario, cierres de caja y auditoría"| SISTEMA
    CLIENTE -->|"consulta menú y promociones vigentes"| SISTEMA
    COCINERO -->|"ve pedidos entrantes y marca 'Preparado'"| SISTEMA
    RECEP -->|"crea pedidos y cobra cuando están listos"| SISTEMA
```

---
 
## Nivel 2 — Contenedores
 
**Para quién es:** el equipo técnico. <br>
**Qué responde:** ¿qué piezas desplegables tiene el sistema y cómo se comunican?
 
```mermaid
flowchart TD
    ADMIN["Dueño / Administrador"]
    CLIENTE["Cliente"]
    COCINERO["Cocina"]
    RECEP["Recepción"]
 
    subgraph EC2 ["Instancia única AWS EC2"]
        WEB["ProyectoJo.Web\nASP.NET Core MVC — Kestrel\n(Areas Admin y Operaciones + sitio público)"]
        HUB["PedidosHub\n/hubs/pedidos — SignalR"]
        API["ProyectoJo.Api\nProceso independiente — ASP.NET Core\nREST + Swagger, endpoints /api/pedidos,\n/api/productos"]
        JSON[("Persistencia\nProyectoJo.Web/Persistencia/*.json\nmenu, finanzas, pedidos, promociones,\nempleados, dispositivos, cierres-caja, auditoria")]
    end
 
    ADMIN -->|HTTPS| WEB
    CLIENTE -->|HTTPS| WEB
    COCINERO -->|"HTTPS + WebSocket"| WEB
    RECEP -->|"HTTPS + WebSocket"| WEB
    WEB <-->|"grupo Cocina / grupo Recepcion"| HUB
 
    WEB -->|"lee/escribe vía Json*Repository"| JSON
    API -.->|"lee/escribe los MISMOS archivos\ncon ruta relativa ../ProyectoJo.Web/Persistencia\n(proceso separado, no pasa por Web)"| JSON
```