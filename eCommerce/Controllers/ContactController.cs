using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using YourNamespace.Models; // EmailSettings modelinin namespace'i
using MailKit.Net.Smtp;
using MimeKit;

namespace YourNamespace.Controllers
{
    public class ContactController : Controller
    {
        private readonly EmailSettings _emailSettings;

        public ContactController(IConfiguration configuration)
        {
            _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string Name, string Email, string Message)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                emailMessage.To.Add(new MailboxAddress("Site Owner", _emailSettings.FromEmail)); // Kendine gönderebilirsin
                emailMessage.Subject = $"Yeni Mesaj: {Name}";
                emailMessage.Body = new TextPart("plain")
                {
                    Text = $"Gönderen: {Name}\nEmail: {Email}\nMesaj:\n{Message}"
                };

                using (var client = new SmtpClient())
                {
                    client.Connect(_emailSettings.Host, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(_emailSettings.Username, _emailSettings.Password);
                    client.Send(emailMessage);
                    client.Disconnect(true);
                }

                ViewBag.Message = "Mesajınız başarıyla gönderildi!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Mesaj gönderilirken bir hata oluştu: {ex.Message}";
            }

            return View();
        }
    }
}
