using GameplaysApi.Controllers;
using GameplaysApi.Data;
using GameplaysApi.Interfaces;
using GameplaysApi.Repositories;
using GameplaysApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Read from the .env file in dev
if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();
}

// Define ports
var httpsPort = int.Parse(Environment.GetEnvironmentVariable("GAMEPLAYS_HTTPS_PORT")!);

// Define JWT config
var hmacSecretKey = Environment.GetEnvironmentVariable("JWT_HMACSECRETKEY");
var validIssuer = Environment.GetEnvironmentVariable("GAMEPLAYS_VALIDISSUERS");
var validAudience = Environment.GetEnvironmentVariable("GAMEPLAYS_VALIDAUDIENCES");

// Define database connection string
var connectionString = Environment.GetEnvironmentVariable("GAMEPLAYS_CONNECTION_STRING");

// Add Kestrel HTTPS config
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(httpsPort, listenOptions =>
    {
        if (builder.Environment.IsDevelopment())
        {
            listenOptions.UseHttps(Environment.GetEnvironmentVariable("GAMEPLAYS_CERTIFICATE_PATH")!);
        }
        else
        {
            var certPath = "/etc/ssl/certs/gameplays.test+1.pem";
            var keyPath = "/etc/ssl/certs/gameplays.test+1-key.pem";

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
        builder.WithOrigins($"http://localhost:5000", "https://gameplays.test")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
    });
});

// Add the database context for MySQL
if (connectionString != null)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 41)))
    );
}
else
{
    throw new Exception("DefaultConnection string must not be null.");
}

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

// Add authentication middleware
if (hmacSecretKey != null)
{
    builder.Services.AddAuthentication(options => {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => {
        // Do NOT map claim types that are extracted when validating a JWT
        options.MapInboundClaims = false;
        // Define JWT validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = validIssuer,
            ValidAudience = validAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(hmacSecretKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
        // Assign custom event handlers
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Pull token from cookie if present
                if (context.HttpContext.Request.Cookies.ContainsKey("jwt"))
                {
                    context.Token = context.HttpContext.Request.Cookies["jwt"];
                }
                
                return Task.CompletedTask;
            }
        };
    });
}
else
{
    throw new Exception("HmacSecretKey must not be null.");
}

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
builder.Services.AddTransient<IJwtTokenService, JwtTokenService>(); // does NOT rely on a request-scoped object
builder.Services.AddScoped<ICookieService, CookieService>();        // relies on request-scoped object HttpResponse
builder.Services.AddScoped<IAuthService, AuthService>();            // relies on request-scoped object HttpResponse
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>(); // relies on request-scoped object HttpRequest

// Add data access repositories for controllers and services
builder.Services.AddScoped<IGamesRepository, GamesRepository>();
builder.Services.AddScoped<IPlaysRepository, PlaysRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Add ApplicationDbContext helper services
builder.Services.AddScoped<EntityTrackingService>();    // relies on request-scoped object ApplicationDbContext
builder.Services.AddScoped<GameHelperService>();        // relies on request-scoped object ApplicationDbContext

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
