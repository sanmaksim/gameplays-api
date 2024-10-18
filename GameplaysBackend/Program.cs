using GameplaysBackend.Data;
using GameplaysBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Define CORS policies
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOriginWithCredentials",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });

    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();

// Retrieve the connection string from user secrets
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

builder.Services.AddTransient<JwtTokenService>();
builder.Services.AddTransient<CookieService>();
builder.Services.AddTransient<AuthService>();

// Retrieve the HmacSecretKey from appsettings.json
var hmacSecretKey = builder.Configuration["JwtSettings:HmacSecretKey"];

if (hmacSecretKey != null)
{
    // Configure authentication
    builder.Services.AddAuthentication(options => {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = "http://127.0.0.1:5000,http://localhost:5000",
            ValidAudience = "http://127.0.0.1:3000,http://localhost:3000",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(hmacSecretKey))
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
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Configure Kestrel to listen on port 5000
app.Urls.Add("http://localhost:5000");

app.Run();
