# Evaluación ATAM — Proyecto Jo'

| Campo  | Valor |
|--------|-------|
| Autor  | Joaquin Uriona |
| Fecha  | 01/08/2026 |
| Método | Architecture Tradeoff Analysis Method (ATAM) |
| Alcance | Arquitectura vigente tras ADR-01 a ADR-13 |

---

## 1. Propósito y contexto de negocio

Proyecto Jo' es un sistema de gestión financiera y administrativa para un negocio pequeño (restaurante), con tres perfiles de usuario reales (dueño/administrador, empleados de Cocina/Recepción, clientes anónimos) y un objetivo de despliegue concreto: una demo en vivo sobre infraestructura real de AWS (ADR-13), no un sistema de producción de uso continuo con SLA formal. Los atributos de calidad priorizados por el equipo, en orden, fueron: **modificabilidad** (arquitectura hexagonal desde ADR-03, para poder cambiar piezas de infraestructura sin tocar lógica de negocio — validado en la práctica con la migración de ADR-10), **seguridad** (tres esquemas de autenticación aislados, hardening de ADR-11), y **rendimiento** en las rutas de uso real más frecuente (ADR-12), por encima de escalabilidad a gran volumen o alta disponibilidad formal — objetivos que el propio ADR-13 declara explícitamente fuera de alcance para "lo más básico posible" de una demo puntual.

Esta evaluación no repite el detalle ya documentado en cada ADR — los cita como evidencia y se concentra en los tres elementos que pide el método: un **riesgo**, un **trade-off**, y un **punto de sensibilidad**, cada uno anclado en una decisión arquitectónica real, no hipotética.

---

## 2. Árbol de utilidad (resumen)

| Atributo de calidad | Escenario concreto | Prioridad (negocio) | Dificultad (técnica) |
|---|---|---|---|
| Modificabilidad | Cambiar el motor de persistencia sin tocar `UseCases`/`Domain` | Alta | Media (ya resuelto, ADR-10) |
| Seguridad | Comprometer una cookie de sesión no debe otorgar acceso a las otras dos identidades | Alta | Media (ya resuelto, ADR-11) |
| Rendimiento | La vista de Cocina/Recepción responde rápido bajo refresco constante durante el servicio | Alta | Media (ya resuelto, ADR-12) |
| Disponibilidad | El sistema sigue operable ante un reinicio del proceso o una falla del servidor de aplicación | Media | **Alta (parcialmente sin resolver — ver Riesgo)** |

---

## 3. Riesgo

> Un riesgo ATAM es una decisión arquitectónica (o la ausencia de una) que podría causar un problema real si no se atiende, aunque hoy no haya causado una falla observada en producción.

### R-1: Claves de Data Protection efímeras — pérdida de sesión masiva ante cualquier reinicio del proceso

**Decisión relacionada:** ninguna — es la *ausencia* de una decisión explícita. `ProyectoJo.Web/Program.cs` no configura un almacenamiento persistente para el *key ring* de ASP.NET Core Data Protection (usado para firmar cookies de autenticación y tokens antiforgery). Por defecto, ASP.NET Core genera una clave nueva en memoria en cada arranque del proceso.

**Por qué es un riesgo real, no hipotético:** se observó directamente durante el primer despliegue (ADR-13) — un `systemctl restart proyectojo-web` mientras un formulario de login ya estaba cargado en el navegador produjo un `400 Bad Request` por token antiforgery inválido, porque el proceso reiniciado ya no reconocía tokens firmados por la instancia anterior. El mismo mecanismo invalida **todas** las cookies de sesión activas (`Jo.Admin`, `Jo.Supervisor`, `Jo.Operaciones`) de **todos** los usuarios conectados en ese momento, no solo la del formulario que falló.

**Impacto concreto si no se atiende:** en el entorno actual (una única instancia EC2, `Restart=on-failure` en el `systemd` de ADR-13), cualquier crash del proceso, cualquier redeploy, o cualquier reinicio del servidor durante el servicio activo del restaurante desloguea simultáneamente al administrador, a Cocina y a Recepción sin aviso — en el peor momento posible (durante una falla, que es justo cuando se necesita que el sistema siga respondiendo).

**Mitigación propuesta, no implementada todavía:** persistir el *key ring* en un directorio del propio EC2 (`AddDataProtection().PersistKeysToFileSystem(...)`) o, mejor dado que ya se depende de RDS, en la propia base de datos — cualquiera de las dos sobrevive a un reinicio del proceso. Queda como deuda explícita, no como olvido: se decidió no implementarlo para el alcance de la demo, priorizando llegar al deploy funcional (ADR-13) antes que cerrar este punto.

---

## 4. Trade-off

> Un trade-off ATAM es un punto de la arquitectura donde una decisión mejora un atributo de calidad a costa de otro — no hay opción "gratis".

### T-1: `AddDbContextPool` — rendimiento en tiempo de ejecución vs. compatibilidad con herramientas de diseño de EF Core

**Decisión relacionada:** ADR-12, sección "Decisión", punto 4.

