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
    public class OrdersController : Controller
    {
        private readonly VeganNewWorldContext _context;
        //::set session common interface
        VNW.Common.MySession _ms = new Common.MySession();

        public OrdersController(VeganNewWorldContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            //::check admin
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "1A")
            {
                return Content("You have no right to access this");
            }

            if (!LoginPrecheck())
                return RedirectToAction("Login", "Customers");

            var veganNewWorldContext = _context.Orders.Include(o => o.Customer);
            return View(await veganNewWorldContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            //::check admin
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "1A")
            {
                return Content("You have no right to access this");
            }

            if (id == null)
            {
                //return NotFound();
                TempData["td_server"] = "缺少編號";
                return RedirectToAction("OrderList");
            }

            if (!LoginPrecheck())
                return RedirectToAction("Login", "Customers");

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o=> o.OrderDetails) //::try to preload OD
                //.Include(o => o.product) //::can not to load here!
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                //return NotFound();
                TempData["td_server"] = "找不到相符的資料";
                return RedirectToAction("OrderList");
            }

            //::check user id
            string UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
            if (order.CustomerId != UserAccount)
            {
                TempData["td_server"] = "您無權查看他人的訂單";
                //return Content("You have no right to access this order");
                return RedirectToAction("OrderList");
            }


            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            //::check admin
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "1A")
            {
                return Content("You have no right to access this");
            }

            if (!LoginPrecheck())
                return RedirectToAction("Login", "Customers");

            //ViewData["CustomerId"] =
            //    new SelectList(_context.Set<Customer>(),
            //    "CustomerId",
            //    "CustomerId");
            //"ContactName");

            //::use custom list
            var members = _context.Customer
                .ToList();
            List<SelectListItem> members_Sorted = new List<SelectListItem>();
            foreach (var ms in members)
            {
                members_Sorted.Add(new SelectListItem
                {
                    Text = ms.CompanyName + " (" + ms.ContactName + ")", 
                    Value = ms.CustomerId
                });
            }
            ViewData["CustomerId_Sorted"] = members_Sorted;

            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,CustomerId,OrderDate,RequiredDate,ShippedDate,ShipVia,Freight,ShipName,ShipAddress,ShipCity,ShipPostalCode,ShipCountry")] Order order)
        {
            if (!LoginPrecheck())
                return RedirectToAction("Login", "Customers");

            if (ModelState.IsValid)
            {

                if (order.OrderDate == null)
                {
                    order.OrderDate = DateTime.Now;
                }

                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Set<Customer>(), "CustomerId", "CustomerId", order.CustomerId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //::check admin
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "1A")
            {
                return Content("You have no right to access this");
            }

            if (!LoginPrecheck())
                return RedirectToAction("Login", "Customers");

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Set<Customer>(), "CustomerId", "CustomerId", order.CustomerId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerId,OrderDate,RequiredDate,ShippedDate,ShipVia,Freight,ShipName,ShipAddress,ShipCity,ShipPostalCode,ShipCountry")] Order order)
        {
            //::check admin
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "1A")
            {
                return Content("You have no right to access this");
            }

            if (!LoginPrecheck())
                return RedirectToAction("Login", "Customers");

            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["CustomerId"] = new SelectList(_context.Set<Customer>(), "CustomerId", "CustomerId", order.CustomerId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //::check admin
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "1A")
            {
                return Content("You have no right to access this");
            }

            if (!LoginPrecheck())
                return RedirectToAction("Login", "Customers");

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!LoginPrecheck())
                return RedirectToAction("Login", "Customers");

            var order = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }

        //::for end user
        public async Task<IActionResult> OrderList()
        {
            if (!LoginPrecheck())
                return RedirectToAction("Login", "Customers");

            //::User ID
            string Userid = _ms.GetMySession("UserAccount", HttpContext.Session);

            var veganNewWorldContext = _context.Orders
                .Where(o=> o.CustomerId == Userid) //sorted
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)  //just try !!!!
                .OrderByDescending(o=>o.OrderId)                
                ;

            if (veganNewWorldContext == null)
            {
                return Content("null");
            }
            return View(await veganNewWorldContext.ToListAsync());
        }

        public bool LoginPrecheck()
        {
            string UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
            string IsUserLogin = _ms.GetMySession("IsUserLogin", HttpContext.Session);
            ViewBag.UserAccount = UserAccount;
            if (UserAccount == null || UserAccount == "" || IsUserLogin == "" || IsUserLogin == null)
            {
                return false; 
                //return Content("請先登入");
            }
            return true;           
        }

        //:: for End-user
        public async Task<IActionResult> NewOrder()
        {
            //if (!LoginPrecheck())
            if (!_ms.LoginPrecheck(HttpContext.Session))
                    return RedirectToAction("Login", "Customers");

            //:: Get customer Id, Name, Info {address}
            string UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
            Models.Customer member = await _context.Customer
                .Where(x => x.CustomerId == UserAccount)                
                .FirstOrDefaultAsync()
                ;

            if (member == null)
            {
                //error case: tbd
                TempData["td_server"] = "發生未知問題於存取建立資料時";
                return RedirectToAction(nameof(OrderList));
            }
            else
            {
                ViewBag.UserAccount = UserAccount; //
                ViewData["member"] = member;
            }
            return View();
        }

        //::for end user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewOrder([Bind(
            "OrderId,CustomerId,OrderDate,RequiredDate,ShippedDate,ShipVia,Freight,ShipName,ShipAddress,ShipCity,ShipPostalCode,ShipCountry"
            )] Order order)
        {

            if(!_ms.LoginPrecheck(HttpContext.Session))
            //if (!LoginPrecheck())
                return RedirectToAction("Login", "Customers");

            if (ModelState.IsValid)
            {

                if (order.CustomerId == null)
                {
                    //::error case
                    TempData["td_server"] = "發生問題, 資料未更新";
                    return RedirectToAction(nameof(OrderList));
                    //return View(order);
                }

                if (order.OrderDate == null)
                {
                    order.OrderDate = DateTime.Now;
                }

                _context.Add(order);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                TempData["td_server"] = "已建立資料";
                return RedirectToAction(nameof(OrderList));

            }

            //ViewData["CustomerId"] = new SelectList(_context.Set<Customer>(), "CustomerId", "CustomerId", order.CustomerId);
            //TBC

            return View(order);
        }


    }
}
