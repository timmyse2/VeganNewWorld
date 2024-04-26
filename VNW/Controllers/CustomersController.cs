using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VNW.Models;
using System.Diagnostics;

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
            //VNW.Common.MySession ms = new Common.MySession();
            //Debug.WriteLine(" my common test" + ms.Test("123"));
            //_ms.SetMySession("ms_test", "1979", HttpContext.Session);
            //Debug.WriteLine(" my common test" + _ms.GetMySession("ms_test", HttpContext.Session));
            //ms.Dispose();

            ViewData["UserAccount"] = HttpContext.Request.Cookies["UserAccount"];
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string account, string password, string pin, string role)
        {
            //::precheck

            if (account == null)
            {
                //return NotFound();
                //return Content("Account Id is null");
                return Json(new { result = "FAIL", detail = "Id is null" });
            }

            //::check pin

            //::check account id and password
            var customer = await _context.Customer
                .FirstOrDefaultAsync(m => m.CustomerId == account);
            if (customer == null)
            {
                //return NotFound();
                //return Content("No matched data");
                return Json(new { result = "FAIL", detail = "no matched data" });
            }
            else
            {
                //::<NOTICE: upper case and lower case><FIXED for ISSUE >
                if(account != customer.CustomerId)
                {
                    //account = customer.CustomerId;
                    return Json(new { result = "NG", detail = "upper case or lower case is mismatched" });
                }
                //<><><>

                ViewData["IsUserLogin"] = "YES";
                //ViewData["IsAdmin"] = "YES";
                //ViewData["IsVenderLogin"] = "YES";
                HttpContext.Response.Cookies.Append("UserAccount", customer.CustomerId);
                //SetMySession("IsAdmin", "YES");   
                _ms.SetMySession("IsUserLogin", "YES", HttpContext.Session);
                _ms.SetMySession("UserAccount", customer.CustomerId, HttpContext.Session);

                return Json(new { result = "PASS", detail = "matched" });
            }
            //::pass case
            //return View();
            return Content("End of Login");            
        }

        public async Task<IActionResult> Logout()
        {
            //_ms.SetMySession("IsUserLogin", "", HttpContext.Session);
            //_ms.SetMySession("UserAccount", "", HttpContext.Session);
            HttpContext.Session.Remove("IsUserLogin");
            HttpContext.Session.Remove("UserAccount");                       

            TempData["td_serverMessage"] = "已登出"; //::
            return RedirectToAction("Login");
            //return Content("LOGOUT");

        }
    }
}
