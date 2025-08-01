using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using eCommerce.DATA.Entity;

public class CategoryModel
{
    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    public string CategoryName { get; set; }

    [Required(ErrorMessage = "Ürün açıklaması zorunludur.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
    public decimal Price { get; set; }

    [Display(Name = "Eski Fiyat")]
    public decimal? OldPrice { get; set; } // ✅ Eklendi

    [Required(ErrorMessage = "Stok miktarı zorunludur.")]
    [Range(0, int.MaxValue, ErrorMessage = "Stok negatif olamaz.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
    public int CategoryId { get; set; }

    public IFormFile ImageFile { get; set; }

    public List<Category> Categories { get; set; }
    public List<Product> Products { get; set; }

    public int? SelectedCategoryId { get; set; }  
}
