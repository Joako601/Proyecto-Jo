# AWS — Paso 2: Usuarios y permisos (IAM)

Segundo documento de la serie. Acá vamos a: proteger el root con MFA, y crear
los usuarios con los que realmente vamos a trabajar de acá en adelante.
Si todavía no leíste [AWS-1-Cuenta.md](AWS-1-Cuenta.md), empezá por ahí.

Esto es todo sobre un servicio de AWS llamado **IAM** (Identity and Access
Management — "gestión de identidad y acceso"). Es gratis, no tiene costo por
usarlo.

## Conceptos básicos (explicados sin dar nada por sabido)

AWS maneja el acceso con estas piezas, que se combinan entre sí:

| Concepto | Qué es | Analogía |
|---|---|---|
| **Root user** | El dueño de la cuenta. Ya lo creaste en el documento 1. | El dueño del edificio, tiene la llave maestra de todo. |
| **Usuario IAM** | Una identidad para una *persona* (vos, un colega) que inicia sesión con usuario/contraseña propios. | Un empleado con su propia tarjeta de acceso. |
| **Grupo IAM** | Una lista de usuarios que comparten los mismos permisos. En vez de darle permisos a cada usuario uno por uno, se los das al grupo. | Un "departamento" — todos los que están adentro tienen la misma tarjeta de acceso. |
| **Política (Policy)** | Un documento que dice exactamente qué se puede y no se puede hacer (ej. "puede crear servidores, no puede borrar la cuenta"). Se la pegás a un usuario, grupo o rol. | El reglamento escrito de qué puertas abre cada tarjeta. |
| **Rol IAM (Role)** | Como un usuario, pero no es para una persona — es para que un *servicio* de AWS (ej. el servidor) tenga permisos temporales, sin usuario ni contraseña. | Una llave que le das a un robot/máquina, no a una persona — y se la podés sacar cuando quieras sin afectar a nadie más. |
| **MFA** | Autenticación multifactor — además de la contraseña, un código de 6 dígitos que cambia cada 30 segundos, generado por una app en tu celular. | El segundo cerrojo de la puerta, además de la llave. |

La regla de oro que vamos a seguir: **las personas usan usuarios IAM (dentro
de un grupo), los servicios usan roles IAM. Nadie usa el root para el día a
día, y ningún servidor tiene jamás la contraseña ni las claves de una
persona.**

## 1. Activar MFA en el root (obligatorio)

Desde 2025, AWS **exige** MFA en la cuenta root — no es opcional. Vamos a
hacerlo primero, antes de tocar cualquier otra cosa.

1. Iniciá sesión en `https://console.aws.amazon.com` con el email y
   contraseña del root (documento 1).
2. Arriba a la derecha, hacé clic en el nombre de la cuenta → **"Security
   credentials"** (Credenciales de seguridad).
3. Buscá la sección **"Multi-factor authentication (MFA)"** → **"Assign MFA
   device"** (Asignar dispositivo MFA).
4. Elegí **"Authenticator app"** (aplicación de autenticación). Vas a
   necesitar una app en tu celular — si no tenés ninguna, instalá **Google
   Authenticator**, **Microsoft Authenticator**, o **Authy** (cualquiera de
   las tres, gratis, de la tienda de apps de tu celular).
5. AWS te muestra un **código QR**. Abrí la app del celular, elegí "agregar
   cuenta" / "escanear código QR", y escaneá lo que te muestra la pantalla.
6. La app va a empezar a mostrarte un código de 6 dígitos que cambia cada 30
   segundos. AWS te pide que ingreses **dos códigos seguidos** (esperá a que
   cambie entre uno y otro) para confirmar que quedó sincronizado.
7. Confirmá. De ahora en más, cada vez que inicies sesión como root, además
   de la contraseña te va a pedir el código de 6 dígitos de la app.

