# AWS — Paso 3: Servicios (el servidor y la base de datos)

Tercer documento de la serie. Acá creamos los recursos reales: el servidor
donde corre `ProyectoJo.Web` (**EC2**) y la base de datos (**RDS**). Antes de
esto, necesitás haber terminado [AWS-1-Cuenta.md](AWS-1-Cuenta.md) y
[AWS-2-Usuarios.md](AWS-2-Usuarios.md) — vas a hacer todo lo de acá logueado
con tu usuario IAM (no con el root).

**Importante sobre costos:** todo lo que vamos a crear entra en el *Free
Tier* de AWS (12 meses gratis, con límites de uso que no vamos a superar en
una demo). Aun así, al final de este documento hay una sección de **"Apagar
todo después de la demo"** — no te la saltees, dejar recursos prendidos
indefinidamente eventualmente empieza a cobrar.

## Conceptos básicos (explicados sin dar nada por sabido)

| Concepto | Qué es |
|---|---|
| **EC2** (Elastic Compute Cloud) | Una computadora virtual que alquilás por hora/segundo. Ahí instalamos Linux, .NET, y corremos `ProyectoJo.Web`. |
| **RDS** (Relational Database Service) | Una base de datos (en nuestro caso PostgreSQL) que AWS administra por vos: hace backups, aplica parches, etc. — no instalás Postgres a mano. |
| **VPC** (Virtual Private Cloud) | Una red privada y aislada, tuya, dentro de AWS. Todo lo que crees (EC2, RDS) vive adentro de una VPC. AWS te crea una VPC "default" automáticamente en cada región — la vamos a usar, no hace falta crear una nueva. |
| **Subnet** | Una subdivisión de la VPC. La VPC default ya viene con varias subnets creadas, una por cada "zona de disponibilidad" (data centers físicamente separados dentro de la misma región). No hay que crear nada acá tampoco. |
| **Security Group** | Un firewall: una lista de reglas de "qué tráfico entra y de dónde". Cada EC2 y cada RDS tiene uno (o más) asignado. Esto es lo único de redes que sí vamos a configurar a mano. |
| **Elastic IP** | Una dirección IP pública fija. Por defecto, la IP pública de un EC2 cambia cada vez que lo apagás y prendés — una Elastic IP la deja fija. |

## 1. Crear el Security Group del servidor (EC2)

Los security groups se pueden crear solos o al mismo tiempo que la instancia.
Los creamos antes para tenerlos ordenados y saber exactamente qué estamos
abriendo.

1. Consola → buscador de arriba → **"EC2"** → entrá al servicio.
2. **Confirmá la región** arriba a la derecha (`sa-east-1` si seguiste la
   recomendación del documento 1).
