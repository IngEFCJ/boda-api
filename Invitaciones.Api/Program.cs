using Invitaciones.Api.business_Logic;
using Invitaciones.Api.Data;
using Invitaciones.Api.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

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
// (Opcional) CORS si Angular está en otro dominio
// ======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontCors", policy =>
    {
        policy
            .WithOrigins(
                "https://icy-beach-09390d410.5.azurestaticapps.net",
                "http://localhost:4200"
            )
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

app.UseHttpsRedirection();

// (Opcional) CORS: va antes de Authorization y MapControllers
app.UseCors("FrontCors");

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
