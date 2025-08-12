    using eCommerce.Controllers;
using eCommerce.DATA.Entity;
using eCommerce.Dtos;
using System.ComponentModel.DataAnnotations;


namespace eCommerce.Models
{
    public class HomeModel
    {
        public List<CategoryDtos> Categories { get; set; }
        public List<Product> Category1Products { get; set; }
        public List<Product> Category2Products { get; set; }
        public List<Product> Category3Products { get; set; }
        public List<Product> Category4Products { get; set; }
        public List<Product> Category5Products { get; set; }

        public List<Product> LatestProducts { get; set; }

        public List<Product> BestSellingProduct { get; set; }

        public List<ProductComment> ProductComments { get; set; }  
        



    }
    public class CategoryModel
    {
        public List<Category> Categories
        {
            get; set;
        }
    }
    



}
