using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PROYECTO_MONGO_DOTNET.Middleware;
using PROYECTO_MONGO_DOTNET.Models;
using PROYECTO_MONGO_DOTNET.Repositories;
using PROYECTO_MONGO_DOTNET.Services;
using PROYECTO_MONGO_DOTNET.Services.Finanzas;
using PROYECTO_MONGO_DOTNET.Services.Proyectos;
using PROYECTO_MONGO_DOTNET.Services.Seguridad;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddJsonOptions(opciones =>
{
    opciones.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.Configure<ApiBehaviorOptions>(opciones =>
{
    opciones.InvalidModelStateResponseFactory = contexto =>
    {
        static string NormalizarCampo(string campo)
        {
            if (campo.StartsWith("$.", StringComparison.Ordinal)) campo = campo[2..];
            if (campo.Contains('.')) campo = campo[(campo.LastIndexOf('.') + 1)..];
            return string.IsNullOrWhiteSpace(campo)
                ? "solicitud"
                : char.ToLowerInvariant(campo[0]) + campo[1..];
        }

        var errores = contexto.ModelState.Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => NormalizarCampo(x.Key),
                x => x.Value!.Errors.Select(e =>
                {
                    var mensaje = e.ErrorMessage ?? "";
                    if (mensaje.Contains("field is required", StringComparison.OrdinalIgnoreCase)) return "Este campo es obligatorio.";
                    if (mensaje.Contains("could not be converted", StringComparison.OrdinalIgnoreCase)) return "Ingrese un valor válido para este campo.";
                    if (mensaje.Contains("must be between", StringComparison.OrdinalIgnoreCase)) return "El valor está fuera del rango permitido.";
                    if (mensaje.Contains("maximum length", StringComparison.OrdinalIgnoreCase)) return "El texto supera la longitud permitida.";
                    if (mensaje.Contains("not valid", StringComparison.OrdinalIgnoreCase)) return "El valor indicado no es válido.";
                    return string.IsNullOrWhiteSpace(mensaje) ? "El valor indicado no es válido." : mensaje;
                }).ToArray());
        return new BadRequestObjectResult(ApiRespuesta<object>.Fallo("Revise los campos señalados.", errores));
    };
});
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery(opciones => opciones.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromMinutes(60);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
    opciones.Cookie.SameSite = SameSiteMode.Strict;
    opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddSingleton<SqlContexto>();
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddSingleton<CodigoDAO>();
builder.Services.AddSingleton<UsuarioDAO>();
builder.Services.AddSingleton<DepartamentoDAO>();
builder.Services.AddSingleton<CargoDAO>();
builder.Services.AddSingleton<PersonaDAO>();
builder.Services.AddSingleton<FamiliarDAO>();
builder.Services.AddSingleton<FormacionDAO>();
builder.Services.AddSingleton<EmpleadoDAO>();
builder.Services.AddSingleton<ReporteHistorialDAO>();
builder.Services.AddSingleton<PermisoDAO>();
builder.Services.AddSingleton<PerfilDAO>();
builder.Services.AddSingleton<AuditoriaDAO>();
builder.Services.AddSingleton<AsignacionUsuarioPerfilDAO>();

builder.Services.AddSingleton<IProyectoRepositorio, ProyectoRepositorio>();
builder.Services.AddSingleton<IAsignacionProyectoRepositorio, AsignacionProyectoRepositorio>();
builder.Services.AddSingleton<ISeguimientoProyectoRepositorio, SeguimientoProyectoRepositorio>();
builder.Services.AddSingleton<IPlanificacionProyectoRepositorio, PlanificacionProyectoRepositorio>();
builder.Services.AddSingleton<ContratoRepositorio>();
builder.Services.AddSingleton<CategoriaGastoRepositorio>();
builder.Services.AddSingleton<ParametroFinancieroRepositorio>();
builder.Services.AddSingleton<MovimientoProyectoRepositorio>();
builder.Services.AddSingleton<CostoProyectoServicio>();
builder.Services.AddSingleton<ProyectoServicio>();
builder.Services.AddSingleton<AsignacionProyectoServicio>();
builder.Services.AddSingleton<SeguimientoProyectoServicio>();
builder.Services.AddSingleton<PlanificacionProyectoServicio>();
builder.Services.AddSingleton<ContratoServicio>();
builder.Services.AddSingleton<CatalogosFinancierosServicio>();
builder.Services.AddSingleton<MovimientoProyectoServicio>();
builder.Services.AddSingleton<SeguridadServicio>();
builder.Services.AddSingleton<SeguridadCrudServicio>();

var app = builder.Build();

app.UseMiddleware<ManejoExcepcionesMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Auth}/{action=Index}/{id?}");

app.Run();

public partial class Program;
