using Microsoft.EntityFrameworkCore;
using eCommerce.DATA.Context;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<eCommerceDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
       .AddCookie(options =>
       {
           options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
           options.Cookie.Name = "LoginCookie";
           options.LoginPath = "/Account/Login";
           options.AccessDeniedPath = "/Account/Login";
       });

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ** ÞÝFRE DÜZELTME ÝÞLEMÝ (Hashlenmemiþ þifreleri bcrypt ile hashle) **
// sadece Development ortamýnda çalýþtýrýyoruz, üretimde kaldýrabilirsin
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<eCommerceDBContext>();

    var usersToUpdate = context.Users
        .Where(u => u.Password != null && !u.Password.StartsWith("$2"))
        .ToList();

    foreach (var user in usersToUpdate)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
    }

    context.SaveChanges();
}

// ROUTE TANIMLAMALARI
app.MapControllerRoute(
   name: "ÜrünEkleme",
   pattern: "yeni-urun",
   defaults: new { controller = "Product", action = "AddProducts" });

app.MapControllerRoute(
   name: "Ürünlerim",
   pattern: "urunlerim",
   defaults: new { controller = "Product", action = "MyProducts" });

app.MapControllerRoute(
   name: "Kategorim",
   pattern: "kategorim",
   defaults: new { controller = "Category", action = "MyCategory" });

app.MapControllerRoute(
   name: "KategoriEkleme",
   pattern: "kategori-ekle",
   defaults: new { controller = "Category", action = "AddCategory" });

app.MapControllerRoute(
    name: "ProductDetails",
    pattern: "Product/Details/{idOrName}",
    defaults: new { controller = "Product", action = "Details" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
