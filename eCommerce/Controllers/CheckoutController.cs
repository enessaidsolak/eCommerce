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

        // Adresleri ekle
        if (user != null && user.Addresses != null)
        {
            model.SavedAddresses = user.Addresses
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

        Address address;

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
            address = new Address
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

        // Sipariş kaydı ve ödeme işlemleri burada yapılabilir

        return RedirectToAction("OrderSuccess");
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
            .FirstOrDefaultAsync(a => a.AddressId == id && a.UserId == currentUserId);

        if (address == null)
            return Json(new { success = false, message = "Adres bulunamadı." });

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }
    
}
