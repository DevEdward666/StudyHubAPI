using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Study_Hub.Data;
using Study_Hub.Services;
using Study_Hub.Services.Interfaces;
using StudyHubApi.Services;
using StudyHubApi.Services.Interfaces;
using System.Text;
using System.Text.Json.Serialization;
using Study_Hub.Models.Entities;
using Study_Hub.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),  npgsqlOptions =>
    {
        npgsqlOptions.MapEnum<SessionStatus>("session_status");
    }));

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JWT");
string secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT secret is not configured. Please set JWT:Secret in configuration.");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPremiseService, PremiseService>();
builder.Services.AddScoped<Study_Hub.Service.Interface.IPushNotificationService, Study_Hub.Service.PushNotificationService>();
// Report service registration
builder.Services.AddScoped<IReportService, Study_Hub.Service.ReportService>();
// Promo service registration
builder.Services.AddScoped<Study_Hub.Services.Interfaces.IPromoService, Study_Hub.Service.PromoService>();
// Global Settings service registration
builder.Services.AddScoped<Study_Hub.Services.Interfaces.IGlobalSettingsService, Study_Hub.Service.GlobalSettingsService>();
// Rate service registration
builder.Services.AddScoped<Study_Hub.Services.Interfaces.IRateService, Study_Hub.Services.RateService>();
// Thermal Printer service registration
builder.Services.AddScoped<Study_Hub.Service.Interface.IThermalPrinterService, Study_Hub.Service.ThermalPrinterService>();

// WiFi Access System Services
builder.Services.AddScoped<Study_Hub.Service.Interface.IWifiService, Study_Hub.Service.WifiService>();
builder.Services.Configure<Study_Hub.Models.RouterOptions>(builder.Configuration.GetSection("Router"));
builder.Services.AddSingleton<Study_Hub.Service.Interface.IRouterManager, Study_Hub.Service.SshRouterManager>();
builder.Services.AddHostedService<Study_Hub.Service.Background.WifiCleanupService>();

// Register PushServiceClient for Web Push
builder.Services.AddHttpClient<Lib.Net.Http.WebPush.PushServiceClient>();

// SignalR Configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Background Services
builder.Services.AddHostedService<Study_Hub.Services.Background.SessionExpiryChecker>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            // Allow localhost
            if (origin.StartsWith("http://localhost") || origin.StartsWith("https://localhost"))
                return true;
            
            // Allow Vercel deployments
            if (origin.Contains("vercel.app"))
                return true;
            
            // Allow Render deployments
            if (origin.Contains("onrender.com"))
                return true;
            
            // Allow specific production domains
            if (origin == "https://study-hub-app-nu.vercel.app")
                return true;
            
            return false;
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials(); // Required for SignalR
    });
});

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Study Hub API",
        Version = "v1",
        Description = "API for Study Hub table management system"
    });

    // JWT Bearer configuration for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Use Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable WebSockets for SignalR
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Map SignalR Hubs
app.MapHub<Study_Hub.Hubs.NotificationHub>("/hubs/notifications");

// Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();