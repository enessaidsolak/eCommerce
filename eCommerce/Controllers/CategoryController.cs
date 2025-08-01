using eCommerce.DATA.Context;
using eCommerce.DATA.Entity;
using eCommerce.DATA.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;


namespace eCommercePanel.Controllers
{
    public class CategoryController : Controller
    {
        private readonly eCommerceDBContext _context;

        public CategoryController(eCommerceDBContext context)
        {
            _context = context;
        }


       
    }

}
