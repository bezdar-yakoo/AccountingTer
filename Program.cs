using AccountingTer.Models;
using AccountingTer.Services;
using AccountingTer.TelegramExtentions;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);


if (args.Contains("MS_SQL") == false)
    builder.Services.AddDbContext<ApplicationContext>(option => option.UseSqlite(builder.Configuration.GetConnectionString("SQLite")));
else
    builder.Services.AddDbContext<ApplicationContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("MS_SQL")));

builder.Services.AddHostedService<TelegramBotService>().AddOptions<TelegramOptions>()
    .BindConfiguration(TelegramOptions.DefaultSectionName);

builder.Services.AddScoped<TelegramController>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
