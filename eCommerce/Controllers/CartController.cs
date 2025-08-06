using eCommerce.DATA.Context;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace eCommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly eCommerceDBContext _context;

        public CartController(eCommerceDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cartItems = GetCartItemsFromCookie();
            return View(cartItems);
        }

        [HttpGet]
        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.FirstOrDefault(x => x.ProductId == id);
            if (product == null) return NotFound();

            var cartItems = GetCartItemsFromCookie();
            var existingItem = cartItems.FirstOrDefault(x => x.ProductId == id);

            if (existingItem != null)
            {
                existingItem.Quantity = 1; // **Burada 1 yapıyoruz, eski quantity kalmasın**
            }
            else
            {
                cartItems.Add(new CartItemModel
                {
                    ProductId = product.ProductId,
                    Name = product.ProductName,
                    Price = product.Price ?? 0,
                    Quantity = 1,
                    ImageUrl = product.ImageUrl
                });
            }

            SaveCartToCookie(cartItems);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cartItems = GetCartItemsFromCookie();
            var item = cartItems.FirstOrDefault(x => x.ProductId == id);
            if (item != null)
            {
                cartItems.Remove(item);
                SaveCartToCookie(cartItems);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cartItems = GetCartItemsFromCookie();
            var item = cartItems.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    cartItems.Remove(item);
                }

                SaveCartToCookie(cartItems);
                return Json(new { success = true, quantity = item.Quantity });
            }

            return Json(new { success = false });
        }

        private List<CartItemModel> GetCartItemsFromCookie()
        {
            var cartJson = Request.Cookies["cart"];
            return string.IsNullOrEmpty(cartJson)
                ? new List<CartItemModel>()
                : JsonSerializer.Deserialize<List<CartItemModel>>(cartJson) ?? new List<CartItemModel>();
        }

        private void SaveCartToCookie(List<CartItemModel> cartItems)
        {
            var options = new CookieOptions { Expires = DateTimeOffset.Now.AddDays(7) };
            Response.Cookies.Append("cart", JsonSerializer.Serialize(cartItems), options);
        }
    }
}
