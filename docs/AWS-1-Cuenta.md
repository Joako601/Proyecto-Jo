# AWS — Paso 1: Crear y dar de alta la cuenta

Primer documento de una serie de tres, pensada para alguien que nunca usó
AWS:

1. **AWS-1-Cuenta.md** (este documento) — crear la cuenta.
2. [AWS-2-Usuarios.md](AWS-2-Usuarios.md) — asegurar la cuenta y crear los
   usuarios con los que vas a trabajar (nunca con el root).
3. [AWS-3-Servicios.md](AWS-3-Servicios.md) — crear los recursos reales
   (el servidor, la base de datos).
4. [Despliegue-AWS.md](Despliegue-AWS.md) — instalar todo adentro del
   servidor y conectar el pipeline de GitHub.

Seguí ese orden. Si creás recursos (paso 3) antes de tener los usuarios bien
armados (paso 2), vas a terminar haciendo todo con el usuario root, que es
justo lo que queremos evitar.

## Qué es una "cuenta de AWS" y qué no es

Una cuenta de AWS **no es un usuario** en el sentido de "usuario y
contraseña" común. Es más parecido a abrir una cuenta bancaria: adentro de
esa cuenta después vas a crear "usuarios" (personas o servicios que pueden
entrar) con permisos distintos. La cuenta en sí es la que tiene la tarjeta de
crédito asociada y la que paga todo lo que se use adentro.

Vamos a crear **una sola cuenta** para este proyecto. No hace falta crear una
cuenta separada por servicio ni nada parecido — eso viene después, con
usuarios dentro de la misma cuenta (documento 2).

## Antes de empezar, necesitás

- Un **email** que no esté ya usado en otra cuenta de AWS. Si tenés Gmail,
  cualquier variante con `+algo` antes de la arroba también funciona como
  dirección distinta si querés separar esto de tu email personal
  (`tuemail+proyectojo@gmail.com`).
- Una **tarjeta de crédito o débito internacional** (Visa/Mastercard). AWS la
  pide sí o sí para validar la cuenta, aunque te quedes dentro de la capa
  gratuita (*Free Tier*) y no te cobre nada.
- Un **teléfono celular** propio, vas a recibir un código por SMS o llamada
  para verificar la identidad.
- 15-20 minutos, y después hasta 24 horas de espera para que la cuenta quede
  100% activada (normalmente son solo un par de minutos).

## Paso a paso

### 1. Ir al sitio de AWS

Entrá a **[aws.amazon.com](https://aws.amazon.com)** y buscá el botón
**"Create an AWS Account"** / **"Crear una cuenta de AWS"** (arriba a la
derecha, o en el centro de la página principal).

### 2. Email y nombre de cuenta

Te va a pedir:

- **Email address** (root user email): el email que elegiste arriba.
- **AWS account name**: un nombre para identificar la cuenta, ej.
  `ProyectoJo`. No es el nombre de una persona, es el nombre de la cuenta —
  podés cambiarlo después si querés.

Después de cargar esto, AWS manda un **código de verificación al email**.
Andá a tu bandeja de entrada, copiá el código, y pegalo donde te lo pide.

### 3. Contraseña del root user

Te va a pedir crear la contraseña de lo que se llama el **root user**
(usuario raíz). Explico qué es esto en detalle en el próximo documento, por
ahora solo importa: **usá una contraseña larga y única, y guardala en un
gestor de contraseñas** (no la reutilices de otro lado). Esta contraseña
tiene, literalmente, control total sobre todo lo que se cree en la cuenta.

### 4. Tipo de cuenta

Te pregunta si la cuenta es **"Personal"** o **"Business"**. Para este
proyecto, elegí **Personal** — las funciones son las mismas, "Business" solo
pide más datos (razón social, etc.) que no necesitamos.

### 5. Datos de contacto

Nombre completo, teléfono, dirección, país. Usá tus datos reales — AWS los
puede pedir para verificar identidad más adelante, y para facturación.

### 6. Datos de la tarjeta

Se cargan como en cualquier compra online. **No te van a cobrar nada ahora**
— es solo para verificar que la tarjeta es válida (puede aparecer un cargo
de prueba de USD 1 que se revierte solo, dependiendo del banco).

### 7. Verificación por teléfono

Elegís el código de país (ej. `+54` Argentina) y ponés tu número. AWS te
manda un **código por SMS o te llama** con un PIN de 4-6 dígitos. Lo cargás
en la página y confirmás.

### 8. Elegir el plan de soporte

Te muestra varios planes: **Basic (gratis)**, Developer, Business, Enterprise
(estos últimos con costo mensual). **Elegí "Basic support - Free"**. No
necesitamos soporte pago para una demo — Basic incluye acceso a
documentación, foros, y el Centro de Ayuda, que alcanza y sobra.

### 9. Confirmación

Después de elegir el plan, AWS te muestra una pantalla diciendo que la cuenta
se está activando. Como dijimos, normalmente son minutos, en algún caso raro
puede tardar hasta 24 horas.

## Elegir la región (importante, hacelo ahora)

Una vez que puedas entrar a la **AWS Management Console**
(`https://console.aws.amazon.com`), vas a ver, arriba a la derecha, un
selector con el nombre de una región (ej. "N. Virginia", "São Paulo").

Una **región** es, literalmente, un lugar físico del mundo donde están los
centros de datos de AWS (ej. `us-east-1` = Virginia, EE. UU.;
`sa-east-1` = São Paulo, Brasil). **Todo lo que crees — el servidor, la base
de datos — tiene que estar en la misma región**, porque si no, no se ven
entre sí.

Recomendación para este proyecto (Argentina/Latinoamérica): **`sa-east-1`
(São Paulo)** — es la región de AWS más cercana geográficamente, así que la
demo va a sentirse más rápida. Elegila una vez, arriba a la derecha, y de ahí
en adelante **fijate siempre que diga esa misma región** antes de crear
cualquier recurso — es el error más común de gente que recién arranca con
AWS: crear algo en una región y después no encontrarlo porque estás mirando
otra.

## El root user: qué puede hacer y qué no deberías hacer con él

| | |
|---|---|
| **Puede hacer** | Literalmente todo: crear/borrar cualquier recurso, cambiar el método de pago, cerrar la cuenta entera, cambiar el plan de soporte, ver y cambiar todos los permisos de todos los usuarios. |
| **No lo vas a usar para** | Nada del trabajo diario: no para crear el servidor, no para crear la base de datos, no para nada de lo que sigue en los próximos documentos. |
| **Por qué** | Si alguien roba la contraseña del root, tiene control total e irreversible de la cuenta (incluso te puede sacar a vos). Un usuario IAM normal, en cambio, se puede desactivar o borrar en segundos sin perder el control de la cuenta. |

A partir del próximo documento, la contraseña del root prácticamente no se
vuelve a escribir. Vas a iniciar sesión con un usuario nuevo que creamos ahí,
que tiene casi los mismos permisos pero es reemplazable.

## Checklist antes de pasar al documento 2

- [ ] Cuenta creada y con el mensaje de "cuenta activada" (o ya pasaron unos
      minutos desde el alta)
- [ ] Pudiste iniciar sesión en `https://console.aws.amazon.com` con el email
      y contraseña del root
- [ ] Elegiste la región (`sa-east-1` recomendado) y la vas a mantener fija
      en todo el proyecto
- [ ] Guardaste la contraseña del root en un lugar seguro (gestor de
      contraseñas), no en un archivo del repo ni en texto plano

Con esto, seguís en [AWS-2-Usuarios.md](AWS-2-Usuarios.md).
