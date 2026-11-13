# Refactorización incremental de Master Monster

Fecha: 13 de noviembre de 2026. Alcance: exclusivamente este aplicativo. No se modificaron scripts SQL, esquemas, archivos externos, credenciales ni respaldos; no hubo commits, pushes ni operaciones contra una base real.

## 1. Diagnóstico y problemas comprobados

- Proyecto ASP.NET Core MVC/Web API de .NET 8 con Microsoft.Data.SqlClient y Swagger. Controladores web heredados conviven con servicios y repositorios de proyectos, planificación, seguridad y finanzas.
- `Program.cs` configura sesiones, permisos propios, antifalsificación y middleware de errores. La sesión se consideraba válida solo por la presencia de `usuarioLogueado`; cambios de estado, perfil y obligatoriedad de clave podían quedar desactualizados durante una sesión existente.
- Empleados, departamentos y cargos eliminaban mediante GET y no comprobaban permisos de operación en el servidor. Logout también cambiaba estado mediante GET.
- El historial aceptaba POST sin validación antifalsificación; sus dos consumidores JavaScript no enviaban token. El borrado no comprobaba permiso de eliminación.
- Los formularios financieros compartían creación y edición, pero sus atributos exigían únicamente permiso de creación.
- La actualización de empleados confiaba en `codigoPersona` y `fotoActual` enviados por el cliente. Las fotos se validaban por extensión/MIME declarados, eran públicas y sus nombres contenían identificadores personales. El controlador mostraba detalles de excepciones internas.
- Guardar/actualizar/eliminar personal modificaba varias tablas con conexiones/transacciones independientes. Las operaciones sobre formación y familiares podían quedar separadas del empleado y la persona.
- La suma de gastos aprobados no filtraba `Tipo`; los ingresos aprobados incrementaban el gasto. Una revisión financiera podía sobrescribir otra y reescribir campos financieros leídos antes de una edición concurrente.
- Los resúmenes de costos y listados de proyectos consultaban información financiera por proyecto. Los listados de empleados consultaban persona, departamento y cargo por fila.
- El incremento de intentos fallidos hacía lectura y actualización separadas. La normalización de errores de model binding podía generar claves duplicadas y una excepción en `ToDictionary`.
- `ConsultarAsync` hacía lecturas síncronas de filas. El mapeo repetía búsqueda de propiedades por columna/fila. La apertura síncrona no liberaba la conexión si `Open` fallaba.

La compilación inicial se intentó con `dotnet build --no-restore`: falló antes de compilar porque no hay SDK .NET instalado. No se pudo establecer si existían errores de compilación previos.

## 2. Mejoras y vulnerabilidades corregidas en el código

- Filtro global de sesión: reconsulta estado de la cuenta, perfil vigente y cambio obligatorio de contraseña antes de los filtros de autorización y antifalsificación. Rechaza cuentas inactivas/bloqueadas y evita usar un perfil revocado de la sesión. Las respuestas autenticadas llevan `Cache-Control: no-store`.
- Política global de antifalsificación para métodos que modifican datos; registro explícito de caché de memoria requerida por sesiones. Solo entrada y login permiten acceso anónimo.
- Eliminación mediante POST, token y permiso específico en personal, cargos y departamentos. Sus antiguas solicitudes GET de eliminación devuelven 405; los formularios y confirmaciones conservan las clases visuales.
- Logout pasa a POST con token. Historial exige token y permiso de eliminación del reporte correspondiente.
- Permisos financieros distinguen crear/editar usando los identificadores enviados por formulario, query o ruta, incluyendo prefijos de model binding como `dto.CodigoContrato`. Un identificador inexistente ya no se convierte silenciosamente en alta para contratos, categorías o movimientos.
- Los permisos heredados se resuelven contra la URL existente de `app.Opcion`, sin inventar códigos de catálogo. Los perfiles deben estar activos.
- La persona y foto anteriores se recuperan del empleado persistido. El repositorio también impide cambiar la asociación persona–empleado durante una actualización.
- Servicio de fotografías con límite de 2 MB, extensiones/MIME compatibles y firmas PNG/JPEG/WebP; nombres GUID, límite efectivo al leer, limpieza de cargas fallidas y comprobación de pertenencia de rutas. Las URLs existentes pasan por un controlador autorizado, conservando las imágenes y sus rutas.
- Errores inesperados de personal se registran con `ILogger` y se presentan mediante un mensaje genérico. La cancelación del cliente no se registra como error interno.
- Los intentos fallidos se incrementan y bloquean mediante un único `UPDATE ... OUTPUT`.
- Validación de datos de movimientos, fecha y tipo; se comprueba empleado activo al guardar contratos.

