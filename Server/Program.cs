using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Server.Features.Buddies;
using Server.Features.Chats;
using Server.Features.DangerousPlaces;
using Server.Features.Journeys;
using Server.Features.Users;
using Server.Infrastructure.Data;
using Server.Infrastructure.Database;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// -------------------- Authentication --------------------

string jwtSecret = builder.Configuration.GetRequiredSection("JwtSettings:Secret").Get<string>()!;
bool sslEnabled = builder.Configuration.GetRequiredSection("SSL:Enabled").Get<bool>();

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
            string? token = null;
            if (context.Request.Cookies.TryGetValue("jwt", out string? jwtCookie))
            {
                token = jwtCookie;
            }

            // Assign token to the context so middleware can validate it
            context.Token = token;
#if DEBUG
            if (!string.IsNullOrEmpty(token))
            {
                log.LogDebug("JWT cookie received: {Snippet}...", token[..Math.Min(token.Length, 20)]);
            }
#endif
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
#if DEBUG
            log.LogWarning(context.Exception, "JWT Authentication failed");
#endif
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
#if DEBUG
            if (context.Principal != null)
            {
                string idClaim = context.Principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                string nameClaim = context.Principal.Identity?.Name ?? "unknown";

                string? expClaim = context.Principal.FindFirst("exp")?.Value;
                string expText = "unknown";
                if (expClaim != null && long.TryParse(expClaim, out Int64 expSeconds))
                {
                    DateTime expDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
                    expText = expDate.ToString("yyyy-MM-dd HH:mm:ss UTC");
                }

                log.LogDebug("JWT Validated: UserId={UserId}, Username={Username}, ExpiresAt={Expiration}",
                    idClaim, nameClaim, expText);
            }
#endif
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

// -------------------- Swagger --------------------
#if DEBUG
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Buddy2Go", Version = "v0.1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });
#endif

//------------- Configure https for docker -------------------------------
// Configure Kestrel for HTTPS inside Docker
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    if (sslEnabled)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(5001, listenOptions =>
            {
                listenOptions.UseHttps("/https/aspnetapp.pfx", "MyPassword123");
            });
        });
    }
    else
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(5001);
        });
    }
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
DbConnectHelper.AddDatabase(builder.Services, builder.Configuration.GetSection("DbSettings"));

// -------------------- Build App --------------------
WebApplication? app = builder.Build();

// -------------------- Seed Database -----------------

for (int i = 0; i < 5; i++)
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
        logger.LogWarning(ex, "Database init failed, retrying {i}/5...", i + 1);
        Thread.Sleep(3000);
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

app.MapControllers();
app.Run();
