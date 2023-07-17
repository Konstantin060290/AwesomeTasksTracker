using AccountingService.BrokerExchange;
using AccountingService.Context;
using AccountingService.MoneyWorker;
using AccountingService.Settings;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationContext>(o=>o
    .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPriceRequestsConsumer, PriceRequestsRequestsConsumer>();
builder.Services.AddScoped<IUserConsumer, UserConsumer>();
builder.Services.AddScoped<ITransactionsConsumer, TransactionsConsumer>();
builder.Services.AddScoped<IMoneyWorker, MoneyWorker>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

var app = builder.Build();

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

app.UseAuthorization();

var scope = app.Services.CreateScope();

scope.ServiceProvider.GetService<IPriceRequestsConsumer>();
scope.ServiceProvider.GetService<IUserConsumer>();
scope.ServiceProvider.GetService<ITransactionsConsumer>();
scope.ServiceProvider.GetService<IMoneyWorker>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();