No se cambió el algoritmo de contraseñas: se conserva PBKDF2 y la migración existente desde MD5 al verificar una clave válida.

## 3. Arquitectura y atomicidad

- Se conserva la arquitectura actual y ADO.NET; no se agregan proyectos de producción ni ORM.
- `SesionActivaFiltro` es scoped; servicios/repositorios sin estado continúan con su registro singleton. No se guardan conexiones o transacciones en campos compartidos.
- `FotoEmpleadoServicio` extrae almacenamiento y validación técnica del controlador.
- `IMovimientoProyectoRepositorio`, `ICategoriaGastoRepositorio` e `ICostoProyectoRepositorio` permiten aislar acceso a datos y probar reglas sin SQL Server.
- `ProyectoTransaccionRepositorio` guarda cambio de estado, seguimiento y cierre de asignaciones en una transacción. Compara el estado esperado para detectar transiciones concurrentes.
- Guardar, actualizar y eliminar empleados ahora comparten una transacción para persona, empleado, familiares y formación. Los repositorios hijos aceptan una transacción externa y conservan funcionamiento autónomo. La disposición de una transacción sin commit revierte los cambios.
- Persistir una asignación usa aislamiento serializable, bloqueo del empleado, comprobación de proyecto abierto/fechas, duplicados y dedicación antes de escribir. La comprobación ocurre dentro de la misma transacción que la escritura; no depende únicamente de la validación previa del servicio.
- Revisar movimientos usa una actualización condicional del estado pendiente y de la fecha de modificación esperada. Solo escribe los campos de revisión. Si otra solicitud ganó o modificó la lectura, el servicio devuelve conflicto 409. Una edición posterior a la aprobación tampoco puede reabrir el movimiento.
- Transiciones y avance ponderado quedan en reglas puras. La detección de ciclos usa recorrido iterativo y evita desbordar la pila con cadenas extensas.

## 4. Rendimiento

- Listado presupuestario: una consulta agregada de proyectos/costos/gastos y una del umbral, en lugar de consultas por cada proyecto. Solo se suman tareas activas y movimientos de tipo GASTO aprobados. Se conserva la fórmula y el redondeo financiero existentes.
- Listado de proyectos: reutiliza el mapa de costos calculado por lote.
- Listados de empleados: cuatro consultas por listado no vacío —empleados, personas, departamentos y cargos— en lugar de una más tres por empleado. Todavía se cargan catálogos/personas completos; esto se identifica como limitación para volúmenes grandes.
- Búsqueda de empleado activo por código: filtro SQL puntual en lugar de cargar todos los activos.
- Infraestructura SQL: caché de metadatos por tipo, resolución de columnas una vez por resultado, `ReadAsync`, tokens de cancelación opcionales y liberación ante fallo de apertura. Parámetros de fecha tipados como `DateTime2`.
- Cliente HTTP compartido para reportes: reutiliza token de formulario o una sola petición concurrente de token, propaga fallos HTTP y permite reintento tras un fallo de obtención.

Estas reducciones se deducen de las rutas de código; no son mediciones de latencia ni planes de ejecución de SQL Server.

## 5. Archivos cambiados y responsabilidad

