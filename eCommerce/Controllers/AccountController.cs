using Microsoft.AspNetCore.Mvc;
using eCommerce.DATA.Context;
using eCommerce.DATA.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BCrypt.Net;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Security.Policy;
using Microsoft.AspNetCore.Authorization;

public class AccountController : Controller
{
    private readonly eCommerceDBContext _context;

    public AccountController(eCommerceDBContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("", "Bu email zaten kayıtlı.");
            return View(model);
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

        var newUser = new User
        {
            Email = model.Email,
            Password = hashedPassword,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, newUser.Email),
            new Claim("UserId", newUser.UserId.ToString()),
            new Claim("Name", newUser.FirstName ?? ""),
            new Claim("SurName", newUser.LastName ?? "")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            ModelState.AddModelError("", "Geçersiz email veya şifre.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim("UserId", user.UserId.ToString()),
            new Claim("Name", user.FirstName ?? ""),
            new Claim("SurName", user.LastName ?? ""),
             new Claim(ClaimTypes.Role, "Admin") // Burada Admin rolü ekleniyor
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // ------------------ Şifre Sıfırlama ------------------

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

  

[HttpPost]
public IActionResult ForgotPassword(ForgotPasswordViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
    if (user == null)
    {
        ViewBag.Error = "Bu e-posta adresine ait kullanıcı bulunamadı.";
        return View(model);
    }

    // Token oluştur ve 1 saat geçerli olsun
    user.ResetToken = Guid.NewGuid().ToString();
    user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

    _context.SaveChanges();

    var resetLink = Url.Action("ResetPassword", "Account", new { token = user.ResetToken }, Request.Scheme);

            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("eneszekiyun25@gmail.com", "onql xehx lqlt wbho"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("eneszekiyun25@gmail.com"),
                    Subject = "Şifre Sıfırlama Talimatları",
                    Body = $"Merhaba,<br/>Şifrenizi sıfırlamak için lütfen <a href='{resetLink}'>buraya tıklayın</a>.<br/>Eğer bu isteği siz yapmadıysanız, bu mesajı görmezden gelin.",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(user.Email);
            smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Şifre sıfırlama e-postası gönderilemedi, lütfen daha sonra tekrar deneyin.";
                Console.WriteLine("Mail gönderme hatası: " + ex.ToString());
                return View(model);
            }

        ViewBag.Message = "Şifre sıfırlama talimatları e-posta adresinize gönderildi.";
    return View();
}


[HttpGet]
    public IActionResult ResetPassword(string token)
    {
        var user = _context.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpires > DateTime.UtcNow);
        if (user == null)
        {
            return BadRequest("Geçersiz veya süresi dolmuş token.");
        }

        return View(new ResetPasswordViewModel { Token = token });
    }

    [HttpPost]
    public IActionResult ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = _context.Users.FirstOrDefault(u => u.ResetToken == model.Token && u.ResetTokenExpires > DateTime.UtcNow);
        if (user == null)
        {
            return BadRequest("Geçersiz veya süresi dolmuş token.");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpires = null;
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi.";
        return RedirectToAction("Login");
    }

    // ------------------ Varolan Şifreleri Hash'leme ------------------

    [HttpGet]
    public IActionResult HashExistingPasswords()
    {
        var users = _context.Users.Where(u => !u.Password.StartsWith("$2")).ToList();

        foreach (var user in users)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        }

        _context.SaveChanges();

        return Content("Şifreler hash'lendi.");
    }

    
}
