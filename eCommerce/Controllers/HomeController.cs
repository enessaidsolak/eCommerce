using eCommerce.DATA.Context;
using eCommerce.Dtos;
using eCommerce.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;

namespace eCommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly eCommerceDBContext _context;

        public HomeController(ILogger<HomeController> logger, eCommerceDBContext context)
        {
            _logger = logger;
            _context = context;
        }
       
        /// <summary>
        /// Anasayfaya verileri ve bunlarý mapler 
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();

            if (categories.Count < 5)
            {
                // Kategori sayýsý 5'ten azsa nasýl davranacaðýný belirle
            }

            HomeModel model = new HomeModel();
            List<CategoryDtos> categoryDtos = new List<CategoryDtos>();
            foreach (var category in categories)
            {
                //Mapleme - haritalandýrma
                CategoryDtos categoryDto = new CategoryDtos();
                categoryDto.CategoryName = category.CategoryName;
                categoryDto.ImageUrl = category.ImageUrl;

                categoryDtos.Add(categoryDto);
            }
            model.Categories = categoryDtos;
            
            model.Category1Products = _context.Products.Where(x => x.CategoryId == categories[0].CategoryId).ToList();
            model.Category2Products = _context.Products.Where(x => x.CategoryId == categories[1].CategoryId).ToList();
            model.Category3Products = _context.Products.Where(x => x.CategoryId == categories[2].CategoryId).ToList();
            model.Category4Products = _context.Products.Where(x => x.CategoryId == categories[3].CategoryId).ToList();
            model.Category5Products = _context.Products.Where(x => x.CategoryId == categories[4].CategoryId).ToList();
            model.LatestProducts = _context.Products
            .OrderByDescending(p => p.ProductId)
            .ToList();

            return View(model);
            

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult HGUFTgygtfUF()
        {
            return View();
        }
    }
}