| Archivos relativos a este directorio | Responsabilidad |
| --- | --- |
| `Program.cs` | Filtros globales, sesión/caché, DI y exclusión de fotos del middleware público de estáticos. |
| `PROYECTO_MONGO_DOTNET.csproj` | Excluir las fuentes/contenido de pruebas del aplicativo publicado. |
| `Infrastructure/Sql/SqlContexto.cs` | Recursos, transacciones explícitas, cancelación, lectura asíncrona, parámetros de fecha y mapeo. |
| `Middleware/ManejoExcepcionesMiddleware.cs` | Distinguir cancelación del cliente de fallo interno. |
| `Services/Seguridad/SesionActivaFiltro.cs` | Estado actual de cuenta/perfil y obligatoriedad de clave. |
| `Services/Seguridad/PermisoModuloAttribute.cs` | Permisos heredados por URL y operación. |
| `Services/Seguridad/Permisos.cs` | Selección de permiso crear/editar según datos de la solicitud. |
| `Repositories/Seguridad/PermisoDAO.cs` | Perfil activo y búsqueda parametrizada de permisos por URL. |
| `Repositories/Seguridad/UsuarioDAO.cs` | Contador atómico de intentos fallidos. |
| `Controllers/Web/AuthController.cs` | Acceso anónimo explícito y logout POST. |
| `Controllers/Web/CargoController.cs`, `DepartamentoController.cs` | Autorización y eliminación POST. |
| `Controllers/Web/EmpleadoController.cs` | Autorización, eliminación POST, asociación persistida, delegación de fotos y errores seguros. |
| `Controllers/Web/FotoEmpleadoController.cs` | Descarga autorizada de fotos con `nosniff`. |
| `Services/Personal/FotoEmpleadoServicio.cs` | Validación y almacenamiento de fotografías. |
| `Repositories/Personal/EmpleadoDAO.cs` | Transacción de personal, integridad de asociación y eliminación de N+1 en listados. |
| `Repositories/Personal/PersonaDAO.cs`, `FamiliarDAO.cs`, `FormacionDAO.cs` | Participación en transacciones externas. |
| `Controllers/Web/ReporteController.cs` | Antifalsificación y permiso de borrado. |
| `Controllers/Web/CatalogosFinancierosController.cs`, `ContratosController.cs`, `MovimientosFinancierosController.cs` | Permisos diferenciados de alta y edición. |
| `Repositories/IMovimientoProyectoRepositorio.cs` | Interfaces de movimientos y categorías. |
| `Repositories/FinanzasRepositorios.cs` | Gastos por tipo/estado y escrituras condicionales de movimientos. |
| `Repositories/CostoProyectoRepositorio.cs` | Proyección agregada de costos. |
| `Services/Finanzas/CostoProyectoServicio.cs` | Fórmula pura y consulta por lote. |
| `Services/Finanzas/MovimientoProyectoServicio.cs`, `ReglasFinanzas.cs` | Validación financiera y conflictos de revisión. |
| `Services/Finanzas/ContratoServicio.cs`, `CatalogosFinancierosServicio.cs` | Empleado activo e identificadores inexistentes. |
| `Repositories/AsignacionProyectoRepositorio.cs` | Validación transaccional de capacidad y duplicados. |
| `Repositories/ProyectoTransaccionRepositorio.cs` | Transición, seguimiento y cierre atómicos. |
| `Services/Proyectos/SeguimientoProyectoServicio.cs` | Delegación de escritura transaccional y eliminación de escrituras duplicadas. |
| `Services/Proyectos/ProyectoServicio.cs` | Costos por lote para el listado. |
| `Services/Proyectos/ReglasProyecto.cs` | Regla pura de transiciones existentes. |
| `Services/Proyectos/ReglasPlanificacion.cs`, `PlanificacionProyectoServicio.cs` | Avance centralizado y detección iterativa de ciclos. |
| `Views/Cargo/Cargos.cshtml`, `Views/Departamento/Departamentos.cshtml`, `Views/Empleado/Empleados.cshtml` | Formularios POST de borrado y tokens. |
| `Views/Reportes/Index.cshtml` | Token en eliminación de historial. |
| `Views/Shared/_Layout.cshtml`, `_ModuleLayout.cshtml` | Carga del cliente HTTP común y formulario de logout. |
| `wwwroot/CSS/estilos.css` | Clase mínima para formularios de acción en línea. |
| `wwwroot/js/http-compartido.js`, `reportes.js`, `reportes-parametrizados.js` | HTTP compartido y consumidores con antifalsificación. |
| `Tests/MasterMonster.Pruebas.csproj`, `Tests/Program.cs` | Ejecutable de 17 casos C# sin base externa. |
| `Tests/http-compartido.test.cjs`, `Tests/README.md` | Cuatro tests Node y procedimiento de verificación/integración. |
| `INFORME_REFACTORIZACION.md` | Diagnóstico, resultados, alcance y pendientes. |

## 6. Contratos y decisiones preservadas

- Se preservan rutas de consulta, DTO, envoltorios de respuestas, vistas, modales, tablas, calendarios y Gantt. No se realizó una comparación visual ejecutada.
- Cambios HTTP deliberados por seguridad: POST `/Logout`, `/EmpleadoController/eliminar`, `/DepartamentoController/eliminar`, `/CargoController/eliminar`. Los consumidores Razor se actualizaron. Los POST de historial requieren token.
- Las URL `/uploads/empleados/{nombre}` permanecen, pero requieren sesión y permiso de lectura del módulo de empleados.
- Las URL de catálogo usadas para personal son las mismas que ya consume el menú: `empleados.jsp`, `departamentos.jsp`, `cargos.jsp`. Deben comprobarse con el catálogo de la base de prueba.
- `06_refactor_proyectos_contextual.sql` documenta que `PRO_ASIGNAR`, `PRO_CIERRE` y `RASI` están ocultos del menú pero conservan permisos históricos para backend. Por eso no se exige `Opcion.Estado='A'` en la autorización general por código. No se ejecutó ni modificó esa migración.
- `04_relaciones_cronograma_sin_horas.sql` retira explícitamente el registro de horas del alcance, inactiva opciones y retira permisos. `RegistroHoraRepositorio` está aislado de la aplicación; se conservó sin conectarlo ni inventar un módulo nuevo.

