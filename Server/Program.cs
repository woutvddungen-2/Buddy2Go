using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Server.Common;
using Server.Features.Buddies;
using Server.Features.Chats;
using Server.Features.DangerousPlaces;
using Server.Features.Journeys;
using Server.Features.Users;
using Server.Infrastructure.Cleanup;
using Server.Infrastructure.Data;
using Server.Infrastructure.Database;
using Server.Services;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

//load variables:
string jwtSecret = builder.Configuration.GetRequiredSection("JwtSettings:Secret").Get<string>()!;
bool sslEnabled = builder.Configuration.GetValue<bool>("SSL:Enabled", false);
int kestrelPort = builder.Configuration.GetValue<int>("PORT", 5001);

// -------------------- Logging --------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Logging.AddFilter("Server", LogLevel.Information);

builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Hosting", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Mvc", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Routing", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.Watch", LogLevel.Warning);

// -------------------- Authentication --------------------

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    ILoggerFactory loggerFactory = LoggerFactory.Create(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    });
    ILogger log = loggerFactory.CreateLogger("JWT");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("jwt", out string? token))
            {
            context.Token = token;
                }
            return Task.CompletedTask;
        }
    };
});

// -------------------- Authorization --------------------
builder.Services.AddAuthorization();

// -------------------- Controllers --------------------
builder.Services.AddControllers();

//-------------------- Services --------------------
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDangerousPlaceService, DangerousPlaceService>();
builder.Services.AddScoped<JourneyService>();
builder.Services.AddScoped<BuddyService>();
builder.Services.AddScoped<ChatService>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ISmsService, SmsService>();

// -------------------- Cleanup Services --------------------
builder.Services.AddScoped<JourneyCleanupService>();
//builder.Services.AddScoped<DangerousPlaceCleanupService>();
builder.Services.AddHostedService<CleanupBackgroundService>();

// -------------------- Swagger --------------------
#if DEBUG
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Buddy2Go", Version = "v0.8" });
});
#endif

//------------- Configure https for docker -------------------------------
// Configure Kestrel for HTTPS inside Docker
const string certPath = "/https/aspnetapp.pfx";
const string certPassword = "MyPassword123";

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        if (sslEnabled && File.Exists(certPath))
        {
            Console.WriteLine($"[Kestrel] HTTPS enabled on port {kestrelPort}");
            options.ListenAnyIP(kestrelPort, listenOptions =>
            {
                listenOptions.UseHttps(certPath, certPassword);
            });
        }
        else
        {
            Console.WriteLine($"[Kestrel] HTTP enabled on port {kestrelPort}");
            options.ListenAnyIP(kestrelPort);
        }
    });
}

// -------------------- CORS --------------------

// Read environment variable
string? urlsEnv = Environment.GetEnvironmentVariable("AllowedOrigins");

// Split into array, removing empty entries and trimming whitespace
string[] urls = Array.Empty<string>();
if (!string.IsNullOrEmpty(urlsEnv))
{
    urls = urlsEnv.Split(';');
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm", policy =>
    {
        policy.WithOrigins(urls)
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// -------------------- Database --------------------
if (!builder.Environment.IsEnvironment("Testing"))
{
    DbConnectHelper.AddDatabase(builder.Services, builder.Configuration.GetSection("DbSettings"));
}

// -------------------- Build App --------------------
WebApplication? app = builder.Build();

// -------------------- Seed Database -----------------

if (!builder.Environment.IsEnvironment("Testing"))
{
    const int maxRetries = 3;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        using IServiceScope scope = app.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ILogger logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            await DbInitializer.InitializeAsync(db, logger);
            logger.LogInformation("Database ready.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database init failed, attempt {Attempt}/{Max}.", attempt, maxRetries);

            if (attempt == maxRetries)
            {
                logger.LogError("Database initialization failed after {Max} attempts. Exiting...", maxRetries);
                throw new DbInitializeException($"Database initialization failed after {maxRetries} attempts.", ex);
            }

            await Task.Delay(3000); // wait before retry
        }
    }
}

// -------------------- Middleware --------------------
app.UseCors("AllowBlazorWasm");
app.UseAuthentication();
app.UseAuthorization();
if (sslEnabled)
{
    app.UseHttpsRedirection();
}

#if DEBUG
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Buddy2Go API v0.1");
        c.RoutePrefix = "";
    });
    Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
}
#endif
app.MapGet("/", () => "Buddy2Go API is running");

app.MapControllers();
app.Run();
