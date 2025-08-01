using System.Collections.Generic;
using eCommerce.DATA.Entity;

namespace eCommerce.DATA.Models
{
    public class ProductModel
    {
        public List<Category> Categories { get; set; }
        public List<Product> Products { get; set; }
        public int? SelectedCategoryId { get; set; }
        public decimal? SelectedPrice { get; set; }

        public string? SelectedKeyword { get; set; }

    }
}
