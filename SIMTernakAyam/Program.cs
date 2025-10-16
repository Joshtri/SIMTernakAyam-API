using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using SIMTernakAyam.Data;
using SIMTernakAyam.Infrastructure;
using SIMTernakAyam.Repositories.Interfaces;
using SIMTernakAyam.Repository;
using SIMTernakAyam.Repository.Interfaces;
using SIMTernakAyam.Services;
using SIMTernakAyam.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// CORS Configuration
var corsPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",  // Vite default port
                "http://localhost:3000",  // React default port
                "http://localhost:4200"   // Angular default port
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Database Configuration
var connectionDb = builder.Configuration.GetConnectionString("DefaultConnection");

// Configure Npgsql to handle DateTime properly
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionDb));

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance for expiry
    };
});

builder.Services.AddAuthorization();

// Register JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Register Dashboard Service
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Repository Registration - Dependency Injection
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

// Service Registration - Dependency Injection
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

builder.Services.AddControllers(options =>
{
    // ubah token [controller]/[action] -> snake_case
    options.Conventions.Add(
        new RouteTokenTransformerConvention(new SnakeCaseParameterTransformer()));
})
.AddJsonOptions(options =>
{
    // Serialize enum sebagai string (bukan angka)
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

    // Case insensitive untuk enum
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddRouting(o =>
{
    o.LowercaseUrls = true;
    o.LowercaseQueryStrings = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

    // JWT Authentication untuk Swagger
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS - HARUS SEBELUM UseAuthentication dan UseAuthorization
app.UseCors(corsPolicy);

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
