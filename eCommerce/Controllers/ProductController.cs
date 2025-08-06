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
            FeaturedCategory = db.Categories
            .Include(c => c.Products)
            .ToList()
            

        };


        model.ProductComments = db.ProductComments.Where(x => x.ProductId == product.ProductId && x.IsActive == true).Include(x => x.User).ToList();
        

        return View(model);

        
        
    }

    public IActionResult List()
    {
        var products = db.Products.ToList();    
        return View(products);
    }
    [HttpPost]
    public IActionResult AddComment(int productId, string comment, string firstName, string email, int rating)
    {
        try
        {
            // Kullanıcıyı isim ve email ile bul
            var user = db.Users.FirstOrDefault(u => u.FirstName == firstName && u.Email == email);

            // Kullanıcı yoksa yeni oluştur
            if (user == null)
            {
                user = new User
                {
                    FirstName = firstName,
                    Email = email,
                    CreatedAt = DateTime.Now
                    // Diğer gerekli alanlar varsa ekle
                };
                db.Users.Add(user);
                db.SaveChanges(); // userId burada oluşacak
            }

            // Yorum oluştur
            var newComment = new ProductComment
            {
                ProductId = productId,
                UserId = user.UserId,  // doğru UserId atandı
                Comment = comment,
                Rating = rating,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            db.ProductComments.Add(newComment);
            db.SaveChanges();

            return RedirectToAction("Details", new { idOrName = productId.ToString() });
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = ex.Message;
            return View("Error");
        }
    }




}
