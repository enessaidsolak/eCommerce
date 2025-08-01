using eCommerce.DATA.Context;
using eCommerce.DATA.Entity;
using eCommerce.DATA.Models;
using eCommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;

public class ProductController : Controller
{
    private readonly eCommerceDBContext db;

    public ProductController(eCommerceDBContext context)
    {
        db = context;
    }

    // ✅ Ürün Detayları (Aynı kategoriye ait diğer ürünleri öne çıkarır, kendisi hariç)
    public IActionResult Details(string idOrName)
    {
        Product product = null;

        // Önce id olarak dene
        if (int.TryParse(idOrName, out int id))
        {
            product = db.Products.FirstOrDefault(p => p.ProductId == id);
        }

        // Eğer id ile bulunamadıysa isim olarak dene
        if (product == null)
        {
            product = db.Products.FirstOrDefault(p => p.ProductName == idOrName);
        }

        if (product == null)
            return NotFound();

        var model = new ProductDetailsViewModel
        {
            Product = product,
            FeaturedProducts = db.Products
                .Where(x => x.CategoryId == product.CategoryId && x.ProductId != product.ProductId)
                .ToList(),
            FeaturedCategory = db.Categories.ToList()
        };

        return View(model);
    }

    public IActionResult List()
    {
        var products = db.Products.ToList();
        return View(products);
    }

}
