using AuthentificationService.BrokerExchange;
using AuthentificationService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationContext>(o=>o
    .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, Role>(opts=> {
        opts.Password.RequiredLength = 1;   // минимальная длина
        opts.Password.RequireNonAlphanumeric = false;   // требуются ли не алфавитно-цифровые символы
        opts.Password.RequireLowercase = false; // требуются ли символы в нижнем регистре
        opts.Password.RequireUppercase = false; // требуются ли символы в верхнем регистре
        opts.Password.RequireDigit = false; // требуются ли цифры
    })
    .AddEntityFrameworkStores<ApplicationContext>();

builder.Services.AddScoped<IAuthenticateTaskTrackerConsumer, AuthenticateTaskTrackerConsumer>();
builder.Services.AddScoped<IAuthenticateAccountingConsumer, AuthenticateAccountingConsumer>();

var app = builder.Build();

var scope = app.Services.CreateScope();

scope.ServiceProvider.GetService<IAuthenticateTaskTrackerConsumer>();
scope.ServiceProvider.GetService<IAuthenticateAccountingConsumer>();

var roleManager = scope.ServiceProvider.GetService<RoleManager<Role>>();
var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
AdminRoleStartInitializer.AdminStartInitialize(userManager!, roleManager!);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();