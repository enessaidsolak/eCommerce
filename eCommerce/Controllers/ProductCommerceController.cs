using eCommerce.DATA.Context;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Controllers;

public class ProductCommerceController : Controller
{
    private readonly eCommerceDBContext db;

    public ProductCommerceController(eCommerceDBContext context)
    {
        db = context;
    }

   
}
