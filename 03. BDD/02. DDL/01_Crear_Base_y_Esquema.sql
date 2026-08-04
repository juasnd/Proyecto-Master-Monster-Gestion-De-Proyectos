/*
  DDL - Base y esquema del aplicativo Master Monster.
  Ejecute este archivo primero en SQL Server.
  Para otro nombre de base, reemplace el valor de DatabaseName.
*/
:setvar DatabaseName "PROYECTO_MONSTER"

IF DB_ID(N'$(DatabaseName)') IS NULL
BEGIN
    DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(N'$(DatabaseName)') + N';';
    EXEC sys.sp_executesql @sql;
END;
GO

USE [$(DatabaseName)];
GO

IF SCHEMA_ID(N'app') IS NULL
    EXEC(N'CREATE SCHEMA app AUTHORIZATION dbo;');
GO
