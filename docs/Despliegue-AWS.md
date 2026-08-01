# AWS — Paso 4: Despliegue (dentro del servidor + pipeline)

Cuarto y último documento de la serie:

1. [AWS-1-Cuenta.md](AWS-1-Cuenta.md) — crear la cuenta.
2. [AWS-2-Usuarios.md](AWS-2-Usuarios.md) — usuarios y permisos (IAM).
3. [AWS-3-Servicios.md](AWS-3-Servicios.md) — crear el servidor (EC2) y la
   base de datos (RDS).
4. **Despliegue-AWS.md** (este documento) — instalar el software adentro del
   servidor y conectar el pipeline de GitHub Actions.

Si todavía no tenés la instancia EC2 corriendo y la base de datos RDS
disponible, andá primero a
[AWS-3-Servicios.md](AWS-3-Servicios.md) — este documento asume que ya
existen.

`ProyectoJo.Api` queda fuera de este flujo — sigue sin persistencia
conectada, ver "Deuda técnica conocida" en [CLAUDE.md](../CLAUDE.md).

## Arquitectura

```
GitHub Actions (workflow_dispatch)
        │  SSH / SCP
        ▼
   EC2 (Ubuntu)
   ├─ nginx (443, TLS)  ──▶  Kestrel (127.0.0.1:5000, systemd)
   └─ efbundle (migraciones)  ──▶  RDS (PostgreSQL)
```

nginx termina TLS y reenvía a Kestrel por loopback; Kestrel nunca queda
expuesto directamente a internet. El pipeline solo se dispara a mano
(`workflow_dispatch`) — no hay deploy automático en push todavía.

## 1. Instalar el software dentro del servidor (una sola vez)

Conectate por SSH a la instancia (la IP es la Elastic IP del documento 3):

```bash
chmod 400 proyectojo-key.pem
ssh -i proyectojo-key.pem ubuntu@<EC2_HOST>
```

### 1.1. Runtime de .NET y nginx

```bash
sudo apt update
sudo apt install -y wget
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-10.0 nginx certbot python3-certbot-nginx
```

(Los dos primeros comandos agregan el repositorio de paquetes de Microsoft —
Ubuntu no trae el runtime de .NET en sus repositorios por defecto.)

### 1.2. Usuario de servicio y directorios de release

Por seguridad, la app no corre como `root` ni como `ubuntu` — corre como un
usuario del sistema sin login propio, dedicado solo a esto:

```bash
sudo useradd --system --no-create-home proyectojo
sudo mkdir -p /opt/proyectojo/releases /etc/proyectojo
sudo chown -R proyectojo:proyectojo /opt/proyectojo
```

### 1.3. Variables de entorno (secretos de la app)

Este archivo vive **solo en el servidor**, nunca en el repo — mismo
principio que "Never hardcode either in launchSettings.json or
appsettings.json" de [CLAUDE.md](../CLAUDE.md). Usá el Endpoint y la
contraseña de RDS que guardaste en el documento 3:

```bash
sudo tee /etc/proyectojo/proyectojo.env > /dev/null <<'EOF'
ConnectionStrings__Default=Host=<endpoint-de-rds>;Port=5432;Database=proyectojo;Username=proyectojo_admin;Password=<la-que-generaste>
Auth__AdminUser=<usuario-admin-del-panel>
Auth__AdminPasswordHash=<hash-de-la-contraseña>
EOF
sudo chmod 600 /etc/proyectojo/proyectojo.env
```

`Auth__AdminPasswordHash` es el hash PBKDF2 que genera
`EnvAuthService`/`AdministradorUseCase` del proyecto — no es la contraseña en
texto plano.

### 1.4. Servicio systemd

Desde tu máquina (no desde el servidor), copiá el archivo del repo al
servidor:

```bash
scp -i proyectojo-key.pem deploy/proyectojo-web.service ubuntu@<EC2_HOST>:/tmp/
```

Y ya en el servidor:

```bash
sudo mv /tmp/proyectojo-web.service /etc/systemd/system/proyectojo-web.service
sudo systemctl daemon-reload
sudo systemctl enable proyectojo-web
```

