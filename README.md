# Prueba Técnica i-Route – Carga y validación de comercios

Solución Full Stack para cargar el archivo `commerce_DDMMYYYY.csv`, validar sus registros por fecha de proceso y consultar los registros rechazados (cuarentena) con su motivo.

| Capa | Tecnología |
|---|---|
| Base de datos | SQL Server (procedimientos almacenados) |
| Backend | .NET 8 – ASP.NET Core Web API, Dapper, CsvHelper |
| Frontend | Angular 22 (componentes standalone, signals, lazy loading), PapaParse |

---

## 1. Estructura del repositorio

```
prueba-iroute/
├── database/                      Scripts SQL (ejecutar en orden)
│   ├── 01_create_database.sql
│   ├── 02_tables.sql
│   ├── 03_sp_create_commerce.sql
│   ├── 04_sp_process_commerce.sql
│   ├── 05_sp_get_quarantine.sql
│   ├── 06_pruebas.sql             (opcional, datos de prueba)
│   └── 07_limpiar_datos.sql       (opcional, vacía las tablas)
├── samples/
│   └── commerce_13102026.csv      Archivo de ejemplo (8 válidos, 7 inválidos)
├── backend/
│   └── Commerce.Api/              API REST .NET 8
└── frontend/
    └── commerce-web/              Aplicación Angular
```

---

## 2. Requisitos

- SQL Server 2017 o superior (Express o Developer) y SSMS
- .NET 8 SDK (Visual Studio 2022 o `dotnet` CLI)
- Node.js LTS y Angular CLI 22 (`npm install -g @angular/cli`)

---

## 3. Instalación y ejecución

### 3.1 Base de datos

En SSMS, ejecutar en orden los scripts de la carpeta `database/`:

1. `01_create_database.sql` – crea la base `CommerceDB`
2. `02_tables.sql` – tablas `commerce` y `commerce_quarantine`
3. `03_sp_create_commerce.sql`
4. `04_sp_process_commerce.sql`
5. `05_sp_get_quarantine.sql`

Los scripts son re-ejecutables (`IF NOT EXISTS` / `CREATE OR ALTER`).

### 3.2 Backend

1. Revisar la cadena de conexión en `backend/Commerce.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "CommerceDb": "Server=localhost\\SQLEXPRESS;Database=CommerceDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```
   Ajustar `Server=` al nombre de la instancia local.
2. Ejecutar:
   - **Visual Studio:** abrir la solución y presionar F5 (perfil `https`), o
   - **CLI:**
     ```bash
     cd backend/Commerce.Api
     dotnet run --launch-profile https
     ```
3. Swagger queda disponible en `https://localhost:7220/swagger`.

### 3.3 Frontend

```bash
cd frontend/commerce-web
npm install
ng serve -o
```

La aplicación se abre en `http://localhost:4200`. La URL de la API se configura en `src/environments/environment.development.ts` (`apiUrl`).

---

## 4. Uso

1. **Carga** – seleccionar o arrastrar `commerce_DDMMYYYY.csv`, revisar la previsualización y presionar **Enviar al servidor**.
2. **Procesamiento** – indicar la fecha (`pc_processdate`) y presionar **Procesar**. Se muestra la cantidad de registros enviados a cuarentena.
3. **Cuarentena** – listado de registros rechazados con su motivo. Permite filtrar por fecha y buscar por texto.

Con el archivo de ejemplo `samples/commerce_13102026.csv` y la fecha `2026-10-13`, el resultado esperado es **7 registros en cuarentena** y 8 registros válidos en `commerce`.

---

## 5. Formato del archivo CSV

Nombre: `commerce_DDMMYYYY.csv` (ej. `commerce_13102026.csv`). Separador `,` o `;`, codificación UTF-8.

| Columna | Descripción |
|---|---|
| `pc_processdate` | Fecha de proceso (`yyyy-MM-dd` o `dd/MM/yyyy`) |
| `pc_codcomred` | Código del comercio en la red |
| `pc_nomcomred` | Nombre del comercio |
| `pc_tipdoc` | Tipo de documento (RUC / CED) |
| `pc_numdoc` | Número de documento |
| `pc_ciudad` | Ciudad |

---

## 6. API REST

Base: `https://localhost:7220/api/commerce`

| Método | Ruta | Entrada | Respuesta |
|---|---|---|---|
| POST | `/upload` | `multipart/form-data`, campo `file` | `200 { fileName, insertedCount }` / `400` |
| POST | `/process` | `{ "processDate": "2026-10-13" }` | `200 { processDate, quarantinedCount }` / `400` |
| GET | `/quarantine?processDate=2026-10-13` | Fecha opcional | `200 [ { id, pcProcessDate, pcNomComRed, pcNumDoc, …, motivo } ]` |

