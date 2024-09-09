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
    public class OrderDetailsController : Controller
    {
        private readonly VeganNewWorldContext _context;
        //::set session common interface
        VNW.Common.MySession _ms = new Common.MySession();

        public OrderDetailsController(VeganNewWorldContext context)
        {
            _context = context;
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index()
        {
            //::check admin
            if (!_ms.CheckAdmin(HttpContext.Session))
                return Content("You have no right to access this page");

            var veganNewWorldContext = _context.OrderDetails.Include(o => o.Order).Include(o => o.Product)
                .OrderByDescending(x=>x.OrderId);
            return View(await veganNewWorldContext.ToListAsync());
        }

        // GET: OrderDetails/Details/5
        //public async Task<IActionResult> Details(int? id)
        public async Task<IActionResult> Details(int? oid, int? pid)
        {
            //::check admin
            if (!_ms.CheckAdmin(HttpContext.Session))
                return Content("You have no right to access this page");

            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            //if (id == null)
            if (pid == null || oid == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderId == oid
                       && m.ProductId == pid
                );
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // GET: OrderDetails/Create
        public async Task<IActionResult> Create()
        {
            //::check admin
            if (!_ms.CheckAdmin(HttpContext.Session))
                return Content("You have no right to access this page");

            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId");
            //ViewData["ProductId"] = new SelectList(_context.Products, "ProductId",
            //  "ProductName");
            //"ProductId");

            #region product data sort            
            //#:: find data from products where 'Discontinued' Flag is not CHEKED
            var prod = await _context.Products
                .Where(p=>p.Discontinued == false)
                //.Select(p=>p.ProductName)                
                .Select(x => new {x.ProductId, x.ProductName, x.UnitPrice })
                .ToListAsync()
                ;
            List<SelectListItem> ProdcutList_Sorted = new List<SelectListItem>();
            foreach (var pi in prod)
            {
                //System.Diagnostics.Debug.WriteLine(" " + pi.ProductName);
                //System.Diagnostics.Debug.WriteLine(" " + pi);
                ProdcutList_Sorted.Add(new SelectListItem {
                    Text = "#" + pi.ProductId + " " + pi.ProductName 
                    , Value = pi.ProductId.ToString()                    
                });
            }
            //System.Diagnostics.Debug.WriteLine(" " + prod.Count());
            //ViewData["ProdcutSorted"] = Newtonsoft.Json.JsonConvert.SerializeObject(prod);
            //ViewData["ProdcutSorted"] = prod;
            ViewData["ProdcutList_Sorted"] = ProdcutList_Sorted;
            #endregion

            return View();
        }

        // POST: OrderDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,ProductId,UnitPrice,Quantity,Discount")] OrderDetail orderDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderDetail);
                await _context.SaveChangesAsync();
                TempData["td_serverMessage"] = "Created";
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            //ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", orderDetail.ProductId);
            ////"ProductId", orderDetail.ProductId);

            //#:: find data from products where 'Discontinued' Flag is not CHEKED
            #region            
            var prod = await _context.Products
                .Where(p => p.Discontinued == false)            
                .Select(x => new { x.ProductId, x.ProductName })
                //.ToList()
                .ToListAsync()
                ;
            List<SelectListItem> ProdcutList_Sorted = new List<SelectListItem>();
            //bool isSelect = false; //default
            foreach (var pi in prod)
            {
                //if (orderDetail.ProductId == pi.ProductId)
                //    isSelect = true;
                //else
                //    isSelect = false;
                ProdcutList_Sorted.Add(new SelectListItem
                {
                    Text = "#" + pi.ProductId + " " + pi.ProductName
                    ,Value = pi.ProductId.ToString(),
                    //Selected = isSelect
                    Selected = (orderDetail.ProductId == pi.ProductId)
                });
            }

            ViewData["ProdcutList_Sorted"] = ProdcutList_Sorted;
            #endregion
            return View(orderDetail);
        }

        // GET: OrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? pid, int? oid)
        {
            //::check admin
            if (!_ms.CheckAdmin(HttpContext.Session))
                return Content("You have no right to access this page");

            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            if (pid == null || oid == null)
            {
                return NotFound();
            }

            //var orderDetail = await _context.OrderDetails.FindAsync(pid);
            var orderDetail = await _context.OrderDetails
                //.Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderId == oid
                       && m.ProductId == pid
                );

            if (orderDetail == null)
            {
                return NotFound();
            }
            ////ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            //ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", 
            //    "OrderId", orderDetail.OrderId);

            //ViewData["ProductId"] = new SelectList(_context.Products, "ProductId",
            //    "ProductName", orderDetail.ProductId);
            //    //"ProductId", orderDetail.ProductId);
            return View(orderDetail);
        }

        // POST: OrderDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int OrderId, [Bind("OrderId,ProductId,UnitPrice,Quantity,Discount,RowVersion")] OrderDetail orderDetail)
        {
            //if (id != orderDetail.OrderId)
            if (OrderId != orderDetail.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderDetailExists(orderDetail.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["td_serverMessage"] = "Updated";
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", orderDetail.ProductId);
            return View(orderDetail);
        }

        // GET: OrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //::check admin
            if (!_ms.CheckAdmin(HttpContext.Session))
                return Content("You have no right to access this page");

            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();
            TempData["td_serverMessage"] = "Deleted";
            return RedirectToAction(nameof(Index));
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails.Any(e => e.OrderId == id);
        }

        //public bool LoginPrecheck()
        //{
        //    string UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
        //    string IsUserLogin = _ms.GetMySession("IsUserLogin", HttpContext.Session);

        //    ////::check admin

        //    ViewBag.UserAccount = UserAccount;
        //    if (UserAccount == null || UserAccount == "" || IsUserLogin == "" || IsUserLogin == null)
        //    {
        //        return false;
        //        //return Content("請先登入");
        //    }
        //    return true;
        //}

        //::Detail for end user (product)
        public async Task<IActionResult> DetailList(int? oid)
        {

            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            if(oid == null)
            {
                TempData["td_serverMessage"] = "訂單編號是空的";
            }

            #region ::check order's customer id first
            string UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
            var preCheckOrder = await _context.Orders
                .Where(o => o.CustomerId == UserAccount && o.OrderId == oid) //sorted 
                .Include(x=>x.Customer) //try to include more
                //.Select(o => new {o.CustomerId, o.OrderId } ) //reduce data
                //.FirstOrDefault()
                .FirstOrDefaultAsync()
                ;
            if (preCheckOrder == null) //This is not your order
            {
                TempData["td_serverMessage"] = "這不是您的訂單!";
                return View(null);
            }
            ViewData["preCheckOrder"] = preCheckOrder;

            #endregion

            var ods = _context.OrderDetails
                .Where(d => d.OrderId == oid) //
                //.Include(o => o.Order) //try
                .Include(p => p.Product)
                ;

            if (ods == null) //
            {
                TempData["td_serverMessage"] = "data is null";
            }

            var res = await ods.ToListAsync();
            if (res == null) //
            {
                TempData["td_serverMessage"] = "data is null";
            }            

            return View(res);
            ////return Json(res);
        }

        //::for 2B Shop
        public async Task<IActionResult> OrderDetailsForShop(int? id)
        {
            //::check Shop
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                //return Content("You have no right to access this");
                return RedirectToAction("Login", "Customers");
            }
            //if (!_ms.LoginPrecheck(HttpContext.Session))
            //    return RedirectToAction("Login", "Customers");

            //::Shop employee account check
            if(id == null)
            {
                TempData["td_serverWarning"] += "data is null;";
                return View();
            }                

            int oid = (int)id;

            #region ::check order's customer id first
            //string UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
            string ShopAccount = _ms.GetMySession("ShopAccount", HttpContext.Session);
            ViewData["ShopAccount"] = ShopAccount;
			ViewData["UserIcon"] = _ms.GetMySession("UserIcon", HttpContext.Session);
            var preCheckOrder = await _context.Orders                
                .Where(o => o.OrderId == oid) //sorted 
                .Include(x => x.Customer) 
                .FirstOrDefaultAsync()
                ;
            if (preCheckOrder == null)
            {
                TempData["td_serverWarning"] = "查不到訂單 "+ id;
                return View(null);
            }
            //::get E info
            if(preCheckOrder.EmployeeId != null)
            {
                var emp = await _context.Employees.Where(e => e.Id == preCheckOrder.EmployeeId).FirstOrDefaultAsync();
                if(emp != null) preCheckOrder.Employee = emp;                
            }
            ViewData["preCheckOrder"] = preCheckOrder;
            #endregion

            var ods = _context.OrderDetails
                .Where(d => d.OrderId == oid) //
                //.Include(o => o.Order) //try
                .Include(p => p.Product)
                .OrderBy(od=>od.RowVersion) //try
                ;

            if (ods == null) //
            {
                TempData["td_serverWarning"] += "data is null;";
            }

            var res = await ods.ToListAsync();
            if (res == null) //
            {
                TempData["td_serverWarning"] += "data is null;";
            }

            return View(res);
        }

    }
}
