using Microsoft.AspNetCore.Mvc;
using eCommerce.DATA.Context;
using eCommerce.DATA.Entity;
using eCommerce.DATA.Models;  // ProductModel için
using System.Linq;
using eCommerce.Models;
using Microsoft.EntityFrameworkCore;

public class ShopController : Controller
{
    private readonly eCommerceDBContext db;

    public ShopController(eCommerceDBContext context)
    {
        db = context;
    }

    public IActionResult Detail(int id)
    {
        var product = db.Products.FirstOrDefault(p => p.ProductId == id);
        if (product == null)
            return NotFound();

        return View(product);
    }

    public IActionResult Index(int? categoryID, decimal? price,string? keyword)
    {
        var model = new ProductModel();
        model.Categories = db.Categories.ToList();

        var query = db.Products.Include(p => p.Category).AsQueryable();

        if (categoryID.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryID.Value);
            model.SelectedCategoryId = categoryID;
        }

        if (price.HasValue)
        {
            query = query.Where(p => p.Price <= price.Value);
            model.SelectedPrice = price;
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(p =>
                p.ProductName.Contains(keyword) ||
                p.Description.Contains(keyword)); // eğer açıklama varsa
            model.SelectedKeyword = keyword;
        }

        model.Products = query.ToList();
        return View(model);
    }

}
