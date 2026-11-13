# Verificación local

Desde la carpeta del aplicativo:

```powershell
node --test Tests/http-compartido.test.cjs
dotnet build PROYECTO_MONGO_DOTNET.csproj
dotnet run --project Tests/MasterMonster.Pruebas.csproj
```

El ejecutable C# contiene 17 casos, no necesita SQL Server y devuelve código de salida distinto de cero si una comprobación falla. Usa el framework compartido de ASP.NET Core y las dependencias ya existentes del aplicativo; no agrega paquetes de pruebas. Se ejecuta con `dotnet run`, no con `dotnet test`.

Se cubren cálculos financieros, tipos y fechas de movimientos, disponibilidad, fechas del proyecto, transiciones, avance ponderado, ciclos y grafos extensos, firmas de fotos, selección del permiso crear/editar, ausencia de sesión y conflicto al revisar movimientos.

Las cuatro pruebas Node ejecutan el cliente HTTP compartido en un contexto aislado: token de formulario, obtención concurrente del token, recuperación tras error y propagación de HTTP 403.

Estado en el entorno de refactorización: Node 4/4 y sintaxis de los 19 scripts correctas. Compilación y ejecutable C# pendientes porque `dotnet --list-sdks` no devuelve SDK alguno. Esto no establece que C# compile ni valida consultas, bloqueos, rollback o interfaz en ejecución.

Pruebas de integración pendientes en una base desechable con el esquema actual:

- Fallar la inserción de formación y verificar rollback de persona, empleado y familiares.
- Enviar dos asignaciones concurrentes de 60 % para un empleado y confirmar que solo una persista.
- Editar, aprobar y rechazar concurrentemente un movimiento; comprobar un único resultado de revisión y HTTP 409 para el perdedor.
- Fallar el seguimiento durante un cierre y comprobar que proyecto y asignaciones permanezcan sin cambios.
- Verificar perfiles revocados, cambio obligatorio de clave, permisos ocultos contextuales, fotos protegidas y tokens en formularios, pestañas e iframes.
- Comparar resúmenes financieros, modales, calendarios, Gantt y exportaciones con los datos de referencia.
