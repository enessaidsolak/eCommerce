using Microsoft.EntityFrameworkCore;
using eCommerce.DATA.Context; // DbContext'in namespace'i

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Buraya kendi connection string'ini yaz
builder.Services.AddDbContext<eCommerceDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

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
   pattern: "/kategorim",
   defaults: new { controller = "Category", action = "MyCategory" });

app.MapControllerRoute(
   name: "KategoriEkleme",
   pattern: "kategori-ekle",
   defaults: new { controller = "Category", action = "AddCategory" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

app.MapControllerRoute(
    name: "ProductDetails",
    pattern: "Product/Details/{idOrName}",
    defaults: new { controller = "Product", action = "Details" });

