using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using SIMTernakAyam.Common;
using SIMTernakAyam.Data;
using SIMTernakAyam.Infrastructure;
using SIMTernakAyam.Repositories.Interfaces;
using SIMTernakAyam.Repository;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services;
using SIMTernakAyam.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
var connectionDb = builder.Configuration.GetConnectionString("DefaultConnection");
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionDb));

// CORS Configuration
var corsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Dependency Injection
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IKandangRepository, KandangRepository>();
builder.Services.AddScoped<IPanenRepository, PanenRepository>();
builder.Services.AddScoped<IBiayaRepository, BiayaRepository>();
builder.Services.AddScoped<IVaksinRepository, VaksinRepository>();
builder.Services.AddScoped<IPakanRepository, PakanRepository>();
builder.Services.AddScoped<IJenisKegiatanRepository, JenisKegiatanRepository>();
builder.Services.AddScoped<IMortalitasRepository, MortalitasRepository>();
builder.Services.AddScoped<IOperasionalRepository, OperasionalRepository>();
builder.Services.AddScoped<IAyamRepository, AyamRepository>();
builder.Services.AddScoped<IKandangAsistenRepository, KandangAsistenRepository>();
builder.Services.AddScoped<IJurnalHarianRepository, JurnalHarianRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IHargaPasarRepository, HargaPasarRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IkandangService, KandangService>();
builder.Services.AddScoped<IPanenService, PanenService>();
builder.Services.AddScoped<IBiayaService, BiayaService>();
builder.Services.AddScoped<IVaksinService, VaksinService>();
builder.Services.AddScoped<IPakanService, PakanService>();
builder.Services.AddScoped<IJenisKegiatanService, JenisKegiatanService>();
builder.Services.AddScoped<IMortalitasService, MortalitasService>();
builder.Services.AddScoped<IOperasionalService, OperasionalService>();
builder.Services.AddScoped<IAyamService, AyamService>();
builder.Services.AddScoped<IStokService, StokService>();
builder.Services.AddScoped<ILaporanService, LaporanService>();
builder.Services.AddScoped<IKandangAsistenService, KandangAsistenService>();
builder.Services.AddScoped<IChartService, ChartService>();
builder.Services.AddScoped<IJurnalHarianService, JurnalHarianService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IHargaPasarService, HargaPasarService>();

// Controller & JSON Config
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SnakeCaseParameterTransformer()));
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SIM Ternak Ayam API",
        Version = "v1",
        Description = "API untuk Sistem Informasi Manajemen Ternak Ayam",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "SIM Ternak Ayam Team"
        }
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header menggunakan Bearer scheme. Format: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

// Static Files - Enable serving files from wwwroot folder
app.UseStaticFiles();

app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIM Ternak Ayam API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "SIM Ternak Ayam API Documentation";
    c.DefaultModelsExpandDepth(-1);
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

// Controller Mapping
app.MapControllers();

// Root Redirect
app.MapGet("/", () => Results.Redirect("/swagger"))
    .WithName("Root")
    .WithTags("Navigation")
    .WithSummary("Redirect to API Documentation")
    .ExcludeFromDescription();

// Health Check Endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Service = "SIM Ternak Ayam API"
}))
    .WithName("HealthCheck")
    .WithTags("Health")
    .WithSummary("Check API health status");

// API Info Endpoint
app.MapGet("/api", () => Results.Ok(new
{
    Message = "Welcome to SIM Ternak Ayam API",
    Documentation = "/swagger",
    Health = "/health",
    Version = "1.0.0",
    Endpoints = new
    {
        Users = "/api/user",
        Kandang = "/api/kandang",
        Panen = "/api/panen",
        Vaksin = "/api/vaksin",
        Ayam = "/api/ayam",
        HargaPasar = "/api/harga-pasar"
        // Tambahkan lainnya jika perlu
    }
}))
    .WithName("ApiInfo")
    .WithTags("Info")
    .WithSummary("Get API information");

// Catch-all 404
app.MapFallback(() => Results.NotFound(new
{
    Error = "Endpoint not found",
    Message = "The requested endpoint does not exist. Please check the URL and try again.",
    Documentation = "/swagger",
    AvailableEndpoints = "/api",
    Timestamp = DateTime.UtcNow
}))
.ExcludeFromDescription();

// DB Connection Log
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        if (db.Database.CanConnect())
            log.LogInformation("✅ Database connected successfully");
        else
            log.LogWarning("⚠️ Database connection failed");
    }
    catch (Exception ex)
    {
        log.LogError("❌ Database connection error: {Error}", ex.Message);
    }
}

app.Run();
