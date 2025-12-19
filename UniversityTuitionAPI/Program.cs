using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Diagnostics;
using UniversityTuitionAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Services
// --------------------

// Database
builder.Services.AddDbContext<UniversityDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Controllers
builder.Services.AddControllers();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            ),
            ValidateLifetime = true,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "University Tuition API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter token only"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// --------------------
// Ensure DB created
// --------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UniversityDbContext>();
    db.Database.EnsureCreated();
}

// --------------------
// Middleware
// --------------------
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Map API controllers
app.MapControllers();

// Serve React build (production)
app.UseDefaultFiles(); // looks for index.html
app.UseStaticFiles();  // serves wwwroot

// Fallback for SPA routing
app.MapFallbackToFile("index.html");

// --------------------
// Start Node.js AI server
// --------------------
var nodeServerPath = Path.Combine(AppContext.BaseDirectory, "server.js"); // must be copied to output folder

if (!File.Exists(nodeServerPath))
{
    Console.WriteLine("server.js not found at: " + nodeServerPath);
}
else
{
    try
    {
        var nodeProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = $"\"{nodeServerPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        nodeProcess.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine("[Node] " + e.Data);
        };
        nodeProcess.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine("[Node ERROR] " + e.Data);
        };

        nodeProcess.Start();
        nodeProcess.BeginOutputReadLine();
        nodeProcess.BeginErrorReadLine();

        // Kill Node process on API exit
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            if (!nodeProcess.HasExited)
                nodeProcess.Kill();
        };

        Console.WriteLine("Node.js server started successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed to start Node.js server: " + ex.Message);
    }
}

// --------------------
// Run API
// --------------------
app.Run();
