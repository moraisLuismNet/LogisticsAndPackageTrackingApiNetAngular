using LogisticPackageTrackingApiNet.Api.Helpers;
using LogisticPackageTrackingApiNet.Api.Middleware;
using LogisticPackageTrackingApiNet.Application.Features.Shipments.Handlers;
using LogisticPackageTrackingApiNet.Application.Features.Tracking.Handlers;
using LogisticPackageTrackingApiNet.Application.Interfaces;
using LogisticPackageTrackingApiNet.Application.Messaging;
using LogisticPackageTrackingApiNet.Domain.Entities;
using LogisticPackageTrackingApiNet.Domain.Interfaces;
using LogisticPackageTrackingApiNet.Infrastructure.Messaging;
using LogisticPackageTrackingApiNet.Infrastructure.Persistence;
using LogisticPackageTrackingApiNet.Infrastructure.Repositories;
using LogisticPackageTrackingApiNet.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Dynamic Database Selection
var dbProvider = builder.Configuration["DatabaseConfig:Provider"];
if (dbProvider == "MongoDb")
{
    builder.Services.AddSingleton<MongoDbContext>();
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        switch (dbProvider)
        {
            case "SqlServer":
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), 
                    sqlOptions => sqlOptions.EnableRetryOnFailure());
                break;
            case "Sqlite":
                options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
                break;
            case "PostgreSql":
                options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql"));
                break;
            case "MySql":
                options.UseMySql(builder.Configuration.GetConnectionString("MySql"), 
                    new MySqlServerVersion(new Version(8, 0, 21)));
                break;
            default:
                options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
                break;
        }
    });
}

// 2. Authentication & JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// 4. Dependency Injection
if (dbProvider == "MongoDb")
{
    builder.Services.AddScoped<IUnitOfWork, LogisticPackageTrackingApiNet.Infrastructure.Repositories.Mongo.MongoUnitOfWork>();
}
else
{
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
}

builder.Services.AddScoped<IShipmentHandler, ShipmentHandler>();
builder.Services.AddScoped<ITrackingHandler, TrackingHandler>();
builder.Services.AddHttpClient<IGeocodingService, GeocodingService>();
builder.Services.AddHttpClient<IEmailSender, BrevoEmailSender>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMQProducer>();
builder.Services.Configure<HostOptions>(options => options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);
builder.Services.AddHostedService<RabbitMQConsumer>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LogisticPackageTrackingApiNet", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder => builder
            .WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Logistic Tracking API v1"));
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        var url = "http://localhost:5096/swagger";
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
    }
    catch { }
});

_ = Task.Run(async () =>
{
    try
    {
        var psi = new System.Diagnostics.ProcessStartInfo("docker")
        {
            ArgumentList = { "ps", "--filter", "name=rabbitmq", "--format", "{{.Names}}" },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var proc = System.Diagnostics.Process.Start(psi);
        if (proc == null) return;
        var output = await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (!output.Trim().Equals("rabbitmq", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Starting RabbitMQ via Docker...");
            var runPsi = new System.Diagnostics.ProcessStartInfo("docker")
            {
                ArgumentList = { "run", "-d", "--name", "rabbitmq", "-p", "5672:5672", "-p", "15672:15672", "rabbitmq:4-management" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var runProc = System.Diagnostics.Process.Start(runPsi);
            if (runProc != null)
            {
                var runOutput = await runProc.StandardOutput.ReadToEndAsync();
                await runProc.WaitForExitAsync();
                if (runProc.ExitCode == 0)
                    Console.WriteLine($"RabbitMQ started: {runOutput.Trim()}");
                else
                    Console.WriteLine("RabbitMQ not available — emails will be sent directly via Brevo");
            }
        }
    }
    catch
    {
        Console.WriteLine("Docker not available — emails will be sent directly via Brevo");
    }
});

// Apply migrations / create database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}

// Seed data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var existing = uow.Users.GetByMailAsync("luis@mail.com").GetAwaiter().GetResult();

        if (existing == null)
        {
            var admin = new User
            {
                Mail = "luis@mail.com",
                FirstName = "Luis",
                LastName = "M",
                Address = "Gran Vía, 20 Madrid",
                Password = "123456",
                PasswordHash = AuthHelpers.HashPassword("123456"),
                Role = "Admin"
            };
            uow.Users.AddAsync(admin).GetAwaiter().GetResult();
            uow.SaveChangesAsync().GetAwaiter().GetResult();
            Console.WriteLine("Admin user created successfully");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seed error: {ex.Message}");
    }
}

app.Run();
