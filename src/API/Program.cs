using EmployeeService.Application.Common.Abstractions.Repositories;
using EmployeeService.Application.MappingProfiles;
using EmployeeService.GrpcServices;
using EmployeeService.Infrastructure.Data;
using EmployeeService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add DbContext
builder.Services.AddDbContext<EmployeeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add gRPC
builder.Services.AddGrpc();

// Add MediatR - scan Application assembly for handlers
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EmployeeMappingProfile).Assembly));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(EmployeeMappingProfile));

// Add Repository pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

// Add Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql");

var app = builder.Build();

// Apply migrations / Create database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Map gRPC service
app.MapGrpcService<EmployeeGrpcServiceImpl>();

// Health check endpoint
app.MapHealthChecks("/health");

// Simple health check endpoint
app.MapGet("/", () => "Employee Service is running");

app.Run();
