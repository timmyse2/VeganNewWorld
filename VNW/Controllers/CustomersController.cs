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
            //string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            //if (UserLevel != "1A")
            //{
            //    TempData["td_server"] = "該頁面只有管理者*或商家員工*可使用，請先登入";
            //    return RedirectToAction("Login", "Customers");
            //}

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

        public async Task<IActionResult> Info()
        {
            string id = _ms.GetMySession("UserAccount", HttpContext.Session);
            if (id == null)
            {
                //return NotFound();
                TempData["td_server"] = "請先登入";
                return RedirectToAction("Login", "Customers");
            }            
            var customer = await _context.Customer
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                //return NotFound();
                TempData["td_server"] = "帳號未登入";
                return RedirectToAction("Login", "Customers");
            }
            ViewData["UserAccount"] = id;
            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            //::1A only
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*或商家員工*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }

            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,CompanyName,ContactName,Address,City,PostalCode,Country,Phone")] Customer customer)
        {
            //::1A only
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*或商家員工*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }

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
            //::1A only
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*或商家員工*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }

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
            //::1A only
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*或商家員工*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }

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

            string retryLockTime = _ms.GetMySession("retryLockTime", HttpContext.Session);
            if (retryLockTime != null)
            {
                DateTime dt_retryLockTime  = Convert.ToDateTime(retryLockTime);
                TimeSpan ts = DateTime.Now - dt_retryLockTime;
                int LimitSec = 180;
                if (ts.TotalSeconds > LimitSec)
                {
                    //::out 3 min then release
                    ViewData["retryLock"] = null;
                    HttpContext.Session.Remove("retryCount");
                }
                else
                {
                    //::in 3 min then hold
                    ViewData["retryLock"] = "Wait: " + (int)(LimitSec - ts.TotalSeconds) + " sec";
                }
            }
            else
            {
                //release
                ViewData["retryLock"] = null;
                HttpContext.Session.Remove("retryCount");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string account, string password, string pin, string role)
        {
            int retryCount = 0;
            string sRtryCount = _ms.GetMySession("retryCount", HttpContext.Session);
            if (sRtryCount != null)
                retryCount = int.Parse(sRtryCount);

            //::precheck
            int errorCode = 0;
            if (account == null)
            {
                //return NotFound();
                //return Content("Account Id is null");
                errorCode = 101;
                return Json(new { result = "FAIL", detail = "Id is null", errorCode, retryCount });
            }           

            //::check account id and password
            var customer = await _context.Customer
                .FirstOrDefaultAsync(m => m.CustomerId == account);
            if (customer == null)
            {
                //return NotFound();
                //return Content("No matched data");
                errorCode = 102;
                retryCount++;
                _ms.SetMySession("retryCount", retryCount.ToString(), HttpContext.Session);
                if (retryCount > 3)
                    _ms.SetMySession("retryLockTime", DateTime.Now.ToString(), HttpContext.Session);
                return Json(new { result = "FAIL", detail = "no matched data", errorCode, retryCount });
            }
            else
            {
                //::<NOTICE: upper case and lower case><FIXED for ISSUE >
                if(account != customer.CustomerId)
                {
                    //account = customer.CustomerId;
                    errorCode = 102;
                    return Json(new { result = "NG", detail = "upper case or lower case is mismatched", errorCode, retryCount });
                }
                //<><><>

                try
                {
                    string Captcha = _ms.GetMySession("Captcha", HttpContext.Session);
                    if (pin != Captcha)
                    {
                        retryCount++;
                        _ms.SetMySession("retryCount", retryCount.ToString(), HttpContext.Session);
                        if (retryCount > 3)
                            _ms.SetMySession("retryLockTime", DateTime.Now.ToString(), HttpContext.Session);
                        errorCode = 103;
                        return Json(new { result = "FAIL", detail = "pin or captcha is mismatched", errorCode, retryCount });
                    }

                    if (customer.Salt == null)
                        customer.Salt = "";
                    string inputPasswordEncoded =
                        EmployeesController.PasswordSalt(password, customer.Salt);
                    if (inputPasswordEncoded != customer.PasswordEncoded) //password
                    {
                        retryCount++;
                        _ms.SetMySession("retryCount", retryCount.ToString(), HttpContext.Session);
                        if (retryCount > 3)
                            _ms.SetMySession("retryLockTime", DateTime.Now.ToString(), HttpContext.Session);
                        errorCode = 104;
                        return Json(new { result = "FAIL", detail = "password is mismatched", errorCode, retryCount });
                    }
                }
                catch
                {
                    errorCode = 105;
                    return Json(new { result = "Error", detail = "tbc", errorCode, retryCount });
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

                //::remove flags
                HttpContext.Session.Remove("Captcha");
                HttpContext.Session.Remove("retryCount");
                HttpContext.Session.Remove("retryLockTime");                

                return Json(new { result = "PASS", detail = "matched", errorCode, retryCount });
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

        //::qucik login for 2B test - disable it in the c
        public async Task<IActionResult> blogin(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null)
            {
                return Json(new { result = "ng", detail = "lost id" });
            }

            string ShopAccount = emp.Name;
            //::pass case
            _ms.SetMySession("IsUserLogin", "NO", HttpContext.Session);
            _ms.SetMySession("UserLevel", "2B", HttpContext.Session);                
            _ms.SetMySession("UserAccount", ShopAccount, HttpContext.Session);
            _ms.SetMySession("ShopAccount", ShopAccount, HttpContext.Session);
            _ms.SetMySession("EmployeeId", id.ToString(), HttpContext.Session);

            return RedirectToAction("Index", "Home");
            //return Json(new { result = "PASS", detail = "shop login" });
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


        public async Task<IActionResult> CheckCaptcha(string Captcha)
        {
            string Result = "";            
            await Task.Run(() =>
            {
                try
                {
                    if(Captcha == _ms.GetMySession("Captcha", HttpContext.Session))                    
                        Result = "PASS";                    
                    else
                        Result = "FAIL";
                }
                catch
                {
                    Result = "Error";
                }
            });
            return Json(new { Result });
        }

        public async Task<IActionResult> Register()
        {

            string Captcha = GenerateCaptcha();
            _ms.SetMySession("Captcha", Captcha, HttpContext.Session);
            Captcha = EncodeCaptcha(Captcha);
            ViewData["Captcha"] = Captcha;
            ViewData["currentPath"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string NewAccount, string NewPassword, string NewPassword_Confirm, string Captcha)
        {
            //::check input data
            if(NewAccount == null || NewPassword == null || NewPassword_Confirm == null || Captcha == null)
            {
                TempData["td_serverWarning"] = "資料不足";
                return View();
            }

            //::check captcha
            string Captcha_expected = _ms.GetMySession("Captcha", HttpContext.Session);
            if (Captcha_expected == null)
            {
                //ng case                
                TempData["td_serverWarning"] = "驗證碼不相符";                
                return View();
            }
            else if (Captcha_expected != Captcha)
            {
                //ng case                
                TempData["td_serverWarning"] = "驗證碼不相符";
                ViewData["Captcha"] = EncodeCaptcha(Captcha_expected);
                return View();
            }

            //::check account format
            string pattern_id = "^\\w+((-\\w+)|(\\.\\w+))*\\@[A-Za-z0-9]+((\\.|-)[A-Za-z0-9]+)*\\.[A-Za-z]+$";
            System.Text.RegularExpressions.Regex regex_id
                = new System.Text.RegularExpressions.Regex(pattern_id);
            if (!regex_id.IsMatch(NewAccount))
            {
                TempData["td_serverWarning"] = "帳號格式不符 " + NewAccount;
                return View();
            }
            //::check account is not exist
            var user = await _context.Customer.AsNoTracking().Where(c=>c.CustomerId == NewAccount).FirstOrDefaultAsync();
            if (user != null)
            {
                TempData["td_serverWarning"] = "帳號已被使用";
                return View();
            }

            //::check pwd (format)
            string pattern_pwd = "^(?=.*[0-9])(?=.*[A-Za-z]).{5,20}$";
            System.Text.RegularExpressions.Regex regex_pwd
                = new System.Text.RegularExpressions.Regex(pattern_pwd);
            if (!regex_pwd.IsMatch(NewPassword))
            {
                TempData["td_serverWarning"] = "密碼格式不符";
                return View();
            }
            if(NewPassword != NewPassword_Confirm)
            {
                TempData["td_serverWarning"] = "密碼驗證不相符";
                return View();
            }

            if (ModelState.IsValid)
            {
                //::encode pwd with salt            
                string Salt = EmployeesController.GenerateSalt();
                string PasswordEncoded = EmployeesController.PasswordSalt(NewPassword, Salt);

                //::create user info in customer
                Customer customer = new Customer()
                {
                    CustomerId = NewAccount,
                    PasswordEncoded = PasswordEncoded,
                    Salt = Salt,
                    ContactName = "",
                    CompanyName = "" //tbd in next phase
                };
                _context.Add(customer);
                await _context.SaveChangesAsync();

                //::show basic user info                
                HttpContext.Response.Cookies.Append("UserAccount", customer.CustomerId);
                TempData["td_server"] = "已建立帳戶";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["td_serverWarning"] = "資料處理異常";
                return View();
            }
        }

        //::api for precheckID
        public async Task<IActionResult> PrecheckID(string account)
        {
            string result = "tbd", detail = "", code ="";

            if (account == null || account == "")
            {
                result = "fail";
                detail = "account is empty";
                code = "100";
                return Json(new { result, detail, code });
            }

            if (account.Length < 5 || account.Length > 25)
            {
                result = "fail";
                detail = "account is too short or too long";
                code = "101";
                return Json(new { result, detail, code });
            }

            var user = await _context.Customer.FindAsync(account);
            if (user != null)
            {
                result = "fail";
                detail = "existed";
                code = "102";
                return Json(new { result, detail, code });
            }

            //check format - such with @
            if(!account.Contains("@"))
            {
                result = "fail";
                detail = "format is wrong";
                code = "103";
                return Json(new { result, detail, code});
            }

            result = "PASS";
            //detail = "";
            //code = "0";
            return Json(new {result, detail, code });
        }

        public async Task<IActionResult> ForgetPWD()
        {
            return View();
        }

        //::for 3C
        public async Task<IActionResult> EditInfo()
        {
            string id = _ms.GetMySession("UserAccount", HttpContext.Session);
            if (id == null)
            {
                //return NotFound();
                TempData["td_server"] = "請先登入";
                return RedirectToAction("Login", "Customers");
            }

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

        //::for 3C
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInfo([Bind("CustomerId,CompanyName,ContactName,Address,City,PostalCode,Country,Phone")] Customer customer)
        {
            string id = _ms.GetMySession("UserAccount", HttpContext.Session);
            if (id == null)
            {
                TempData["td_server"] = "請先登入";
                return RedirectToAction("Login", "Customers");
            }
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    #region keep origin password
                    //:: do not let password show in view or frontend
                    if (customer.PasswordEncoded == null || customer.PasswordEncoded == "")
                    {
                        var customer_original = await _context.Customer.Where(c => c.CustomerId == id)
                            .AsNoTracking().FirstOrDefaultAsync();
                        if (customer_original == null)
                        {
                            return Content("can not find origin data");
                        }
                        customer.PasswordEncoded = customer_original.PasswordEncoded;
                        customer.Salt = customer_original.Salt;                        
                    }
                    #endregion

                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    TempData["td_serverWarning"] = "";
                    TempData["td_serverMessage"] = "已更新";
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
                return RedirectToAction(nameof(Info));
            }
            return View(customer);
        }


        //::for 3C
        public async Task<IActionResult> EditPassword()
        {
            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            string id = _ms.GetMySession("UserAccount", HttpContext.Session);
            if (id == null)
            {
                //return NotFound();
                TempData["td_server"] = "請先登入";
                return RedirectToAction("Login", "Customers");
            }

            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            string Captcha = VNW.Controllers.CustomersController.GenerateCaptcha();
            _ms.SetMySession("Captcha", Captcha, HttpContext.Session);
            Captcha = VNW.Controllers.CustomersController.EncodeCaptcha(Captcha);
            ViewData["Captcha"] = Captcha;
            ViewData["currentPath"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //
        public async Task<IActionResult> EditPassword(string id, string OldPassword, string NewPassword, string NewPassword_Confirm, string Captcha)
        {
            string _id = _ms.GetMySession("UserAccount", HttpContext.Session);
            if (id == null || id != _id)
            {
                //return NotFound();
                TempData["td_server"] = "請先登入";
                return RedirectToAction("Login", "Customers");
            }

            var customer = await _context.Customer.Where(c => c.CustomerId == id).FirstOrDefaultAsync();            

            string Captcha_expected = _ms.GetMySession("Captcha", HttpContext.Session);
            ViewData["Captcha"] = VNW.Controllers.CustomersController.EncodeCaptcha(Captcha_expected);
            if (Captcha == null || Captcha.Length != 4)
            {
                TempData["td_serverWarning"] = "驗證碼長度錯誤";
                return View(customer);
            }
            if (Captcha != Captcha_expected)
            {
                //error
                TempData["td_serverWarning"] = "驗證碼不符";
                return View(customer);
            }

            if (customer == null)
            {
                //return Content("no matched data");
                TempData["td_serverWarning"] = "找不到相符的資料!";
                return View();
            }

            //::check pwd length, format...
            if (OldPassword == null || OldPassword.Length > 20)
            {
                TempData["td_serverWarning"] = "舊密碼未輸入或格式不合";
                return View(customer);
            }
            //::compare password
            string OldPassword_Encoded = EmployeesController.PasswordSalt(OldPassword, customer.Salt);

            if (customer.PasswordEncoded != OldPassword_Encoded)
            {
                //error
                TempData["td_serverWarning"] = "舊密碼不符合";
                return View(customer);
            }

            if (NewPassword == null || NewPassword.Length < 5 || NewPassword.Length > 20)
            {
                TempData["td_serverWarning"] = "新密碼長度過短、長";
                return View(customer);
            }

            //:: 0~9, a~z, A~Z 
            string pattern = "^(?=.*[0-9])(?=.*[A-Za-z]).{5,20}$";
            System.Text.RegularExpressions.Regex regex
                = new System.Text.RegularExpressions.Regex(pattern);
            if (!regex.IsMatch(NewPassword))
            {
                TempData["td_serverWarning"] = "新密碼格式不符";
                return View(customer);
            }

            if (NewPassword != NewPassword_Confirm)
            {
                //error
                TempData["td_serverWarning"] = "新密碼與新密碼確認不一致";
                return View(customer);
            }

            //::udpate new Salt, 10 chars randomly
            customer.Salt = EmployeesController.GenerateSalt();
            string NewPassword_Encoded = EmployeesController.PasswordSalt(NewPassword, customer.Salt);

            if (ModelState.IsValid)
            {
                try
                {
                    //update pwd in DB
                    customer.PasswordEncoded = NewPassword_Encoded;
                    _context.Update(customer);
                    await _context.SaveChangesAsync();

                    //pass case
                    TempData["td_serverWarning"] = "";
                    TempData["td_serverMessage"] = "已更新密碼";
                    //return View(emp);
                    //return RedirectToAction("Details", new { id });
                    return RedirectToAction("Info");
                }
                catch (DbUpdateConcurrencyException)
                {
                    TempData["td_serverWarning"] = "DbUpdateConcurrencyException";
                    return View();
                }
                catch
                {
                    TempData["td_serverWarning"] = "Exception";
                    return View();
                }
            }

            TempData["td_serverWarning"] = "Unknown problem";
            return View();
        }

        //::api for 3C
        [HttpPost]
        public async Task<IActionResult> CheckOldPassword(string id, string OldPassword)
        {
            string Result = "", Detail = "";

            //string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            //if (UserLevel != "2B" && UserLevel != "1A")
            //{
            //    Result = "Fail"; Detail = "Access Deny";
            //    return Json(new { Result, Detail });
            //}

            if (OldPassword == null || OldPassword.Length <= 3)
            {
                Result = "Fail"; Detail = "PWD is empty or wrong";
                return Json(new { Result, Detail });
            }

            var customer = await _context.Customer.Where(c => c.CustomerId == id).FirstOrDefaultAsync();
            if (customer == null)
            {
                Result = "Fail"; Detail = "No matched data";
                return Json(new { Result, Detail });
            }

            //::retry over 3 times
            int retryCount = 0; //from session
            string sRtryCount = _ms.GetMySession("retryCount", HttpContext.Session);
            if (sRtryCount == null)
                retryCount = 0;
            else
                retryCount = int.Parse(sRtryCount);

            string OldPassword_Encoded = EmployeesController.PasswordSalt(OldPassword, customer.Salt);

            if (customer.PasswordEncoded == OldPassword_Encoded)
            {
                Result = "PASS"; Detail = "";

                //::reset session
                HttpContext.Session.Remove("retryCount");
                retryCount = 0;
                return Json(new { Result, Detail, retryCount });
            }

            //::loose rule, retry count only accumulate if db value is mismatched
            if (retryCount >= 3)
            {
                Result = "Fail"; Detail = "Halt! Retry over 3 times";
                return Json(new { Result, Detail, retryCount });
            }
            retryCount++;
            _ms.SetMySession("retryCount", retryCount.ToString(), HttpContext.Session);

            Result = "Fail"; Detail = "pwd is mismatched";
            return Json(new { Result, Detail, retryCount });
        }
    }
}
