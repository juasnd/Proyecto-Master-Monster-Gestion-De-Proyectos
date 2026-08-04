/* DDL - Índices secundarios para consultas y relaciones. */
:setvar DatabaseName "PROYECTO_MONSTER"
USE [$(DatabaseName)];
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_ActividadProyecto_Fase]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_ActividadProyecto_Fase] ON [app].[ActividadProyecto]
(
	[CodigoProyecto] ASC,
	[CodigoFase] ASC,
	[Activo] ASC,
	[Orden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_app_Asignacion_Empleado]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_app_Asignacion_Empleado] ON [app].[AsignacionProyecto]
(
	[CodigoEmpleado] ASC,
	[FechaInicio] ASC,
	[FechaFin] ASC,
	[Activo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_DependenciaTarea_Preced]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_DependenciaTarea_Preced] ON [app].[DependenciaTarea]
(
	[CodigoPredecesora] ASC,
	[CodigoTarea] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_DependenciaTarea_Proyecto]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_DependenciaTarea_Proyecto] ON [app].[DependenciaTarea]
(
	[CodigoProyecto] ASC,
	[CodigoTarea] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_EntregableProyecto_Relacion]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_EntregableProyecto_Relacion] ON [app].[EntregableProyecto]
(
	[TipoRelacion] ASC,
	[CodigoRelacion] ASC,
	[Activo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_EntregableProyecto_Tarea]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_EntregableProyecto_Tarea] ON [app].[EntregableProyecto]
(
	[CodigoTarea] ASC,
	[Activo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_FaseProyecto_Proyecto]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_FaseProyecto_Proyecto] ON [app].[FaseProyecto]
(
	[CodigoProyecto] ASC,
	[Activo] ASC,
	[Orden] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_app_PerfilOpcion_Activo]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_app_PerfilOpcion_Activo] ON [app].[PerfilOpcion]
(
	[CodigoPerfil] ASC,
	[CodigoOpcion] ASC,
	[FechaRetiro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_app_Persona_Cedula]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_app_Persona_Cedula] ON [app].[Persona]
(
	[Cedula] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_app_RegistroHora_EmpleadoFecha]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_app_RegistroHora_EmpleadoFecha] ON [app].[RegistroHora]
(
	[CodigoEmpleado] ASC,
	[Fecha] ASC,
	[Estado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_RegistroHora_Tarea]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_RegistroHora_Tarea] ON [app].[RegistroHora]
(
	[CodigoTarea] ASC,
	[Estado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_TareaProyecto_Actividad]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_TareaProyecto_Actividad] ON [app].[TareaProyecto]
(
	[CodigoActividad] ASC,
	[Activo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_TareaProyecto_Proyecto]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_TareaProyecto_Proyecto] ON [app].[TareaProyecto]
(
	[CodigoProyecto] ASC,
	[Activo] ASC,
	[CodigoFase] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_TareaProyecto_Responsable]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_TareaProyecto_Responsable] ON [app].[TareaProyecto]
(
	[CodigoResponsable] ASC,
	[Activo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_app_UsuarioPerfil_Activo]    Script Date: 28/07/2026 7:41:32 ******/
CREATE NONCLUSTERED INDEX [IX_app_UsuarioPerfil_Activo] ON [app].[UsuarioPerfil]
(
	[Login] ASC,
	[FechaRetiro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]