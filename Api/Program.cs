using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Api.Services;
using Core.Interfaces;
using Infrastructure.Repositories;
using AutoMapper;
using System.Text.Json.Serialization;
using Core.POCO;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//For Indentity
builder.Services.AddAuthorization();
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    }).AddEntityFrameworkStores<StoreContext>().AddDefaultTokenProviders();

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
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
;






var app = builder.Build();

//Middlewares
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication(); 
app.UseAuthorization();
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == 404)
    {
        response.ContentType = "application/json";
        await response.WriteAsJsonAsync(new AppError(404, "Oups, pas par là !"));
    }
});

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
};*/

app.MapControllers();



app.Run();
