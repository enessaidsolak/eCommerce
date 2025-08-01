using Microsoft.AspNetCore.Http;
using eCommerce.DATA.Entity;
using System.Collections.Generic;
using eCommerce.DATA.Entity;

namespace eCommerce.Models
{
    public class ProductEditViewModel
    {
        public Product Product { get; set; }
        public List<Category> Categories { get; set; }
        public IFormFile? ImageFile { get; set; }
        public List<Product> FeaturedProducts { get; set; }
    }
}