Los errores se devuelven en formato estándar **ProblemDetails** (RFC 7807):

```json
{ "status": 400, "title": "Solicitud inválida", "detail": "El archivo está vacío o no fue enviado." }
```

### Validaciones de la carga (`/upload`)

- El archivo no puede ser nulo ni estar vacío (incluye un archivo con solo el encabezado) → `400 BadRequest`
- El nombre debe cumplir `commerce_DDMMYYYY.csv` y `DDMMYYYY` debe ser una fecha real
- Tamaño máximo 10 MB
- Deben existir todas las columnas y `pc_processdate` debe ser una fecha válida (el error indica la fila)

---

## 7. Reglas de validación (`sp_process_commerce`)

| Regla | Motivo registrado |
|---|---|
| `pc_nomcomred` vacío o solo espacios | El nombre del comercio (nomcomred) se encuentra vacío |
| `pc_numdoc` vacío | El número de documento (numdoc) se encuentra vacío |
| `pc_numdoc` con letras | El número de documento (numdoc) contiene letras |
| `pc_numdoc` con caracteres especiales | El número de documento (numdoc) contiene caracteres especiales |

Si un registro incumple varias reglas, los motivos se concatenan con ` | `. Los registros inválidos se insertan en `commerce_quarantine` y se eliminan de `commerce` dentro de una **transacción**; el procedimiento retorna la cantidad de registros en cuarentena.

---

## 8. Decisiones de diseño

**Base de datos**
- Las columnas de texto de `commerce` (incluida `pc_numdoc`) son `NVARCHAR` y aceptan `NULL`: la tabla debe recibir datos sin validar, porque la validación ocurre después en el procedimiento.
- Llave primaria `id IDENTITY` para identificar con precisión las filas a mover, e índice sobre `pc_processdate`, columna por la que siempre se filtra.
- Validación basada en conjuntos (sin cursores) y comparaciones con intercalación binaria (`Latin1_General_100_BIN2`) para que `[a-z]` y `[0-9]` no dependan de la configuración regional.
- Mover a cuarentena y eliminar se ejecutan en una sola transacción con `XACT_ABORT ON`: no hay registros perdidos ni duplicados. Procesar dos veces la misma fecha devuelve 0 (idempotente).

**Backend**
- Arquitectura en capas: `Controller → Service → Repository → Stored Procedure`, con inyección de dependencias por interfaces.
- **Dapper** como micro-ORM: toda la lógica de datos está en procedimientos almacenados, por lo que no se requiere un ORM completo.
- La carga invoca `sp_create_commerce` por cada registro usando **una sola conexión y una sola transacción**; si una fila falla, se revierte el archivo completo.
- Manejo global de errores con `IExceptionHandler` (.NET 8): errores de validación → 400; errores inesperados → 500 con mensaje genérico (el detalle solo queda en el log).
- CORS habilitado para `http://localhost:4200` y `CancellationToken` propagado hasta SQL Server.

**Frontend**
- Componentes standalone con carga diferida (lazy loading) por ruta.
- Estado con **signals** (`signal`, `computed`), compatible con el modo zoneless de Angular 22.
- Las llamadas HTTP se centralizan en el servicio `CommerceApi`; los errores ProblemDetails se traducen a mensajes legibles.
- Validación en el cliente (nombre, tamaño, columnas) para respuesta inmediata, repetida en el servidor.
- La previsualización dibuja como máximo 100 filas para mantener la interfaz fluida con archivos grandes.

```
frontend/commerce-web/src/app/
├── core/
│   ├── models/commerce.models.ts     Tipos del CSV y de la API
│   ├── services/commerce-api.ts      Llamadas HTTP
│   ├── services/csv-reader.ts        Lectura del CSV para la previsualización
│   └── utils/                        Fechas y mensajes de error
└── pages/
    ├── upload/                       Pantalla de Carga
    ├── process/                      Pantalla de Procesamiento
    └── quarantine/                   Pantalla de Cuarentena
```

---

## 9. Mejoras propuestas

- Registrar los archivos cargados en una tabla de control para evitar cargar dos veces el mismo archivo.
- Para archivos muy grandes, reemplazar la llamada por registro con `SqlBulkCopy` o un parámetro con valores de tabla (TVP).
- Pantalla de inicio de sesión con autenticación JWT.
- Paginación del listado de cuarentena en el servidor.
- Pruebas unitarias del servicio (xUnit + mocks del repositorio) y de los componentes Angular.
