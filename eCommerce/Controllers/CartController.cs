using eCommerce.DATA.Context;
using eCommerce.DATA.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
                existingItem.Quantity = existingItem.Quantity + 1;
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

            return Json(new { success = true });
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

            if (string.IsNullOrWhiteSpace(cartJson))
                return new List<CartItemModel>();

            try
            {
                return JsonSerializer.Deserialize<List<CartItemModel>>(cartJson) ?? new List<CartItemModel>();
            }
            catch
            {
                // Cookie bozuk veya format hatalıysa boş liste dön
                return new List<CartItemModel>();
            }
        }

        private void SaveCartToCookie(List<CartItemModel> cartItems)
        {
            var options = new CookieOptions { Expires = DateTimeOffset.Now.AddDays(7) };
            Response.Cookies.Append("cart", JsonSerializer.Serialize(cartItems), options);
        }
        [HttpPost]
        [HttpPost]
        public JsonResult ApplyCouponAjax(string couponCode)
        {
            var cartItems = GetCartItemsFromCookie();
            decimal subtotal = cartItems.Sum(x => x.Price * x.Quantity);
            decimal shipping = subtotal >= 300 ? 0m : 3m; // Kargo durumu
            decimal discountAmount = 0;
            string message = "";
            bool success = false;

            int userId = Convert.ToInt32(User.FindFirstValue("UserId"));

            var coupon = _context.Coupons.FirstOrDefault(c => c.Code == couponCode && c.IsActive);
            if (coupon == null)
            {
                message = "Geçersiz kupon kodu.";
            }
            else if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < DateTime.Now)
            {
                message = "Kupon süresi dolmuş.";
            }
            else if (_context.CouponUsages.Any(x => x.CouponId == coupon.Id && x.UserId == userId))
            {
                message = "Bu kuponu daha önce kullandınız.";
            }
            else
            {
                discountAmount = subtotal * (coupon.DiscountRate / 100m);

                // Kullanım kaydı ekle
                _context.CouponUsages.Add(new CouponUsage
                {
                    CouponId = coupon.Id,
                    UserId = userId,
                    UsedAt = DateTime.Now
                });

                _context.SaveChanges();

                message = $"Kupon uygulandı. %{coupon.DiscountRate} indirim kazandınız!";
                success = true;
            }

            return Json(new
            {
                success = success,
                message = message,
                discountAmountFormatted = discountAmount.ToString("C", new System.Globalization.CultureInfo("tr-TR")),
                subtotalFormatted = subtotal.ToString("C", new System.Globalization.CultureInfo("tr-TR")),
                totalAfterDiscountFormatted = (subtotal - discountAmount + shipping).ToString("C", new System.Globalization.CultureInfo("tr-TR"))
            });
        }









    }
}
