SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('app.DependenciaTarea', 'U') IS NULL
BEGIN
    CREATE TABLE app.DependenciaTarea
    (
        CodigoProyecto varchar(10) NOT NULL,
        CodigoTarea varchar(10) NOT NULL,
        CodigoPredecesora varchar(10) NOT NULL,
        TipoDependencia char(2) NOT NULL CONSTRAINT DF_DependenciaTarea_Tipo DEFAULT 'FS',
        DesfaseDias int NOT NULL CONSTRAINT DF_DependenciaTarea_Desfase DEFAULT 0,
        CreadoPor varchar(80) NOT NULL,
        FechaCreacion datetime2 NOT NULL CONSTRAINT DF_DependenciaTarea_Fecha DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_DependenciaTarea PRIMARY KEY(CodigoTarea, CodigoPredecesora),
        CONSTRAINT FK_DependenciaTarea_Proyecto FOREIGN KEY(CodigoProyecto) REFERENCES app.Proyecto(Codigo),
        CONSTRAINT FK_DependenciaTarea_Tarea FOREIGN KEY(CodigoTarea) REFERENCES app.TareaProyecto(CodigoTarea),
        CONSTRAINT FK_DependenciaTarea_Preced FOREIGN KEY(CodigoPredecesora) REFERENCES app.TareaProyecto(CodigoTarea),
        CONSTRAINT CK_DependenciaTarea_Distinta CHECK(CodigoTarea <> CodigoPredecesora),
        CONSTRAINT CK_DependenciaTarea_Tipo CHECK(TipoDependencia IN('FS','SS','FF','SF')),
        CONSTRAINT CK_DependenciaTarea_Desfase CHECK(DesfaseDias BETWEEN -3650 AND 3650)
    );

    CREATE INDEX IX_DependenciaTarea_Proyecto ON app.DependenciaTarea(CodigoProyecto, CodigoTarea);
    CREATE INDEX IX_DependenciaTarea_Preced ON app.DependenciaTarea(CodigoPredecesora, CodigoTarea);
END;

COMMIT TRANSACTION;
