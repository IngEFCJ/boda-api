using Invitaciones.Api.business_Logic;
using Invitaciones.Api.Data;
using Invitaciones.Api.Interfaces;
using Invitaciones.Api.DTO;
using Invitaciones.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;

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
// DI (Admin Auth)
// ======================
builder.Services.AddScoped<IAdminAuthRepository, AdminAuthRepository>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();

// ======================
// DI (Admin Events)
// ======================
builder.Services.AddScoped<IAdminEventRepository, AdminEventRepository>();
builder.Services.AddScoped<IAdminEventService, AdminEventService>();

// ======================
// DI (Admin Invitations)
// ======================
builder.Services.AddScoped<IAdminInvitationRepository, AdminInvitationRepository>();
builder.Services.AddScoped<IAdminInvitationService, AdminInvitationService>();

// ======================
// DI (Admin Tickets)
// ======================
builder.Services.AddScoped<IAdminTicketRepository, AdminTicketRepository>();
builder.Services.AddScoped<IAdminTicketService, AdminTicketService>();

// ======================
// DI (Admin Venue)
// ======================
builder.Services.AddScoped<IAdminVenueRepository, AdminVenueRepository>();
builder.Services.AddScoped<IAdminVenueService, AdminVenueService>();

// ======================
// DI (Admin Check-In)
// ======================
builder.Services.AddScoped<IAdminCheckInRepository, AdminCheckInRepository>();
builder.Services.AddScoped<IAdminCheckInService, AdminCheckInService>();

// ======================
// DI (Admin Seats)
// ======================
builder.Services.AddScoped<IAdminSeatRepository, AdminSeatRepository>();
builder.Services.AddScoped<IAdminSeatService, AdminSeatService>();

// ======================
// DI (Admin Bulk Import)
// ======================
builder.Services.AddScoped<IAdminBulkImportService, AdminBulkImportService>();

// ======================
// DI (Admin Dashboard)
// ======================
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

// ======================
// JWT Authentication
// ======================
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"]
    ?? throw new InvalidOperationException("Missing configuration: Jwt:SecretKey");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

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
            .WithOrigins(allowedOrigins.Concat(new[] { "http://localhost:4201" }).Distinct().ToArray())
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

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "ESTA ES LA NUEVA API - " + typeof(Program).Assembly.GetName().Name);

app.UseMiddleware<AdminExceptionMiddleware>();

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