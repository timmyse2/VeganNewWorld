using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VNW.Models;

using VNW.ViewModels;//
using System.Diagnostics; //for debug
using VNW.Common; //for lib
using Newtonsoft.Json; //for json

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

            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            var veganNewWorldContext = _context.Orders.Include(o => o.Customer).OrderByDescending(x=>x.OrderId);
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

            if (!_ms.LoginPrecheck(HttpContext.Session))
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

            if (!_ms.LoginPrecheck(HttpContext.Session))
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
            if (!_ms.LoginPrecheck(HttpContext.Session))
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

            if (!_ms.LoginPrecheck(HttpContext.Session))
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

            if (!_ms.LoginPrecheck(HttpContext.Session))
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

            if (!_ms.LoginPrecheck(HttpContext.Session))
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
            if (!_ms.LoginPrecheck(HttpContext.Session))
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

        //::UX for end user
        public async Task<IActionResult> OrderList()
        {
            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            //::User ID
            string Userid = _ms.GetMySession("UserAccount", HttpContext.Session);            
            ViewBag.UserAccount = Userid;

            var orders = _context.Orders
                .Where(o=> o.CustomerId == Userid) //sorted
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)  //get count of od
                //.Include(x=>x.prod)
                .OrderByDescending(o=>o.OrderId)                
                ;

            //::page for order - tbd

            if (orders == null)
            {
                return Content("null");
            }

            //try to put image - but this method is not good!
            //:: get image from 1st item only
            foreach (var o in orders)
            {
                foreach (var od in o.OrderDetails)
                {
                    if(od.Product == null)
                    {
                        var qtest = _context.Products
                            .Where(x => x.ProductId == od.ProductId).First();
                        if(qtest!= null)
                            od.Product = qtest;
                    }
                    break; //just 1st one
                }
            }            

            return View(await orders.ToListAsync());
        }

        //public bool LoginPrecheck()
        //{
        //    string UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
        //    string IsUserLogin = _ms.GetMySession("IsUserLogin", HttpContext.Session);
        //    ViewBag.UserAccount = UserAccount;
        //    if (UserAccount == null || UserAccount == "" || IsUserLogin == "" || IsUserLogin == null)
        //    {
        //        return false; 
        //        //return Content("請先登入");
        //    }
        //    return true;           
        //}

        //::NOT for End-user
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

        //::NOT for end user
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
                #region
                //::check last order is opened or not, try to merge od|p in same order id
                #endregion

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


        //::set official data in Orders and OrderDetails
        public async Task<IActionResult> CreateOrderAndDetails()
        {
            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            try
            {
                //::get temporary data of ShoppingCart from cookie
                string pidJSON = null;
                List<ShoppingCart> shoppingCarts = new List<ShoppingCart>();
                pidJSON = HttpContext.Request.Cookies["pidJSON"];
                if (pidJSON == null)
                {
                    TempData["td_serverWarning"] = "訂單是空的，請選擇商品";
                    return View();
                }
                else
                {
                    shoppingCarts = JsonConvert.DeserializeObject<List<VNW.ViewModels.ShoppingCart>>(pidJSON);
                    if (shoppingCarts.Count <= 0)
                    {
                        TempData["td_serverWarning"] = "訂單是空的，請選擇商品";
                        //::error case
                        return View();
                    }
                    else
                    {
                        //TempData["td_serverInfo"] = "取得資料" + shoppingCarts.Count;

                        //::find matched data from DB
                        #region sync stock data 
                        List<int> pids = new List<int>();
                        List<int> pids_issue = new List<int>();
                        foreach (var s in shoppingCarts) //get pid from cookie                        
                            pids.Add(s.Pid);

                        //::find product data from DB
                        var queryP = await _context.Products
                            //.Select(x => new { x.ProductId, x.UnitsInStock })
                            .Where(x => pids.Contains(x.ProductId))
                            .ToListAsync();
                        //.ToList();                            

                        if (queryP != null)
                        {
                            //Debug.WriteLine(" Mathced DB Count: " + queryDB.Count());
                            //put matched stock in cookie
                            foreach (var q in queryP)
                            {
                                Debug.WriteLine("id: " + q.ProductId + ", stock: " + q.UnitsInStock);
                                var sc = shoppingCarts.Where(x => x.Pid == q.ProductId).First();
                                if (sc != null)
                                {
                                    //::check stock is enough     

                                    if (sc.Qty > q.UnitsInStock)
                                    {
                                        //::show warning or error???
                                        pids_issue.Add(sc.Pid);
                                    }

                                    if (sc.Stock != q.UnitsInStock)
                                    {
                                        sc.Stock = (short)q.UnitsInStock;
                                        //::show warning
                                        pids_issue.Add(sc.Pid);
                                    }
                                }
                            }
                            Debug.WriteLine("\n EOD ");

                            if (pids_issue.Count > 0)
                            {
                                //someting is worng?
                                TempData["td_serverWarning"] += " 數量與庫存不合; ";
                            }
                            //pass case

                            //::get info customer 
                            //:: Get customer Id, Name, Info {address}
                            string UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
                            Models.Customer member = await _context.Customer
                                .Where(x => x.CustomerId == UserAccount)
                                .FirstOrDefaultAsync();

                            if (member == null)
                            {
                                TempData["td_serverWarning"] += " 客戶資訊不明; ";
                                return View();
                            }
                            ViewData["member"] = member;

                            int currentOrderId = 0;
                            //:: set Order
                            //  Create New Order or merge to old recordset?
                            Models.Order newOrder = new Order
                            {
                                CustomerId = member.CustomerId,
                                //OrderId = currentOrderId, //auto create in sql server
                                ShipAddress = member.Address,
                                ShipCity = member.City,
                                ShipName = member.CompanyName,
                                ShipCountry = member.Country,
                                ShipPostalCode = member.PostalCode,
                                Freight = 0,
                                ShipVia = 1,
                                OrderDate = DateTime.Now,
                            };
                            ViewData["newOrder"] = newOrder;

                            //CreateOrder(newOrder);
                            _context.Add(newOrder);
                            await _context.SaveChangesAsync();

                            //::get order id, check order
                            currentOrderId = newOrder.OrderId;
                            if (currentOrderId == 0)
                            {
                                //error case
                                return Content("error oid is not ready!?");
                            }

                            //::create Details = o.id + {p.id s} + qty
                            //check Detail is exist or not
                            //currentOrderId = 17258;
                            List<Models.OrderDetail> ods = new List<OrderDetail>();
                            foreach (var p in queryP)
                            {
                                Models.OrderDetail od = new OrderDetail
                                {
                                    ProductId = p.ProductId,
                                    OrderId = currentOrderId, //current Order                                    
                                    UnitPrice = (decimal)p.UnitPrice,
                                    //Quantity = 1, 
                                    Discount = 0,
                                    Product = p, //set queried product data
                                };

                                //s::et qty from cart
                                var sc = shoppingCarts.Where(x => x.Pid == p.ProductId).First();
                                if (sc != null)
                                {
                                    od.Quantity = (short)sc.Qty;
                                    ods.Add(od);

                                    //::write to DB
                                    _context.Add(od);
                                    await _context.SaveChangesAsync();

                                    #region
                                    //::update data Product: InStock, OnOrder
                                    if (p.UnitsInStock == null)
                                        p.UnitsInStock = 0;
                                    p.UnitsInStock -= od.Quantity;

                                    if (p.UnitsOnOrder == null)
                                        p.UnitsOnOrder = 0;
                                    p.UnitsOnOrder += od.Quantity;

                                    _context.Update(p); //::write to product
                                    await _context.SaveChangesAsync();
                                    #endregion
                                }
                                else
                                {
                                    //error case? TBD
                                }
                            }
                            ViewData["OrderDetails"] = ods;

                            //::update product, reduce UnitsInStock, update UnitsOnOrder...

                            //::clear data from shopping cart cookie
                            foreach (var p in queryP)
                            {
                                var sc = shoppingCarts.Where(x => x.Pid == p.ProductId).First();
                                shoppingCarts.Remove(sc);
                            }                            
                            pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                            HttpContext.Response.Cookies.Append("pidJSON", pidJSON);

                            TempData["td_serverInfo"] += " 無異常; ";
                            return View();
                        }
                        else
                        {
                            //fail case
                            TempData["td_serverWarning"] += " 購物車內容不正確; ";
                        }
                        #endregion
                    }
                    //return View(shoppingCarts);
                    //???
                    return View();
                }
            }
            catch
            {
                //var res2 = new { result = "Err", detail = "", prodCount = 0 };
                //return Json(res2);
                TempData["td_serverWarning"] += " 發生未知錯誤; ";
                return View();
            }
            //return View();
        }


        //::Order List for Business Shop side
        public async Task<IActionResult> OrderListForShop()
        {
            //::check Shop
            //string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            //if (UserLevel != "2B")
            //{
              //  return Content("You have no right to access this");
            //}

            //if (!_ms.LoginPrecheck(HttpContext.Session))
            //    return RedirectToAction("Login", "Customers");

            var qO = _context.Orders.Include(o => o.Customer)
                .OrderByDescending(x => x.OrderId);
            return View(await qO.ToListAsync());            
        }

        //::for Business Shop side - Ready for shipping
        public async Task<IActionResult> OrderReadyForShop(int id)
        {
            //oid
            var qO = await _context.Orders.Where(x => x.OrderId == id).FirstAsync();

            if (qO != null)
            { 
                //::update shipped date                
                qO.ShippedDate = DateTime.Now;

                //::update status = 'shipping'
                _context.Update(qO);
                await _context.SaveChangesAsync();

                //::update product?  stock, on order?


                //return Content("pass case");
                return RedirectToAction("OrderListForShop");
            }


            //::Fail case
            return Content("fail case");
            //return View();
        }
    }
}
