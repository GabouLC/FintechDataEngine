using FintechDataEngine.Application.Interfaces;
using FintechDataEngine.Application.Services;
using FintechDataEngine.Infrastructure.Parsers;
using FintechDataEngine.Infrastructure.Repositories;
using Scalar.AspNetCore; // <-- La nueva librería

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 1. El nuevo estándar de .NET 10 para generar la documentación
builder.Services.AddOpenApi(); 

// --- INYECCIÓN DE DEPENDENCIAS ---
builder.Services.AddScoped<IExcelParser, FastExcelParser>();

string connectionString = "Server=DESKTOPGABO\\SQLEXPRESS;Database=FintechDB;Trusted_Connection=True;TrustServerCertificate=True;";
builder.Services.AddScoped<IFinancialRecordRepository>(sp => new SqlFinancialRecordRepository(connectionString));

builder.Services.AddScoped<FinancialDataProcessorService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // 2. Mapeamos el nuevo estándar visual
    app.MapOpenApi();
    app.MapScalarApiReference(); // <-- Esto reemplaza a app.UseSwaggerUI()
}

app.UseAuthorization();
app.MapControllers();

app.Run();