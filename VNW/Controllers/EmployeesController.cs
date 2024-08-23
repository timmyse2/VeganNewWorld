using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VNW.Models;

using System.Security.Cryptography; //for hash password
using System.Text; //for encoding
using System.Data.SqlClient; //::for sql
using System.Diagnostics; //::for debug
using Microsoft.Extensions.Configuration; //for IConfiguration
using VNW.Controllers;

namespace VNW.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly VeganNewWorldContext _context;
        private readonly IConfiguration _config;
        //::set session common interface
        VNW.Common.MySession _ms = new Common.MySession();

        public EmployeesController(VeganNewWorldContext context, IConfiguration config)
        {
            _context = context;
            _config = config;   //:: for config c14
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*或商家員工*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }
            ViewBag.ShopAccount = _ms.GetMySession("ShopAccount", HttpContext.Session);
            //return View(await _context.Employees.ToListAsync());

            var emps = await _context.Employees
                //.Include(x => x.ReportsToNavigation) //try 
                //.Include(e => e.InverseReportsToNavigation.Where(x => x.ReportsTo == e.Id))
                .Take(30)
                .ToListAsync();

            foreach(var e in emps)
            {
                if(e.ReportsTo != null)
                {
                    var e2 = emps.Where(x => x.Id == e.ReportsTo).First();
                    if (e2 != null)
                        e.ReportsToNavigation = e2;
                }
            }

            return View(emps);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*或商家員工*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }

            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.ShopAccount = _ms.GetMySession("ShopAccount", HttpContext.Session);

            //::set NP
            if (employee.ReportsTo != null)
            {
                var leader = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == employee.ReportsTo);
                if (leader != null)
                    employee.ReportsToNavigation = leader;
            }

            var followers = await _context.Employees
                .Where(e => e.ReportsTo == employee.Id)
                .ToListAsync();
            if(followers != null)
            {
                employee.InverseReportsToNavigation = followers;
                //foreach(var f2 in followers)
                //{
                //    if(f2.ReportsTo != null)
                //    {
                //        //f2.InverseReportsToNavigation
                //    }
                //}
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,PasswordEncoded,Name,Title,Extension,ReportsTo,PhotoPath")] Employee employee)
        {
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*或商家員工*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }

            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*或商家員工*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }
            if (id == null)
            {
                return NotFound();
            }            

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            //::
            ViewBag.ShopAccount = _ms.GetMySession("ShopAccount", HttpContext.Session);
            if (UserLevel == "2B" & ViewBag.ShopAccount != employee.Email )
            {
                return Content("You have no right to update other employee's info");
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,PasswordEncoded,Name,Title,Extension,ReportsTo,PhotoPath,Salt")] Employee employee)
        {
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*或商家員工*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }
            if (id != employee.Id)
            {
                //return NotFound();
                return Content("Your account is not match!");
            }
            //::
            ViewBag.ShopAccount = _ms.GetMySession("ShopAccount", HttpContext.Session);
            if (UserLevel == "2B" & ViewBag.ShopAccount != employee.Email)
            {
                return Content("You have no right to update other employee's info");
            }

            #region keep origin password
            //:: do not let password show in view or frontend
            if (employee.PasswordEncoded == null || employee.PasswordEncoded == "")
            {
                var employee_original = await _context.Employees.Where(e => e.Id == id)
                    .AsNoTracking().FirstOrDefaultAsync();
                if(employee_original == null)
                {
                    return Content("can not find origin data");
                }
                employee.PasswordEncoded = employee_original.PasswordEncoded;
                employee.Salt = employee_original.Salt;
                //return Content("Notice: password is miss!");
            }
            #endregion


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            return View(employee);
        }


        public async Task<IActionResult> EditPassword(int? id)
        {
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*或商家員工*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }

            if(false)
            {
                int retryCount = 0;
                string sRtryCount = _ms.GetMySession("retryCount", HttpContext.Session);
                if (sRtryCount == null)
                    retryCount = 0;
                else
                    retryCount = int.Parse(sRtryCount);
                if (retryCount >= 3)
                {
                    TempData["td_serverWarning"] = "舊密碼驗證不過，待10~15鐘後才可再試!";
                    return View();
                }
            }

            var emp = await _context.Employees.Where(e => e.Id  == id).FirstOrDefaultAsync();
            if (emp == null)
            {
                return Content("no matched data");
            }

            string Captcha = VNW.Controllers.CustomersController.GenerateCaptcha();
            _ms.SetMySession("Captcha", Captcha, HttpContext.Session);
            Captcha = VNW.Controllers.CustomersController.EncodeCaptcha(Captcha);
            ViewData["Captcha"] = Captcha;
            ViewData["currentPath"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";
            return View(emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //?
        public async Task<IActionResult> EditPassword(int? id, string OldPassword, string NewPassword, string NewPassword_Confirm, string Captcha)
        {
            var emp = await _context.Employees.Where(e => e.Id == id).FirstOrDefaultAsync();

            string Captcha_expected = _ms.GetMySession("Captcha", HttpContext.Session);            
            ViewData["Captcha"] = VNW.Controllers.CustomersController.EncodeCaptcha(Captcha_expected);
            if (emp == null)
            {
                //return Content("no matched data");
                TempData["td_serverWarning"] = "找不到相符的資料!";
                return View();
            }

            //::check pwd length, format...
            if (OldPassword == null || OldPassword.Length > 20)
            {
                TempData["td_serverWarning"] = "舊密碼未輸入或格式不合";
                return View(emp);
            }
            //::compare password
            //string secretKey = "vnw2024";
            //HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey));
            //string OldPassword_Encoded =
            //  Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(OldPassword)));
            string OldPassword_Encoded = PasswordSalt(OldPassword, emp.Salt);
            
            if (emp.PasswordEncoded != OldPassword_Encoded)
            {
                //error
                TempData["td_serverWarning"] = "舊密碼不符合";
                return View(emp);
            }

            if (NewPassword == null || NewPassword.Length < 5 || NewPassword.Length > 20)
            {
                TempData["td_serverWarning"] = "新密碼長度過短、長";
                return View(emp);
            }

            //:: 0~9, a~z, A~Z 
            string pattern = "^(?=.*[0-9])(?=.*[A-Za-z]).{5,20}$";
            System.Text.RegularExpressions.Regex regex
                = new System.Text.RegularExpressions.Regex(pattern);
            if (!regex.IsMatch(NewPassword))
            {
                TempData["td_serverWarning"] = "新密碼格式不符";
                return View(emp);
            }

            if (NewPassword != NewPassword_Confirm)
            {
                //error
                TempData["td_serverWarning"] = "新密碼與新密碼確認不一致";
                return View(emp);
            }

            //::udpate new Salt, 10 chars randomly
            emp.Salt = GenerateSalt();
            //string NewPassword_Encoded =
            //  Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(NewPassword)));
            string NewPassword_Encoded = PasswordSalt(NewPassword, emp.Salt);

            if (Captcha == null || Captcha.Length != 4)
            {
                TempData["td_serverWarning"] = "驗證碼長度錯誤";
                return View(emp);
            }
            if (Captcha != Captcha_expected)
            {
                //error
                TempData["td_serverWarning"] = "驗證碼不符";
                return View(emp);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //update pwd in DB
                    emp.PasswordEncoded = NewPassword_Encoded;
                    _context.Update(emp);
                    await _context.SaveChangesAsync();                    

                    //pass case
                    TempData["td_serverWarning"] = "";
                    TempData["td_serverMessage"] = "已更新密碼";
                    //return View(emp);
                    return RedirectToAction("Details", new { id });
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

        //::generate 10 chars randomly
        public static string GenerateSalt()
        {
            string res = ""; // "0987654321";
            var rand = new Random();
            for (int i = 0; i <= 9; i++)
            {
                res += rand.Next(0, 9);
            }
            return res;
        }

        //::api for 2B
        [HttpPost]
        public async Task<IActionResult> CheckOldPassword(int? id, string OldPassword)
        {
            string Result = "", Detail ="";

            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                Result = "Fail"; Detail = "Access Deny";
                return Json(new { Result, Detail });
            }

            if (OldPassword == null || OldPassword.Length <= 3)
            {
                Result = "Fail"; Detail = "PWD is empty or wrong";
                return Json(new { Result, Detail});
            }

            var emp = await _context.Employees.Where(e => e.Id == id).FirstOrDefaultAsync();
            if (emp == null)
            {
                Result = "Fail"; Detail = "No matched data";
                return Json(new { Result, Detail});
            }

            //::retry over 3 times
            int retryCount = 0; //from session
            string sRtryCount = _ms.GetMySession("retryCount", HttpContext.Session);
            if (sRtryCount == null)
                retryCount = 0;
            else
                retryCount = int.Parse(sRtryCount);

            //string secretKey = "vnw2024";
            //HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey));
            //string OldPassword_Encoded =
            //  Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(OldPassword)));
            string OldPassword_Encoded = PasswordSalt(OldPassword, emp.Salt);

            if (emp.PasswordEncoded == OldPassword_Encoded)
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

        //:: static function method - HMACSHA256 with Salt
        public static string PasswordSalt(string password, string salt)
        {
            string PasswordEncoded = "";
            //fail case
            if(salt == null | password == null)
            {
                return null;
            }
            string secretKey = "vnw2024";
            HMACSHA256 hmac2 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            PasswordEncoded = password + salt;
            //PasswordEncoded = BitConverter.ToString(
            //  hmac2.ComputeHash(Encoding.UTF8.GetBytes(PasswordEncoded))); //.Replace("-", string.Empty);               
            PasswordEncoded = Convert.ToBase64String(
                hmac2.ComputeHash(Encoding.UTF8.GetBytes(PasswordEncoded)));
            return PasswordEncoded;            
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                TempData["td_server"] = "該頁面只有管理者*可使用，請先登入";
                return RedirectToAction("Login", "Customers");
            }
            var employee = await _context.Employees.FindAsync(id);
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> ShopLogin(string account, string password, string pin, string role)
        {
            string ShopAccount = "";
            string result = "", detail = "";
            int errorCode = 0;

            await Task.Run(() =>
            {

                string Captcha = _ms.GetMySession("Captcha", HttpContext.Session);
                if (Captcha == null)
                    Captcha = "null";

                if (account == "" || password == "")
                {
                    //fail case
                    result = "fail";
                    detail = "lost some parameters";                    
                    ShopAccount = "unknown";
                    errorCode = 101;
                }
                else if (pin != Captcha)
                {
                    errorCode = 103;
                    result = "fail";
                    detail = "captcha is mismatch";
                }
                else
                {
                    //::DB access, find account
                    var employee = _context.Employees
                        .Where(e => e.Email == account).FirstOrDefault(); ;

                    //::find matched account
                    if (employee != null)
                    {
                        #region hash pwd
                        //string secretKey = "vnw2024";
                        //HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey));

                        //byte[] Pwd_Encoded = hmac.ComputeHash(Encoding.UTF8.GetBytes(Pwd_Original));
                        //Debug.WriteLine("key: " + secretKey);
                        //Debug.Write("hmac key: ");
                        //foreach(var b in hmac.Key)                        
                        //    Debug.Write(" " + b);
                        //Debug.WriteLine("");
                        //Debug.WriteLine("Pwd_Original: " + Pwd_Original);
                        /*
                        Debug.WriteLine("Pwd_Encoded: " + Convert.ToBase64String(Pwd_Encoded));
                        Debug.Write("Pwd_Encoded: " );
                        foreach (var b in Pwd_Encoded)
                            Debug.Write(" " + b);
                        Debug.WriteLine("");
                        Debug.WriteLine("Pwd_Encoded String: " + BitConverter.ToString(Pwd_Encoded));
                        Debug.WriteLine("Pwd_Encoded String: " + BitConverter.ToString(Pwd_Encoded).Replace("-", string.Empty));                        
                        */
                        string expectPasswordEncoded
                            = employee.PasswordEncoded;
                        //= "TEaDO6tjVVcTcUNZ0pAMkuLMDV8=";
                        //Convert.ToBase64String(Pwd_Encoded);

                        //Debug.WriteLine("Pwd_Encoded Base64Bit: " + expectPasswordEncoded);
                        //string inputPasswordEncoded =
                        //  Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

                        if (employee.Salt == null)
                            employee.Salt = "";
                        string inputPasswordEncoded = //VNW.Controllers.
                            EmployeesController.PasswordSalt(password, employee.Salt);

                        #endregion
                        //::pwd decode
                        //if (password == Pwd_Decoded)
                        if (inputPasswordEncoded == expectPasswordEncoded)
                        {
                            //::pass case
                            errorCode = 0;
                            ShopAccount = account;
                            _ms.SetMySession("IsUserLogin", "NO", HttpContext.Session);
                            _ms.SetMySession("UserLevel", "2B", HttpContext.Session);
                            //_ms.SetMySession("UserAccount", "Illyasviel@Einzbern2017", HttpContext.Session);
                            _ms.SetMySession("UserAccount", ShopAccount, HttpContext.Session);
                            _ms.SetMySession("ShopAccount", ShopAccount, HttpContext.Session);
                            _ms.SetMySession("EmployeeId", employee.Id.ToString(), HttpContext.Session);

                            result = "PASS";
                            detail = "vender login at " + DateTime.Now;
                            //ShopAccount = "wolf2024@vwn.tw";                            
                        }
                        else
                        {
                            result = "fail";
                            detail = "password is mismatched";
                            ShopAccount = "";
                            errorCode = 104;
                        }
                    }
                    else
                    {
                        result = "fail";
                        detail = "There is no matched user";
                        ShopAccount = account;
                        errorCode = 102;
                    }
                }
            });
            //return Json(new { result = "PASS", detail = "shop side login at " + DateTime.Now, shopAccount = ShopAccount });
            return Json(new { result, detail, shopAccount = ShopAccount, errorCode });
        }

        public IActionResult SqlTest()
        {
            string connectionString = "";
            //connectionString = "Server=.\\SQLEXPRESS;Database=DB_VeganNewWorld;Trusted_Connection=True;MultipleActiveResultSets=true";
            connectionString = _config.GetConnectionString("VeganNewWorldContext");
            //Microsoft.Extensions.Configuration
            //  GetConnectionString("VeganNewWorldContext");

            string stemp = "";
            int rowCount = 0;

            string query = "SELECT TOP 100 [Id], [Name], [Email] FROM Employees";
            ViewBag.queryString = query;
            //ViewBag.connectionString = connectionString; //debug only

            List<Employee> emps = new List<Employee>();           

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // 在這裡執行 T-SQL 查詢
                
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int FC = reader.FieldCount;
                        stemp += "<table class=\"table table-bordered\">";

                        stemp += "<thead><tr>";
                        for (int i = 0; i < FC; i++)
                        {
                            string s = reader.GetName(i);
                            Debug.Write(s + " \t\t\t");
                            stemp += "<td>" + s + "</td>";
                        }
                        stemp += "</tr></thead>";
                        while (reader.Read())
                        {
                            stemp += "<tr>";
                            for (int i = 0; i < FC; i++)
                            {
                                string s = "";
                                if (reader[i] != DBNull.Value)
                                    s = reader[i].ToString();
                                else
                                    s = "...";

                                Debug.Write(s + " \t\t\t");
                                stemp += "<td>" + s + "</td>";
                            }
                            stemp += "</tr>";

                            Employee emp = new Employee(){
                                Id = (int)reader[0],
                                Name = reader["Name"].ToString(),                                
                            };
                            //emp.Id = (int) reader[0];
                            //emp.Name = reader["Name"].ToString();
                            emps.Add(emp);

                            rowCount++;
                        }
                        stemp += "</table>";
                    }
                }
                connection.Close();
            }
            //return Content(" " + stemp);
            
            ViewBag.stemp = stemp;
            return View(emps);
        }

    }
}
