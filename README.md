# PROYECTO MONSTER — Sistema de Gestión de Proyectos

Sistema web (MVC + API REST) para la administración integral de proyectos, personal, finanzas y seguridad. Desarrollado en **ASP.NET Core 8** con **SQL Server** como motor de base de datos.

## Descripción general

El aplicativo permite gestionar el ciclo de vida completo de un proyecto: planificación por fases, actividades y tareas con dependencias, asignación de equipo, registro y aprobación de horas, seguimiento de avance, control presupuestal y cierre. Incluye módulos de administración de personal (personas, familiares, formación, contratos), finanzas por proyecto (presupuesto, categorías de gasto, movimientos) y un módulo de seguridad con usuarios, perfiles, permisos por opción de menú y auditoría.

## Tecnologías

- **.NET 8** (ASP.NET Core MVC + Web API)
- **SQL Server** (base de datos relacional)
- **Microsoft.Data.SqlClient** (acceso a datos)
- **Swashbuckle / Swagger** (documentación de la API)
- **HTML / CSS / JavaScript** (Razor Views)

## Módulos principales

- **Autenticación y Seguridad**: login, sesiones, usuarios, perfiles, permisos por opción de menú, bloqueo por intentos fallidos y auditoría de operaciones.
- **Personal**: personas, empleados, familiares, formación académica, departamentos, cargos y contratos.
- **Proyectos**: registro de proyectos con código, presupuesto, prioridad y estado; asignación de empleados por rol y disponibilidad; fases, actividades, tareas, dependencias y entregables; cálculo de avance ponderado y validación de cierre.
- **Seguimiento**: registro y aprobación de horas, historial de avance y estados, reportes.
- **Finanzas**: categorías de gasto, parámetros financieros, movimientos (ingresos/gastos) con aprobación, costo laboral y control de saldo disponible con alertas de consumo.

## Estructura del proyecto

```
04. APLICATIVO/PROYECTO_MONGO_DOTNET/
├── Controllers/        # Api (V1) y Web (MVC)
├── Services/           # Lógica de negocio (Proyectos, Finanzas, Seguridad)
├── Repositories/       # Acceso a datos (DAO)
├── Models/             # Entidades, DTOs y ViewModels
├── Database/SqlServer/ # Scripts de evolución de la base de datos
├── Infrastructure/     # SqlContexto (conexión y acceso a datos)
├── Middleware/         # Manejo global de excepciones
└── Views/              # Vistas Razor
```

## Base de datos

- Base de datos: `PROYECTO_MONSTER`
- Esquema: `app`
- Scripts de creación (DDL) y datos iniciales (DML) en la carpeta `03. BDD/`
- Scripts de evolución del esquema en `Database/SqlServer/`

Conexión (appsettings.json):

```json
"ConnectionStrings": {
  "SqlServer": "Server=localhost;Database=PROYECTO_MONSTER;Integrated Security=True;TrustServerCertificate=True;Encrypt=True"
}
```

> La cadena también puede definirse mediante la variable de entorno `SQLSERVER_CONNECTION_STRING`.

## Cómo ejecutar

1. Restaurar la base de datos `PROYECTO_MONSTER` (ver respaldo en `03. BDD/04. RESPALDO/`).
2. Compilar y ejecutar: `dotnet run` dentro de la carpeta del proyecto.
3. Abrir la interfaz web (MVC) o la documentación Swagger (solo en desarrollo).

## Respaldo de base de datos

La copia de seguridad completa (FULL) de la base de datos se encuentra en:

```
03. BDD/04. RESPALDO/PROYECTO_MONSTER_FULL_20260804.bak
```

Para restaurarla:

```sql
RESTORE DATABASE [PROYECTO_MONSTER]
FROM DISK = N'...\PROYECTO_MONSTER_FULL_20260804.bak'
WITH REPLACE;
```

## Documentación

En `05. DOCUMENTACIÓN/` se incluyen el informe técnico (Word) y la presentación (PowerPoint) del proyecto.