**Importante:** si perdés el celular con la app, podés perder el acceso al
MFA del root. AWS tiene un proceso de recuperación (verificación de
identidad más estricta), pero para evitar el dolor de cabeza: no desinstales
la app del celular, y si cambiás de celular, migrá la cuenta de MFA antes de
borrar el anterior.

## 2. Poner un alias a la cuenta (opcional pero recomendado)

Por defecto, el link para iniciar sesión como usuario IAM (no root) usa el
número de cuenta de 12 dígitos, algo como
`https://123456789012.signin.aws.amazon.com/console`. Le podés poner un
nombre:

1. Consola → buscador de arriba → escribí **"IAM"** → entrá al servicio IAM.
2. En el **Dashboard** de IAM, vas a ver "AWS Account" con un botón
   **"Customize"** al lado del alias.
3. Poné algo simple, ej. `proyectojo` (tiene que ser único en todo AWS, así
   que si está ocupado probá `proyectojo-demo` o similar).
4. A partir de ahora, el link de login es
   `https://proyectojo.signin.aws.amazon.com/console` — mucho más fácil de
   recordar y compartir si alguien más necesita loguearse.

## 3. Crear el grupo y usuario administrador (el que vas a usar vos)

Vamos a crear un **grupo** con los permisos, y después un **usuario** adentro
de ese grupo — nunca le pegamos permisos a un usuario directamente, siempre
a través de un grupo (así, si mañana necesitás otro usuario con el mismo
nivel de acceso, solo lo agregás al grupo).

### 3.1. Crear el grupo `Administradores`

1. En IAM (menú de la izquierda) → **"User groups"** → **"Create group"**.
2. Nombre del grupo: `Administradores`.
3. Más abajo, en **"Attach permissions policies"**, buscá y tildá
   **`AdministratorAccess`** (es una política que ya viene creada por AWS,
   no hay que escribir nada).
4. **"Create group"**.

**Qué permite `AdministratorAccess` exactamente:** acceso total a crear,
modificar y borrar *cualquier recurso* de AWS (servidores, bases de datos,
redes, otros usuarios IAM, etc.) — la única diferencia real con el root es
que **no puede**: cerrar la cuenta de AWS, cambiar el plan de soporte, ni
algunas acciones específicas de facturación/legal que AWS reserva
exclusivamente al root. Para el 99% del trabajo, es como tener el control
total.

### 3.2. Crear tu usuario personal

1. IAM → **"Users"** → **"Create user"**.
2. Nombre de usuario: el tuyo, ej. `joaquin`.
3. Tildá **"Provide user access to the AWS Management Console"** (para que
   este usuario pueda loguearse por la web, no solo usarse por línea de
   comandos).
4. Contraseña: **"Custom password"**, poné una propia y fuerte (o dejá que
   AWS la genere). Destildá "User must create a new password at next
   sign-in" si sos el único que la va a usar — o dejalo tildado si querés
   cambiarla en el primer login, es indistinto para este caso.
5. Siguiente → en **"Permissions options"** elegí **"Add user to group"** →
   tildá el grupo `Administradores` que creaste recién.
6. Revisá y **"Create user"**.
7. Al final te muestra las credenciales (usuario + contraseña + el link de
   login). **Copialas ahora** — la contraseña generada no se vuelve a
   mostrar después.

### 3.3. Activar MFA en tu usuario también

Mismo procedimiento que el paso 1, pero ahora parado en tu usuario:

1. IAM → **"Users"** → hacé clic en tu usuario (`joaquin`).
2. Pestaña **"Security credentials"** → **"Assign MFA device"**.
3. Repetís exactamente los mismos pasos con la app del celular (podés usar
   la misma app, se pueden tener varias cuentas ahí adentro).

A partir de ahora, **cerrá sesión del root y no lo vuelvas a usar** salvo que
necesites específicamente algo de facturación o cerrar la cuenta. Todo el
resto de este proyecto (documentos 3 y 4) lo hacés logueado como tu usuario
nuevo, en `https://proyectojo.signin.aws.amazon.com/console` (o la URL con
el número de cuenta si no pusiste alias).

