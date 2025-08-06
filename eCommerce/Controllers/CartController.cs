using eCommerce.DATA.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace eCommerce.Controllers
{
    
    public class CartController : Controller
    {
        private readonly eCommerceDBContext dbContext;
        private eCommerceDBContext _context;

        public CartController(eCommerceDBContext context)
        {
            _context = context;
        }

        public ActionResult Index() 
        {

            var cartJson = Request.Cookies["cart"];
            List<CartItemModel> cartItems;

            if (string.IsNullOrEmpty(cartJson))
            {
                cartItems = new List<CartItemModel>(); // Sepet boşsa boş liste
            }
            else
            {
                cartItems = JsonSerializer.Deserialize<List<CartItemModel>>(cartJson) ?? new List<CartItemModel>();
            }

            // View'e gönder
            return View(cartItems);
        }


        public IActionResult AddToCart(int id)
        {
            // Ürünü veritabanından çek
            var product = _context.Products.FirstOrDefault(x => x.ProductId == id);
            if (product == null)
                return NotFound();

            // Cookie'den mevcut sepeti oku
            var cartJson = Request.Cookies["cart"];
            List<CartItemModel> cartItems = string.IsNullOrEmpty(cartJson)
                ? new List<CartItemModel>()
                : JsonSerializer.Deserialize<List<CartItemModel>>(cartJson);

            // Aynı ürün varsa adedini artır
            var existingItem = cartItems.FirstOrDefault(x => x.ProductId == id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cartItems.Add(new CartItemModel
                {
                    ProductId = product.ProductId,
                    Name = product.ProductName,
                    Price = product.Price ?? 0,
                    Cookie = product.ProductName,
                    Quantity = 1,
                    ImageUrl = product.ImageUrl
                });
              
            }

            // Cookie'ye geri yaz
            Response.Cookies.Append("cart", JsonSerializer.Serialize(cartItems), new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7)
            });

            return RedirectToAction("Index");
        }


    }

}
