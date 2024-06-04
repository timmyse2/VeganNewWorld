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

            //::page

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
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerId,OrderDate,RequiredDate,ShippedDate,ShipVia,Freight,ShipName,ShipAddress,ShipCity,ShipPostalCode,ShipCountry,Payment,Status")] Order order)
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
        public async Task<IActionResult> OrderList(int? page)
        {
            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            //::User ID
            string Userid = _ms.GetMySession("UserAccount", HttpContext.Session);            
            ViewBag.UserAccount = Userid;

            //::page for order
            #region page
            int ipp = 10; // item per page
            int _page = 1, _take = ipp, _skip = 0;
            if (page != null)
                _page = (int)page - 1;
            else
            {
                //::get page from cookie
                var _cookepage = HttpContext.Request.Cookies["page_customerOrder"];
                try
                {
                    if (_cookepage == null)
                        _page = 0;
                    else
                        _page = int.Parse(_cookepage);
                }
                catch
                {
                    _page = 0;
                }
            }
            HttpContext.Response.Cookies.Append("page_customerOrder", _page.ToString());

            int totalCount = _context.Orders
                .Where(o => o.CustomerId == Userid).Count();

            int totalPages = totalCount / ipp;
            if (_page >= totalPages)
                _page = totalPages; //::debug
            _skip = _page * ipp; //(totalPages- _page) * ipp;
            if (_skip < 0) _skip = 0;
            #endregion

            var orders = _context.Orders
                .Where(o=> o.CustomerId == Userid) //sorted
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)  //get count of od
                //.Include(x=>x.prod)
                .OrderByDescending(o => o.OrderId)
                .Skip(_skip).Take(_take) //::notice the sequence
                ;            

            if (orders == null)
            {
                return Content("null");
            }

            ViewData["page"] = _page;
            ViewData["totalCount"] = totalCount;
            ViewData["ipp"] = ipp;

            //try to put image - but this method is not good!
            //:: get image from 1st item only

            ///orders.ElementAt[0].
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


        //::for customer
        public async Task<IActionResult> OrderSetAddressPay()
        {
            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            //::Customer's info: name, address...
            string UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
            Models.Customer customerInfo = await _context.Customer
                .Where(x => x.CustomerId == UserAccount)
                .FirstOrDefaultAsync();

            if(customerInfo == null)
            {
                //error case
                TempData["td_serverWarning"] = "發生異常: 無法取得用戶資料!";
                return View();
            }
            OrderViewModel ovm = new OrderViewModel
            {
                OrderBase = new Order()
            };
            ovm.OrderBase.Customer = customerInfo;
            //ViewData["customerInfo"] = customerInfo;           

            //::Receiver's info
            //::Total Price Sum
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
                    shoppingCarts = JsonConvert.DeserializeObject<List<ShoppingCart>>(pidJSON);
                    //ovm.CartItems = shoppingCarts;
                    if (shoppingCarts.Count <= 0)
                    {
                        TempData["td_serverWarning"] = "訂單是空的，請選擇商品";
                        //::error case
                        return View();
                    }
                    else
                    {
                        //::find matched data from DB
                        #region sync stock data 
                        List<int> pids = new List<int>();
                        //List<int> pids_issue = new List<int>();
                        foreach (var s in shoppingCarts) //get pid from cookie                        
                            pids.Add(s.Pid);

                        //::find product data from DB
                        var queryP = await _context.Products                            
                            .Where(x => pids.Contains(x.ProductId))
                            .ToListAsync();                                          

                        if (queryP != null)
                        {
                            //put matched stock in cookie
                            foreach (var q in queryP)
                            {
                                Debug.WriteLine("id: " + q.ProductId + ", stock: " + q.UnitsInStock);
                                var sc = shoppingCarts.Where(x => x.Pid == q.ProductId).First();
                                if (sc != null)
                                {
                                    if (sc.Stock != q.UnitsInStock)
                                    {
                                        sc.Stock = (short)q.UnitsInStock;
                                    }
                                }
                            }
                            Debug.WriteLine("\n EOD ");

                            int currentOrderId = 0;
                            //:: set Order
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
                                }
                                else
                                {
                                    //error case?
                                }
                            }
                            //ViewData["OrderDetails"] = ods;
                            ovm.Ods = ods;
                        }
                        else
                        {
                            //fail case
                            TempData["td_serverWarning"] += " 購物車內容不正確; ";
                        }
                        #endregion
                    }
                }
            }
            catch
            {
                //var res2 = new { result = "Err", detail = "", prodCount = 0 };
                //return Json(res2);
                TempData["td_serverWarning"] += " 發生未知錯誤; ";
                return View();
            }


            //::Order data - shipVia, Payment, Invoice

            //::from cookie
            string ShipVia =  HttpContext.Request.Cookies["ShipVia"];
            string Payment = HttpContext.Request.Cookies["Payment"];
            string Invoice = HttpContext.Request.Cookies["Invoice"];


            //ViewData["ShipVia"] = ShipVia;
            //ViewData["Payment"] = Payment;
            //ViewData["Invoice"] = Invoice;

             //= int.Parse((string)ViewData["ShipVia"]);
            if(ShipVia != null)
                ovm.OrderBase.ShipVia = int.Parse(ShipVia);            
            if(Payment != null)
            {
                ovm.OrderBase.Payment = (PayEnum)int.Parse(Payment);
                //ovm.Payment = (PayEnum)int.Parse(Payment);
            }
            if (Invoice != null)
            {
                ovm.Invoice = (InvoiceEnum)int.Parse(Invoice);
            }               

            return View(ovm);
            //return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> OrderSetAddressPay(OrderViewModel ovm)
        //{
        //    //::set data: Shipvia, Freight
        //    //:: add Payment, Invoice
        //    //:: Receiver's address|name|phone            

        //    if(ovm != null)
        //    {
        //        if (ovm.OrderBase.ShipVia == (int)ShipViaTypeEnum.Shop)                
        //            ovm.OrderBase.Freight = 50;                
        //        else if (ovm.OrderBase.ShipVia == (int)ShipViaTypeEnum.Witch)                
        //            ovm.OrderBase.Freight = 100;                
        //        else                
        //            ovm.OrderBase.Freight = 0;

        //        ovm.Invoice = InvoiceEnum.Donate;
        //        ovm.TotalPriceSum = 1000;

        //        //Receiver's info
        //        bool isCopy = true;
        //        if (isCopy) //copy info from Customer table
        //        {
        //            ovm.OrderBase.ShipAddress = ovm.OrderBase.Customer.Address;
        //            ovm.OrderBase.ShipName = ovm.OrderBase.Customer.CompanyName;
        //            //ovm.OrderBase.Customer.Phone = "???";
        //        }


        //        //ovm.OrderBase.name

        //        //keep data in cookie


        //    }
        //    return View();
        //}
        
        [HttpPost]
        //::API for Customer Order
        public async Task<IActionResult> APISetAddressPay(int? ShipVia, int? Payment, int? Invoice)
        {
            string _result = "tbc", _detail = "tbc", _time="";
            if (ShipVia == null || Payment == null || Invoice == null)
            {
                _result = "NG"; _detail = "parameter(s) are null";
                var res0 = new { result = _result, detail = _detail};
                return Json(res0);
            }
            await Task.Run(() => {
                try
                {
                    _time = DateTime.Now.ToString();

                    #region ::Do NOT use model now, just set cookie
                    //OrderViewModel ovm = null; // new OrderViewModel();
                    //if (ovm != null)
                    //{
                    //    ovm.OrderBase.Payment = (PayEnum)Payment;
                    //    ovm.Invoice = (InvoiceEnum)Invoice;

                    //    ovm.OrderBase = new Models.Order();
                    //    ovm.OrderBase.ShipVia = ShipVia;
                    //    if (ovm.OrderBase.ShipVia == (int)ShipViaTypeEnum.Shop)
                    //        ovm.OrderBase.Freight = 50;
                    //    else if (ovm.OrderBase.ShipVia == (int)ShipViaTypeEnum.Witch)
                    //        ovm.OrderBase.Freight = 100;
                    //    else
                    //        ovm.OrderBase.Freight = 0;

                    //    ovm.Invoice = InvoiceEnum.Donate;
                    //    ovm.TotalPriceSum = 1000;

                    //    //Receiver's info
                    //    bool isCopy = false;
                    //    if (isCopy) //copy info from Customer table
                    //    {
                    //        ovm.OrderBase.ShipAddress = ovm.OrderBase.Customer.Address;
                    //        ovm.OrderBase.ShipName = ovm.OrderBase.Customer.CompanyName;
                    //        //ovm.OrderBase.Customer.Phone = "???";
                    //    }
                    //    else
                    //    {

                    //    }
                    //    string ovmJSON = "";
                    //    //keep data in cookie : NewOrderVM_Temp
                    //    ovmJSON = JsonConvert.SerializeObject(ovm);
                    //    HttpContext.Response.Cookies.Append("NewOrderVM_Temp", ovmJSON);
                    //}
                    #endregion

                    HttpContext.Response.Cookies.Append("ShipVia", ShipVia.ToString());
                    HttpContext.Response.Cookies.Append("Payment", Payment.ToString());
                    HttpContext.Response.Cookies.Append("Invoice", Invoice.ToString());
                    _result = "PASS"; _detail = " " + ShipVia + "," + Payment + "," + Invoice;
                }
                catch (Exception ex)
                {
                    _result = "Error"; _detail = "exception " + ex;
                }
            });
            
            var res = new { result = _result, detail = _detail, time= _time };
            return Json(res);
        }

        //::for end user, check order before create data in DB
        public async Task<IActionResult> CheckOrder()
        {
            //return RedirectToAction("CreateOrderAndDetails", "Orders");
            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            //::Share same Action Result for CheckOrder and CreateOrderAndDetails
            return RedirectToAction("CreateOrderAndDetails");

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
                    shoppingCarts = JsonConvert.DeserializeObject<List<ShoppingCart>>(pidJSON);
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
                        //List<int> pids_issue = new List<int>();
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

                                    //if(sc.Qty > q.UnitsInStock)
                                    //{
                                    //    //::show warning or error???
                                    //    pids_issue.Add(sc.Pid);
                                    //}
                                    //q.UnitsOnOrder

                                    if (sc.Stock != q.UnitsInStock)
                                    {
                                        sc.Stock = (short)q.UnitsInStock;
                                        //::show warning
                                        //pids_issue.Add(sc.Pid);
                                    }
                                }
                            }
                            Debug.WriteLine("\n EOD ");

                            //if(pids_issue.Count > 0)
                            //{
                            //someting is worng?
                            //  TempData["td_serverWarning"] += " 數量與庫存不合; ";
                            //}                            

                            //::get info customer 
                            //:: Get customer Id, Name, Info {address}
                            string UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
                            Models.Customer customerInfo = await _context.Customer
                                .Where(x => x.CustomerId == UserAccount)
                                .FirstOrDefaultAsync();

                            if (customerInfo == null)
                            {
                                TempData["td_serverWarning"] += " 客戶資訊不明; ";
                                return View();
                            }

                            if (customerInfo == null)
                            {
                                //error case
                                TempData["td_serverWarning"] = "發生異常: 無法取得用戶資料!";
                                return View();
                            }
                            //ViewData["member"] = customerInfo;

                            //::from cookie
                            string ShipVia = HttpContext.Request.Cookies["ShipVia"];
                            string Payment = HttpContext.Request.Cookies["Payment"];
                            string Invoice = HttpContext.Request.Cookies["Invoice"];
                            ViewData["ShipVia"] = ShipVia;
                            ViewData["Payment"] = Payment;
                            ViewData["Invoice"] = Invoice;
                            if (ShipVia == null || Payment == null)
                            {
                                //error case!
                                TempData["td_serverWarning"] += " 運貨方式設定異常; ";
                                return View();
                            }

                            int currentOrderId = 0;
                            //:: set Order
                            //  Create New Order or merge to old recordset?
                            Models.Order newOrder = new Order
                            {
                                CustomerId = customerInfo.CustomerId,
                                OrderId = 0, //auto create in sql server
                                ShipAddress = customerInfo.Address,
                                ShipCity = customerInfo.City,
                                ShipName = customerInfo.CompanyName,
                                ShipCountry = customerInfo.Country,
                                ShipPostalCode = customerInfo.PostalCode,
                                //Freight = 0,
                                //ShipVia = 1,
                                OrderDate = DateTime.Now,
                            };

                            //::get data from step2
                            //newOrder.ShipVia = (int)VNW.Models.ShipViaTypeEnum.Witch;
                            newOrder.ShipVia = int.Parse(ShipVia);
                            if (newOrder.ShipVia == (int)ShipViaTypeEnum.Shop)
                                newOrder.Freight = 50;
                            else if (newOrder.ShipVia == (int)ShipViaTypeEnum.Witch)
                                newOrder.Freight = 100;
                            else
                                newOrder.Freight = 0;
                            newOrder.Payment = (Common.PayEnum)int.Parse(Payment);

                            OrderViewModel ovm = new OrderViewModel
                            {
                                OrderBase = newOrder
                            };
                            ovm.OrderBase.Customer = customerInfo;
                            //ViewData["newOrder"] = newOrder;

                            ////CreateOrder(newOrder);
                            //_context.Add(newOrder);
                            //await _context.SaveChangesAsync();

                            //::get order id, check order
                            //currentOrderId = newOrder.OrderId;
                            //if (currentOrderId <= 0)
                            //{
                            //error case
                            //  return Content("error oid is not ready!?");
                            //}

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
                                    //if (sc.Qty > p.UnitsInStock)
                                    //{
                                    //    //error case TBD
                                    //}
                                    od.Quantity = (short)sc.Qty;
                                    ods.Add(od);

                                    ////::write to DB
                                    //_context.Add(od);
                                    //await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    //error case?
                                }
                            }
                            //ViewData["OrderDetails"] = ods;
                            ovm.Ods = ods;

                            //::update product, reduce stock

                            //::clear shopping cart cookie

                            //return Json(ods);

                            //TempData["td_serverInfo"] += " 無異常; ";
                            return View(ovm);
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

        //::set official data in Orders and OrderDetails
        //[HttpPost]
        public async Task<IActionResult> CreateOrderAndDetails(int? isSaveAndUpdateDB)
        {
            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            //::set this flag to switch DB updating/creating or not
            bool _isSaveAndUpdateDB = false;            
            if(isSaveAndUpdateDB != null && isSaveAndUpdateDB == 1)
            {
                _isSaveAndUpdateDB = true;
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    //::get temporary data of ShoppingCart from cookie
                    string pidJSON = null;
                    List<ShoppingCart> shoppingCarts = new List<ShoppingCart>();
                    pidJSON = HttpContext.Request.Cookies["pidJSON"];
                    if (pidJSON == null)
                    {
                        TempData["td_serverWarning"] = "訂單是空的，請選擇商品";
                        if (_isSaveAndUpdateDB)
                            return View();
                        else
                            return View("CheckOrder");
                    }
                    else
                    {
                        shoppingCarts = JsonConvert.DeserializeObject<List<VNW.ViewModels.ShoppingCart>>(pidJSON);
                        if (shoppingCarts.Count <= 0)
                        {
                            TempData["td_serverWarning"] = "訂單是空的，請選擇商品";
                            //::error case
                            if (_isSaveAndUpdateDB)
                                return View();
                            else
                                return View("CheckOrder");
                        }
                        else
                        {
                            //TempData["td_serverInfo"] = "取得資料" + shoppingCarts.Count;

                            //::find matched data from DB
                            #region sync stock data 
                            List<int> pids = new List<int>();
                            //List<int> pids_issue = new List<int>();
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

                                        //if (sc.Qty > q.UnitsInStock)
                                        //{
                                        //::show warning or error???
                                        //pids_issue.Add(sc.Pid);
                                        //}

                                        if (sc.Stock != q.UnitsInStock)
                                        {
                                            sc.Stock = (short)q.UnitsInStock;
                                            //::show warning
                                            //pids_issue.Add(sc.Pid);
                                        }
                                    }
                                }
                                Debug.WriteLine("\n EOD ");

                                //if (pids_issue.Count > 0)
                                //{
                                //someting is worng?
                                //TempData["td_serverWarning"] += " 數量與庫存不合; ";
                                //}
                                //pass case

                                //::get info customer 
                                //:: Get customer Id, Name, Info {address}
                                string UserAccount = _ms.GetMySession("UserAccount", HttpContext.Session);
                                Models.Customer customerInfo = await _context.Customer
                                    .Where(x => x.CustomerId == UserAccount)
                                    .FirstOrDefaultAsync();

                                if (customerInfo == null)
                                {
                                    TempData["td_serverWarning"] += " 客戶資訊不明; ";
                                    if (_isSaveAndUpdateDB)
                                        return View();
                                    else
                                        return View("CheckOrder");
                                }
                                //ViewData["member"] = customerInfo;

                                //::from cookie
                                string ShipVia = HttpContext.Request.Cookies["ShipVia"];
                                string Payment = HttpContext.Request.Cookies["Payment"];
                                string Invoice = HttpContext.Request.Cookies["Invoice"];
                                ViewData["ShipVia"] = ShipVia;
                                ViewData["Payment"] = Payment;
                                ViewData["Invoice"] = Invoice;

                                if (ShipVia == null || Payment == null)
                                {
                                    //error case
                                    TempData["td_serverWarning"] += " 運送方式異常; ";
                                    if (_isSaveAndUpdateDB)
                                        return View();
                                    else
                                        return View("CheckOrder");
                                }

                                int currentOrderId = 0;
                                //:: set Order
                                //  Create New Order or merge to old recordset?
                                Models.Order newOrder = new Order
                                {
                                    CustomerId = customerInfo.CustomerId,
                                    //OrderId = currentOrderId, //auto create in sql server
                                    ShipAddress = customerInfo.Address,
                                    ShipCity = customerInfo.City,
                                    ShipName = customerInfo.CompanyName,
                                    ShipCountry = customerInfo.Country,
                                    ShipPostalCode = customerInfo.PostalCode,
                                    //Freight = 0,
                                    //ShipVia = 1,
                                    OrderDate = DateTime.Now,
                                };

                                newOrder.ShipVia = int.Parse(ShipVia);
                                if (newOrder.ShipVia == (int)ShipViaTypeEnum.Shop)
                                    newOrder.Freight = 50;
                                else if (newOrder.ShipVia == (int)ShipViaTypeEnum.Witch)
                                    newOrder.Freight = 100;
                                else
                                    newOrder.Freight = 0;
                                newOrder.Payment = (Common.PayEnum)int.Parse(Payment);

                                //ViewData["newOrder"] = newOrder;
                                OrderViewModel ovm = new OrderViewModel
                                {
                                    OrderBase = newOrder
                                };
                                ovm.OrderBase.Customer = customerInfo;

                                if (_isSaveAndUpdateDB)
                                {
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

                                        if (_isSaveAndUpdateDB)
                                        {
                                            //::write to DB table OrderDetail
                                            _context.Add(od);
                                            await _context.SaveChangesAsync();
                                            #region
                                            //::update data Product: InStock, OnOrder
                                            if (p.UnitsInStock == null)
                                                p.UnitsInStock = 0;
                                            p.UnitsInStock -= od.Quantity;

                                            //if (p.UnitsOnOrder == null)
                                            //    p.UnitsOnOrder = 0;
                                            //p.UnitsOnOrder += od.Quantity;

                                            _context.Update(p); //::write to product
                                            await _context.SaveChangesAsync();
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        //error case? TBD
                                    }
                                }
                                //ViewData["OrderDetails"] = ods;
                                ovm.Ods = ods;

                                //::update product, reduce UnitsInStock, update UnitsOnOrder...

                                if (_isSaveAndUpdateDB)
                                {
                                    //::clear data from shopping cart cookie
                                    foreach (var p in queryP)
                                    {
                                        var sc = shoppingCarts.Where(x => x.Pid == p.ProductId).First();
                                        shoppingCarts.Remove(sc);
                                    }
                                    pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                                    HttpContext.Response.Cookies.Append("pidJSON", pidJSON);

                                    transaction.Commit();//key
                                }

                                TempData["td_serverInfo"] += " 無異常; ";
                                //return View();

                                if (_isSaveAndUpdateDB)
                                    return View(ovm);
                                else
                                    return View("CheckOrder", ovm);
                            }
                            else
                            {
                                //fail case
                                TempData["td_serverWarning"] += " 購物車內容不正確; ";
                            }
                            #endregion
                        }
                        if (_isSaveAndUpdateDB)
                            return View();
                        else
                            return View("CheckOrder");
                    }
                }
                catch
                {
                    //var res2 = new { result = "Err", detail = "", prodCount = 0 };
                    //return Json(res2);
                    TempData["td_serverWarning"] += " 發生未知錯誤; ";
                    if (_isSaveAndUpdateDB)
                        return View();
                    else
                        return View("CheckOrder");
                }
            }


            //return View();
        }

        //::Order List for Business Shop side
        public async Task<IActionResult> OrderListForShop(int? page)
        {
            //::check Shop
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B")
            {
              //return Content("You have no right to access this");
              return RedirectToAction("Login", "Customers");
            }

            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            #region page
            int ipp = 10; // item per page
            int _page = 1, _take = ipp, _skip = 0;
            if (page != null)
                _page = (int)page - 1;
            else
            {
                //::get page from cookie
                var _cookepage = HttpContext.Request.Cookies["page_shoporder"];
                try
                {
                    if (_cookepage == null)                    
                        _page = 0;                    
                    else                    
                        _page = int.Parse(_cookepage);                    
                }
                catch
                {
                    _page = 0;
                }
            }
            HttpContext.Response.Cookies.Append("page_shoporder", _page.ToString());

            int totalCount = _context.Orders.Count();
            int totalPages = totalCount / ipp;
            if (_page >= totalPages)
                _page = totalPages; //::debug
            _skip = _page * ipp; //(totalPages- _page) * ipp;
            if (_skip < 0) _skip = 0;
            #endregion

            var q = _context.Orders.Include(o => o.Customer)                
                .OrderByDescending(x => x.OrderId)
                .Skip(_skip).Take(_take);

            ViewData["page"] = _page;
            ViewData["totalCount"] = totalCount;
            ViewData["UserAccount"] = _ms.GetMySession("UserAccount", HttpContext.Session);
            ViewData["ShopAccount"] = _ms.GetMySession("ShopAccount", HttpContext.Session);

            return View(await q.ToListAsync());            
        }

        //::for Business Shop side - Ready for shipping
        
        public async Task<IActionResult> ShipOrderForShop(int id)
        {
            //::check Shop
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B")
            {
                //return Content("You have no right to access this");
                return RedirectToAction("Login", "Customers");
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                //oid
                var qO = await _context.Orders.Where(x => x.OrderId == id)
                    .FirstOrDefaultAsync();
                //.FirstAsync();

                if (qO != null)
                {
                    //::pre check Status
                    if(qO.Status == OrderStatusEnum.Shipped)
                    {
                        //TempData["td_serverMessage"] = "已設定出貨 訂單:" + id;
                        TempData["td_serverWarning"] = "之前已經出貨, 無法重覆流程 " + id;
                        return RedirectToAction("OrderDetailsForShop//" + id, "orderDetails");
                    }

                    //::update shipped date                
                    qO.ShippedDate = DateTime.Now;
                    qO.Status = OrderStatusEnum.Shipped;
                    //::update status = 'shipping'
                    _context.Update(qO);
                    await _context.SaveChangesAsync();

                    //::update product: 
                    //units in stock
                    //reserved

                    //return Content("pass case");
                    transaction.Commit();
                    TempData["td_serverMessage"] = "已設定出貨 訂單:" + id;
                    return RedirectToAction("OrderDetailsForShop//" + id, "orderDetails");
                    //return RedirectToAction("OrderListForShop");
                }
            }
            //::Fail case
            return Content("fail case");
            //return View();
        }

        //::for user
        //public async Task CancelOrderAsync(int orderId)
        public async Task<IActionResult> CancelOrderAsync(int? id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    //::order: cancel
                    var qO = await _context.Orders.Where(x => x.OrderId == id)
                        .FirstOrDefaultAsync();
                    //status

                    if (qO == null)
                    {
                        //NG case
                        return Content("order is not exist");
                    }

                    if(qO.Status == OrderStatusEnum.Cancelled 
                        || qO.Status == OrderStatusEnum.Canceling)
                    {
                        //return Content("Order was cancelled before, it could not be cancelled again");
                        TempData["td_serverWarning"] = "訂單之前已取消，不能再次執行";
                        //return RedirectToAction("OrderDetailsForShop", "orderdetails", id);
                        return RedirectToAction("OrderDetailsForShop/" + id, "orderdetails");
                    }

                    //::o.detail, update product - stock, reserved
                        var qOds = await _context.OrderDetails.Where(x => x.OrderId == id)
                        //.Include(x=>x.Product)
                        .ToListAsync();

                    if (qOds != null)
                    {
                        foreach (var od in qOds)
                        {
                            //od.ProductId
                            var p = await _context.Products.Where(x => x.ProductId == od.ProductId).FirstAsync();

                            if (p != null)
                            {
                                p.UnitsInStock += od.Quantity;
                                //p.UnitsReserved += od.Quantity;                            
                                _context.Products.Update(p);
                            }
                        }
                    }

                    qO.Status = OrderStatusEnum.Cancelled;
                    _context.Orders.Update(qO);

                    await _context.SaveChangesAsync();
                    //pass case
                    transaction.Commit();
                    //redirection

                    TempData["td_serverMessage"] = "已將訂單取消";
                    return RedirectToAction("OrderDetailsForShop/" + id, "orderdetails");
                    //return Content(":)  This order is Cacencelled");
                    //return View();
                }
                catch
                {
                    return Content("Exception :(");
                }

            }
        }

        public async Task<IActionResult> VMTest(int id)
        {
            var qO = await _context.Orders.Where(x => x.OrderId == id)
                .FirstOrDefaultAsync();

            if (qO != null)
            {
                var qD = await _context.OrderDetails
                    .Where(x => x.OrderId == id)
                    .Include(x=>x.Product)
                    .ToListAsync();

                OrderViewModel odvm = new OrderViewModel();
                odvm.Ods = qD;
                odvm.OrderBase = qO;
                //odvm.CustomerId = qO.CustomerId;
                odvm.OrderBase.OrderId = qO.OrderId;
                //odvm.OrderBase.Payment = PayEnum.CashOnDelivery;
                //if (qD.Count > 0)
                //{
                    //odvm.OD = qD.ElementAt(0);
                    //odvm.ods
                //}

                return View(odvm);
            }

            return View();
        }
        public async Task<IActionResult> VMEditTest(int id)
        {
            var q_order = await _context.Orders.Where(x => x.OrderId == id)
                .FirstOrDefaultAsync();

            if (q_order != null)
            {
                var q_details = await _context.OrderDetails
                    .Where(x => x.OrderId == id)
                    .Include(x => x.Product)
                    .ToListAsync();

                OrderViewModel odvm = new OrderViewModel();
                odvm.Ods = q_details;
                odvm.OrderBase = q_order;
                //odvm.CustomerId = q_order.CustomerId;
                odvm.OrderBase.OrderId = q_order.OrderId;
                odvm.OrderBase.Payment = PayEnum.CashOnDelivery;
                odvm.TotalPriceSum = 17258;
                odvm.Invoice = InvoiceEnum.Donate;
                //if (qD.Count > 0)
                //{
                //odvm.OD = qD.ElementAt(0);
                //odvm.ods
                //}

                return View(odvm);
            }

            return View();
        }
        [HttpPost]
        public IActionResult VMEditTest(int id,
            OrderViewModel ovm)
        {
            Debug.WriteLine("id " + id);
            if(ovm == null)
            {
                return Content("Model is null");
            }

            if(ovm.Ods == null)
            {
                Debug.WriteLine("ods is null");
            }

            return View(ovm);            
            ////return RedirectToAction("VMTest", "Orders", ovm);
            //return View("VMEditTest", ovm);
            //return View("VMTest", ovm);
        }
    }
}
