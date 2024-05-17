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
    public class ProductsController : Controller
    {
        private readonly VeganNewWorldContext _context;
        //::set session common interface
        VNW.Common.MySession _ms = new Common.MySession();

        public ProductsController(VeganNewWorldContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            //::check admin
            if (!_ms.CheckAdmin(HttpContext.Session))
                return Content("You have no right to access this page");

            var veganNewWorldContext = _context.Products.Include(p => p.Category);

            ViewBag.UserAccount =
                _ms.GetMySession("UserAccount", HttpContext.Session);

            return View(await veganNewWorldContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
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

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            //::check admin
            if (!_ms.CheckAdmin(HttpContext.Session))
                return Content("You have no right to access this page");

            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            //ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "CategoryId", "CategoryId");
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "CategoryId", 
                "CategoryName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,CategoryId,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["td_serverMessage"] = "Created";
                return RedirectToAction(nameof(Index));
            }

            TempData["td_serverMessage"] = "Warning! Something is not valid";

            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "CategoryId", "CategoryId", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
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

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            //ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "CategoryId", "CategoryId", product.CategoryId);
            //::<udpate- show Name in select list>
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "CategoryId", "CategoryName", product.CategoryId);            

            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, 
            [Bind("ProductId,ProductName,CategoryId,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued,Picture,Description"
                )] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "CategoryId", "CategoryId", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
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

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["td_serverMessage"] = "Deleted";
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        // GET: Products Index for end user
        public async Task<IActionResult> ProductList(int? cat, string catName)
        {
            MySession ms = new MySession();

            if(cat == null)
            {
                try
                {
                    string _cat = "0";
                    _cat = ms.GetMySession("catId", HttpContext.Session);
                    if (_cat != null)
                        cat = int.Parse(_cat);
                    //return Content("Cat ID is null");
                }
                catch
                {
                }
            }

            var veganNewWorldContext = 
                _context.Products
                .Where(p=>p.CategoryId == cat) //::cat id
                .Include(p => p.Category)
                ;

            //::category name or id on view
            ViewBag.catId = cat;
            ms.SetMySession("catId", cat.ToString(),HttpContext.Session);
            
            if (catName != null)
            {
                ms.SetMySession("catName", catName,HttpContext.Session);
            }
            else
            {
                string _catName = ms.GetMySession("catName", HttpContext.Session);
                if (_catName != null)
                {
                    catName = _catName;
                }
            }
            ViewBag.catName = catName;

            //SetMySession("catName", ViewBag.catName);
            return View(await veganNewWorldContext.ToListAsync());
        }

        // GET: Products/Details/5 for end user
        public async Task<IActionResult> Product_Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            MySession ms = new MySession();
            string _catName = ms.GetMySession("catName", HttpContext.Session);
            ViewBag.catName = _catName;

            return View(product);
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


        //::api for adding p.id in cookie 
        //public async Task<IActionResult> AddProductInOrder(int? pid)
        public async Task<IActionResult> AddProductInOrder(int? pid, string pname, string img, int? price)
        {
            //::check pid
            int _pid;
            if (pid == 0 || pid == null)
            {
                var res0 = new { result = "FAIL", detail = "id is null", prodCount=0 };
                return Json(res0);
            }
            _pid = (int)pid;
            string _result = "tbc", _detail = "tbc";
            int _prodCount = 0;

            await Task.Run(() => {
                try
                {
                    string pidJSON = null;
                    List<ShoppingCart> shoppingCarts = new List<VNW.ViewModels.ShoppingCart>();
                    pidJSON = HttpContext.Request.Cookies["pidJSON"];
                    bool isUpdateData = false;
                    if (pidJSON == null)
                    {
                        //if null then add new             
                        isUpdateData = true;
                        //shoppingCarts.Add(new ShoppingCart
                        //{
                        //    Pid = _pid,
                        //    Qty = 1,
                        //    Name = "",
                        //    Img = ""
                        //});
                        //pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                    }
                    else
                    {
                        //::merge data
                        shoppingCarts = JsonConvert.DeserializeObject<List<VNW.ViewModels.ShoppingCart>>(pidJSON);

                        //::found exist item repeatedly
                        var found = shoppingCarts.Find(x => x.Pid == _pid);
                        //ShoppingCart tsc = new ShoppingCart { Pid = _pid, Qty = 1 };
                        //if (shoppingCarts.Contains(tsc))                    
                        //if (shoppingCarts.Contains(new ShoppingCart { Pid = _pid, Qty = 1 }))
                        if (found != null)
                        {
                            //::do not add it to new item again
                            //Debug.WriteLine("Found ");
                            //found.Qty++;
                            //pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                            isUpdateData = false;
                            _result = "PASS";
                            _detail = "Repeated";// pidJSON;
                            _prodCount = shoppingCarts.Count;
                        }
                        else
                        {
                            isUpdateData = true;
                            //Debug.WriteLine("not find ");
                        }
                    }
                    //Debug.WriteLine("shoppingCarts.Count " + shoppingCarts.Count);
                    //ViewBag.shoppingCartsCount = shoppingCarts.Count;

                    if (isUpdateData)
                    {
                        //::load stock value from DB
                        short _stock = 0;

                        var query = _context.Products.Find(_pid);
                        if (query != null)
                        {
                            _stock = (short)query.UnitsInStock;
                            shoppingCarts.Add(new VNW.ViewModels.ShoppingCart
                            {
                                Pid = _pid,
                                Qty = 1,
                                Name = query.ProductName,
                                Price = (int)query.UnitPrice,
                                Img = query.Picture,
                                Stock = _stock, //add stock
                            });
                            pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                            HttpContext.Response.Cookies.Append("pidJSON", pidJSON);
                            _result = "PASS";
                            _detail = "new";// pidJSON;
                            _prodCount = shoppingCarts.Count;
                        }
                        else
                        {
                            //error case
                            //var res3 = new { result = "fail", detail = "no match data", prodCount = 0 };
                            //return Json(res3);
                            _result = "NG"; _detail = "query is null"; _prodCount = 0;
                        }

                        //shoppingCarts.Add(new VNW.ViewModels.ShoppingCart
                        //{
                        //    Pid = _pid,
                        //    Qty = 1,
                        //    Name = pname,
                        //    Price = (int) price,
                        //    Img =  img,
                        //    Stock = _stock, //add stock
                        //});
                        //pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                    }

                    //HttpContext.Response.Cookies.Append("pidJSON", pidJSON);
                    //var res2 = new { result = "PASS", detail = pidJSON, prodCount = shoppingCarts.Count };
                    //return Json(res2);
                }
                catch (Exception ex)
                {
                    //var res2 = new { result = "Err", detail = "" + ex.ToString(), prodCount = 0 };
                    //return Json(res2);

                    _result = "Err"; _detail = ex.ToString(); _prodCount = 0;
                }
            });


            var res = new { result = _result, detail = _detail, prodCount = _prodCount };
            return Json(res);            
        }

        //::get data from Cookie - API for testing
        public async Task<IActionResult> GetShoppingCart()
        {
            string _result = "tbc", _detail = "tbc";
            int _prodCount = 0;
            await Task.Run(() => {
                try
                {
                    string pidJSON = null;
                    List<VNW.ViewModels.ShoppingCart> shoppingCarts = new List<VNW.ViewModels.ShoppingCart>();
                    pidJSON = HttpContext.Request.Cookies["pidJSON"];
                    if (pidJSON == null)
                    {
                        //var res1 = new { result = "PASS", detail = "no data", prodCount = 0 };
                        //return Json(res1);
                        _result = "NG"; _detail = "no data"; _prodCount = 0;
                    }
                    else
                    {
                        //::PASS CASE
                        shoppingCarts = JsonConvert.DeserializeObject<List<VNW.ViewModels.ShoppingCart>>(pidJSON);
                        //::Do not return data if it just needs to get 'COUNT' now
                        //pidJSON = JsonConvert.SerializeObject(shoppingCarts);

                        //Debug.WriteLine("shoppingCarts.Count " + shoppingCarts.Count);
                        //ViewBag.shoppingCartsCount = shoppingCarts.Count;
                        //HttpContext.Response.Cookies.Append("pidJSON", pidJSON);

                        _result = "PASS"; _detail = pidJSON; _prodCount = shoppingCarts.Count;

                        //var res2 = new { result = "PASS", detail = pidJSON, prodCount = shoppingCarts.Count };
                        //return Json(res2);
                    }
                }
                catch
                {
                    //var res2 = new { result = "Err", detail = "", prodCount = 0 };
                    //return Json(res2);
                    _result = "Err"; _detail = "???"; _prodCount = 0;
                }

            });
            var res = new { result = _result, detail = _detail, prodCount = _prodCount };
            return Json(res);
        }

        //::api for remove product from shopping cart
        public async Task<IActionResult> RemoveShoppingCart(int? pid)
        {
            //if (!_ms.LoginPrecheck(HttpContext.Session))
            //    return RedirectToAction("Login", "Customers");

            string _result = "tbc"; string _detail = "tbc"; int _prodCount = 0;

            //::check pid
            int _pid = 0;
            if (pid == 0 || pid == null)
            {
                var res0 = new { result = "FAIL", detail = "id is null", prodCount=0 };
                return Json(res0);
            }
            _pid = (int)pid;
            
            await Task.Run(() => {
                try
                {
                    string pidJSON = null;
                    List<VNW.ViewModels.ShoppingCart> shoppingCarts = new List<VNW.ViewModels.ShoppingCart>();
                    pidJSON = HttpContext.Request.Cookies["pidJSON"];
                    if (pidJSON == null)
                    {
                        //var res1 = new { result = "empty", detail = "", count = 0 };
                        //return Json(res1);
                        _result = "NG"; _detail = "empty"; _prodCount = 0;
                    }
                    else
                    {
                        shoppingCarts = JsonConvert.DeserializeObject<List<VNW.ViewModels.ShoppingCart>>(pidJSON);

                        //::found exist item repeatedly
                        var found = shoppingCarts.Find(x => x.Pid == _pid);
                        if (found != null)
                        {
                            shoppingCarts.Remove(found);
                            pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                            HttpContext.Response.Cookies.Append("pidJSON", pidJSON);

                            _result = "PASS"; _detail = ""; _prodCount = shoppingCarts.Count;
                            //_result = "PASS"; _detail = pidJSON; _prodCount = shoppingCarts.Count;
                        }
                        else
                        {
                            //pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                            _result = "NG"; _detail = "empty"; _prodCount = 0;
                        }
                        //var res2 = new { result = "PASS", detail = pidJSON, prodCount = shoppingCarts.Count };
                        //return Json(res2);
                    }
                }
                catch
                {
                    //var res2 = new { result = "Err", detail = "", prodCount = 0 };
                    //return Json(res2);
                    _result = "Err"; _detail = ""; _prodCount = 0;
                }

            });
            var res = new { result = _result, detail = _detail, prodCount= _prodCount };
            return Json(res);
        }

        //::for end user, Shopping Cart|Step|Prepare Order
        //public IActionResult PrepareOrder()
        public async Task<IActionResult> PrepareOrder()
        {
            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            //::<Timmy May 1,2024><case A - use ajax to get p list>
            //return View();

            //::<Timmy May 6,2024><case B - use Model to provide p list>
            try
            {
                string pidJSON = null;
                List<VNW.ViewModels.ShoppingCart> shoppingCarts = new List<VNW.ViewModels.ShoppingCart>();
                pidJSON = HttpContext.Request.Cookies["pidJSON"];
                if (pidJSON == null)
                {
                    //var res1 = new { result = "NG", detail = "", prodCount = 0 };
                    //return Json(res1);
                    TempData["td_serverWarning"] = "訂單是空的，請選擇商品";
                    return View();
                }
                else
                {
                    //::<Timmy May7 2024><try to set await>
                    await Task.Run(()=> {
                        shoppingCarts = JsonConvert.DeserializeObject<List<VNW.ViewModels.ShoppingCart>>(pidJSON);
                        pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                        //Debug.WriteLine("shoppingCarts.Count " + shoppingCarts.Count);
                        //ViewBag.shoppingCartsCount = shoppingCarts.Count;
                        HttpContext.Response.Cookies.Append("pidJSON", pidJSON);
                        //var res2 = new { result = "PASS", detail = pidJSON, prodCount = shoppingCarts.Count };
                        //return Json(res2);
                        //TempData["td_serverMessage"] = "";

                        #region ::sync stock value from DB
                        //foreach(var s in shoppingCarts)
                        //{
                        //    var query = (from p in _context.Products
                        //             where p.ProductId == s.Pid
                        //             select new {p.ProductId, p.UnitsInStock })
                        //             .First();
                        //    if(query != null)
                        //    {
                        //        //var q1Res = q1.First(); // x => x.ProductId == s.Pid);
                        //        //if(q1Res != null)
                        //        //  s.Stock = (short)q1Res.UnitsInStock;
                        //        s.Stock = (short)query.UnitsInStock;
                        //    }                           

                        //    //VNW.Models.Product q2 = _context.Products.Find(s.Pid);
                        //    //if (q2 != null)
                        //    //{
                        //    //    s.Stock = (short) q2.UnitsInStock;
                        //    //    //Debug.WriteLine(" " + q2.UnitsInStock);
                        //    //}

                        //    //var q3 = _context.Products.Select(x => new { x.ProductId, x.UnitsInStock })
                        //    //  .Where(x=>x.ProductId == s.Pid);
                        //}
                        #endregion
                        #region sync stock - use Contains to send SQL only one time
                        List<int> Ids = new List<int>(); // { 1, 70, 20, 5, 80 };
                        foreach(var s in shoppingCarts) //get pid from cookie                        
                            Ids.Add(s.Pid);
                        //::find stock from DB
                        var queryDB = _context.Products 
                            .Select(x => new {x.ProductId, x.UnitsInStock })
                            .Where(x => Ids.Contains(x.ProductId)).ToList();
                        if(queryDB != null)
                        {
                            Debug.WriteLine(" Mathced DB Count: " + queryDB.Count());                            
                            //put matched stock in cookie
                            foreach(var q in queryDB)
                            {
                                Debug.WriteLine("id: " + q.ProductId + ", stock: " + q.UnitsInStock);
                                var sc = shoppingCarts.Where(x => x.Pid == q.ProductId).First();
                                if(sc !=null)
                                    sc.Stock = (short)q.UnitsInStock;                                
                            }
                            Debug.WriteLine("\n EOD ");
                        }
                        #endregion

                        if (shoppingCarts.Count <= 0)
                        {
                            TempData["td_serverWarning"] = "訂單是空的，請選擇商品";
                        }
                        else
                            TempData["td_serverInfo"] = "取得資料" + shoppingCarts.Count;
                    });
                    return View(shoppingCarts);
                }
            }
            catch
            {
                //var res2 = new { result = "Err", detail = "", prodCount = 0 };
                //return Json(res2);
                TempData["td_serverWarning"] = "發生未知錯誤";
                return View();
            }
        }

        //::API for end user, update qty        
        public async Task<IActionResult> UpdateQty(int? pid, int? qty)
        //public IActionResult UpdateQty(int? pid, int? qty)
        {
            string _result = "tbc", _detail = "tbc";
            //::check pid
            int _pid;
            if (pid == 0 || pid == null || qty == null || qty == 0)
            {
                //var res = new { result = "FAIL", detail = "id is null" };
                //return Json(res);
                _result = "FAIL"; _detail = "id or key value is null";
            }
            else
            {
                await Task.Run(() => {
                    try
                    {
                        _pid = (int)pid;
                        string pidJSON = null;
                        List<VNW.ViewModels.ShoppingCart> shoppingCarts = new List<VNW.ViewModels.ShoppingCart>();
                        pidJSON = HttpContext.Request.Cookies["pidJSON"];
                        if (pidJSON != null)
                        {
                            //::merge data
                            shoppingCarts = JsonConvert.DeserializeObject<List<VNW.ViewModels.ShoppingCart>>(pidJSON);

                            //::found exist item
                            var found = shoppingCarts.Find(x => x.Pid == _pid);
                            if (found != null)
                            {
                                found.Qty = (int)qty;
                                #region 
                                //::Read stock from DB
                                var query = _context.Products
                                .Where(x=>x.ProductId == _pid)
                                .Select(x=> new { x.ProductId, x.UnitsInStock }).First();                                
                                //.Find(_pid);
                                if (query != null)
                                {
                                    found.Stock = (short)query.UnitsInStock;
                                }
                                else
                                    found.Stock = 0; //TBC
                                #endregion

                                pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                                HttpContext.Response.Cookies.Append("pidJSON", pidJSON);
                                //var res1 = new { result = "PASS", detail = found };
                                //return Json(res1);
                                _result = "PASS"; _detail = found.Stock.ToString();   //pidJSON; // "";
                            }
                            else
                            {
                                //::not found
                                _result = "NG"; _detail = "no match data";
                            }
                        }
                        else
                        {
                            //HttpContext.Response.Cookies.Append("pidJSON", pidJSON);
                            //var res2 = new { result = "NG", detail = "no match data" };
                            //return Json(res2);                        
                            _result = "NG"; _detail = "something is null";
                        }

                    }
                    catch (Exception ex)
                    {
                        //var res2 = new { result = "Err", detail = "" };
                        //return Json(res2);
                        _result = "ERROR"; _detail = "" + ex.ToString();
                    }
                });
            }         
            var res = new { result = _result, detail = _detail};
            return Json(res);
        }

        //::api for remove all products from shopping cart
        public async Task<IActionResult> ClearShoppingCart()
        {
            string _result = "tbc", _detail = "tbc";
            await Task.Run(() => {
                try
                {
                    HttpContext.Response.Cookies.Delete("pidJSON");
                    //var res1 = new { result = "PASS", detail = "cleared" };
                    //return Json(res1);
                    _result = "PASS"; _detail = "cleared";
                }
                catch (Exception ex)
                {
                    //var res2 = new { result = "Err", detail = "..." + ex.ToString() };
                    //return Json(res2);
                    _result = "Err"; _detail = "..." + ex.ToString();
                }
            });
            var res = new { result = _result, detail = _detail };
            return Json(res);
        }

        //::for end user, check order before create data in DB
        public async Task<IActionResult> CheckOrder()
        {
            return RedirectToAction("CreateOrderAndDetails", "Orders");


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
                        var queryP =  await _context.Products
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
                                    
                                    if(sc.Qty > q.UnitsInStock)
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
                            
                            if(pids_issue.Count > 0)
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

                            if(member == null)
                            {
                                TempData["td_serverWarning"] += " 客戶資訊不明; ";
                                return View();
                            }
                            ViewData["member"] = member;

                            int currentOrderId = 0;                            
                            //:: set Order
                            //  Create New Order or merge to old recordset?
                            Models.Order newOrder = new Order {                                
                                CustomerId = member.CustomerId,
                                //OrderId = currentOrderId, //auto create in sql server
                                ShipAddress = member.Address,
                                ShipCity= member.City,
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
                                Models.OrderDetail od = new OrderDetail {
                                    ProductId = p.ProductId,
                                    OrderId = currentOrderId, //current Order                                    
                                    UnitPrice = (decimal)p.UnitPrice,
                                    //Quantity = 1, 
                                    Discount = 0, 
                                    Product = p, //set queried product data
                                };

                                //s::et qty from cart
                                var sc = shoppingCarts.Where(x => x.Pid == p.ProductId).First();
                                if(sc != null)
                                {
                                    od.Quantity = (short) sc.Qty;
                                    ods.Add(od);

                                    //::write to DB
                                    _context.Add(od);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    //error case?
                                }
                            }
                            ViewData["OrderDetails"] = ods;

                            //::update product, reduce stock

                            //::clear shopping cart cookie

                            //return Json(ods);

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

    }
}
