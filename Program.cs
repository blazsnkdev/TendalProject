using Microsoft.AspNetCore.Authentication.Cookies; // Agrega este using
using Microsoft.EntityFrameworkCore;
using TendalProject.Models;
using TendalProject.Service;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BdTendalDefinitivoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("cn1")));

builder.Services.AddScoped<UsuarioService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuario/Login"; 
        options.AccessDeniedPath = "/Usuario/Login";
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(59);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; 
});

builder.Services.AddDistributedMemoryCache();
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();
app.UseRouting();

app.MapControllerRoute(
    name: "default1",
    pattern: "{controller=Usuario}/{action=Login}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
