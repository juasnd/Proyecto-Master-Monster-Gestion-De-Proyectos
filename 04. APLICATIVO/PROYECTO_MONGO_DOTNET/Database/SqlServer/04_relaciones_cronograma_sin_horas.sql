SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('app.Cargo', 'EsJefeProyecto') IS NULL
    ALTER TABLE app.Cargo ADD EsJefeProyecto bit NOT NULL CONSTRAINT DF_Cargo_EsJefeProyecto DEFAULT 0;

EXEC(N'UPDATE app.Cargo SET EsJefeProyecto=1 WHERE Estado=''A'' AND UPPER(Descripcion) IN (N''JEFE DE PROYECTO'',N''JEFE DE PROYECTOS'');');

-- Corrige proyectos heredados cuyo jefe no tiene una designacion vigente.
EXEC(N'
UPDATE p
SET CodigoJefeProyecto = j.Codigo
FROM app.Proyecto p
CROSS APPLY
(
    SELECT TOP (1) e.Codigo
    FROM app.Empleado e
    INNER JOIN app.Cargo c ON c.Codigo=e.CodigoCargo
    WHERE e.Estado=''A'' AND c.Estado=''A'' AND c.EsJefeProyecto=1
    ORDER BY e.Codigo
) j
WHERE p.Activo=1
  AND NOT EXISTS
  (
      SELECT 1
      FROM app.Empleado e
      INNER JOIN app.Cargo c ON c.Codigo=e.CodigoCargo
      WHERE e.Codigo=p.CodigoJefeProyecto
        AND e.Estado=''A'' AND c.Estado=''A'' AND c.EsJefeProyecto=1
  );');

IF OBJECT_ID('app.ActividadProyecto', 'U') IS NULL
BEGIN
    CREATE TABLE app.ActividadProyecto
    (
        CodigoActividad varchar(10) NOT NULL CONSTRAINT PK_ActividadProyecto PRIMARY KEY,
        CodigoFase varchar(10) NOT NULL,
        CodigoProyecto varchar(10) NOT NULL,
        Nombre nvarchar(180) NOT NULL,
        Descripcion nvarchar(1000) NULL,
        CodigoResponsable varchar(10) NOT NULL,
        Orden int NOT NULL CONSTRAINT DF_ActividadProyecto_Orden DEFAULT 1,
        FechaInicio date NOT NULL,
        FechaFin date NOT NULL,
        Estado varchar(20) NOT NULL CONSTRAINT DF_ActividadProyecto_Estado DEFAULT 'PENDIENTE',
        Activo bit NOT NULL CONSTRAINT DF_ActividadProyecto_Activo DEFAULT 1,
        CreadoPor varchar(80) NOT NULL,
        FechaCreacion datetime2 NOT NULL CONSTRAINT DF_ActividadProyecto_Fecha DEFAULT SYSUTCDATETIME(),
        ModificadoPor varchar(80) NULL,
        FechaModificacion datetime2 NULL,
        CONSTRAINT FK_ActividadProyecto_Fase FOREIGN KEY(CodigoFase) REFERENCES app.FaseProyecto(CodigoFase),
        CONSTRAINT FK_ActividadProyecto_Proyecto FOREIGN KEY(CodigoProyecto) REFERENCES app.Proyecto(Codigo),
        CONSTRAINT FK_ActividadProyecto_Responsable FOREIGN KEY(CodigoResponsable) REFERENCES app.Empleado(Codigo),
        CONSTRAINT CK_ActividadProyecto_Fechas CHECK(FechaFin>=FechaInicio),
        CONSTRAINT CK_ActividadProyecto_Estado CHECK(Estado IN('PENDIENTE','EN_PROCESO','COMPLETADA'))
    );
    CREATE INDEX IX_ActividadProyecto_Fase ON app.ActividadProyecto(CodigoProyecto,CodigoFase,Activo,Orden);
END;

IF COL_LENGTH('app.TareaProyecto', 'CodigoActividad') IS NULL
BEGIN
    ALTER TABLE app.TareaProyecto ADD CodigoActividad varchar(10) NULL;
    ALTER TABLE app.TareaProyecto ADD CONSTRAINT FK_TareaProyecto_Actividad
        FOREIGN KEY(CodigoActividad) REFERENCES app.ActividadProyecto(CodigoActividad);
    CREATE INDEX IX_TareaProyecto_Actividad ON app.TareaProyecto(CodigoActividad,Activo);
END;
GO

IF NOT EXISTS(SELECT 1 FROM app.Codigo WHERE Tabla='gepro_actividad' AND Campo='CODIGOACTIVIDAD')
    INSERT app.Codigo(Tabla,Campo,Prefijo,Longitud,Ultimo,Descripcion)
    VALUES('gepro_actividad','CODIGOACTIVIDAD','ACT',10,0,N'Actividades de proyectos');

DECLARE @BaseActividad int=(SELECT Ultimo FROM app.Codigo WHERE Tabla='gepro_actividad' AND Campo='CODIGOACTIVIDAD');
DECLARE @NuevasActividades TABLE
(
    Numero int,
    CodigoFase varchar(10),
    CodigoProyecto varchar(10),
    CodigoResponsable varchar(10),
    FechaInicio date,
    FechaFin date
);

INSERT @NuevasActividades(Numero,CodigoFase,CodigoProyecto,CodigoResponsable,FechaInicio,FechaFin)
SELECT ROW_NUMBER() OVER(ORDER BY f.CodigoFase),f.CodigoFase,f.CodigoProyecto,
       COALESCE(MIN(t.CodigoResponsable),p.CodigoJefeProyecto),
       COALESCE(MIN(t.FechaInicio),f.FechaInicio),COALESCE(MAX(t.FechaFin),f.FechaFin)
FROM app.FaseProyecto f
JOIN app.Proyecto p ON p.Codigo=f.CodigoProyecto
LEFT JOIN app.TareaProyecto t ON t.CodigoFase=f.CodigoFase AND t.Activo=1
WHERE f.Activo=1 AND NOT EXISTS(SELECT 1 FROM app.ActividadProyecto a WHERE a.CodigoFase=f.CodigoFase AND a.Activo=1)
GROUP BY f.CodigoFase,f.CodigoProyecto,p.CodigoJefeProyecto,f.FechaInicio,f.FechaFin;

INSERT app.ActividadProyecto(CodigoActividad,CodigoFase,CodigoProyecto,Nombre,Descripcion,CodigoResponsable,Orden,FechaInicio,FechaFin,Estado,Activo,CreadoPor,FechaCreacion)
SELECT 'ACT'+RIGHT(REPLICATE('0',7)+CONVERT(varchar(7),@BaseActividad+Numero),7),CodigoFase,CodigoProyecto,
       N'Actividad general',N'Actividad creada para organizar la planificación existente.',CodigoResponsable,1,FechaInicio,FechaFin,
       'PENDIENTE',1,'MIGRACION',SYSUTCDATETIME()
FROM @NuevasActividades;

UPDATE app.Codigo SET Ultimo=@BaseActividad+(SELECT COUNT(*) FROM @NuevasActividades)
WHERE Tabla='gepro_actividad' AND Campo='CODIGOACTIVIDAD';

UPDATE t
SET CodigoActividad=(SELECT TOP(1) a.CodigoActividad FROM app.ActividadProyecto a WHERE a.CodigoFase=t.CodigoFase AND a.Activo=1 ORDER BY a.Orden,a.CodigoActividad)
FROM app.TareaProyecto t
WHERE t.CodigoActividad IS NULL;

UPDATE a
SET Estado=CASE WHEN datos.Total>0 AND datos.Completadas=datos.Total THEN 'COMPLETADA'
                WHEN datos.Iniciadas>0 THEN 'EN_PROCESO' ELSE 'PENDIENTE' END
FROM app.ActividadProyecto a
CROSS APPLY(SELECT COUNT(*) Total,SUM(CASE WHEN t.Estado='COMPLETADA' THEN 1 ELSE 0 END) Completadas,SUM(CASE WHEN t.Estado<>'PENDIENTE' THEN 1 ELSE 0 END) Iniciadas FROM app.TareaProyecto t WHERE t.CodigoActividad=a.CodigoActividad AND t.Activo=1) datos;

UPDATE app.Opcion SET Estado='I' WHERE Codigo IN('PRO_HORAS','PRO_APROBAR_HORAS','RHOR');
UPDATE app.PerfilOpcion SET FechaRetiro=COALESCE(FechaRetiro,SYSUTCDATETIME()),ModificadoPor=COALESCE(ModificadoPor,'MIGRACION')
WHERE CodigoOpcion IN('PRO_HORAS','PRO_APROBAR_HORAS','RHOR') AND FechaRetiro IS NULL;
UPDATE app.RegistroHora SET Estado='ANULADO',ObservacionRevision=COALESCE(NULLIF(ObservacionRevision,''),N'Módulo retirado del alcance.'),FechaModificacion=SYSUTCDATETIME(),ModificadoPor='MIGRACION'
WHERE Estado='PENDIENTE';

COMMIT TRANSACTION;
