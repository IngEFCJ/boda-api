using Invitaciones.Api.business_Logic;
using Invitaciones.Api.Data;
using Invitaciones.Api.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicyName = "FrontCors";

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ======================
// DI (Invitaciones)
// ======================
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IInvitationRepository, InvitationRepository>();
builder.Services.AddScoped<IPublicInvitationService, PublicInvitationService>();

// ======================
// CORS para el frontend publico
// ======================
var configuredOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[]
    {
        "https://icy-beach-09390d410.5.azurestaticapps.net",
        "http://localhost:4200"
    };

var allowedOrigins = configuredOrigins
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Boda API v1");
    c.RoutePrefix = "swagger";
});

// CORS debe ejecutarse antes de redirecciones, Authorization y MapControllers.
app.UseCors(CorsPolicyName);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/", () => "ESTA ES LA NUEVA API - " + typeof(Program).Assembly.GetName().Name);

app.MapControllers();

app.Run();


// ======================
// Connection Factory
// ======================
public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _cs;

    public SqlConnectionFactory(IConfiguration config)
    {
        _cs = config.GetConnectionString("Default")
              ?? throw new InvalidOperationException("Missing connection string: Default");
    }

    public IDbConnection Create() => new SqlConnection(_cs);
}
