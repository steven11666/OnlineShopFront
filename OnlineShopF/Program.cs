using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineShopF.Areas.Identity.Data;
using OnlineShopF.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession();

builder.Services.AddDbContext<OnlineShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OnlineShopFContext") ?? throw new InvalidOperationException("Connection string 'OnlineShopFContext' not found."))); 

builder.Services.AddDbContext<OnlineShopUserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OnlineShopFContext") ?? throw new InvalidOperationException("Connection string 'OnlineShopFContext' not found.")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<OnlineShopUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<OnlineShopUserContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();