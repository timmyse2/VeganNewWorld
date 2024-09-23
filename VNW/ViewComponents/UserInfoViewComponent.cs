using Microsoft.AspNetCore.Mvc;

using VNW.Models; //for _context
using System.Threading.Tasks; //for task

namespace VNW.ViewComponents
{
    public class UserInfoViewComponent : ViewComponent
    {
        //private readonly DatabaseContext _context;
        private readonly VeganNewWorldContext _context;
        VNW.Common.MySession _ms = new Common.MySession();
        public UserInfoViewComponent(VeganNewWorldContext context)
        {
            _context = context;
        }

        //public async Task<IViewComponentResult> InvokeAsync(int TopPricing)
        //{

        //    //var products = await _context.Products
        //    //                             .OrderByDescending(p => p.Price)
        //    //                             .Take(TopPricing)
        //    //                             .ToListAsync();

        //    //return View(products);
        //    //return View("MyProduct", products);
        //    return View();
        //}

        //public IViewComponentResult Invoke()
        //{
        //    return View();
        //}

        public async Task<IViewComponentResult> InvokeAsync(int TestP)
        {
            ViewBag.ShopAccount = _ms.GetMySession("ShopAccount", HttpContext.Session);
            ViewData["UserIcon"] = _ms.GetMySession("UserIcon", HttpContext.Session);
            if(TestP == 1)
            {
                ViewData["Show"] = "Yes";
            }
            return View();
        }
    }    
}


