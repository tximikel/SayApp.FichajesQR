using Microsoft.EntityFrameworkCore;
using SayApp.FichajesQR.Data.DbContexts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Leer configuración
var configuration = builder.Configuration;

// Obtener nombres de las conexiones desde ConnectionName
var fichajesConnName = configuration.GetSection("ConnectionName")["Fichajes"];
var suasorConnName = configuration.GetSection("ConnectionName")["Suasor"];

// Registrar AppDBContext
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString(fichajesConnName)));

// Registrar SuasorDbContext (solo lectura)
builder.Services.AddDbContext<SuasorDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString(suasorConnName)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