**El trade-off:** registrar `ProyectoJoDbContext` con `AddDbContextPool` en vez de `AddDbContext` mejora el **rendimiento** bajo carga sostenida — reutiliza instancias del contexto entre requests en vez de pagar su inicialización completa en cada uno, directamente relevante para la ruta más caliente del sistema (Cocina/Recepción, ADR-12). Pero esa misma decisión rompe el mecanismo estándar con el que las herramientas de diseño de EF Core (`dotnet ef`, y los *migration bundles* que corren en el pipeline de despliegue de ADR-13) construyen una instancia del `DbContext` — no pueden hacerlo a través del *service provider* de la aplicación cuando está pooleado. Esto no es un bug de esta implementación puntual, es una limitación documentada de EF Core.

**Cómo se resolvió, y qué costó:** se agregó `ProyectoJoDbContextFactory` (`IDesignTimeDbContextFactory<ProyectoJoDbContext>`) exclusivamente para las herramientas de diseño, sin tocar el registro `AddDbContextPool` que usa la aplicación en tiempo de ejecución — la ganancia de rendimiento se conserva íntegra. El costo real fue **tiempo de diagnóstico durante el primer despliegue**: el síntoma observado (`efbundle` fallando al intentar levantar la aplicación web completa como *fallback*, con un `DirectoryNotFoundException` aparentemente no relacionado sobre `Areas/Admin/wwwroot`) no apuntaba obviamente a la causa raíz (`AddDbContextPool`), y encontrarla requirió descartar varias hipótesis primero (ver ADR-13, problema #1 y #3).

**Por qué se mantiene la decisión, no se revierte:** el rendimiento en tiempo de ejecución es la ruta que se ejecuta miles de veces durante el uso real del sistema (cada refresco de Cocina/Recepción); el costo de la incompatibilidad con las herramientas de diseño se paga una única vez, al momento de construir el pipeline de despliegue, no en cada operación. El trade-off se acepta explícitamente porque el costo es de una sola vez y el beneficio es recurrente.

---

## 5. Punto de sensibilidad

> Un punto de sensibilidad ATAM es un parámetro arquitectónico donde un cambio pequeño produce una variación grande y desproporcionada en un atributo de calidad — el sistema es "sensible" a ese parámetro específico.

### S-1: El origen permitido en `proyectojo-rds-sg` — la confidencialidad de todos los datos depende de una única regla de security group

**Decisión relacionada:** ADR-13, sección "Decisión" ("Arquitectura de red").

**El parámetro sensible:** la regla de entrada del puerto 5432 en `proyectojo-rds-sg` tiene como origen configurado el **security group del EC2** (`proyectojo-ec2-sg`), no una IP ni un rango CIDR. Es la única barrera real entre la base de datos completa (todos los pedidos, finanzas, credenciales con hash de todos los administradores y empleados) e internet — RDS no tiene ninguna otra capa de red intermedia en esta arquitectura (no hay VPN, no hay bastion host separado, no hay un firewall de aplicación delante).

**Por qué es un punto de sensibilidad, no solo "una configuración más":** un cambio mínimo y plausible en este único parámetro —por ejemplo, alguien copiando la regla del EC2 (que sí permite `0.0.0.0/0` en los puertos 80/443, correctamente, porque es un servidor web público) y aplicándola por error también al de RDS, o eligiendo "Anywhere" en el desplegable de origen durante una reconfiguración apurada— cambia el atributo de **confidencialidad** de la base de datos de "inalcanzable desde internet" a "expuesta directamente a cualquier IP del mundo" en un solo paso, sin ningún otro control compensatorio en la arquitectura actual que lo mitigue. No es un cambio gradual: es un salto de todo a nada en la protección real del dato más sensible del sistema.

**Evidencia de que el equipo ya identificó esta sensibilidad como real:** la documentación de infraestructura (`docs/AWS-3-Servicios.md`) marca explícitamente esta regla con una advertencia dedicada contra usar "Anywhere" — señal de que el propio proceso de construir la arquitectura reconoció el peso desproporcionado de este único parámetro sobre el atributo de seguridad del sistema completo.

**Qué mitigaría la sensibilidad, no implementado:** ninguna capa de defensa en profundidad adicional existe hoy si esa única regla fallara — ni una VPC privada sin ruta a internet para RDS, ni autenticación de red adicional (IAM database authentication de RDS), ni alertas automáticas ante cambios de security groups (AWS Config rules). Queda, igual que R-1, como deuda consciente y no como omisión: el costo de esas capas adicionales no se justificó para el alcance de una demo puntual, pero es exactamente el tipo de control que un sistema en producción continua necesitaría antes de manejar datos reales de clientes.

---

## 6. Síntesis

Los tres puntos analizados comparten un patrón: ninguno es un defecto de implementación puntual, los tres son **consecuencias directas de decisiones documentadas** (ADR-12 para el trade-off, ADR-13 para el riesgo y el punto de sensibilidad) tomadas conscientemente para el alcance real del proyecto — una demo en vivo con un equipo sin experiencia previa en AWS, no un sistema en producción continua. Eso no los vuelve menos reales: si Proyecto Jo' pasara de demo a un despliegue de uso continuo, R-1 (claves efímeras) y S-1 (sensibilidad del security group de RDS) serían los dos primeros puntos a cerrar antes de manejar datos de clientes reales, en ese orden de prioridad.
