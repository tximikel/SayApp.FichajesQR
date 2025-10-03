using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SayApp.FichajesQR.Data.DbContexts;
using SayApp.FichajesQR.Data.DbContexts.App.Entities;
using SayApp.FichajesQR.Worker;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// --- A�ade estas dos l�neas para Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// --------------------------------------------

// Leer configuraci�n
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

builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.MapControllers();

Console.WriteLine(">>> Worker API arrancando y escuchando peticiones HTTP/HTTPS...");

// --- A�ade este bloque para habilitar Swagger en desarrollo ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// --------------------------------------------------------------

app.Run();
