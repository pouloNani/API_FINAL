using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Api.Services;
using Core.Interfaces;
using Infrastructure.Repositories;
using System.Text.Json.Serialization;
using Core.POCO;
using StackExchange.Redis;
using Infrastructure.Services;
using Infrastructure.AI;
using Infrastructure.AI.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration["Redis"]!));

// Cart
builder.Services.AddScoped<ICartService, CartService>();

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = true;
    options.Password.RequireLowercase       = true;
    options.User.RequireUniqueEmail         = true;
})
.AddEntityFrameworkStores<StoreContext>()
.AddDefaultTokenProviders();

// JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();

// Cookie 401/403
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return context.Response.WriteAsJsonAsync(new AppError(401, "Vous devez être connecté."));
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return context.Response.WriteAsJsonAsync(new AppError(403, "Accès refusé."));
    };
});

builder.Services.AddSingleton<IEmailSender<AppUser>, NoOpEmailSender>();

// Repositories
builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));

builder.Services.AddScoped<ITokenService, TokenService>();

// LLM Providers
builder.Services.AddHttpClient<OllamaClient>();
builder.Services.AddHttpClient<MistralClient>();
builder.Services.AddHttpClient<GeminiClient>();
builder.Services.AddHttpClient<ClaudeAIClient>();
builder.Services.AddHttpClient<ScoutClient>();

// Agent
builder.Services.AddScoped<LLMClientFactory>();
builder.Services.AddScoped<ToolExecutor>();
builder.Services.AddScoped<AgentLoop>();
builder.Services.AddScoped<ConversationHistory>();

builder.Services.AddHttpClient<GeocodingService>();

builder.Services.AddScoped<ILLMClient>(sp =>
    sp.GetRequiredService<LLMClientFactory>().Create());

var app = builder.Build();

// Middlewares
app.UseMiddleware<ExceptionMiddleware>();
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == 404)
    {
        response.ContentType = "application/json";
        await response.WriteAsJsonAsync(new AppError(404, "Oups, pas par là !"));
    }
});

app.UseAuthentication();
app.UseAuthorization();

// Role Manager
var scope = app.Services.CreateScope();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

/*string[] roles = new string[] { "Admin", "Client", "Owner" };
foreach (var role in roles)
{
    if (!await roleManager.RoleExistsAsync(role))
    {
        await roleManager.CreateAsync(new IdentityRole(role));
        Console.WriteLine($"Role {role} created");
    }
}*/

app.MapControllers();

app.Run();