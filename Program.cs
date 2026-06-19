#nullable enable
using System.Reflection;
using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MediatR;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Warehouse.Data;
using Warehouse.Middleware;
using Warehouse.Helpers;
using Warehouse.Data.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// Validate essential configuration
if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")))
{
    throw new InvalidOperationException("DefaultConnection is not configured. Please set ConnectionStrings:DefaultConnection in appsettings.json.");
}

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// DbContext - SQL Server
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<Warehouse.Models.Identity.ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Identity cookie paths (Identity registers its own cookie scheme "Identity.Application")
// Do NOT add a separate .AddCookie() — it would conflict with Identity's built-in cookie handler.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Access/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
});

// JWT Bearer for API endpoints only
builder.Services.AddAuthentication()
.AddJwtBearer(options =>
{
    var jwtSection = configuration.GetSection("JwtSettings");
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSection["Secret"] ?? string.Empty))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

// In-memory cache (for ICacheService / MemoryCacheService)
builder.Services.AddMemoryCache();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MediatR
builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Warehouse API", Version = "v1" });

    // Only include controllers marked with [ApiController] attribute
    // This prevents MVC-only Razor controllers from being scanned as API endpoints
    c.DocInclusionPredicate((docName, apiDescription) =>
    {
        // Only include endpoints for controllers with [ApiController] attribute
        var controllerActionDescriptor = apiDescription.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        if (controllerActionDescriptor == null)
            return false;

        var hasApiControllerAttribute = controllerActionDescriptor.ControllerTypeInfo
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.ApiControllerAttribute), true)
            .Any();

        return hasApiControllerAttribute;
    });

    // JWT Bearer auth in swagger
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter JWT Bearer token **_only_**",
        Reference = new OpenApiReference
        {
            Id = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

// Serilog - basic configuration (console + rolling file)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/warehouse-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// App services & DI
builder.Services.AddHttpContextAccessor();

// Register application services and repositories via helper
ServiceRegistration.RegisterApplicationServices(builder.Services, configuration);

var app = builder.Build();

// Ensure database is migrated and seed Identity roles/admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        // Apply pending migrations (safe for dev). Use Migrate instead of EnsureCreated so migrations are consistent.
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger("Program");
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

// Seed roles and admin user — pass root IServiceProvider so the seeder creates its own scope
try
{
    RoleAndUserSeeder.SeedAsync(app.Services).GetAwaiter().GetResult();
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while seeding Identity roles/admin.");
    throw;
}

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Authorization middleware for handling access denied/unauthorized
app.UseStatusCodePages(async ctx =>
{
    var response = ctx.HttpContext.Response;
    if (response.StatusCode == 401)
    {
        response.Redirect("/Access/UnauthorizedPage");
    }
    else if (response.StatusCode == 403)
    {
        response.Redirect("/Access/AccessDenied");
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse API V1");
});

// Area routes (if areas used)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();