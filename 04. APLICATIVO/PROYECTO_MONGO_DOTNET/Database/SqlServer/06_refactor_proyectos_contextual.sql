SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* Menú contextual: conserva permisos históricos para las acciones de backend. */
UPDATE app.Opcion SET Descripcion=N'Tablas', Estado='A' WHERE Codigo='PRO_TAB';
UPDATE app.Opcion SET Descripcion=N'Procesos', Estado='A' WHERE Codigo='PRO_PROC';
UPDATE app.Opcion SET Descripcion=N'Gestión de proyectos', Url=N'Proyectos', CodigoPadre='PRO_TAB', Nivel=2, Orden=10, Estado='A' WHERE Codigo='PRO_PROYECTOS';
UPDATE app.Opcion SET Descripcion=N'Planificación y seguimiento', Url=N'Proyectos/Planificacion', CodigoPadre='PRO_PROC', Nivel=2, Orden=10, Estado='A' WHERE Codigo='PRO_SEGUIMIENTO';
UPDATE app.Opcion SET Estado='I' WHERE Codigo IN('PRO_ASIGNAR','PRO_CIERRE','RASI');

/* Los permisos PRO_ASIGNAR y PRO_CIERRE permanecen asignados: ahora autorizan
   acciones contextuales y ya no generan opciones independientes en el menú. */

IF COL_LENGTH('app.EntregableProyecto','TipoRelacion') IS NULL
    ALTER TABLE app.EntregableProyecto ADD TipoRelacion varchar(10) NULL;
IF COL_LENGTH('app.EntregableProyecto','CodigoRelacion') IS NULL
    ALTER TABLE app.EntregableProyecto ADD CodigoRelacion varchar(10) NULL;

EXEC(N'UPDATE app.EntregableProyecto
SET TipoRelacion=COALESCE(NULLIF(TipoRelacion,''''),''TAREA''),
    CodigoRelacion=COALESCE(NULLIF(CodigoRelacion,''''),CodigoTarea)
WHERE TipoRelacion IS NULL OR CodigoRelacion IS NULL OR TipoRelacion='''' OR CodigoRelacion='''';');

EXEC(N'ALTER TABLE app.EntregableProyecto ALTER COLUMN CodigoTarea varchar(10) NULL;');
EXEC(N'ALTER TABLE app.EntregableProyecto ALTER COLUMN TipoRelacion varchar(10) NOT NULL;');
EXEC(N'ALTER TABLE app.EntregableProyecto ALTER COLUMN CodigoRelacion varchar(10) NOT NULL;');

IF EXISTS(SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID('app.EntregableProyecto') AND name='CK_EntregableProyecto_Estado')
    ALTER TABLE app.EntregableProyecto DROP CONSTRAINT CK_EntregableProyecto_Estado;
EXEC(N'ALTER TABLE app.EntregableProyecto WITH CHECK ADD CONSTRAINT CK_EntregableProyecto_Estado CHECK(Estado IN(''PENDIENTE'',''ENTREGADO'',''JUSTIFICADO''));');

IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID('app.EntregableProyecto') AND name='CK_EntregableProyecto_Relacion')
    EXEC(N'ALTER TABLE app.EntregableProyecto WITH CHECK ADD CONSTRAINT CK_EntregableProyecto_Relacion CHECK
    (
        (TipoRelacion=''TAREA'' AND CodigoTarea IS NOT NULL AND CodigoRelacion=CodigoTarea)
        OR (TipoRelacion IN(''FASE'',''ACTIVIDAD'') AND CodigoTarea IS NULL)
    );');

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID('app.EntregableProyecto') AND name='IX_EntregableProyecto_Relacion')
    EXEC(N'CREATE INDEX IX_EntregableProyecto_Relacion ON app.EntregableProyecto(TipoRelacion,CodigoRelacion,Activo);');

IF NOT EXISTS
(
    SELECT 1 FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id=dc.parent_object_id AND c.column_id=dc.parent_column_id
    WHERE dc.parent_object_id=OBJECT_ID('app.AsignacionProyecto') AND c.name='HorasPlanificadas'
)
    ALTER TABLE app.AsignacionProyecto ADD CONSTRAINT DF_AsignacionProyecto_HorasPlanificadas DEFAULT 0 FOR HorasPlanificadas;

IF EXISTS(SELECT 1 FROM app.ParametroFinanciero WHERE CodigoParametro='HORAS_MENSUALES_REFERENCIA')
    UPDATE app.ParametroFinanciero SET Descripcion=N'Horas mensuales de referencia cuando un empleado no tiene contrato vigente',TipoDato='DECIMAL',ValorDecimal=COALESCE(ValorDecimal,160),Activo=1,ModificadoPor='MIGRACION',FechaModificacion=SYSUTCDATETIME() WHERE CodigoParametro='HORAS_MENSUALES_REFERENCIA';
ELSE
    INSERT app.ParametroFinanciero(Id,CodigoParametro,Descripcion,TipoDato,ValorDecimal,Activo,ModificadoPor,FechaModificacion)
    VALUES(NULL,'HORAS_MENSUALES_REFERENCIA',N'Horas mensuales de referencia cuando un empleado no tiene contrato vigente','DECIMAL',160,1,'MIGRACION',SYSUTCDATETIME());

COMMIT TRANSACTION;
