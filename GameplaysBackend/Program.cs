using GameplaysBackend.Data;
using GameplaysBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Retrieve the cert pwd from app settings
var pfxPassword = builder.Configuration["CertSettings:PfxPassword"];

// Configure Kestrel to Use HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps("Certificates/certificate.pfx", pfxPassword);
    });
});

// Define CORS policies
var devCorsPolicy = "DevCorsPolicy";
var prodCorsPolicy = "ProdCorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(devCorsPolicy, builder =>
    {
        builder.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
    });

    options.AddPolicy(prodCorsPolicy, builder => {
       builder.WithOrigins("http://localhost")
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add DbContext with MySQL configuration
if (connectionString != null)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 39)))
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

// Retrieve the HmacSecretKey from app settings for JWT signing key comparison below
var hmacSecretKey = builder.Configuration["JwtSettings:HmacSecretKey"];

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
            ValidIssuer = "https://localhost:5001",
            ValidAudience = "http://localhost:3000",
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

if (app.Environment.IsDevelopment())
{
    app.UseCors(devCorsPolicy);
}
else
{
    app.UseCors(prodCorsPolicy);
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
