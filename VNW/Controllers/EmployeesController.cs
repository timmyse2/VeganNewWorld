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

namespace VNW.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly VeganNewWorldContext _context;
        //::set session common interface
        VNW.Common.MySession _ms = new Common.MySession();

        public EmployeesController(VeganNewWorldContext context)
        {
            _context = context;
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
                .Take(10)
                .ToListAsync();

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
                .Where(m => m.ReportsTo == employee.Id)
                .ToListAsync();
            if(followers != null)
            {
                employee.InverseReportsToNavigation = followers;
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,PasswordEncoded,Name,Title,Extension,ReportsTo,PhotoPath")] Employee employee)
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

            var emp = await _context.Employees.Where(e => e.Id  == id).FirstOrDefaultAsync();
            if (emp == null)
            {
                return Content("no matched data");
            }
            return View(emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //?
        public async Task<IActionResult> EditPassword(int? id, string OldPassword, string NewPassword, string NewPassword_Confirm, string Captcha)
        {
            var emp = await _context.Employees.Where(e => e.Id == id).FirstOrDefaultAsync();
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
            string OldPassword_Encoded = PasswordSalt(OldPassword, "1234567890");

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
            if(false)
            {
                //:: 0~9, a~z, A~Z 
                // '^(?=.*[0-9])(?=.*[A-Za-z]).{5,20}$'
                TempData["td_serverWarning"] = "新密碼格式不符";
                return View(emp);
            }

            if (NewPassword != NewPassword_Confirm)
            {
                //error
                TempData["td_serverWarning"] = "新密碼與新密碼確認不一致";
                return View(emp);
            }

            //string NewPassword_Encoded =
            //  Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(NewPassword)));
            string NewPassword_Encoded = PasswordSalt(NewPassword, "1234567890");

            if (Captcha == null || Captcha.Length != 4)
            {
                TempData["td_serverWarning"] = "驗證碼長度錯誤";
                return View(emp);
            }
            if (Captcha != "1314")
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

        //::api for 2B
        //[HttpPost]
        public async Task<IActionResult> CheckOldPassword(int? id, string OldPassword)
        {
            string Result = "", Detail ="";
            
            //::try try
            //string saltTestRes = PasswordSalt(OldPassword, "1234567890");
            //return Content(saltTestRes);

            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                Result = "Fail"; Detail = "Access Deny";
                return Json(new { Result, Detail });
            }

            //::retry over 3 times
            int retryCount = 0; //from session
            string sRtryCount = _ms.GetMySession("retryCount", HttpContext.Session);
            if (sRtryCount == null)            
                retryCount = 0;            
            else            
                retryCount = int.Parse(sRtryCount);
            if (retryCount >= 3)
            {
                Result = "Fail"; Detail = "Halt! Retry over 3 times";                
                return Json(new { Result, Detail, retryCount });
            }
            retryCount++;
            _ms.SetMySession("retryCount", retryCount.ToString(), HttpContext.Session);

            if (OldPassword == null || OldPassword.Length <= 3)
            {
                Result = "Fail"; Detail = "PWD is empty or wrong";
                return Json(new { Result, Detail, retryCount });
            }

            var emp = await _context.Employees.Where(e => e.Id == id).FirstOrDefaultAsync();
            if (emp == null)
            {
                Result = "Fail"; Detail = "No matched data";
                return Json(new { Result, Detail, retryCount });
            }

            //string secretKey = "vnw2024";
            //HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey));
            //string OldPassword_Encoded =
            //  Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(OldPassword)));
            string OldPassword_Encoded = PasswordSalt(OldPassword, "1234567890");

            if (emp.PasswordEncoded == OldPassword_Encoded)
            {
                Result = "PASS"; Detail = "";

                //::reset session
                HttpContext.Session.Remove("retryCount");
                retryCount = 0;

                return Json(new { Result, Detail, retryCount });
            }

            Result = "Fail"; Detail = "pwd is mismatched";
            return Json(new { Result, Detail, retryCount });
        }

        //function
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
    }
}
