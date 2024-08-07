using System;
//using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VNW.Models;

using System.Diagnostics; //for debug
//using System.Security.Cryptography; //for hash password
using System.Text; //for encoding

using SixLabors.ImageSharp; //for graphic captcha
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
//using SixLabors.ImageSharp.Drawing;
//using System.Drawing; 
//using System.Drawing.Imaging;
//using System.Drawing.Common;
using System.IO;


namespace VNW.Controllers
{
    public class CustomersController : Controller
    {
        private readonly VeganNewWorldContext _context;

        //::set session common interface
        VNW.Common.MySession _ms = new Common.MySession();

        public CustomersController(VeganNewWorldContext context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            //check admin
            if(!_ms.CheckAdmin(HttpContext.Session))            
                return View(null);
            
            return View(await _context.Customer.ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,CompanyName,ContactName,Address,City,PostalCode,Country,Phone")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CustomerId,CompanyName,ContactName,Address,City,PostalCode,Country,Phone")] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var customer = await _context.Customer.FindAsync(id);
            _context.Customer.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(string id)
        {
            return _context.Customer.Any(e => e.CustomerId == id);
        }


        //::login for end-user
        public async Task<IActionResult> Login()
        {
            await Task.Run(()=> {
                //VNW.Common.MySession ms = new Common.MySession();
                //Debug.WriteLine(" my common test" + ms.Test("123"));
                //_ms.SetMySession("ms_test", "1979", HttpContext.Session);
                //Debug.WriteLine(" my common test" + _ms.GetMySession("ms_test", HttpContext.Session));
                //ms.Dispose();

                ViewData["currentHost"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
                ViewData["currentBase"] = $"{HttpContext.Request.PathBase}";
                ViewData["UserAccount"] = HttpContext.Request.Cookies["UserAccount"];
                string Captcha = GenerateCaptcha();                
                _ms.SetMySession("Captcha", Captcha, HttpContext.Session);
                Captcha = EncodeCaptcha(Captcha);
                ViewData["Captcha"] = Captcha;
            });

            #region Grpahic Captcha
            //string PathBase = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";            
            //string imageUrl = PathBase + "/images/joker.png";
            //System.Net.Http.HttpClient tc = new System.Net.Http.HttpClient();
            //byte[] imageBytes = await tc.GetByteArrayAsync(imageUrl);
            //string base64Image = Convert.ToBase64String(imageBytes);                        
            //ViewData["ImageBase64"] = base64Image;
            #endregion

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string account, string password, string pin, string role)
        {
            //::precheck
            int errorCode = 0;
            if (account == null)
            {
                //return NotFound();
                //return Content("Account Id is null");
                errorCode = 101;
                return Json(new { result = "FAIL", detail = "Id is null", errorCode });
            }           

            //::check account id and password
            var customer = await _context.Customer
                .FirstOrDefaultAsync(m => m.CustomerId == account);
            if (customer == null)
            {
                //return NotFound();
                //return Content("No matched data");
                errorCode = 102;
                return Json(new { result = "FAIL", detail = "no matched data", errorCode });
            }
            else
            {
                //::<NOTICE: upper case and lower case><FIXED for ISSUE >
                if(account != customer.CustomerId)
                {
                    //account = customer.CustomerId;
                    errorCode = 102;
                    return Json(new { result = "NG", detail = "upper case or lower case is mismatched", errorCode });
                }
                //<><><>

                try
                {
                    string Captcha = "1314";
                    Captcha = _ms.GetMySession("Captcha", HttpContext.Session);
                    if (pin != Captcha)
                    {
                        errorCode = 103;
                        return Json(new { result = "FAIL", detail = "pin or captcha is mismatched", errorCode });
                    }

                    if (password == "") //password
                    {
                        errorCode = 104;
                        return Json(new { result = "FAIL", detail = "password is mismatched", errorCode });
                    }
                }
                catch
                {
                    errorCode = 105;
                    return Json(new { result = "Error", detail = "tbc", errorCode });
                }

                ViewData["IsUserLogin"] = "YES";
                //ViewData["IsAdmin"] = "YES";
                //ViewData["IsVenderLogin"] = "YES";
                HttpContext.Response.Cookies.Append("UserAccount", customer.CustomerId);
                //SetMySession("IsAdmin", "YES");   
                _ms.SetMySession("IsUserLogin", "YES", HttpContext.Session);
                _ms.SetMySession("UserAccount", customer.CustomerId, HttpContext.Session);
                _ms.SetMySession("UserLevel", "3C", HttpContext.Session);
                //:: 1A(admin) 2B(business vender) 3C(customer)
                HttpContext.Session.Remove("Captcha");

                return Json(new { result = "PASS", detail = "matched", errorCode });
            }
            //::pass case
            //return View();
            //return Content("End of Login");            
        }

        public async Task<IActionResult> Logout()
        //public IActionResult Logout()
        {
            await Task.Run(() => {
                //_ms.SetMySession("IsUserLogin", "", HttpContext.Session);
                //_ms.SetMySession("UserAccount", "", HttpContext.Session);
                HttpContext.Session.Remove("IsUserLogin");
                HttpContext.Session.Remove("UserAccount");
                HttpContext.Session.Remove("UserLevel");
                HttpContext.Session.Remove("ShopAccount");
                TempData["td_server"] = "已登出"; //::
            });
            return RedirectToAction("Login");
            //return Content("LOGOUT");
        }

        //::for Admin
        //[HttpPost]
        public async Task<IActionResult> AdminLogin(string account, string password, string pin, string role)
        //public IActionResult AdminLogin(string account, string password, string pin, string role)
        {

            await Task.Run(() =>
            {
                //::pass case
                _ms.SetMySession("IsUserLogin", "YES", HttpContext.Session);
                _ms.SetMySession("UserAccount", "Saber@Emiya2006", HttpContext.Session);
                _ms.SetMySession("UserLevel", "1A", HttpContext.Session);
            }); 
            return Json(new { result = "PASS", detail = "admin login" });
        }

        //::refer to github xxx    
        public static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        public static string GenerateCaptcha()
        {
            string _pin = "";
            try
            {
                //::4 digitals
                var rand = new Random();
                for (int i = 0; i <= 3; i++)
                {
                    _pin += rand.Next(0, 9);
                }
                //::keep value in session
                //_ms.SetMySession("Pin", "1A", HttpContext.Session);
                return _pin;
            }
            catch
            {
                return null;
            }
        }

        public static string EncodeCaptcha(string Captcha)
        {
            string Captcha_Encoded = "";
            char[] rd = new char[] { ' ', '\'', '.', '_', '-' };
            var rand = new Random();

            foreach (char c in Captcha)
            {
                int r = rand.Next(0, 4);
                Debug.WriteLine("r: " + r);
                Captcha_Encoded += c + rd[r].ToString();
            }
            return Captcha_Encoded;
        }

        public async Task<IActionResult> UpdateCaptcha()
        {
            string Result = "";
            string Captcha = null;
            await Task.Run(() =>
            {
                try
                {
                    Captcha = GenerateCaptcha();
                    _ms.SetMySession("Captcha", Captcha, HttpContext.Session);
                    //::generate new image
                    Captcha = EncodeCaptcha(Captcha);
                    Result = "PASS";
                }
                catch
                {
                    Result = "Error";
                }
            });
            return Json(new { Result, Captcha});
        }               

        //::Generate Graphic Captcha, but it is not ready
        public IActionResult GenerateImage()
        {
            int width = 300;
            int height = 100;
            SixLabors.ImageSharp.Image image = new Image<Rgba32>(width, height);
            try
            {
                image.Mutate(ctx => {
                    //ctx.DrawImage()
                    ctx.BackgroundColor(Color.Blue);                    
                    //ctx.Fil
                    //ctx.DrawImage.fi    
                    //ctx.DrawImage                
                    //ctx.DrawText
                });
                //image.Mutate(ctx => ctx.Fill(Color.White));
                //image.Mutate(ctx => ctx.DrawPolygon(Color.Red, 5, new PointF(10, 10), new PointF(190, 10), new PointF(190, 90), new PointF(10, 90)));
                //image.Save("output.png");
                MemoryStream ms = new MemoryStream();
                image.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());                
                ms.Seek(0, SeekOrigin.Begin);
                return File(ms.ToArray(), "image/png");
            }
            catch
            {
                return null;
            }

            ////::use System.Draw.Bitmap, but it is not work on this platform
            //    using (Bitmap bitmap = new Bitmap(200, 100))
            //    {
            //        using (Graphics g = Graphics.FromImage(bitmap))
            //        {
            //            g.Clear(Color.White);
            //            g.DrawString("Hello, ASP.NET Core!", new Font("Arial", 20), Brushes.Black, new PointF(10, 40));
            //        }
            //        using (MemoryStream ms = new MemoryStream())
            //        {
            //            bitmap.Save(ms, ImageFormat.Png);
            //            ms.Seek(0, SeekOrigin.Begin);
            //            return File(ms.ToArray(), "image/png");
            //        }
            //    }

        }

    }
}
