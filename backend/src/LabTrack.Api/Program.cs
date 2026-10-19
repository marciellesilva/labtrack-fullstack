using System.Text.Json.Serialization;
using LabTrack.Api.Middleware;
using LabTrack.Application;
using LabTrack.Infrastructure;
using LabTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LabTrack API", Version = "v1",
        Description = "Gestão de amostras e resultados de análises laboratoriais" });
    var xml = Path.Combine(AppContext.BaseDirectory, "LabTrack.Api.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

// CORS para o front-end Angular em desenvolvimento (ng serve).
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"])
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddHealthChecks().AddDbContextCheck<LabTrackDbContext>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Configuration.GetValue("Swagger:Enabled", app.Environment.IsDevelopment()))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();
app.MapHealthChecks("/health");

// Aplica as migrations automaticamente (útil no Docker Compose).
if (app.Configuration.GetValue("Database:MigrateOnStartup", false))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LabTrackDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

/// <summary>Exposto para os testes de integração (WebApplicationFactory).</summary>
public partial class Program;
