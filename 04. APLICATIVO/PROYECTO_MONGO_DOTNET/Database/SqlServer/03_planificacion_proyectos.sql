SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('app.FaseProyecto', 'U') IS NULL
BEGIN
    CREATE TABLE app.FaseProyecto
    (
        CodigoFase varchar(10) NOT NULL CONSTRAINT PK_FaseProyecto PRIMARY KEY,
        CodigoProyecto varchar(10) NOT NULL,
        Nombre nvarchar(150) NOT NULL,
        Descripcion nvarchar(1000) NULL,
        Orden int NOT NULL CONSTRAINT DF_FaseProyecto_Orden DEFAULT 1,
        FechaInicio date NOT NULL,
        FechaFin date NOT NULL,
        Estado varchar(20) NOT NULL CONSTRAINT DF_FaseProyecto_Estado DEFAULT 'PENDIENTE',
        Activo bit NOT NULL CONSTRAINT DF_FaseProyecto_Activo DEFAULT 1,
        CreadoPor varchar(80) NOT NULL,
        FechaCreacion datetime2 NOT NULL CONSTRAINT DF_FaseProyecto_Fecha DEFAULT SYSUTCDATETIME(),
        ModificadoPor varchar(80) NULL,
        FechaModificacion datetime2 NULL,
        CONSTRAINT FK_FaseProyecto_Proyecto FOREIGN KEY (CodigoProyecto) REFERENCES app.Proyecto(Codigo),
        CONSTRAINT CK_FaseProyecto_Fechas CHECK (FechaFin >= FechaInicio),
        CONSTRAINT CK_FaseProyecto_Estado CHECK (Estado IN ('PENDIENTE','EN_PROCESO','COMPLETADA'))
    );
    CREATE INDEX IX_FaseProyecto_Proyecto ON app.FaseProyecto(CodigoProyecto, Activo, Orden);
END;

IF OBJECT_ID('app.TareaProyecto', 'U') IS NULL
BEGIN
    CREATE TABLE app.TareaProyecto
    (
        CodigoTarea varchar(10) NOT NULL CONSTRAINT PK_TareaProyecto PRIMARY KEY,
        CodigoFase varchar(10) NOT NULL,
        CodigoProyecto varchar(10) NOT NULL,
        Nombre nvarchar(180) NOT NULL,
        Descripcion nvarchar(1000) NULL,
        CodigoResponsable varchar(10) NOT NULL,
        FechaInicio date NOT NULL,
        FechaFin date NOT NULL,
        Estado varchar(20) NOT NULL CONSTRAINT DF_TareaProyecto_Estado DEFAULT 'PENDIENTE',
        Peso decimal(5,2) NOT NULL,
        HorasPlanificadas decimal(10,2) NOT NULL CONSTRAINT DF_TareaProyecto_Horas DEFAULT 0,
        CostoHoraPlanificado decimal(19,4) NOT NULL CONSTRAINT DF_TareaProyecto_CostoHora DEFAULT 0,
        FechaCompletada datetime2 NULL,
        Activo bit NOT NULL CONSTRAINT DF_TareaProyecto_Activo DEFAULT 1,
        CreadoPor varchar(80) NOT NULL,
        FechaCreacion datetime2 NOT NULL CONSTRAINT DF_TareaProyecto_Fecha DEFAULT SYSUTCDATETIME(),
        ModificadoPor varchar(80) NULL,
        FechaModificacion datetime2 NULL,
        CONSTRAINT FK_TareaProyecto_Fase FOREIGN KEY (CodigoFase) REFERENCES app.FaseProyecto(CodigoFase),
        CONSTRAINT FK_TareaProyecto_Proyecto FOREIGN KEY (CodigoProyecto) REFERENCES app.Proyecto(Codigo),
        CONSTRAINT FK_TareaProyecto_Responsable FOREIGN KEY (CodigoResponsable) REFERENCES app.Empleado(Codigo),
        CONSTRAINT CK_TareaProyecto_Fechas CHECK (FechaFin >= FechaInicio),
        CONSTRAINT CK_TareaProyecto_Estado CHECK (Estado IN ('PENDIENTE','EN_PROCESO','COMPLETADA')),
        CONSTRAINT CK_TareaProyecto_Peso CHECK (Peso > 0 AND Peso <= 100),
        CONSTRAINT CK_TareaProyecto_Horas CHECK (HorasPlanificadas >= 0)
    );
    CREATE INDEX IX_TareaProyecto_Proyecto ON app.TareaProyecto(CodigoProyecto, Activo, CodigoFase);
    CREATE INDEX IX_TareaProyecto_Responsable ON app.TareaProyecto(CodigoResponsable, Activo);
