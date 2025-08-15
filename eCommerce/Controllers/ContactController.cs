using Microsoft.AspNetCore.Mvc;

namespace YourProjectNamespace.Controllers
{
    public class ContactController : Controller
    {
        // GET: Contact
        public IActionResult Index()
        {
            return View();
        }

        // POST: Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string Name, string Email, string Message)
        {
            // Burada istersen mail gönderebilir veya veritabanına kaydedebilirsin
            ViewBag.Message = "Mesajınız başarıyla gönderildi!";
            return View();
        }
    }
}