## 7. Verificaciones realmente ejecutadas

| Comprobación | Resultado |
| --- | --- |
| Estado Git inicial | Sin cambios locales reportados. |
| Compilación inicial y posterior | Bloqueadas: `No .NET SDKs were found`. No se compiló C#. |
| `dotnet --list-sdks` | Sin SDK disponible. |
| `dotnet run --project Tests/MasterMonster.Pruebas.csproj` | Bloqueado por el mismo motivo; los 17 casos C# no fueron ejecutados. |
| `node --test Tests/http-compartido.test.cjs` | 4/4 correctos. |
| `node --check` sobre `wwwroot/js/*.js` | 19 scripts correctos sintácticamente. |
| `git diff --check` | Sin errores al finalizar. |
| Revisión estática de llamadas, SQL y consumidores afectados | Realizada; no equivale a pruebas de integración. |
| SQL Server, consultas, concurrencia y rollback reales | No ejecutados. |
| Navegación, login, formularios y apariencia en navegador | No ejecutados. |

## 8. Pendientes y límites

La refactorización cubre un conjunto coherente de seguridad, atomicidad y costos; no constituye una certificación exhaustiva del aplicativo ni demuestra ausencia de regresiones.

- Compilar con SDK 8 y ejecutar los casos C# antes de aceptar el cambio. Ejecutar regresión funcional y visual sobre una base desechable con datos conocidos.
- Planificación aún tiene operaciones de tarea/dependencias/estados derivados/avance en varias llamadas. Aunque reemplazar dependencias ya tiene transacción propia, la operación completa todavía necesita una unidad transaccional y control de concurrencia por proyecto. El recálculo de avance y su seguimiento también siguen separados.
- La comprobación completa de requisitos de finalización ocurre antes de la transacción de cierre. Aún debe protegerse frente a cambios simultáneos de planificación.
- La reactivación o cambio de fechas de proyectos puede alterar capacidad comprometida; debe unificarse con la política de concurrencia de asignaciones. Tampoco se probaron interbloqueos de SQL Server ni su recuperación.
- Contratos conservan una comprobación previa de solapamiento sin garantía transaccional ante altas concurrentes. La creación de usuarios utiliza compensación de errores en lugar de una única transacción; auditoría y varias operaciones de seguridad también requieren revisión adicional de atomicidad.
- Persisten filtros y paginación en memoria de reportes, consultas por integrante al calcular capacidad y consultas por usuario para completar perfiles. La búsqueda de empleados todavía filtra en memoria. No se hicieron cambios masivos sin poder probar contratos y ordenaciones.
- No se centralizaron todas las validaciones ni se eliminaron todas las instanciaciones, constantes repetidas o advertencias de nulabilidad: estas últimas requieren compilación para inventariarlas.
- Las fotos se validan por firma, no mediante decodificación completa ni reencodificación. No se migraron archivos existentes a almacenamiento privado fuera de `wwwroot`. Si el despliegue sirve esa carpeta directamente mediante IIS/proxy/CDN, también debe impedir su publicación independiente del pipeline ASP.NET.
- La sesión usa memoria del proceso, como corresponde a una instancia; una instalación con varios nodos requiere un almacén distribuido y claves de protección de datos compartidas.
- No se inventaron reglas de autoaprobación, prohibición de déficit, cierres financieros o permisos por propietario que no estén definidas en el código existente.

## 9. Recomendaciones posteriores, sin ejecución automática

1. Priorizar SDK/build, tests C# y pruebas de integración indicadas en `Tests/README.md`; después cerrar las transacciones pendientes de planificación, contratos y auditoría.
2. Revisar planes de ejecución e índices existentes antes de proponer índices sobre `AsignacionProyecto(CodigoEmpleado,Activo,FechaInicio,FechaFin)`, `TareaProyecto(CodigoProyecto,Activo)` y `MovimientoProyecto(CodigoProyecto,Tipo,Estado)`, con columnas incluidas según mediciones. No se ejecutó DDL.
3. Valorar una restricción filtrada para una sola asignación activa por proyecto/empleado y reglas de concurrencia/versionado, solo después de auditar datos y acordar una migración explícita.
4. Llevar filtros/paginación de reportes a SQL parametrizado manteniendo ordenaciones y respuestas actuales; reducir las cargas completas de personas/catálogos cuando el volumen lo justifique.
5. Añadir CI de build y pruebas, verificación de roles con una matriz de permisos y pruebas visuales de modales, Gantt, calendarios y exportaciones.
