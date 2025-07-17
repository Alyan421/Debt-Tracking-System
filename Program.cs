using Debt_Tracking_System.EntityFrameworkCore;
using Debt_Tracking_System.Managers.Transactions;
using Debt_Tracking_System.Managers.Customers;
using Debt_Tracking_System.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Automapper configuration
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Register managers
builder.Services.AddScoped<ITransactionManager, TransactionManager>();
builder.Services.AddScoped<ICustomerManager, CustomerManager>(); // Optional if used

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);


// Read allowed origins from environment variable
var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS")?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

// Enable CORS with environment-based policy
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("EnvCors", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    }
    else
    {
        options.AddPolicy("EnvCors", policy =>
        {
            if (allowedOrigins != null && allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
            else
            {
                policy.DisallowCredentials();
            }
        });
    }
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Debt Tracking API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at app root
    });
}

app.UseHttpsRedirection();

app.UseCors("EnvCors");

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();