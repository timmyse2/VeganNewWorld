using Microsoft.AspNetCore.Mvc;

using VNW.Models; //for _context
using System.Threading.Tasks; //for task

namespace VNW.ViewComponents
{
    public class PagesViewComponent : ViewComponent
    {
        //private readonly DatabaseContext _context;
        //private readonly VeganNewWorldContext _context;
        //VNW.Common.MySession _ms = new Common.MySession();
        public PagesViewComponent(VeganNewWorldContext context)
        {
            //_context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }    
}