END;

IF OBJECT_ID('app.EntregableProyecto', 'U') IS NULL
BEGIN
    CREATE TABLE app.EntregableProyecto
    (
        CodigoEntregable varchar(10) NOT NULL CONSTRAINT PK_EntregableProyecto PRIMARY KEY,
        CodigoTarea varchar(10) NOT NULL,
        Nombre nvarchar(180) NOT NULL,
        Descripcion nvarchar(1000) NULL,
        FechaCompromiso date NOT NULL,
        Estado varchar(20) NOT NULL CONSTRAINT DF_EntregableProyecto_Estado DEFAULT 'PENDIENTE',
        Evidencia nvarchar(500) NULL,
        Observacion nvarchar(1000) NULL,
        FechaEntrega datetime2 NULL,
        Activo bit NOT NULL CONSTRAINT DF_EntregableProyecto_Activo DEFAULT 1,
        CreadoPor varchar(80) NOT NULL,
        FechaCreacion datetime2 NOT NULL CONSTRAINT DF_EntregableProyecto_Fecha DEFAULT SYSUTCDATETIME(),
        ModificadoPor varchar(80) NULL,
        FechaModificacion datetime2 NULL,
        CONSTRAINT FK_EntregableProyecto_Tarea FOREIGN KEY (CodigoTarea) REFERENCES app.TareaProyecto(CodigoTarea),
        CONSTRAINT CK_EntregableProyecto_Estado CHECK (Estado IN ('PENDIENTE','ENTREGADO'))
    );
    CREATE INDEX IX_EntregableProyecto_Tarea ON app.EntregableProyecto(CodigoTarea, Activo);
END;

IF COL_LENGTH('app.RegistroHora', 'CodigoTarea') IS NULL
BEGIN
    ALTER TABLE app.RegistroHora ADD CodigoTarea varchar(10) NULL;
    ALTER TABLE app.RegistroHora ADD CONSTRAINT FK_RegistroHora_Tarea
        FOREIGN KEY (CodigoTarea) REFERENCES app.TareaProyecto(CodigoTarea);
    CREATE INDEX IX_RegistroHora_Tarea ON app.RegistroHora(CodigoTarea, Estado);
END;

IF NOT EXISTS (SELECT 1 FROM app.Codigo WHERE Tabla='gepro_fase' AND Campo='CODIGOFASE')
    INSERT app.Codigo(Tabla,Campo,Prefijo,Longitud,Ultimo,Descripcion) VALUES('gepro_fase','CODIGOFASE','FAS',10,0,N'Fases de proyectos');
IF NOT EXISTS (SELECT 1 FROM app.Codigo WHERE Tabla='gepro_tarea' AND Campo='CODIGOTAREA')
    INSERT app.Codigo(Tabla,Campo,Prefijo,Longitud,Ultimo,Descripcion) VALUES('gepro_tarea','CODIGOTAREA','TAR',10,0,N'Tareas de proyectos');
IF NOT EXISTS (SELECT 1 FROM app.Codigo WHERE Tabla='gepro_entregable' AND Campo='CODIGOENTREGABLE')
    INSERT app.Codigo(Tabla,Campo,Prefijo,Longitud,Ultimo,Descripcion) VALUES('gepro_entregable','CODIGOENTREGABLE','ENT',10,0,N'Entregables de tareas');

-- Recupera horas históricas cuando existe una única tarea compatible para el
-- mismo proyecto y responsable. Los casos ambiguos permanecen sin vínculo.
UPDATE h
SET CodigoTarea = candidatos.CodigoTarea
FROM app.RegistroHora h
CROSS APPLY
(
    SELECT MIN(t.CodigoTarea) CodigoTarea, COUNT(*) Cantidad
    FROM app.TareaProyecto t
    WHERE t.CodigoProyecto=h.CodigoProyecto
      AND t.CodigoResponsable=h.CodigoEmpleado
      AND t.Activo=1
) candidatos
WHERE h.CodigoTarea IS NULL AND candidatos.Cantidad=1;

UPDATE app.Opcion
SET Descripcion=N'Planificación, seguimiento y avance'
WHERE Codigo='PRO_SEGUIMIENTO';

COMMIT TRANSACTION;
