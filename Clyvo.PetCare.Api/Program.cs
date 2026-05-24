using System.Reflection;
using System.Text.Json.Serialization;
using Clyvo.PetCare.Api.Application.Services;
using Clyvo.PetCare.Api.Application.Services.Interfaces;
using Clyvo.PetCare.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ───────────────────────────────────────────────────────────────────────────────
// Controllers + serialização JSON
// ───────────────────────────────────────────────────────────────────────────────
builder.Services
    .AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();

// ───────────────────────────────────────────────────────────────────────────────
// Banco de dados — Oracle FIAP
// ───────────────────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("OracleFiap")
    ?? throw new InvalidOperationException("Connection string 'OracleFiap' não configurada em appsettings.json.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(connectionString, oracle =>
            oracle.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion11))
           .EnableDetailedErrors(builder.Environment.IsDevelopment()));

// ───────────────────────────────────────────────────────────────────────────────
// Services (camada de aplicação)
// ───────────────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<ITutorService, TutorService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IVeterinarioService, VeterinarioService>();
builder.Services.AddScoped<IConsultaService, ConsultaService>();
builder.Services.AddScoped<IVacinaService, VacinaService>();

// ───────────────────────────────────────────────────────────────────────────────
// Swagger / OpenAPI
// ───────────────────────────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clyvo Pet Care API",
        Version = "v1",
        Description = "API REST para continuidade do cuidado e engajamento na jornada de saúde de pets (cachorros). " +
                      "Desenvolvido para o Challenge FIAP 2026 — Clyvo VET.",
        Contact = new OpenApiContact
        {
            Name = "Equipe Clyvo Pet Care",
            Email = "contato@clyvo.com.br"
        },
        License = new OpenApiLicense { Name = "Uso acadêmico — FIAP" }
    });

    // Inclui comentários XML nas descrições do Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    c.EnableAnnotations();
});

// ───────────────────────────────────────────────────────────────────────────────
// CORS — liberado em dev, restrito em produção (ajustar antes do deploy)
// ───────────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

var app = builder.Build();

// ───────────────────────────────────────────────────────────────────────────────
// Pipeline HTTP
// ───────────────────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clyvo Pet Care API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Clyvo Pet Care — API Docs";
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Endpoint raiz amigável — redireciona para o Swagger
app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.Run();
