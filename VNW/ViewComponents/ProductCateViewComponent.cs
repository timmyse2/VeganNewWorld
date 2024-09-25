using Microsoft.AspNetCore.Mvc;

using VNW.Models; //for _context
using System.Threading.Tasks; //for task
using Microsoft.EntityFrameworkCore; //for ToListAsync

namespace VNW.ViewComponents
{
    public class ProductCateViewComponent :  ViewComponent
    {
        private readonly VeganNewWorldContext _context;
        //::set session common interface
        VNW.Common.MySession _ms = new Common.MySession();

        public ProductCateViewComponent(VeganNewWorldContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = _context.Category;
            ViewData["categories"] = await categories.ToListAsync();

            return View();
        }
    }
}