## 4. Usuario de solo lectura (opcional, para invitados en la demo)

Si el día de la demo alguien más va a estar mirando la consola de AWS (para
ver el servidor prendido, logs, etc.) pero **no necesita poder cambiar nada**:

1. IAM → **"User groups"** → **"Create group"** → nombre `SoloLectura`.
2. Adjuntá la política **`ReadOnlyAccess`** (también ya viene creada por
   AWS).
3. IAM → **"Users"** → **"Create user"** → agregalo al grupo `SoloLectura`.

**Qué permite `ReadOnlyAccess` exactamente:** puede *ver* absolutamente todo
(instancias, bases de datos, configuración, logs), pero **no puede crear,
modificar, borrar, iniciar ni detener nada**. Ideal para mostrarle la consola
a alguien sin riesgo de que toque algo sin querer.

Si nadie más va a entrar a la consola, **te salteás este paso completo** — no
hace falta para que la demo funcione.

## 5. Rol IAM para el servidor (EC2) — sin usuario, sin contraseña

Esto es distinto a todo lo anterior: el servidor (que vamos a crear en el
documento 3) **no inicia sesión como una persona**. No tiene usuario ni
contraseña de AWS. En cambio, se le asigna un **rol**, que le da permisos
"prestados" mientras la máquina está prendida.

Hoy, con el diseño que armamos (el pipeline de GitHub se conecta por SSH, no
usa la API de AWS desde adentro del servidor), **el servidor no necesita
ningún permiso de AWS**. Aun así, es buena práctica crear el rol vacío ahora,
para tenerlo listo:

1. IAM → **"Roles"** → **"Create role"**.
2. **Trusted entity type**: **"AWS service"**.
3. **Use case**: elegí **"EC2"** de la lista.
4. Siguiente → en **"Add permissions"**, **no tildes ninguna política** (lo
   dejamos vacío a propósito).
5. Nombre del rol: `proyectojo-ec2-role`.
6. **"Create role"**.

Este rol se lo vas a asignar a la instancia EC2 cuando la crees en el
documento 3. Si en algún momento la app necesita hablar con otro servicio de
AWS (por ejemplo, guardar backups en S3), el permiso correspondiente se
agrega **a este rol**, nunca poniendo una clave de acceso (*access key*)
suelta adentro del servidor — esa es la fuga de credenciales más común y más
evitable en AWS.

## Resumen: quién puede hacer qué

| Identidad | Tipo | Login | Puede | No puede |
|---|---|---|---|---|
| **Root** | Cuenta | Email + contraseña + MFA | Todo, sin excepción | — |
| **`joaquin`** (grupo `Administradores`) | Usuario IAM | Usuario + contraseña + MFA | Crear/modificar/borrar cualquier recurso (servidores, bases de datos, otros usuarios) | Cerrar la cuenta, cambiar plan de soporte/facturación a nivel cuenta |
| **Usuario en `SoloLectura`** (opcional) | Usuario IAM | Usuario + contraseña | Ver todo | Crear, modificar, borrar, iniciar, detener cualquier cosa |
| **`proyectojo-ec2-role`** | Rol IAM | No inicia sesión — se asigna a la instancia EC2 | Nada, por ahora (vacío a propósito) | Todo, hasta que se le agregue algo explícitamente |

## Checklist antes de pasar al documento 3

- [ ] MFA activado en el root
- [ ] Grupo `Administradores` creado con la política `AdministratorAccess`
- [ ] Tu usuario personal creado, agregado a ese grupo, con MFA propio
- [ ] Ya no estás logueado como root (usás tu usuario de acá en adelante)
- [ ] (Opcional) grupo/usuario `SoloLectura` si alguien más va a mirar la
      consola
- [ ] Rol `proyectojo-ec2-role` creado (vacío)

Seguís en [AWS-3-Servicios.md](AWS-3-Servicios.md), ya logueado con tu
usuario nuevo.
