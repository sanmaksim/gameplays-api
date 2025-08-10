using GameplaysApi.Config;
using GameplaysApi.Controllers;
using GameplaysApi.Data;
using GameplaysApi.Interfaces;
using GameplaysApi.Repositories;
using GameplaysApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.RateLimiting;

// Load environment variables for Development
var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
bool isDevelopment = string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);
if (isDevelopment) DotNetEnv.Env.Load();

// Build the app configuration
var builder = WebApplication.CreateBuilder(args);

// Bind & validate the AuthConfig environment variables required by the startup config
var authConfig = new AuthConfig();
builder.Configuration.Bind(nameof(AuthConfig), authConfig);
if (authConfig.HmacSecretKey == null) throw new ArgumentNullException($"{nameof(authConfig.HmacSecretKey)} must not be null.");
if (authConfig.JwtCookieName == null) throw new ArgumentNullException($"{nameof(authConfig.JwtCookieName)} must not be null.");
if (authConfig.ValidAudiences == null) throw new ArgumentNullException($"{nameof(authConfig.ValidAudiences)} must not be null.");
if (authConfig.ValidIssuers == null) throw new ArgumentNullException($"{nameof(authConfig.ValidIssuers)} must not be null.");

// Bind & validate the ConnectionConfig environment variables required by the startup config
var connectionConfig = new ConnectionConfig();
builder.Configuration.Bind(nameof(ConnectionConfig), connectionConfig);
if (connectionConfig.ConnectionString == null) throw new ArgumentNullException($"{nameof(connectionConfig.ConnectionString)} must not be null.");
if (connectionConfig.CorsOriginDev == null) throw new ArgumentNullException($"{nameof(connectionConfig.CorsOriginDev)} must not be null.");
if (connectionConfig.CorsOriginTest == null) throw new ArgumentNullException($"{nameof(connectionConfig.CorsOriginTest)} must not be null.");
if (connectionConfig.DevCertPath == null) throw new ArgumentNullException($"{nameof(connectionConfig.DevCertPath)} must not be null.");
if (!int.TryParse(connectionConfig.HttpsPort, out int httpsPort)) throw new ArgumentNullException($"{nameof(connectionConfig.HttpsPort)} must not be null.");
if (connectionConfig.ProdCertPath == null) throw new ArgumentNullException($"{nameof(connectionConfig.ProdCertPath)} must not be null.");
if (connectionConfig.ProdKeyPath == null) throw new ArgumentNullException($"{nameof(connectionConfig.ProdKeyPath)} must not be null.");

// Configure Kestrel HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(httpsPort, listenOptions =>
    {
        if (isDevelopment)
        {
            listenOptions.UseHttps(connectionConfig.DevCertPath);
        }
        else
        {
            var certPath = connectionConfig.ProdCertPath;
            var keyPath = connectionConfig.ProdKeyPath;

            var certificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
            certificate = new X509Certificate2(certificate.Export(X509ContentType.Pfx));

            listenOptions.UseHttps(certificate);
        }
    });
});

// Add CORS policies
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOriginWithCredentials", builder =>
    {
        builder.WithOrigins
        (
            connectionConfig.CorsOriginDev, 
            connectionConfig.CorsOriginTest
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Add the database context for MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionConfig.ConnectionString, new MySqlServerVersion(new Version(8, 0, 41)))
);

// Rate limit proxied API requests to 1 per second and provide a response upon rejection
builder.Services.AddRateLimiter(options => {
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: "global",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1,
                QueueLimit = 5,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                Window = TimeSpan.FromSeconds(1)
            }
        ));
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
    };
});

// Configure authentication middleware
builder.Services.AddAuthentication(options => {
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => {
    // Do NOT map JWT claim types to .NET claim types
    // when validating the extracted access token
    options.MapInboundClaims = false;
    // Define token validation parameters
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = authConfig.ValidIssuers,
        ValidAudience = authConfig.ValidAudiences,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.HmacSecretKey)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    // Assign custom event handlers
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Pull access token from cookie if present
            if (context.HttpContext.Request.Cookies.ContainsKey(authConfig.JwtCookieName))
            {
                context.Token = context.HttpContext.Request.Cookies[authConfig.JwtCookieName];
            }
                
            return Task.CompletedTask;
        }
    };
});

// Bind the necessary environment variables for Dependency Injection & validate
builder.Services.Configure<AuthConfig>(builder.Configuration.GetSection(nameof(AuthConfig)));
builder.Services.AddOptions<AuthConfig>()
    .BindConfiguration(nameof(AuthConfig))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.Configure<GameConfig>(builder.Configuration.GetSection(nameof(GameConfig)));
builder.Services.AddOptions<GameConfig>()
    .BindConfiguration(nameof(GameConfig))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Add authorization policy services
builder.Services.AddAuthorization();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Gameplays API", Version = "v1" });
});
builder.Services.AddControllersWithViews();

// Required for communicating with the Giant Bomb API
builder.Services.AddHttpClient<GamesController>();

// Required by AuthService for accessing HttpContext.User outside the scope of a Controller
builder.Services.AddHttpContextAccessor();

// Add custom user auth related services
builder.Services.AddScoped<IAuthService, AuthService>();                 // relies on request-scoped object HttpResponse
builder.Services.AddScoped<ICookieService, CookieService>();             // relies on request-scoped object HttpResponse
builder.Services.AddTransient<IJwtTokenService, JwtTokenService>();      // does NOT rely on a request-scoped object
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>(); // relies on request-scoped object HttpRequest
builder.Services.AddSingleton<IUserService, UserService>();              // utility service that is stateless and thread-safe

// Add data access repositories for controllers and services
builder.Services.AddScoped<IGamesRepository, GamesRepository>();
builder.Services.AddScoped<IPlaysRepository, PlaysRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Add ApplicationDbContext helper services
builder.Services.AddScoped<EntityTrackingService>();    // relies on request-scoped object ApplicationDbContext
builder.Services.AddScoped<GameService>();              // relies on request-scoped object ApplicationDbContext

// Build the application
var app = builder.Build();

// Apply migrations and ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate(); // Applies any pending migrations
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowSpecificOriginWithCredentials");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("v1/swagger.json", "Gameplays API v1");
});
app.Run();
