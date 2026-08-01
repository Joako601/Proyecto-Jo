# Resumen operativo del despliegue

Este documento no es un paso a paso más — es el resumen de **qué quedó
armado, dónde está cada cosa y cómo operarlo** después de haber pasado por
toda la serie de documentos (`AWS-1-Cuenta.md` → `AWS-2-Usuarios.md` →
`AWS-3-Servicios.md` → `Despliegue-AWS.md`). Pensalo como la chuleta rápida
para cuando te olvidés cómo funciona todo esto.

## Qué hay prendido ahora mismo

| Recurso | Nombre/valor |
|---|---|
| Región | `us-east-2` (o la que hayas usado) |
| EC2 | `proyectojo-web`, Ubuntu 24.04 |
| Elastic IP | `3.21.188.146` |
| Dominio (sin comprar ninguno) | `3.21.188.146.nip.io` — resuelve solo a la Elastic IP, así conseguimos HTTPS sin registrar un dominio propio |
| RDS | `proyectojo-db`, PostgreSQL, base `proyectojo`, usuario `proyectojo_admin` |
| URL pública | `https://3.21.188.146.nip.io` |
| Login Admin | `https://3.21.188.146.nip.io/Admin/Login` |

## Dónde están las credenciales

Ninguna contraseña real vive en este archivo ni en ningún archivo del repo
a propósito — así evitamos que termine subida a GitHub por error.

| Qué | Dónde consultarla |
|---|---|
| Password de RDS (`proyectojo_admin`) | La que generaste al crear la RDS. Si la perdiste: consola RDS → `proyectojo-db` → **Modify** → **Master password**. |
| Usuario/contraseña del panel Admin | `Auth__AdminUser` / `Auth__AdminPasswordHash` dentro de `/etc/proyectojo/proyectojo.env`, en el servidor. Para verlos: `sudo cat /etc/proyectojo/proyectojo.env` (por SSH). |
| `.pem` de la instancia EC2 | La carpeta donde lo descargaste al crear el key pair (en tu máquina, fuera del repo). |
| Secrets de GitHub Actions | Repo → Settings → Secrets and variables → Actions (no se pueden volver a ver el valor, solo sobreescribir). |

Si necesitás generar un nuevo hash de contraseña para el panel Admin en
algún momento (porque cambiaste la contraseña o creaste otro usuario),
usá el mismo método que ya armamos: un pequeño script de C# que replica el
algoritmo PBKDF2 de `EnvAuthService` (`ProyectoJo.Infrastructure/Auth/EnvAuthService.cs`) —
pedímelo y te lo genero, o correlo vos si preferís no compartir la
contraseña.

## Cómo conectarte al servidor

```bash
ssh -i "<ruta-a-tu-pem>" ubuntu@3.21.188.146
```

## Cómo hacer un nuevo deploy

1. Mergeá tus cambios a `main` (el pipeline solo está disponible ahí, no
   en ramas de feature — es una restricción de GitHub para
   `workflow_dispatch`).
2. GitHub → **Actions** → **Deploy** → **Run workflow**.
3. El pipeline publica, genera el bundle de migraciones, lo sube al
   servidor, aplica migraciones pendientes contra RDS, y reinicia el
   servicio.

**Importante:** cada reinicio del servicio invalida las sesiones activas y
cualquier formulario ya cargado en el navegador (las claves de seguridad
son efímeras, no persisten en disco — ver sección de problemas conocidos
más abajo). Si alguien tiene una pestaña abierta, va a necesitar recargar
la página después de un deploy.

## Cómo revisar si algo anda mal

```bash
sudo systemctl status proyectojo-web.service
sudo journalctl -u proyectojo-web.service --no-pager | tail -50
```

`Active: active (running)` es buena señal. Si dice `activating (auto-restart)`
o `failed`, el `journalctl` va a tener el motivo real (excepción de .NET,
error de conexión a la base, etc.).

## Cómo cargar datos para la demo

La base está vacía — es una instalación nueva, no un problema. `--seed`
(el importador viejo) ya no funciona porque sus archivos JSON de origen no
existen más en el repo.

1. Entrá a `/Admin/Login` con el usuario/hash de `proyectojo.env` (login
   SuperAdmin, no necesita ninguna fila en la base).
2. Cargá platillos, insumos, alguna promoción, etc. directamente desde el
   panel.

## Problemas reales que encontramos y cómo se resolvieron

Documentado por si vuelve a pasar (por ejemplo, si recreás toda la
infraestructura desde cero para otra demo):

- **`efbundle` fallaba con un error de `DbContext`/`DbContextOptions`**:
  las herramientas de EF Core no pueden construir el contexto a través del
  service provider de la app cuando está registrado con
  `AddDbContextPool` (una limitación real de EF Core, no un bug nuestro).
  Se resolvió agregando
  `ProyectoJo.Infrastructure/Persistence/EfCore/ProyectoJoDbContextFactory.cs`,
  una `IDesignTimeDbContextFactory<ProyectoJoDbContext>` explícita.
- **RDS rechazaba la conexión pidiendo SSL**: el connection string necesita
  `SSL Mode=Require;Trust Server Certificate=true` al final — Postgres en
  RDS exige conexión encriptada por defecto.
- **El servicio systemd tardaba 90s y fallaba por timeout**: estaba
  configurado como `Type=notify`, que espera una señal que ASP.NET Core no
  envía por defecto. Se cambió a `Type=simple` en
  `deploy/proyectojo-web.service`.
- **La app crasheaba al arrancar en producción** (`DirectoryNotFoundException`
  en `Areas/Admin/wwwroot`), y aunque no crasheara, el panel Admin se
  hubiera visto sin estilos: esa carpeta (todo el CSS/JS específico de
  Admin) nunca se copiaba al publicar. Se agregó un `<Content Include=...>`
  en `ProyectoJo.Web.csproj` para que sí se copie.
- **El puerto 22 bloqueaba a GitHub Actions**: el security group solo
  dejaba pasar SSH desde "My IP" (la tuya). Los runners de GitHub usan IPs
  dinámicas, así que hubo que abrir el 22 a `0.0.0.0/0` — el login sigue
  protegido porque solo acepta la clave privada, no contraseña.
- **nginx no arrancaba con `http2 on;`**: la versión de nginx de Ubuntu
  24.04 (1.24.x) es anterior a esa sintaxis; se usa la forma vieja
  `listen 443 ssl http2;`.
- **Certbot no podía emitir el certificado la primera vez** (dependencia
  circular: nginx necesita un certificado válido para arrancar, pero
  certbot necesita que nginx arranque para emitirlo): se resolvió pidiendo
  el certificado con `certbot certonly --webroot` mientras nginx corría
  solo con el bloque del puerto 80, y recién después se restauró la
  configuración completa con HTTPS.

## Índice de toda la serie

1. [AWS-1-Cuenta.md](AWS-1-Cuenta.md) — crear la cuenta.
2. [AWS-2-Usuarios.md](AWS-2-Usuarios.md) — usuarios y permisos (IAM).
3. [AWS-3-Servicios.md](AWS-3-Servicios.md) — crear EC2/RDS, y cómo apagar
   todo al terminar.
4. [Despliegue-AWS.md](Despliegue-AWS.md) — instalar todo dentro del
   servidor y conectar el pipeline.
5. **Despliegue-Resumen-Operativo.md** (este documento) — chuleta rápida
   de referencia.
