using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VNW.Models;

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
            var emp = await _context.Employees.Where(e => e.Id  == id).FirstOrDefaultAsync();
            if (emp == null)
            {
                return Content("no matched data");
            }
            return View(emp);
        }

        [HttpPost]
        public async Task<IActionResult> EditPassword(int? id, string OldPassword, string NewPassword)
        {
            return View();
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
