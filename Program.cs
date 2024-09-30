using AccountingTer.Models;
using AccountingTer.Services;
using AccountingTer.TelegramExtentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql;



var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationContext>(option => option.UseSqlite(builder.Configuration.GetConnectionString("SQLite")));


//builder.Services.AddDbContext<ApplicationContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("MS_SQL")));
//builder.Services.AddDbContext<ApplicationContext>(option => option.UseMySql(builder.Configuration.GetConnectionString("MySql"), new MySqlServerVersion(new Version(5, 1, 1)), mySqlOptionsAction: options => { options.EnableRetryOnFailure(); }));

builder.Services.AddScoped<TelegramController>();

builder.Services.AddSingleton<TelegramBotService>().AddOptions<TelegramOptions>().BindConfiguration(TelegramOptions.DefaultSectionName);
builder.Services.AddHostedService<TelegramBotService>(provider => provider.GetService<TelegramBotService>());

builder.Services.AddHostedService<DailyCommandsService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationContext>();
    context.Database.Migrate();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
