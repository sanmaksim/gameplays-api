using GameplaysApi.Controllers;
using GameplaysApi.Data;
using GameplaysApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load(); // read from .env
}

// Define ports
var httpsPort = int.Parse(Environment.GetEnvironmentVariable("GAMEPLAYS_HTTPS_PORT")!);

// Configure Kestrel to Use HTTPS
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

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();

// Retrieve connection string from app settings
var connectionString = Environment.GetEnvironmentVariable("GAMEPLAYS_CONNECTION_STRING");

// Add DbContext with MySQL configuration
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

// Add custom JWT and cookie handling middleware
builder.Services.AddTransient<JwtTokenService>();
builder.Services.AddTransient<CookieService>();
builder.Services.AddTransient<AuthService>();

// Register EntityTrackingService
builder.Services.AddScoped<EntityTrackingService>();

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

// Add HttpClient Service since we will be communicating directly with the Giant Bomb API
builder.Services.AddHttpClient<GamesController>();

// Retrieve the HmacSecretKey from app settings for JWT signing key comparison below
var hmacSecretKey = Environment.GetEnvironmentVariable("GAMEPLAYS_HMACSECRETKEY");

// Define JWT settings
var validIssuer = Environment.GetEnvironmentVariable("GAMEPLAYS_VALIDISSUERS");
var validAudience = Environment.GetEnvironmentVariable("GAMEPLAYS_VALIDAUDIENCES");

if (hmacSecretKey != null)
{
    // Add authentication middleware
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
        };
        // Read token from cookie
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.HttpContext.Request.Cookies["jwt"];
                return Task.CompletedTask;
            }
        };
    });
}
else
{
    throw new Exception("HmacSecretKey must not be null.");
}

// Add authorization middleware
builder.Services.AddAuthorization();


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
app.Run();
