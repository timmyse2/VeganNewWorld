using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VNW.Models;

namespace VNW.Controllers
{
    public class HomeController : Controller
    {
        //::set session common interface
        private VNW.Common.MySession _ms = new Common.MySession();

        public IActionResult Index()
        {

            bool IsAdmin = true;
            if (!_ms.CheckAdmin(HttpContext.Session))
                IsAdmin = false;

            ViewBag.UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
            ViewBag.ShopAccount = _ms.GetMySession("ShopAccount", HttpContext.Session);
            ViewBag.UserIcon = _ms.GetMySession("UserIcon", HttpContext.Session);
            ViewBag.IsUserLogin = _ms.GetMySession("IsUserLogin", HttpContext.Session);
            ViewBag.IsAdmin = IsAdmin;
            ViewData["UserLevel"] = _ms.GetMySession("UserLevel", HttpContext.Session);
            return View();
        }

        public IActionResult About()
        {
            //ViewData["Message"] = "Your application description page.";
            ViewData["Message"] = "分享世界的蔬食、素食料理...";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
