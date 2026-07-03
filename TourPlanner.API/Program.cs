using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TourPlanner.BL;
using TourPlanner.DAL;

var builder = WebApplication.CreateBuilder(args);

// DB Context - Connection-String: ENV-Variable hat Vorrang, sonst aus Config
// (appsettings.Development.json ueberschreibt appsettings.json automatisch im Dev-Modus)
var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<TourPlannerDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped<ITourRepository, TourRepository>();
builder.Services.AddScoped<ITourLogRepository, TourLogRepository>();

// ORS Service - API-Key aus ENV oder Config
var orsApiKey = Environment.GetEnvironmentVariable("ORS_API_KEY")
    ?? builder.Configuration["OrsApiKey"]
    ?? "";
builder.Services.AddSingleton<IOrsService>(sp =>
    new OrsService(orsApiKey, sp.GetRequiredService<ILogger<OrsService>>()));

// Map Image Service
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<IMapImageService>(sp =>
    new MapImageService(sp.GetRequiredService<HttpClient>(), sp.GetRequiredService<ILogger<MapImageService>>()));

// Services
var configuredImageDir = Path.Combine(Directory.GetCurrentDirectory(),
    builder.Configuration["ImageDirectory"] ?? "Images");
builder.Services.AddScoped<ITourService>(sp =>
    new TourService(
        sp.GetRequiredService<ITourRepository>(),
        sp.GetRequiredService<IOrsService>(),
        sp.GetRequiredService<IMapImageService>(),
        configuredImageDir,
        sp.GetRequiredService<ILogger<TourService>>()));
builder.Services.AddScoped<ITourLogService, TourLogService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // damit keine Endlosschleife bei Tour <-> TourLog
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    // Enum als String serialisieren (nicht als int)
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS fuer Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Statische Dateien fuer Tour-Bilder servieren
var imageDir = Path.Combine(Directory.GetCurrentDirectory(),
    builder.Configuration["ImageDirectory"] ?? "Images");
if (!Directory.Exists(imageDir))
    Directory.CreateDirectory(imageDir);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(imageDir),
    RequestPath = "/images"
});

app.UseCors("AllowFrontend");

app.MapControllers();

// Automatische DB-Migration beim Startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TourPlannerDbContext>();
    db.Database.Migrate();
}

app.Run();
