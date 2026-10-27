# Veterinaria — Sistema Web de Gestión Veterinaria

**Repositorio:** [CamargoNatalia/Veterinaria](https://github.com/CamargoNatalia/Veterinaria)

## 1. Descripción del proyecto

Veterinaria es una aplicación web orientada a la administración de una clínica veterinaria. Permite centralizar la información de clientes, mascotas, especies, razas, veterinarios, turnos, consultas e historias clínicas.

El objetivo del proyecto es optimizar los procesos administrativos y clínicos de una veterinaria mediante una solución web que centraliza la información, facilita la gestión de turnos y consultas, mantiene un registro organizado de los antecedentes médicos de las mascotas y controla el acceso a las funcionalidades mediante autenticación y roles.

El sistema utiliza ASP.NET Core MVC, vistas Razor, Entity Framework Core y MySQL. También incorpora un módulo Vue.js que intercambia datos JSON con el servidor, búsquedas asíncronas y una Web API REST protegida mediante JWT.

## 2. Integrantes

- Guillermo Concha (gconcha@ulp.edu.ar)
- Natalia Camargo (camargonatalia83@gmail.com)

## 3. Tecnologías y arquitectura

- **Backend:** C#, ASP.NET Core MVC y .NET 10.
- **Persistencia:** Entity Framework Core, proveedor Pomelo para MySQL y migraciones.
- **Frontend:** vistas Razor, HTML, CSS, JavaScript y Vue.js 3.
- **Interacción asíncrona:** `fetch`, AJAX y respuestas JSON.
- **Autenticación:** cookies para la aplicación MVC y tokens JWT Bearer para la Web API de Mascotas.
- **Autorización:** atributos `[Authorize]` y restricciones según rol.
- **Organización del código:** modelos, controladores, vistas, interfaces, repositorios y contexto de base de datos, con inyección de dependencias.
- **Herramientas:** GitHub y Postman.

El módulo Vue.js utiliza la sesión MVC mediante cookies.

## 4. Funcionalidades

- Administración de clientes, mascotas, especies y razas mediante operaciones CRUD.
- Gestión de usuarios: inicio y cierre de sesión, perfiles, cambio de contraseña y avatar.
- Control de permisos para los roles **Administrador**, **Empleado** y **Veterinario**.
- Administración de veterinarios, turnos y consultas.
- Registro de consultas asociadas a mascotas, usuarios, veterinarios y turnos.
- Almacenamiento y consulta de archivos clínicos asociados a consultas, accesibles desde el historial de la mascota.
- CRUD de especies mediante Vue.js y peticiones HTTP asíncronas, sin recargar la página.
- Búsqueda AJAX de clientes y búsqueda asíncrona de mascotas en formularios de Turnos y Consultas.
- Paginación del lado del servidor en los listados principales.
- Web API REST de solo lectura de mascotas, autenticada mediante JWT.

### Roles y ejemplo de permisos

El acceso se comprueba en el servidor y no únicamente ocultando botones en las vistas.

Por ejemplo, `ClienteController` permite consultar clientes a los tres roles, mientras que las operaciones de creación, edición y eliminación están limitadas a **Administrador** y **Empleado**.

## 5. Modelo de datos

El contexto `Data/VeterinariaContext.cs` declara nueve entidades principales:

| Entidad     | Finalidad                                            |
| ----------- | ---------------------------------------------------- |
| Cliente     | Datos de las personas responsables de las mascotas   |
| Mascota     | Información de los animales registrados              |
| Especie     | Catálogo de especies                                 |
| Raza        | Catálogo de razas                                    |
| Usuario     | Cuentas, roles y credenciales del sistema            |
| Veterinario | Datos profesionales y asociación a un usuario        |
| Turno       | Citas programadas para las mascotas                  |
| Consulta    | Atención clínica, diagnóstico y tratamiento indicado |
| HClinica    | Metadatos de archivos clínicos asociados a consultas |

### Relaciones relevantes

- Cliente 1:N Mascota.
- Especie 1:N Raza.
- Especie 1:N Mascota.
- Raza 1:N Mascota.
- Mascota 1:N Turno.
- Usuario 1:N Turno.
- Mascota 1:N Consulta.
- Usuario 1:N Consulta.
- Veterinario 1:N Consulta, con vínculo opcional desde `Consulta`.
- Consulta 1:N HClinica.
- Usuario 1:0..1 Veterinario.
- Turno 1:0..1 Consulta, con vínculo opcional desde `Consulta`.

**Diagrama Entidad-Relación:** [Ver DER](docs/der.png).

La entidad `Consulta` se relaciona obligatoriamente con `Mascota` y `Usuario`, y opcionalmente con `Veterinario` y `Turno`. Estas relaciones permiten registrar las atenciones médicas, identificar a la mascota atendida y al usuario responsable del registro, cumpliendo con el requisito de una entidad relacionada con otras dos.

## 6. Matriz de evidencias de requerimientos mínimos

La siguiente matriz identifica el cumplimiento de los requisitos técnicos establecidos por el profesor.

📄 [Ver Matriz de Evidencias de Veterinaria](docs/MatrizEvidencias_Veterinaria.docx)

## 7. Web API REST y JWT

La Web API de mascotas expone las siguientes rutas:

| Método | Ruta                 | Descripción                                                                     |
| ------ | -------------------- | ------------------------------------------------------------------------------- |
| POST   | `/api/auth/login`    | Valida las credenciales y devuelve un JWT                                       |
| GET    | `/api/mascotas`      | Devuelve las mascotas; requiere JWT válido y rol autorizado                     |
| GET    | `/api/mascotas/{id}` | Devuelve una mascota por su identificador; requiere JWT válido y rol autorizado |

El token se envía mediante el encabezado `Authorization: Bearer <TOKEN>`.

La API de Mascotas admite los roles **Administrador**, **Empleado** y **Veterinario**. Para comprobar la autenticación y la autorización se incluye la colección de Postman, junto con las evidencias de las respuestas HTTP `200`, `401` y `403`.

- [Colección de Postman](docs/Veterinaria%20-%20Web%20API%20JWT.postman_collection.json)
- [Evidencias de pruebas Postman](docs/evidencias-postman/)

## 8. Instalación y ejecución local

### Requisitos previos

- SDK de .NET 10.
- Servidor MySQL accesible.
- Herramientas de Entity Framework Core (`dotnet-ef`).
- Git.

### Clonar el repositorio

```bash
git clone https://github.com/CamargoNatalia/Veterinaria.git
cd Veterinaria
dotnet restore
```

### Configurar la conexión y la clave JWT

Configurar `ConnectionStrings:VeterinariaConnection` para MySQL y `Jwt:Key` mediante **User Secrets** o variables de entorno.

La clave JWT debe tener al menos 32 bytes. No se deben publicar contraseñas ni claves secretas en el repositorio.

Ejemplo con User Secrets, desde la carpeta del proyecto:

```bash
dotnet user-secrets set "ConnectionStrings:VeterinariaConnection" "server=localhost;database=veterinaria;user=USUARIO;password=CLAVE"
dotnet user-secrets set "Jwt:Key" "REEMPLAZAR_POR_UNA_CLAVE_ALEATORIA_DE_32_BYTES_O_MAS"
```

### Preparar la base de datos

Hay dos alternativas:

**Opción A: crear o actualizar la base mediante migraciones**

Con MySQL configurado y disponible, ejecutar:

```bash
dotnet ef database update
```

**Opción B: restaurar el script SQL**

También se puede utilizar el [script SQL con estructura y datos de prueba](docs/base-datos.sql).

Estas alternativas deben tratarse como procedimientos distintos de preparación. Si se restaura el SQL sobre una base que ya contiene las tablas y los datos, no se deben aplicar las migraciones indiscriminadamente sobre esa misma base.

### Ejecutar la aplicación

Desde la carpeta del proyecto:

```bash
dotnet run
```

Abrir en el navegador la URL local que informe la aplicación al iniciarse.

## 9. Usuarios de prueba

Las siguientes credenciales permiten probar los distintos roles de la aplicación:

| Rol           | Correo         | Contraseña de prueba |
| ------------- | -------------- | -------------------- |
| Administrador | `nc@gmail.com` | **`87654321`**       |
| Empleado      | `gc@gmail.com` | 0987654              |
| Veterinario   | `jl@gmail.com` | `666333`             |

Estas credenciales son para el entorno de prueba del proyecto. No deben reutilizarse en un entorno de producción.

## 10. Repositorio y documentación

- [Código fuente](./)
- [Migraciones de Entity Framework Core](Migrations/)
- [.gitignore](.gitignore)
- [Colección de Postman](docs/Veterinaria%20-%20Web%20API%20JWT.postman_collection.json)
- [Evidencias de pruebas Postman](docs/evidencias-postman/)
- [Diagrama Entidad-Relación (DER)](docs/der.png)
- [Archivo editable del DER](docs/der_veterinaria.drawio)
- [Base de datos: estructura y datos de prueba (SQL)](docs/base-datos.sql)

## 11. Equipo y propósito académico

Este proyecto se desarrolla como trabajo integrador final de Lab2 y aplica conceptos de desarrollo web MVC, persistencia relacional, autenticación y autorización, manejo de archivos, interacción asíncrona, Vue.js y diseño de Web API REST protegidas mediante JWT.
