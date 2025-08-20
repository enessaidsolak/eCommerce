using eCommerce.DATA.Context;
using eCommerce.DATA.Entity;
using eCommerce.WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Craftgate;
using Stripe;
using Stripe.Checkout;



[Authorize]
public class CheckoutController : Controller
{
    private readonly eCommerceDBContext _context;

    public CheckoutController(eCommerceDBContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        int currentUserId = GetCurrentUserId();

        var user = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId);

        var model = new CheckoutViewModel();

        // Adresleri ekle (sadece aktif olanları)
        if (user != null && user.Addresses != null)
        {
            model.SavedAddresses = user.Addresses
                .Where(a => a.IsActive == true)
                // sadece aktif olanları al
                .Select(a => new eCommerce.WEB.Models.AddressViewModel
                {
                    Id = a.AddressId,
                    FullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}",
                    Mobile = a.Mobile ?? "",
                    AddressLine = a.AddressLine ?? "",
                    City = a.City ?? "",
                    Country = a.Country ?? "",
                    Postcode = a.PostalCode ?? ""
                })
                .ToList();
        }
        else
        {
            model.SavedAddresses = new List<eCommerce.WEB.Models.AddressViewModel>();
        }

        // Eğer hiç aktif adres yoksa da boş liste at (Dropdown boş da kalsa “Yeni adres ekle” seçilebilir)
        if (model.SavedAddresses == null)
            model.SavedAddresses = new List<eCommerce.WEB.Models.AddressViewModel>();



        // Sepeti cookie üzerinden al
        List<eCommerce.WEB.Models.CartItemModel> cartItems = new List<eCommerce.WEB.Models.CartItemModel>();
        var cartCookie = Request.Cookies["cart"];
        if (!string.IsNullOrEmpty(cartCookie))
        {
            try
            {
                cartItems = JsonSerializer.Deserialize<List<eCommerce.WEB.Models.CartItemModel>>(cartCookie);
            }
            catch
            {
                cartItems = new List<eCommerce.WEB.Models.CartItemModel>();
            }
        }
        model.CartItems = cartItems;

        // Ara toplam, kargo ve toplam hesaplama
        model.Subtotal = cartItems.Sum(x => x.Price * x.Quantity);
        model.ShippingCost = 0; // İstersen burayı seçilen shippingOption ile hesaplayabilirsin
        model.Total = model.Subtotal + model.ShippingCost;
        // Ara toplam hesapla
        model.Subtotal = cartItems.Sum(x => x.Price * x.Quantity);

        // Kargo hesaplama: 300 TL ve üzeri ücretsiz, altı 3 TL
        model.ShippingCost = model.Subtotal >= 300 ? 0 : 3;

        // Toplam
        model.Total = model.Subtotal + model.ShippingCost;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
    {
        int currentUserId = GetCurrentUserId();
        var user = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId);

        if (user == null)
        {
            ModelState.AddModelError("", "Kullanıcı bulunamadı.");
            return View(model);
        }

        eCommerce.DATA.Entity.Address address;

        if (model.SelectedAddressId > 0)
        {
            // Kayıtlı adres seçildi
            address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == model.SelectedAddressId && a.UserId == user.UserId);

            if (address == null)
            {
                ModelState.AddModelError("", "Seçilen adres bulunamadı.");
                return View(model);
            }
        }
        else
        {
            // Yeni adres ekle
            address = new eCommerce.DATA.Entity.Address
            {
                UserId = user.UserId,
                AddressLine = model.AddressLine,
                City = model.City,
                Country = model.Country,
                PostalCode = model.Postcode,
                Mobile = model.Mobile
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
        }

        // Cart'ı cookie üzerinden al
        List<CartItemModel> cartItems = new List<CartItemModel>();
        var cartCookie = Request.Cookies["cart"];
        if (!string.IsNullOrEmpty(cartCookie))
        {
            cartItems = JsonSerializer.Deserialize<List<CartItemModel>>(cartCookie) ?? new List<CartItemModel>();
        }

        // Toplamları tekrar hesapla
        var subtotal = cartItems.Sum(x => x.Price * x.Quantity);
        var shippingCost = subtotal >= 300 ? 0 : 3;
        var total = subtotal + shippingCost;

        // Stripe için ödeme işlemi
        var domain = "https://localhost:44352"; // Canlıda domainin olacak


        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(total * 100), // kuruş cinsinden
                    Currency = "try",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Sipariş Ödemesi"
                    }
                },
                Quantity = 1
            }
        },
            Mode = "payment",
            SuccessUrl = domain + "/Checkout/Success?session_id={CHECKOUT_SESSION_ID}",

            CancelUrl = domain + "/Checkout/Cancel",
        };

        var service = new SessionService();
        Session session = service.Create(options);

        return Redirect(session.Url);
    }


    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("UserId")?.Value;
        if (claim == null) throw new System.Exception("UserId bulunamadı.");
        return int.Parse(claim);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress([FromBody] int id)
    {
        int currentUserId = GetCurrentUserId();

        var address = await _context.Addresses
            .Include(x => x.Orders)
            .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == currentUserId);

        if (address == null)
            return Json(new { success = false, message = "Adres bulunamadı." });

        // Eğer sipariş varsa pasif yap, yoksa direkt sil
        if (address.Orders.Any())
        {
            address.IsActive = false;
            await _context.SaveChangesAsync();
            return Json(new
            {
                success = true,
                message = "Adres aktif siparişler nedeniyle pasif hale getirildi."
            });
        }

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Adres silindi." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAddress([FromBody] AddressViewModel model)
    {
        int currentUserId = GetCurrentUserId();

        if (string.IsNullOrEmpty(model.AddressLine) || string.IsNullOrEmpty(model.City))
        {
            return Json(new { success = false, message = "Adres ve şehir alanları zorunludur." });
        }

        var address = new eCommerce.DATA.Entity.Address
        {
            UserId = currentUserId,
            AddressLine = model.AddressLine,
            City = model.City,
            Country = model.Country,
            PostalCode = model.Postcode,
            Mobile = model.Mobile
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        return Json(new { success = true, id = address.AddressId });
    }


    [HttpGet]
    public async Task<IActionResult> Success(string session_id)
    {
        // session_id yoksa checkout sayfasına yönlendir
        if (string.IsNullOrEmpty(session_id))
            return RedirectToAction("Index", "Checkout");

        var service = new SessionService();
        var session = service.Get(session_id);

        if (session == null || session.PaymentStatus != "paid")
            return RedirectToAction("Index", "Checkout");

        int currentUserId = GetCurrentUserId();

        // Cookie’den sepeti çek
        var cartCookie = Request.Cookies["cart"];
        List<CartItemModel> cartItems = new List<CartItemModel>();
        if (!string.IsNullOrEmpty(cartCookie))
        {
            cartItems = JsonSerializer.Deserialize<List<CartItemModel>>(cartCookie) ?? new List<CartItemModel>();
        }

        if (!cartItems.Any())
        {
            ViewData["Title"] = "Sipariş Başarılı";
            return View();
        }

        // Kullanıcının en son eklediği veya seçtiği adresi al
        var address = await _context.Addresses
            .Where(a => a.UserId == currentUserId)
            .OrderByDescending(a => a.AddressId)
            .FirstOrDefaultAsync();

        if (address == null)
            return RedirectToAction("Index", "Checkout");

        // Toplamları hesapla
        var subtotal = cartItems.Sum(x => x.Price * x.Quantity);
        var shippingCost = subtotal >= 300 ? 0 : 3;
        var total = subtotal + shippingCost;

        // Orders tablosuna kayıt at
        var order = new Order
        {
            UserId = currentUserId,
            AddressId = address.AddressId,
            OrderDate = DateTime.Now,
            TotalAmount = total,
            Status = "Ödeme Alındı",
            PaymentId = session.PaymentIntentId
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // OrderId oluşur

        // Sepetteki ürünleri OrderItems tablosuna ekle
        foreach (var item in cartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            };
            _context.OrderItems.Add(orderItem);
        }

        await _context.SaveChangesAsync();

        // Cookie’deki sepeti temizle
        Response.Cookies.Delete("cart");

        // --- Sipariş özetini View’a gönder ---
        var orderDetails = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

        var model = new OrderSuccessViewModel
        {
            OrderId = orderDetails.OrderId,
            OrderDate = orderDetails.OrderDate ?? DateTime.Now,
            TotalAmount = orderDetails.TotalAmount ?? 0,
            Address = $"{orderDetails.Address.AddressLine}, {orderDetails.Address.City}, {orderDetails.Address.Country}, {orderDetails.Address.PostalCode}",
            Products = orderDetails.OrderItems.Select(oi => new OrderProductDto
            {
                Name = oi.Product?.ProductName ?? "Ürün adı yok",
                ImageUrl = !string.IsNullOrEmpty(oi.Product?.ImageUrl)
                             ? "/" + oi.Product.ImageUrl.TrimStart('/') // başına / ekle
                             : "/img/default-product.png",
                Quantity = oi.Quantity ?? 0,
                Price = oi.UnitPrice ?? 0
            }).ToList()
        };


        ViewData["Title"] = "Sipariş Başarılı";
        return View(model);
    }




    [HttpGet]
    public IActionResult Cancel()
    {
        ViewData["Title"] = "Sipariş İptal Edildi";
        return View();
    }
}
    