3. Menú de la izquierda → **"Security Groups"** (dentro de "Network &
   Security") → **"Create security group"**.
4. Nombre: `proyectojo-ec2-sg`. Descripción: `Servidor web ProyectoJo`.
5. VPC: dejá la que dice **"default"**.
6. **Inbound rules** (reglas de entrada) — agregá estas tres, una por una con
   "Add rule":

   | Type | Protocol | Port range | Source | Por qué |
   |---|---|---|---|---|
   | HTTP | TCP | 80 | `0.0.0.0/0` (Anywhere-IPv4) | Para que nginx redirija a HTTPS |
   | HTTPS | TCP | 443 | `0.0.0.0/0` (Anywhere-IPv4) | El tráfico real de la web |
   | SSH | TCP | 22 | **"My IP"** | Para conectarte vos por SSH. AWS detecta tu IP actual sola si elegís esta opción — **no uses "Anywhere"** acá, es la puerta que usan los ataques automatizados de fuerza bruta más comunes en todo internet. |

   Sobre el SSH: tu IP de internet de casa puede cambiar con el tiempo (según
   tu proveedor). Si en algún momento no podés conectarte por SSH y antes sí
   podías, este es el primer lugar para revisar — volvés a esta regla,
   "Edit inbound rules", y actualizás el source a "My IP" de nuevo.

7. **Outbound rules** (salida): dejá la que viene por defecto (todo
   permitido) — no hace falta restringir la salida para este proyecto.
8. **"Create security group"**.

## 2. Crear el Security Group de la base de datos (RDS)

1. Mismo lugar (EC2 → Security Groups) → **"Create security group"**.
2. Nombre: `proyectojo-rds-sg`. Descripción: `Base de datos PostgreSQL`.
3. VPC: la misma **"default"**.
4. **Inbound rules** — una sola regla:

   | Type | Protocol | Port range | Source | Por qué |
   |---|---|---|---|---|
   | PostgreSQL | TCP | 5432 | `proyectojo-ec2-sg` (elegilo del desplegable, escribí "sg-" y te va a aparecer) | Solo el servidor EC2 puede hablarle a la base — nadie más en internet, ni siquiera vos directamente desde tu casa. |

   Este es el punto más importante de todo el documento: **la base de datos
   nunca queda expuesta a internet**. El *source* no es una IP, es *el otro
   security group* — eso significa "cualquier cosa que tenga el
   `proyectojo-ec2-sg` puesto, y nada más".

5. **"Create security group"**.

## 3. Crear la base de datos (RDS PostgreSQL)

1. Buscador de arriba → **"RDS"** → entrá al servicio.
2. Confirmá la misma región de siempre.
3. **"Create database"**.
4. **Choose a database creation method**: **"Standard create"** (nos deja ver
   y elegir cada opción, a diferencia de "Easy create").
5. **Engine type**: **PostgreSQL**. Versión: la más reciente disponible que
   empiece con 15 o 16 (compatible con Npgsql, el driver que usa el
   proyecto).
6. **Templates**: elegí **"Free tier"** — esto ya preconfigura varias
   opciones de abajo para no pasarte de los límites gratuitos.
7. **Settings**:
   - **DB instance identifier**: `proyectojo-db` (nombre del *servidor* de
     base de datos, no de la base en sí).
   - **Master username**: `proyectojo_admin` (evitá `postgres` o `admin` a
     secas, son los primeros que prueba cualquier ataque automatizado).
   - **Master password**: generá una larga y guardala ya mismo en un gestor
     de contraseñas — la vas a necesitar en el documento 4 para el
     connection string. AWS te da la opción de "Auto generate a password"
     y te la muestra una vez.
8. **Instance configuration**: con el template "Free tier" ya te deja
   marcada **`db.t3.micro`** o **`db.t4g.micro`** — dejala así.
9. **Storage**: 20 GB (el mínimo, ya viene marcado con Free tier), tipo
   `gp2`/`gp3`. Destildá "Enable storage autoscaling" si querés evitar
   sorpresas de costo — para una demo, 20 GB sobra de sobra.
10. **Connectivity**:
    - **Virtual private cloud (VPC)**: la default (misma de siempre).
    - **Public access**: **"No"** — esto es tan importante como el security
      group. Si esto queda en "Yes", la base queda alcanzable desde
      internet aunque el security group esté bien configurado; dejarlo en
      "No" es una segunda capa de protección.
    - **VPC security group**: elegí **"Choose existing"** → seleccioná
      `proyectojo-rds-sg` (el que creaste en el paso 2). Sacá el
      "default" si quedó tildado también.
11. **Database authentication**: dejá **"Password authentication"**.
12. Más abajo, **"Additional configuration"**:
    - **Initial database name**: `proyectojo` (así ya queda creada la base
      con ese nombre, la que usa el proyecto).
    - **Backup retention period**: 1 día alcanza para una demo (esto es lo
      que hace que RDS sea "administrado" — hace backups solo).
    - **Enable deletion protection**: dejalo **destildado** — lo vamos a
      querer poder borrar fácil después de la demo (sección final de este
      documento).
13. **"Create database"**.

La creación tarda unos **5-10 minutos**. Vas a ver el estado pasar de
"Creating" a "Available". No sigas al paso 4 hasta que diga "Available".

Cuando esté lista, hacé clic en la base → copiá el **Endpoint** (algo como
`proyectojo-db.xxxxxxxxxx.sa-east-1.rds.amazonaws.com`) — lo vas a necesitar
en el documento 4 para armar el connection string.

## 4. Crear el servidor (instancia EC2)

1. Volvé a **EC2** → **"Instances"** → **"Launch instance"**.
2. **Name**: `proyectojo-web`.
3. **Application and OS Images (AMI)**: buscá **"Ubuntu"** y elegí la
   versión LTS más reciente marcada como **"Free tier eligible"** (hoy,
   Ubuntu Server 24.04 LTS).
4. **Instance type**: **`t2.micro`** o **`t3.micro`** (la que aparezca
   marcada "Free tier eligible" — usá esa, no una más grande, no la
   necesitamos).
5. **Key pair (login)**: **"Create new key pair"**.
   - Nombre: `proyectojo-key`.
   - Tipo: **ED25519**.
   - Formato: **.pem** (para Linux/Mac/Git Bash en Windows) — si vas a
     conectarte desde PowerShell puro con un cliente que solo entiende
     `.ppk`, elegí ese, pero `.pem` funciona con el `ssh` que ya tenés
     disponible en Git Bash.
   - Al hacer clic en "Create key pair" se **descarga el archivo
     automáticamente** — este es el único momento en que existe, AWS no
     guarda una copia. **Guardalo fuera de la carpeta del repo** (el
     `.gitignore` del proyecto ya bloquea `*.pem` por las dudas, pero mejor
     ni tentar poniéndolo ahí). Esta es la clave privada que vas a cargar
     como secret `EC2_SSH_KEY` en GitHub (documento 4).
6. **Network settings** → **"Edit"**:
   - VPC: la default.
   - **Firewall (security groups)**: elegí **"Select existing security
     group"** → `proyectojo-ec2-sg` (el que creaste en el paso 1). Sacá
     cualquier grupo que haya quedado tildado por defecto.
7. **Configure storage**: 20-30 GB alcanza (gp3, el tipo que viene por
   defecto).
8. **Advanced details** → **"IAM instance profile"**: elegí
   `proyectojo-ec2-role` (el rol vacío que creaste en el documento 2).
9. Revisá el resumen de la derecha y **"Launch instance"**.

La instancia tarda menos de un minuto en pasar a estado **"Running"**.

## 5. Asignar una Elastic IP (para que la dirección no cambie)

Por defecto, si alguna vez reiniciás o detenés/prendés la instancia, la IP
pública cambia. Como en el documento 4 vamos a cargar esa IP como secret de
GitHub, conviene fijarla:

1. EC2 → menú izquierdo → **"Elastic IPs"** → **"Allocate Elastic IP
   address"** → dejá las opciones por defecto → **"Allocate"**.
2. Seleccioná la IP recién creada → **"Actions"** → **"Associate Elastic IP
   address"**.
3. **Resource type**: Instance. **Instance**: `proyectojo-web`.
   **"Associate"**.
4. Esa IP (la que aparece en la columna "Elastic IP") es la que vas a usar
   como `EC2_HOST` en GitHub y como el dominio/A record de tu DNS si le
   ponés un nombre.

**Nota de costo:** una Elastic IP es gratis **mientras está asociada a una
instancia corriendo**. Si la liberás (paso "Apagar todo" más abajo) o la
dejás asociada a una instancia detenida, empieza a cobrar centavos por hora
— otra razón más para hacer la limpieza final si no vas a seguir usando esto
después de la demo.

## Resumen de lo que quedó creado

| Recurso | Nombre | Público desde internet |
|---|---|---|
| Security group EC2 | `proyectojo-ec2-sg` | 80/443 sí, 22 solo tu IP |
| Security group RDS | `proyectojo-rds-sg` | No — solo desde `proyectojo-ec2-sg` |
| Base de datos | `proyectojo-db` (PostgreSQL) | No |
| Servidor | `proyectojo-web` (Ubuntu, EC2) | Sí, por 80/443 |
| IP fija | Elastic IP asociada a `proyectojo-web` | — |

## Apagar/borrar todo después de la demo

Si esto era solo para una demo puntual y no va a seguir corriendo, hacé esto
para no seguir pagando (aunque sea centavos):

1. **RDS** → seleccioná `proyectojo-db` → **"Actions"** → **"Delete"** →
   destildá "Create final snapshot" (no la necesitás) → confirmá escribiendo
   `delete me` donde te lo pide.
2. **EC2** → **"Instances"** → seleccioná `proyectojo-web` → **"Instance
   state"** → **"Terminate instance"** (terminar, no solo "Stop" — "Stop" la
   deja apagada pero le sigue reservando el disco, que sigue costando algo).
3. **Elastic IPs** → seleccioná la IP → **"Actions"** → **"Release Elastic IP
   address"** (si no la liberás y no está asociada a nada, cobra).
4. Los security groups no cuestan nada, los podés dejar o borrar (EC2 →
   Security Groups → Delete) una vez que ya no hay nada usándolos.

Si en cambio esto va a seguir funcionando después de la demo, dejalo todo
como está y seguís directo al documento 4.

## Checklist antes de pasar al documento 4

- [ ] `proyectojo-ec2-sg` creado (80, 443 abiertos; 22 solo "My IP")
- [ ] `proyectojo-rds-sg` creado (5432 solo desde `proyectojo-ec2-sg`)
- [ ] Base de datos `proyectojo-db` en estado "Available", con el Endpoint
      copiado
- [ ] Instancia `proyectojo-web` en estado "Running", con el rol
      `proyectojo-ec2-role` asignado
- [ ] Archivo `.pem` descargado y guardado fuera del repo
- [ ] Elastic IP asociada a la instancia

Seguís en [Despliegue-AWS.md](Despliegue-AWS.md) para instalar todo adentro
del servidor (.NET, nginx) y conectar el pipeline de GitHub Actions.
