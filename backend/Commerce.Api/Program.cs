using Commerce.Api.Exceptions;
using Commerce.Api.Repositories;
using Commerce.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// Configuración
// ---------------------------------------------------------------------
const string FrontendCorsPolicy = "Frontend";

// Orígenes permitidos para el frontend (appsettings.json -> Cors:AllowedOrigins).
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];

// ---------------------------------------------------------------------
// Servicios (inyección de dependencias)
// ---------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Manejo global de errores con respuestas ProblemDetails (RFC 7807).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// CORS: permite que la app Angular (localhost:4200) consuma la API.
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Capas de la aplicación: Controller -> Service -> Repository -> SQL Server.
builder.Services.AddScoped<ICommerceRepository, CommerceRepository>();
builder.Services.AddScoped<ICommerceService, CommerceService>();

var app = builder.Build();

// ---------------------------------------------------------------------
// Pipeline HTTP (el orden importa)
// ---------------------------------------------------------------------
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthorization();

app.MapControllers();

app.Run();