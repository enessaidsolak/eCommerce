using eCommerce.DATA.Entity;
using System.Collections.Generic;

public class CategoryDetailsViewModel
{
    public List<Category> FeaturedCategory { get; set; }
    public List<Product> Products { get; set; } // ✅ Liste olmalı
}