Todavía no hay ningún release en `/opt/proyectojo/current`, así que el
primer arranque real del servicio ocurre recién en el primer deploy
(sección 3).

### 1.5. nginx + certificado TLS

Necesitás un **dominio** apuntando a la Elastic IP (un registro DNS tipo `A`)
para que certbot pueda emitir el certificado — no funciona solo con la IP.
Si todavía no tenés un dominio, cualquier proveedor de DNS barato/gratuito
sirve para una demo.

```bash
scp -i proyectojo-key.pem deploy/nginx-proyectojo.conf ubuntu@<EC2_HOST>:/tmp/
```

En el servidor:

```bash
sudo mv /tmp/nginx-proyectojo.conf /etc/nginx/sites-available/proyectojo
sudo sed -i 's/DOMINIO/tu-dominio.com/' /etc/nginx/sites-available/proyectojo
sudo ln -s /etc/nginx/sites-available/proyectojo /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo certbot --nginx -d tu-dominio.com
sudo nginx -t && sudo systemctl reload nginx
```

`certbot --nginx` pide un email de contacto y edita el archivo de nginx solo
para agregar la configuración TLS — el archivo del repo ya deja el resto
armado (proxy a Kestrel, WebSockets para SignalR).

## 2. Secrets de GitHub

En el repo de GitHub: **Settings → Environments** → **"New environment"** →
nombre `production` (esto te permite, opcionalmente, exigir que alguien
apruebe manualmente cada deploy antes de que corra — recomendado aunque sea
solo vos, como paso de "¿seguro que quiero desplegar ahora?"). Adentro de ese
environment, o en **Settings → Secrets and variables → Actions** si preferís
no usar environments, cargá:

| Secreto | Valor | De dónde sale |
|---|---|---|
| `EC2_HOST` | La Elastic IP o el dominio | Documento 3, paso 5 |
| `EC2_USER` | `ubuntu` | Usuario por defecto de la AMI de Ubuntu |
| `EC2_SSH_KEY` | El contenido completo del archivo `.pem` (clave privada) | Documento 3, paso 4 |
| `EC2_SSH_PORT` | Opcional, default `22` | — |
| `RDS_CONNECTION_STRING` | El mismo connection string del paso 1.3 de acá arriba | Documento 3, paso 3 |

Para `EC2_SSH_KEY`: abrí el archivo `.pem` con un editor de texto (no Word),
copiá **todo** el contenido incluyendo las líneas
`-----BEGIN OPENSSH PRIVATE KEY-----` y `-----END OPENSSH PRIVATE KEY-----`,
y pegalo tal cual como valor del secret.

Ninguno de estos valores vive en el repo ni en `appsettings.json` — el
`.gitignore` del proyecto además bloquea `*.pem` y `*.env` por si alguno
termina sin querer en esta carpeta.

## 3. Deploy

Con todo lo anterior configurado: **Actions → Deploy → Run workflow**. El
workflow (`.github/workflows/deploy.yml`):

1. Publica `ProyectoJo.Web` en modo Release.
2. Genera un *migrations bundle* (`efbundle`, autocontenido — no necesita el
   SDK instalado en el EC2).
3. Sube el release a `/opt/proyectojo/releases/<run_id>` por SCP.
4. Aplica las migraciones pendientes contra RDS ejecutando `efbundle`
   **antes** de activar el release nuevo.
5. Apunta el symlink `/opt/proyectojo/current` al release nuevo y reinicia
   `proyectojo-web`.
6. Borra releases viejos, dejando los últimos 5 (para poder hacer rollback).

Después del primer deploy, entrá a `https://tu-dominio.com` — deberías ver
la pantalla de login del panel Admin.

## 4. Rollback

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
- El pipeline es manual (`workflow_dispatch`) a propósito, hasta confirmar
  que la infraestructura funciona de punta a punta. Pasar a deploy
  automático en push a `main` es un cambio de una línea en `deploy.yml` una
  vez validado.
- Si ya no vas a seguir usando esto después de la demo, no te olvides de la
  sección "Apagar/borrar todo después de la demo" en
  [AWS-3-Servicios.md](AWS-3-Servicios.md).
