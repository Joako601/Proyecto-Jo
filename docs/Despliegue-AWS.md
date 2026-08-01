# Despliegue en AWS

Este documento describe cómo desplegar `ProyectoJo.Web` a una instancia EC2, con
PostgreSQL en RDS y despliegues disparados manualmente desde GitHub Actions.
`ProyectoJo.Api` queda fuera de este flujo — ver "Deuda técnica conocida" en
[CLAUDE.md](../CLAUDE.md).

## Arquitectura

```
GitHub Actions (workflow_dispatch)
        │  SSH / SCP
        ▼
   EC2 (Ubuntu)
   ├─ nginx (443, TLS)  ──▶  Kestrel (127.0.0.1:5000, systemd)
   └─ efbundle (migraciones)  ──▶  RDS (PostgreSQL)
```

nginx termina TLS y reenvía a Kestrel por loopback; Kestrel nunca queda expuesto
directamente a internet. El pipeline solo se dispara a mano
(`workflow_dispatch`) — no hay deploy automático en push todavía.

## 1. RDS (PostgreSQL)

1. Crear una instancia RDS PostgreSQL (versión compatible con Npgsql — 15 o 16).
2. Security group de RDS: permitir el puerto 5432 **solo** desde el security
   group de la instancia EC2, no desde `0.0.0.0/0`.
3. Guardar el connection string completo
   (`Host=...;Port=5432;Database=proyectojo;Username=...;Password=...`) — se
   usa en el secreto `RDS_CONNECTION_STRING` (paso 4) y en el `.env` del EC2
   (paso 2).

## 2. EC2 (una sola vez)

1. Lanzar una instancia Ubuntu LTS. Security group: 80/443 abiertos al mundo,
   22 (SSH) restringido a tu IP o a los runners de GitHub Actions.
2. Instalar el runtime de ASP.NET Core 10 y nginx:
   ```bash
   sudo apt update
   sudo apt install -y aspnetcore-runtime-10.0 nginx certbot python3-certbot-nginx
   ```
3. Crear el usuario de servicio y los directorios de release:
   ```bash
   sudo useradd --system --no-create-home proyectojo
   sudo mkdir -p /opt/proyectojo/releases /etc/proyectojo
   sudo chown -R proyectojo:proyectojo /opt/proyectojo
   ```
4. Crear `/etc/proyectojo/proyectojo.env` (root-only, nunca en el repo — mismo
   principio que "Never hardcode either in launchSettings.json or
   appsettings.json" de CLAUDE.md):
   ```bash
   sudo tee /etc/proyectojo/proyectojo.env > /dev/null <<'EOF'
   ConnectionStrings__Default=Host=...;Port=5432;Database=proyectojo;Username=...;Password=...
   Auth__AdminUser=...
   Auth__AdminPasswordHash=...
   EOF
   sudo chmod 600 /etc/proyectojo/proyectojo.env
   ```
5. Copiar [`deploy/proyectojo-web.service`](../deploy/proyectojo-web.service) a
   `/etc/systemd/system/proyectojo-web.service` y habilitarlo:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable proyectojo-web
   ```
   (Todavía no hay ningún release en `/opt/proyectojo/current`, así que el
   primer `start` real ocurre en el primer deploy — paso 5 más abajo.)
6. Copiar [`deploy/nginx-proyectojo.conf`](../deploy/nginx-proyectojo.conf) a
   `/etc/nginx/sites-available/proyectojo`, reemplazar `DOMINIO` por el dominio
   real, habilitarlo y pedir el certificado:
   ```bash
   sudo ln -s /etc/nginx/sites-available/proyectojo /etc/nginx/sites-enabled/
   sudo certbot --nginx -d tu-dominio.com
   sudo nginx -t && sudo systemctl reload nginx
   ```

## 3. Secrets de GitHub

En **Settings → Environments → production** (crear el environment
`production` — permite exigir un aprobador manual antes de cada deploy si
querés esa capa extra de seguridad) o en **Settings → Secrets and variables →
Actions**, cargar:

| Secreto | Valor |
|---|---|
| `EC2_HOST` | IP pública o DNS de la instancia |
| `EC2_USER` | usuario SSH (`ubuntu` en Ubuntu AMI estándar) |
| `EC2_SSH_KEY` | clave privada SSH completa (PEM) con acceso a esa instancia |
| `EC2_SSH_PORT` | opcional, default `22` |
| `RDS_CONNECTION_STRING` | mismo connection string del paso 1, usado solo para aplicar migraciones durante el deploy |

Ninguno de estos valores vive en el repo ni en `appsettings.json` — coherente
con la regla existente del proyecto de nunca hardcodear credenciales.

## 4. Deploy

Con todo lo anterior configurado: **Actions → Deploy → Run workflow**. El
workflow (`.github/workflows/deploy.yml`):

1. Publica `ProyectoJo.Web` en modo Release.
2. Genera un *migrations bundle* (`efbundle`, autocontenido — no necesita el
   SDK instalado en el EC2).
3. Sube el release a `/opt/proyectojo/releases/<run_id>` por SCP.
4. Aplica las migraciones pendientes contra RDS ejecutando `efbundle` **antes**
   de activar el release nuevo.
5. Apunta el symlink `/opt/proyectojo/current` al release nuevo y reinicia
   `proyectojo-web`.
6. Borra releases viejos, dejando los últimos 5 (para poder hacer rollback).

## 5. Rollback

Si un deploy sale mal, no hace falta re-ejecutar el workflow — las
migraciones son aditivas (nunca se borra una columna/tabla existente en las
migraciones de este proyecto), así que alcanza con volver el symlink al
release anterior y reiniciar:

```bash
ls /opt/proyectojo/releases          # ver releases disponibles
sudo ln -sfn /opt/proyectojo/releases/<run_id_anterior> /opt/proyectojo/current
sudo systemctl restart proyectojo-web
```

## Pendiente / fuera de alcance de este documento

- `ProyectoJo.Api` no se despliega — sigue sin persistencia conectada (ver
  Deuda técnica conocida en CLAUDE.md).
- El pipeline es manual (`workflow_dispatch`) a propósito, hasta confirmar que
  la infraestructura funciona de punta a punta. Pasar a deploy automático en
  push a `main` es un cambio de una línea en `deploy.yml` una vez validado.
