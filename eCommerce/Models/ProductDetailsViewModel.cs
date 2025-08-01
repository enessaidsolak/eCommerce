using System.Collections.Generic;
using eCommerce.DATA.Entity;  

namespace eCommerce.DATA.Models  
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<Product> FeaturedProducts { get; set; }
        public List<Category>FeaturedCategory { get; set; }

    }
    
}
