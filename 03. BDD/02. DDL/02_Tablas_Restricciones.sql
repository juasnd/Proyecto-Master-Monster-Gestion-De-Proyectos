/* DDL - Tablas, claves, restricciones y valores por defecto. Generado desde la estructura vigente de PROYECTO_MONSTER el 2026-07-28. */
:setvar DatabaseName "PROYECTO_MONSTER"
USE [$(DatabaseName)];
GO
/****** Object:  Table [app].[ActividadProyecto]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[ActividadProyecto](
	[CodigoActividad] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoFase] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoProyecto] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Nombre] [nvarchar](180) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](1000) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoResponsable] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Orden] [int] NOT NULL,
	[FechaInicio] [date] NOT NULL,
	[FechaFin] [date] NOT NULL,
	[Estado] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Activo] [bit] NOT NULL,
	[CreadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
 CONSTRAINT [PK_ActividadProyecto] PRIMARY KEY CLUSTERED 
(
	[CodigoActividad] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[AsignacionProyecto]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[AsignacionProyecto](
	[Id] [varchar](24) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoAsignacion] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoProyecto] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoEmpleado] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Rol] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[PorcentajeDedicacion] [decimal](5, 2) NOT NULL,
	[HorasPlanificadas] [decimal](10, 2) NOT NULL,
	[FechaInicio] [datetime2](7) NOT NULL,
	[FechaFin] [datetime2](7) NOT NULL,
	[Estado] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Activo] [bit] NOT NULL,
	[Observacion] [nvarchar](max) COLLATE Modern_Spanish_CI_AS NULL,
	[CostoHoraSnapshot] [decimal](18, 4) NULL,
	[AsignadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaAsignacion] [datetime2](7) NOT NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[CodigoAsignacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [app].[Auditoria]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Auditoria](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CodigoOrigen] [varchar](50) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoPersona] [varchar](10) COLLATE Modern_Spanish_CI_AS NULL,
	[Login] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[Accion] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Tabla] [varchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Detalle] [nvarchar](max) COLLATE Modern_Spanish_CI_AS NULL,
	[Fecha] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [app].[Cargo]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Cargo](
	[CodigoDepartamento] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Codigo] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Estado] [char](1) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[EsJefeProyecto] [bit] NOT NULL,
 CONSTRAINT [PK_app_Cargo] PRIMARY KEY CLUSTERED 
(
	[CodigoDepartamento] ASC,
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[CategoriaGasto]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[CategoriaGasto](
	[Id] [varchar](24) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoCategoria] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Nombre] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](300) COLLATE Modern_Spanish_CI_AS NULL,
	[Activo] [bit] NOT NULL,
	[CreadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[CodigoCategoria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Codigo]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Codigo](
	[Tabla] [varchar](60) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Campo] [varchar](60) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Prefijo] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Longitud] [int] NOT NULL,
	[Ultimo] [int] NOT NULL,
	[Descripcion] [nvarchar](200) COLLATE Modern_Spanish_CI_AS NULL,
 CONSTRAINT [PK_app_Codigo] PRIMARY KEY CLUSTERED 
(
	[Tabla] ASC,
	[Campo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Contrato]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Contrato](
	[Id] [varchar](24) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoContrato] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoEmpleado] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[TipoContrato] [varchar](40) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[SueldoBase] [decimal](18, 2) NOT NULL,
	[HorasMensuales] [decimal](8, 2) NOT NULL,
	[FechaInicio] [datetime2](7) NOT NULL,
	[FechaFin] [datetime2](7) NULL,
	[Estado] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Activo] [bit] NOT NULL,
	[CreadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[CodigoContrato] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Departamento]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Departamento](
	[Codigo] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Estado] [char](1) COLLATE Modern_Spanish_CI_AS NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[DependenciaTarea]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[DependenciaTarea](
	[CodigoProyecto] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoTarea] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoPredecesora] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[TipoDependencia] [char](2) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[DesfaseDias] [int] NOT NULL,
	[CreadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_DependenciaTarea] PRIMARY KEY CLUSTERED 
(
	[CodigoTarea] ASC,
	[CodigoPredecesora] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Empleado]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Empleado](
	[Codigo] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoDepartamento] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoCargo] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoPersona] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoDepartamentoPago] [varchar](10) COLLATE Modern_Spanish_CI_AS NULL,
	[Estado] [char](1) COLLATE Modern_Spanish_CI_AS NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[EntregableProyecto]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[EntregableProyecto](
	[CodigoEntregable] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoTarea] [varchar](10) COLLATE Modern_Spanish_CI_AS NULL,
	[Nombre] [nvarchar](180) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](1000) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaCompromiso] [date] NOT NULL,
	[Estado] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Evidencia] [nvarchar](500) COLLATE Modern_Spanish_CI_AS NULL,
	[Observacion] [nvarchar](1000) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaEntrega] [datetime2](7) NULL,
	[Activo] [bit] NOT NULL,
	[CreadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
	[TipoRelacion] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoRelacion] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
 CONSTRAINT [PK_EntregableProyecto] PRIMARY KEY CLUSTERED 
(
	[CodigoEntregable] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[EstadoCivil]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[EstadoCivil](
	[Codigo] [varchar](3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[EstadoSistema]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[EstadoSistema](
	[Codigo] [char](1) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Familiar]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Familiar](
	[Codigo] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoPersona] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoParentesco] [varchar](3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Nombre] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Apellido] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaNacimiento] [date] NULL,
	[Telefono] [varchar](20) COLLATE Modern_Spanish_CI_AS NULL,
	[CargaFamiliar] [char](1) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Observacion] [nvarchar](500) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[FaseProyecto]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[FaseProyecto](
	[CodigoFase] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoProyecto] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Nombre] [nvarchar](150) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](1000) COLLATE Modern_Spanish_CI_AS NULL,
	[Orden] [int] NOT NULL,
	[FechaInicio] [date] NOT NULL,
	[FechaFin] [date] NOT NULL,
	[Estado] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Activo] [bit] NOT NULL,
	[CreadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
 CONSTRAINT [PK_FaseProyecto] PRIMARY KEY CLUSTERED 
(
	[CodigoFase] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Formacion]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Formacion](
	[Codigo] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoEmpleado] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Nivel] [nvarchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[Titulo] [nvarchar](150) COLLATE Modern_Spanish_CI_AS NULL,
	[Institucion] [nvarchar](150) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaInicio] [date] NULL,
	[FechaFin] [date] NULL,
	[Observacion] [nvarchar](500) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[MovimientoProyecto]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[MovimientoProyecto](
	[Id] [varchar](24) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoMovimiento] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoProyecto] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Tipo] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoCategoria] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Concepto] [nvarchar](300) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Valor] [decimal](18, 2) NOT NULL,
	[Fecha] [date] NOT NULL,
	[Estado] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[RegistradoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
	[AprobadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaAprobacion] [datetime2](7) NULL,
	[Observacion] [nvarchar](500) COLLATE Modern_Spanish_CI_AS NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[CodigoMovimiento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Opcion]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Opcion](
	[Codigo] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoSistema] [varchar](10) COLLATE Modern_Spanish_CI_AS NULL,
	[Descripcion] [nvarchar](150) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Url] [nvarchar](250) COLLATE Modern_Spanish_CI_AS NULL,
	[Icono] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoPadre] [varchar](20) COLLATE Modern_Spanish_CI_AS NULL,
	[Nivel] [int] NOT NULL,
	[Orden] [int] NOT NULL,
	[Estado] [char](1) COLLATE Modern_Spanish_CI_AS NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[ParametroFinanciero]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[ParametroFinanciero](
	[Id] [varchar](24) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoParametro] [varchar](30) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](200) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[TipoDato] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[ValorTexto] [nvarchar](500) COLLATE Modern_Spanish_CI_AS NULL,
	[ValorDecimal] [decimal](18, 4) NULL,
	[ValorEntero] [int] NULL,
	[Activo] [bit] NOT NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaModificacion] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CodigoParametro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Parentesco]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Parentesco](
	[Codigo] [varchar](3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Perfil]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Perfil](
	[Codigo] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](150) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Observacion] [nvarchar](max) COLLATE Modern_Spanish_CI_AS NULL,
	[Estado] [char](1) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CreadoPor] [varchar](50) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[ModificadoPor] [varchar](50) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [app].[PerfilOpcion]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[PerfilOpcion](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CodigoPerfil] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoOpcion] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[PuedeVer] [bit] NOT NULL,
	[PuedeCrear] [bit] NOT NULL,
	[PuedeEditar] [bit] NOT NULL,
	[PuedeEliminar] [bit] NOT NULL,
	[FechaAsignacion] [datetime2](7) NOT NULL,
	[FechaRetiro] [datetime2](7) NULL,
	[ModificadoPor] [varchar](50) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Persona]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Persona](
	[Codigo] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Tipo] [varchar](3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoSexo] [varchar](3) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoEstadoCivil] [varchar](3) COLLATE Modern_Spanish_CI_AS NULL,
	[Nombres] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Apellidos] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Cedula] [varchar](20) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaNacimiento] [date] NULL,
	[CargasFamiliares] [int] NOT NULL,
	[Direccion] [nvarchar](200) COLLATE Modern_Spanish_CI_AS NULL,
	[Celular] [varchar](20) COLLATE Modern_Spanish_CI_AS NULL,
	[TelefonoDomicilio] [varchar](20) COLLATE Modern_Spanish_CI_AS NULL,
	[Email] [varchar](150) COLLATE Modern_Spanish_CI_AS NULL,
	[Foto] [nvarchar](500) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Proyecto]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Proyecto](
	[Id] [varchar](24) COLLATE Modern_Spanish_CI_AS NULL,
	[Codigo] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Nombre] [nvarchar](150) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](max) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoDepartamento] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoJefeProyecto] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaInicio] [datetime2](7) NOT NULL,
	[FechaFinPlanificada] [datetime2](7) NOT NULL,
	[FechaFinReal] [datetime2](7) NULL,
	[Estado] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Prioridad] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Avance] [decimal](5, 2) NOT NULL,
	[Presupuesto] [decimal](18, 2) NOT NULL,
	[Activo] [bit] NOT NULL,
	[Observacion] [nvarchar](max) COLLATE Modern_Spanish_CI_AS NULL,
	[CreadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [app].[RegistroHora]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[RegistroHora](
	[Id] [varchar](24) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoRegistro] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoProyecto] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoEmpleado] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Fecha] [date] NOT NULL,
	[Horas] [decimal](6, 2) NOT NULL,
	[Actividad] [nvarchar](500) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Estado] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[RegistradoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
	[AprobadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaAprobacion] [datetime2](7) NULL,
	[ObservacionRevision] [nvarchar](500) COLLATE Modern_Spanish_CI_AS NULL,
	[CostoHoraAplicado] [decimal](18, 4) NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
	[CodigoTarea] [varchar](10) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[CodigoRegistro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[ReporteHistorial]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[ReporteHistorial](
	[Codigo] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Fecha] [datetime2](7) NOT NULL,
	[Usuario] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Perfil] [varchar](20) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoReporte] [varchar](20) COLLATE Modern_Spanish_CI_AS NULL,
	[Modulo] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NULL,
	[TipoReporte] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NULL,
	[Formato] [varchar](20) COLLATE Modern_Spanish_CI_AS NULL,
	[TotalRegistros] [int] NOT NULL,
	[Filtros] [nvarchar](max) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [app].[ResetClave]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[ResetClave](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CodigoPersona] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Login] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Token] [varchar](200) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[FechaExpira] [datetime2](7) NOT NULL,
	[Usado] [char](1) COLLATE Modern_Spanish_CI_AS NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[SeguimientoProyecto]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[SeguimientoProyecto](
	[Id] [varchar](24) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoSeguimiento] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoProyecto] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[AvanceAnterior] [decimal](5, 2) NOT NULL,
	[AvanceNuevo] [decimal](5, 2) NOT NULL,
	[EstadoAnterior] [varchar](20) COLLATE Modern_Spanish_CI_AS NULL,
	[EstadoNuevo] [varchar](20) COLLATE Modern_Spanish_CI_AS NULL,
	[Observacion] [nvarchar](max) COLLATE Modern_Spanish_CI_AS NULL,
	[RegistradoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CodigoSeguimiento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [app].[Sesion]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Sesion](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CodigoPersona] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Login] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaInicio] [datetime2](7) NOT NULL,
	[FechaFin] [datetime2](7) NULL,
	[Ip] [varchar](45) COLLATE Modern_Spanish_CI_AS NULL,
	[Navegador] [nvarchar](300) COLLATE Modern_Spanish_CI_AS NULL,
	[Estado] [char](1) COLLATE Modern_Spanish_CI_AS NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Sexo]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Sexo](
	[Codigo] [varchar](3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Sistema]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Sistema](
	[Codigo] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[TareaProyecto]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[TareaProyecto](
	[CodigoTarea] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoFase] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoProyecto] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Nombre] [nvarchar](180) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](1000) COLLATE Modern_Spanish_CI_AS NULL,
	[CodigoResponsable] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaInicio] [date] NOT NULL,
	[FechaFin] [date] NOT NULL,
	[Estado] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Peso] [decimal](5, 2) NOT NULL,
	[HorasPlanificadas] [decimal](10, 2) NOT NULL,
	[CostoHoraPlanificado] [decimal](19, 4) NOT NULL,
	[FechaCompletada] [datetime2](7) NULL,
	[Activo] [bit] NOT NULL,
	[CreadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
	[CodigoActividad] [varchar](10) COLLATE Modern_Spanish_CI_AS NULL,
 CONSTRAINT [PK_TareaProyecto] PRIMARY KEY CLUSTERED 
(
	[CodigoTarea] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[TipoPersona]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[TipoPersona](
	[Codigo] [varchar](3) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Descripcion] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Observacion] [nvarchar](250) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[Usuario]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[Usuario](
	[CodigoPersona] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Login] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[ClaveHash] [varchar](500) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Algoritmo] [varchar](30) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Estado] [char](1) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[IntentosFallidos] [int] NOT NULL,
	[CambioClave] [char](1) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[PieFirma] [nvarchar](150) COLLATE Modern_Spanish_CI_AS NULL,
	[TokenRecuperacion] [varchar](200) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaExpiraToken] [datetime2](7) NULL,
	[UltimoAcceso] [datetime2](7) NULL,
	[BloqueadoHasta] [datetime2](7) NULL,
	[Observacion] [nvarchar](500) COLLATE Modern_Spanish_CI_AS NULL,
	[CreadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaCreacion] [datetime2](7) NOT NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[FechaModificacion] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[Login] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [app].[UsuarioPerfil]    Script Date: 28/07/2026 7:41:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [app].[UsuarioPerfil](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CodigoPersona] [varchar](10) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[Login] [varchar](80) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[CodigoPerfil] [varchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[FechaAsignacion] [datetime2](7) NOT NULL,
	[FechaRetiro] [datetime2](7) NULL,
	[AsignadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
	[ModificadoPor] [varchar](80) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [app].[ActividadProyecto] ADD  CONSTRAINT [DF_ActividadProyecto_Orden]  DEFAULT ((1)) FOR [Orden]
GO
ALTER TABLE [app].[ActividadProyecto] ADD  CONSTRAINT [DF_ActividadProyecto_Estado]  DEFAULT ('PENDIENTE') FOR [Estado]
GO
ALTER TABLE [app].[ActividadProyecto] ADD  CONSTRAINT [DF_ActividadProyecto_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [app].[ActividadProyecto] ADD  CONSTRAINT [DF_ActividadProyecto_Fecha]  DEFAULT (sysutcdatetime()) FOR [FechaCreacion]
GO
ALTER TABLE [app].[AsignacionProyecto] ADD  CONSTRAINT [DF_AsignacionProyecto_HorasPlanificadas]  DEFAULT ((0)) FOR [HorasPlanificadas]
GO
ALTER TABLE [app].[Cargo] ADD  DEFAULT ('A') FOR [Estado]
GO
ALTER TABLE [app].[Cargo] ADD  CONSTRAINT [DF_Cargo_EsJefeProyecto]  DEFAULT ((0)) FOR [EsJefeProyecto]
GO
ALTER TABLE [app].[Departamento] ADD  DEFAULT ('A') FOR [Estado]
GO
ALTER TABLE [app].[DependenciaTarea] ADD  CONSTRAINT [DF_DependenciaTarea_Tipo]  DEFAULT ('FS') FOR [TipoDependencia]
GO
ALTER TABLE [app].[DependenciaTarea] ADD  CONSTRAINT [DF_DependenciaTarea_Desfase]  DEFAULT ((0)) FOR [DesfaseDias]
GO
ALTER TABLE [app].[DependenciaTarea] ADD  CONSTRAINT [DF_DependenciaTarea_Fecha]  DEFAULT (sysutcdatetime()) FOR [FechaCreacion]
GO
ALTER TABLE [app].[Empleado] ADD  DEFAULT ('A') FOR [Estado]
GO
ALTER TABLE [app].[EntregableProyecto] ADD  CONSTRAINT [DF_EntregableProyecto_Estado]  DEFAULT ('PENDIENTE') FOR [Estado]
GO
ALTER TABLE [app].[EntregableProyecto] ADD  CONSTRAINT [DF_EntregableProyecto_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [app].[EntregableProyecto] ADD  CONSTRAINT [DF_EntregableProyecto_Fecha]  DEFAULT (sysutcdatetime()) FOR [FechaCreacion]
GO
ALTER TABLE [app].[Familiar] ADD  DEFAULT ('S') FOR [CargaFamiliar]
GO
ALTER TABLE [app].[FaseProyecto] ADD  CONSTRAINT [DF_FaseProyecto_Orden]  DEFAULT ((1)) FOR [Orden]
GO
ALTER TABLE [app].[FaseProyecto] ADD  CONSTRAINT [DF_FaseProyecto_Estado]  DEFAULT ('PENDIENTE') FOR [Estado]
GO
ALTER TABLE [app].[FaseProyecto] ADD  CONSTRAINT [DF_FaseProyecto_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [app].[FaseProyecto] ADD  CONSTRAINT [DF_FaseProyecto_Fecha]  DEFAULT (sysutcdatetime()) FOR [FechaCreacion]
GO
ALTER TABLE [app].[Opcion] ADD  DEFAULT ((1)) FOR [Nivel]
GO
ALTER TABLE [app].[Opcion] ADD  DEFAULT ((0)) FOR [Orden]
GO
ALTER TABLE [app].[Opcion] ADD  DEFAULT ('A') FOR [Estado]
GO
ALTER TABLE [app].[Perfil] ADD  DEFAULT ('A') FOR [Estado]
GO
ALTER TABLE [app].[Perfil] ADD  DEFAULT (sysutcdatetime()) FOR [FechaCreacion]
GO
ALTER TABLE [app].[PerfilOpcion] ADD  DEFAULT ((0)) FOR [PuedeVer]
GO
ALTER TABLE [app].[PerfilOpcion] ADD  DEFAULT ((0)) FOR [PuedeCrear]
GO
ALTER TABLE [app].[PerfilOpcion] ADD  DEFAULT ((0)) FOR [PuedeEditar]
GO
ALTER TABLE [app].[PerfilOpcion] ADD  DEFAULT ((0)) FOR [PuedeEliminar]
GO
ALTER TABLE [app].[Persona] ADD  DEFAULT ((0)) FOR [CargasFamiliares]
GO
ALTER TABLE [app].[TareaProyecto] ADD  CONSTRAINT [DF_TareaProyecto_Estado]  DEFAULT ('PENDIENTE') FOR [Estado]
GO
ALTER TABLE [app].[TareaProyecto] ADD  CONSTRAINT [DF_TareaProyecto_Horas]  DEFAULT ((0)) FOR [HorasPlanificadas]
GO
ALTER TABLE [app].[TareaProyecto] ADD  CONSTRAINT [DF_TareaProyecto_CostoHora]  DEFAULT ((0)) FOR [CostoHoraPlanificado]
GO
ALTER TABLE [app].[TareaProyecto] ADD  CONSTRAINT [DF_TareaProyecto_Activo]  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [app].[TareaProyecto] ADD  CONSTRAINT [DF_TareaProyecto_Fecha]  DEFAULT (sysutcdatetime()) FOR [FechaCreacion]
GO
ALTER TABLE [app].[Usuario] ADD  DEFAULT ((0)) FOR [IntentosFallidos]
GO
ALTER TABLE [app].[Usuario] ADD  DEFAULT ('N') FOR [CambioClave]
GO
ALTER TABLE [app].[ActividadProyecto]  WITH CHECK ADD  CONSTRAINT [FK_ActividadProyecto_Fase] FOREIGN KEY([CodigoFase])
REFERENCES [app].[FaseProyecto] ([CodigoFase])
GO
ALTER TABLE [app].[ActividadProyecto] CHECK CONSTRAINT [FK_ActividadProyecto_Fase]
GO
ALTER TABLE [app].[ActividadProyecto]  WITH CHECK ADD  CONSTRAINT [FK_ActividadProyecto_Proyecto] FOREIGN KEY([CodigoProyecto])
REFERENCES [app].[Proyecto] ([Codigo])
GO
ALTER TABLE [app].[ActividadProyecto] CHECK CONSTRAINT [FK_ActividadProyecto_Proyecto]
GO
ALTER TABLE [app].[ActividadProyecto]  WITH CHECK ADD  CONSTRAINT [FK_ActividadProyecto_Responsable] FOREIGN KEY([CodigoResponsable])
REFERENCES [app].[Empleado] ([Codigo])
GO
ALTER TABLE [app].[ActividadProyecto] CHECK CONSTRAINT [FK_ActividadProyecto_Responsable]
GO
ALTER TABLE [app].[AsignacionProyecto]  WITH CHECK ADD  CONSTRAINT [FK_app_Asignacion_Empleado] FOREIGN KEY([CodigoEmpleado])
REFERENCES [app].[Empleado] ([Codigo])
GO
ALTER TABLE [app].[AsignacionProyecto] CHECK CONSTRAINT [FK_app_Asignacion_Empleado]
GO
ALTER TABLE [app].[AsignacionProyecto]  WITH CHECK ADD  CONSTRAINT [FK_app_Asignacion_Proyecto] FOREIGN KEY([CodigoProyecto])
REFERENCES [app].[Proyecto] ([Codigo])
GO
ALTER TABLE [app].[AsignacionProyecto] CHECK CONSTRAINT [FK_app_Asignacion_Proyecto]
GO
ALTER TABLE [app].[Cargo]  WITH CHECK ADD  CONSTRAINT [FK_app_Cargo_Departamento] FOREIGN KEY([CodigoDepartamento])
REFERENCES [app].[Departamento] ([Codigo])
GO
ALTER TABLE [app].[Cargo] CHECK CONSTRAINT [FK_app_Cargo_Departamento]
GO
ALTER TABLE [app].[Contrato]  WITH CHECK ADD  CONSTRAINT [FK_app_Contrato_Empleado] FOREIGN KEY([CodigoEmpleado])
REFERENCES [app].[Empleado] ([Codigo])
GO
ALTER TABLE [app].[Contrato] CHECK CONSTRAINT [FK_app_Contrato_Empleado]
GO
ALTER TABLE [app].[DependenciaTarea]  WITH CHECK ADD  CONSTRAINT [FK_DependenciaTarea_Preced] FOREIGN KEY([CodigoPredecesora])
REFERENCES [app].[TareaProyecto] ([CodigoTarea])
GO
ALTER TABLE [app].[DependenciaTarea] CHECK CONSTRAINT [FK_DependenciaTarea_Preced]
GO
ALTER TABLE [app].[DependenciaTarea]  WITH CHECK ADD  CONSTRAINT [FK_DependenciaTarea_Proyecto] FOREIGN KEY([CodigoProyecto])
REFERENCES [app].[Proyecto] ([Codigo])
GO
ALTER TABLE [app].[DependenciaTarea] CHECK CONSTRAINT [FK_DependenciaTarea_Proyecto]
GO
ALTER TABLE [app].[DependenciaTarea]  WITH CHECK ADD  CONSTRAINT [FK_DependenciaTarea_Tarea] FOREIGN KEY([CodigoTarea])
REFERENCES [app].[TareaProyecto] ([CodigoTarea])
GO
ALTER TABLE [app].[DependenciaTarea] CHECK CONSTRAINT [FK_DependenciaTarea_Tarea]
GO
ALTER TABLE [app].[Empleado]  WITH CHECK ADD  CONSTRAINT [FK_app_Empleado_Cargo] FOREIGN KEY([CodigoDepartamento], [CodigoCargo])
REFERENCES [app].[Cargo] ([CodigoDepartamento], [Codigo])
GO
ALTER TABLE [app].[Empleado] CHECK CONSTRAINT [FK_app_Empleado_Cargo]
GO
ALTER TABLE [app].[Empleado]  WITH CHECK ADD  CONSTRAINT [FK_app_Empleado_Persona] FOREIGN KEY([CodigoPersona])
REFERENCES [app].[Persona] ([Codigo])
GO
ALTER TABLE [app].[Empleado] CHECK CONSTRAINT [FK_app_Empleado_Persona]
GO
ALTER TABLE [app].[EntregableProyecto]  WITH CHECK ADD  CONSTRAINT [FK_EntregableProyecto_Tarea] FOREIGN KEY([CodigoTarea])
REFERENCES [app].[TareaProyecto] ([CodigoTarea])
GO
ALTER TABLE [app].[EntregableProyecto] CHECK CONSTRAINT [FK_EntregableProyecto_Tarea]
GO
ALTER TABLE [app].[Familiar]  WITH CHECK ADD  CONSTRAINT [FK_app_Familiar_Persona] FOREIGN KEY([CodigoPersona])
REFERENCES [app].[Persona] ([Codigo])
GO
ALTER TABLE [app].[Familiar] CHECK CONSTRAINT [FK_app_Familiar_Persona]
GO
ALTER TABLE [app].[FaseProyecto]  WITH CHECK ADD  CONSTRAINT [FK_FaseProyecto_Proyecto] FOREIGN KEY([CodigoProyecto])
REFERENCES [app].[Proyecto] ([Codigo])
GO
ALTER TABLE [app].[FaseProyecto] CHECK CONSTRAINT [FK_FaseProyecto_Proyecto]
GO
ALTER TABLE [app].[Formacion]  WITH CHECK ADD  CONSTRAINT [FK_app_Formacion_Empleado] FOREIGN KEY([CodigoEmpleado])
REFERENCES [app].[Empleado] ([Codigo])
GO
ALTER TABLE [app].[Formacion] CHECK CONSTRAINT [FK_app_Formacion_Empleado]
GO
ALTER TABLE [app].[MovimientoProyecto]  WITH CHECK ADD  CONSTRAINT [FK_app_Movimiento_Categoria] FOREIGN KEY([CodigoCategoria])
REFERENCES [app].[CategoriaGasto] ([CodigoCategoria])
GO
ALTER TABLE [app].[MovimientoProyecto] CHECK CONSTRAINT [FK_app_Movimiento_Categoria]
GO
ALTER TABLE [app].[MovimientoProyecto]  WITH CHECK ADD  CONSTRAINT [FK_app_Movimiento_Proyecto] FOREIGN KEY([CodigoProyecto])
REFERENCES [app].[Proyecto] ([Codigo])
GO
ALTER TABLE [app].[MovimientoProyecto] CHECK CONSTRAINT [FK_app_Movimiento_Proyecto]
GO
ALTER TABLE [app].[PerfilOpcion]  WITH CHECK ADD  CONSTRAINT [FK_app_PerfilOpcion_Opcion] FOREIGN KEY([CodigoOpcion])
REFERENCES [app].[Opcion] ([Codigo])
GO
ALTER TABLE [app].[PerfilOpcion] CHECK CONSTRAINT [FK_app_PerfilOpcion_Opcion]
GO
ALTER TABLE [app].[PerfilOpcion]  WITH CHECK ADD  CONSTRAINT [FK_app_PerfilOpcion_Perfil] FOREIGN KEY([CodigoPerfil])
REFERENCES [app].[Perfil] ([Codigo])
GO
ALTER TABLE [app].[PerfilOpcion] CHECK CONSTRAINT [FK_app_PerfilOpcion_Perfil]
GO
ALTER TABLE [app].[Proyecto]  WITH CHECK ADD  CONSTRAINT [FK_app_Proyecto_Departamento] FOREIGN KEY([CodigoDepartamento])
REFERENCES [app].[Departamento] ([Codigo])
GO
ALTER TABLE [app].[Proyecto] CHECK CONSTRAINT [FK_app_Proyecto_Departamento]
GO
ALTER TABLE [app].[Proyecto]  WITH CHECK ADD  CONSTRAINT [FK_app_Proyecto_Jefe] FOREIGN KEY([CodigoJefeProyecto])
REFERENCES [app].[Empleado] ([Codigo])
GO
ALTER TABLE [app].[Proyecto] CHECK CONSTRAINT [FK_app_Proyecto_Jefe]
GO
ALTER TABLE [app].[RegistroHora]  WITH CHECK ADD  CONSTRAINT [FK_app_Hora_Empleado] FOREIGN KEY([CodigoEmpleado])
REFERENCES [app].[Empleado] ([Codigo])
GO
ALTER TABLE [app].[RegistroHora] CHECK CONSTRAINT [FK_app_Hora_Empleado]
GO
ALTER TABLE [app].[RegistroHora]  WITH CHECK ADD  CONSTRAINT [FK_app_Hora_Proyecto] FOREIGN KEY([CodigoProyecto])
REFERENCES [app].[Proyecto] ([Codigo])
GO
ALTER TABLE [app].[RegistroHora] CHECK CONSTRAINT [FK_app_Hora_Proyecto]
GO
ALTER TABLE [app].[RegistroHora]  WITH CHECK ADD  CONSTRAINT [FK_RegistroHora_Tarea] FOREIGN KEY([CodigoTarea])
REFERENCES [app].[TareaProyecto] ([CodigoTarea])
GO
ALTER TABLE [app].[RegistroHora] CHECK CONSTRAINT [FK_RegistroHora_Tarea]
GO
ALTER TABLE [app].[SeguimientoProyecto]  WITH CHECK ADD  CONSTRAINT [FK_app_Seguimiento_Proyecto] FOREIGN KEY([CodigoProyecto])
REFERENCES [app].[Proyecto] ([Codigo])
GO
ALTER TABLE [app].[SeguimientoProyecto] CHECK CONSTRAINT [FK_app_Seguimiento_Proyecto]
GO
ALTER TABLE [app].[TareaProyecto]  WITH CHECK ADD  CONSTRAINT [FK_TareaProyecto_Actividad] FOREIGN KEY([CodigoActividad])
REFERENCES [app].[ActividadProyecto] ([CodigoActividad])
GO
ALTER TABLE [app].[TareaProyecto] CHECK CONSTRAINT [FK_TareaProyecto_Actividad]
GO
ALTER TABLE [app].[TareaProyecto]  WITH CHECK ADD  CONSTRAINT [FK_TareaProyecto_Fase] FOREIGN KEY([CodigoFase])
REFERENCES [app].[FaseProyecto] ([CodigoFase])
GO
ALTER TABLE [app].[TareaProyecto] CHECK CONSTRAINT [FK_TareaProyecto_Fase]
GO
ALTER TABLE [app].[TareaProyecto]  WITH CHECK ADD  CONSTRAINT [FK_TareaProyecto_Proyecto] FOREIGN KEY([CodigoProyecto])
REFERENCES [app].[Proyecto] ([Codigo])
GO
ALTER TABLE [app].[TareaProyecto] CHECK CONSTRAINT [FK_TareaProyecto_Proyecto]
GO
ALTER TABLE [app].[TareaProyecto]  WITH CHECK ADD  CONSTRAINT [FK_TareaProyecto_Responsable] FOREIGN KEY([CodigoResponsable])
REFERENCES [app].[Empleado] ([Codigo])
GO
ALTER TABLE [app].[TareaProyecto] CHECK CONSTRAINT [FK_TareaProyecto_Responsable]
GO
ALTER TABLE [app].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_app_Usuario_Persona] FOREIGN KEY([CodigoPersona])
REFERENCES [app].[Persona] ([Codigo])
GO
ALTER TABLE [app].[Usuario] CHECK CONSTRAINT [FK_app_Usuario_Persona]
GO
ALTER TABLE [app].[UsuarioPerfil]  WITH CHECK ADD  CONSTRAINT [FK_app_UsuarioPerfil_Perfil] FOREIGN KEY([CodigoPerfil])
REFERENCES [app].[Perfil] ([Codigo])
GO
ALTER TABLE [app].[UsuarioPerfil] CHECK CONSTRAINT [FK_app_UsuarioPerfil_Perfil]
GO
ALTER TABLE [app].[UsuarioPerfil]  WITH CHECK ADD  CONSTRAINT [FK_app_UsuarioPerfil_Usuario] FOREIGN KEY([Login])
REFERENCES [app].[Usuario] ([Login])
GO
ALTER TABLE [app].[UsuarioPerfil] CHECK CONSTRAINT [FK_app_UsuarioPerfil_Usuario]
GO
ALTER TABLE [app].[ActividadProyecto]  WITH CHECK ADD  CONSTRAINT [CK_ActividadProyecto_Estado] CHECK  (([Estado]='COMPLETADA' OR [Estado]='EN_PROCESO' OR [Estado]='PENDIENTE'))
GO
ALTER TABLE [app].[ActividadProyecto] CHECK CONSTRAINT [CK_ActividadProyecto_Estado]
GO
ALTER TABLE [app].[ActividadProyecto]  WITH CHECK ADD  CONSTRAINT [CK_ActividadProyecto_Fechas] CHECK  (([FechaFin]>=[FechaInicio]))
GO
ALTER TABLE [app].[ActividadProyecto] CHECK CONSTRAINT [CK_ActividadProyecto_Fechas]
GO
ALTER TABLE [app].[DependenciaTarea]  WITH CHECK ADD  CONSTRAINT [CK_DependenciaTarea_Desfase] CHECK  (([DesfaseDias]>=(-3650) AND [DesfaseDias]<=(3650)))
GO
ALTER TABLE [app].[DependenciaTarea] CHECK CONSTRAINT [CK_DependenciaTarea_Desfase]
GO
ALTER TABLE [app].[DependenciaTarea]  WITH CHECK ADD  CONSTRAINT [CK_DependenciaTarea_Distinta] CHECK  (([CodigoTarea]<>[CodigoPredecesora]))
GO
ALTER TABLE [app].[DependenciaTarea] CHECK CONSTRAINT [CK_DependenciaTarea_Distinta]
GO
ALTER TABLE [app].[DependenciaTarea]  WITH CHECK ADD  CONSTRAINT [CK_DependenciaTarea_Tipo] CHECK  (([TipoDependencia]='SF' OR [TipoDependencia]='FF' OR [TipoDependencia]='SS' OR [TipoDependencia]='FS'))
GO
ALTER TABLE [app].[DependenciaTarea] CHECK CONSTRAINT [CK_DependenciaTarea_Tipo]
GO
ALTER TABLE [app].[EntregableProyecto]  WITH CHECK ADD  CONSTRAINT [CK_EntregableProyecto_Estado] CHECK  (([Estado]='JUSTIFICADO' OR [Estado]='ENTREGADO' OR [Estado]='PENDIENTE'))
GO
ALTER TABLE [app].[EntregableProyecto] CHECK CONSTRAINT [CK_EntregableProyecto_Estado]
GO
ALTER TABLE [app].[EntregableProyecto]  WITH CHECK ADD  CONSTRAINT [CK_EntregableProyecto_Relacion] CHECK  (([TipoRelacion]='TAREA' AND [CodigoTarea] IS NOT NULL AND [CodigoRelacion]=[CodigoTarea] OR ([TipoRelacion]='ACTIVIDAD' OR [TipoRelacion]='FASE') AND [CodigoTarea] IS NULL))
GO
ALTER TABLE [app].[EntregableProyecto] CHECK CONSTRAINT [CK_EntregableProyecto_Relacion]
GO
ALTER TABLE [app].[FaseProyecto]  WITH CHECK ADD  CONSTRAINT [CK_FaseProyecto_Estado] CHECK  (([Estado]='COMPLETADA' OR [Estado]='EN_PROCESO' OR [Estado]='PENDIENTE'))
GO
ALTER TABLE [app].[FaseProyecto] CHECK CONSTRAINT [CK_FaseProyecto_Estado]
GO
ALTER TABLE [app].[FaseProyecto]  WITH CHECK ADD  CONSTRAINT [CK_FaseProyecto_Fechas] CHECK  (([FechaFin]>=[FechaInicio]))
GO
ALTER TABLE [app].[FaseProyecto] CHECK CONSTRAINT [CK_FaseProyecto_Fechas]
GO
ALTER TABLE [app].[TareaProyecto]  WITH CHECK ADD  CONSTRAINT [CK_TareaProyecto_Estado] CHECK  (([Estado]='COMPLETADA' OR [Estado]='EN_PROCESO' OR [Estado]='PENDIENTE'))
GO
ALTER TABLE [app].[TareaProyecto] CHECK CONSTRAINT [CK_TareaProyecto_Estado]
GO
ALTER TABLE [app].[TareaProyecto]  WITH CHECK ADD  CONSTRAINT [CK_TareaProyecto_Fechas] CHECK  (([FechaFin]>=[FechaInicio]))
GO
ALTER TABLE [app].[TareaProyecto] CHECK CONSTRAINT [CK_TareaProyecto_Fechas]
GO
ALTER TABLE [app].[TareaProyecto]  WITH CHECK ADD  CONSTRAINT [CK_TareaProyecto_Horas] CHECK  (([HorasPlanificadas]>=(0)))
GO
ALTER TABLE [app].[TareaProyecto] CHECK CONSTRAINT [CK_TareaProyecto_Horas]
GO
ALTER TABLE [app].[TareaProyecto]  WITH CHECK ADD  CONSTRAINT [CK_TareaProyecto_Peso] CHECK  (([Peso]>(0) AND [Peso]<=(100)))
GO
ALTER TABLE [app].[TareaProyecto] CHECK CONSTRAINT [CK_TareaProyecto_Peso]