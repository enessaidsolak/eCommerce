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
        public IActionResult ApplyCoupon(string couponCode)
        {
            var cartItems = GetCartItemsFromCookie(); // Sepetteki ürünleri al
            decimal subtotal = cartItems.Sum(x => x.Price * x.Quantity);
            decimal shipping = 3m;

            var coupon = _context.Coupons.FirstOrDefault(c => c.Code == couponCode && c.IsActive);

            decimal discountAmount = 0;
            string couponMessage = null;

            // Kullanıcının kimliği (login sistemin varsa bu şekilde alırsın)
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Login yoksa geçici bir kullanıcı ID'si de verebilirsin:
            // string userId = "test-user";

            if (coupon == null)
            {
                couponMessage = "Geçersiz kupon kodu.";
            }
            else if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < DateTime.Now)
            {
                couponMessage = "Kuponun süresi dolmuş.";
            }
            else
            {
                var alreadyUsed = _context.CouponUsages
                    .Any(x => x.CouponId == coupon.Id && x.UserId == userId);

                if (alreadyUsed)
                {
                    couponMessage = "Bu kuponu daha önce kullandınız.";
                }
                else if (coupon.UsageLimit > 0 && coupon.UsageCount >= coupon.UsageLimit)
                {
                    couponMessage = "Bu kuponun kullanım limiti doldu.";
                }
                else
                {
                    discountAmount = subtotal * coupon.DiscountRate;

                    // Kullanımı kaydet
                    coupon.UsageCount++;
                    _context.CouponUsages.Add(new CouponUsage
                    {
                        CouponId = coupon.Id,
                        UserId = userId,
                        UsedAt = DateTime.Now
                    });
                    _context.SaveChanges();
                }
            }

            ViewBag.DiscountAmount = discountAmount;
            ViewBag.CouponMessage = couponMessage;
            ViewBag.TotalAfterDiscount = subtotal - discountAmount + shipping;

            return View("Index", cartItems); // Sepet sayfasına geri dön
        }

        [HttpGet]
        public IActionResult GetBasketSize()
        {
            var cartItems = GetCartItemsFromCookie();
            var totalCount = cartItems.Sum(x => x.Quantity);
            return Json(totalCount);
        }




    }
}
