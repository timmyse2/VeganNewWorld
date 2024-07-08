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
        public async Task<IActionResult> Index(int? page, string condition)
        {
            //::check admin
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (!_ms.CheckAdmin(HttpContext.Session) && UserLevel != "2B")
                return Content("You have no right to access this page");

            //::condition
            IQueryable<VNW.Models.Product> q0 = null;
            //var q0 = _context.Products;

            string _condition = null;
            if (condition == null) //no condition in url            
                _condition = HttpContext.Request.Cookies["condition_shopProduct"];            
            else            
                _condition = condition;                    
            switch (_condition)
            {
                case "reserved":
                    q0 = _context.Products.Where(p => p.UnitsReserved > 0)
                        .OrderByDescending(x => x.UnitsReserved)
                        ;
                    break;
                case "less": 
                    q0 = _context.Products.Where(p => p.UnitsInStock <= 0 || p.UnitsInStock <= p.ReorderLevel)
                        .OrderByDescending(x => x.LastModifiedTime);
                    break;
                case "error": //error only 
                    q0 = _context.Products.Where(p => p.UnitsReserved < 0 || p.UnitsReserved > p.UnitsInStock)
                        .OrderByDescending(x => x.LastModifiedTime)
                        ;
                    break;
                case "all": //all with page  
                    q0 = _context.Products.OrderByDescending(x => x.ProductId);
                    //clear speical condition
                    HttpContext.Response.Cookies.Append("condition_shopProduct", "");
                    _condition = null;
                    break;
                default: 
                    //use condition from cookie
                    q0 = _context.Products;                    
                    break;
            }            
            if(_condition != null)
                HttpContext.Response.Cookies.Append("condition_shopProduct", _condition);
            ViewData["condition"] = _condition;

            #region page
            int ipp = 10; // item per page
            int _page = 1, _take = ipp, _skip = 0;
            if (page != null)
                _page = (int)page - 1;
            else
            {
                //::get page from cookie
                var _cookepage = HttpContext.Request.Cookies["page_shopProduct"];
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
            HttpContext.Response.Cookies.Append("page_shopProduct", _page.ToString());
            int totalCount = q0.Count();
                //_context.Products.Count();
            int totalPages = totalCount / ipp;
            if (_page >= totalPages)
                _page = totalPages; //::debug
            _skip = _page * ipp; //(totalPages- _page) * ipp;
            if (_skip < 0) _skip = 0;
            ViewData["page"] = _page;
            ViewData["totalCount"] = totalCount;
            ViewData["ipp"] = ipp;
            #endregion

            ViewBag.UserAccount =
                _ms.GetMySession("UserAccount", HttpContext.Session);
            ViewBag.ShopAccount =
                _ms.GetMySession("ShopAccount", HttpContext.Session);
            ViewBag.UserLevel = UserLevel;

            //var query = _context.Products
            var query = q0
                .Include(p => p.Category)
                //.OrderByDescending(x => x.UnitsReserved)
                //.ThenByDescending(x => x.ProductId)                
                .Skip(_skip).Take(_take);
            return View(await query.ToListAsync());
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
            [Bind("ProductId,ProductName,CategoryId,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued,Picture,Description,UnitsReserved,LastModifiedTime"
                )] Product product)
        {
            //::check Shop
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            //::check admin
            if (!_ms.CheckAdmin(HttpContext.Session))
                return Content("You have no right to access this page");
            if (!_ms.LoginPrecheck(HttpContext.Session))
                return RedirectToAction("Login", "Customers");

            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //::precheck data be updated by another 
                    var originalProduct = await _context.Products
                        .AsNoTracking() //KEY
                        .Where(x => x.ProductId == id)
                        .Select(x => new { x.ProductId, x.LastModifiedTime})
                        .FirstOrDefaultAsync();
                    if (originalProduct == null)
                    {
                        return Content("Error: original data is null ");
                    }
                    //::check timeStamp or RowVersion
                    if (originalProduct.LastModifiedTime.ToString() != product.LastModifiedTime.ToString())
                    {
                        return Content("TimeStamp is not match! Someone changed data at the same time");
                    }

                    //::precheck then auto fine tune
                    if (product.UnitsOnOrder == null)
                        product.UnitsOnOrder = 0;
                    if (product.UnitsInStock == null)
                        product.UnitsInStock = 0;
                    if (product.ReorderLevel == null)
                        product.ReorderLevel = 0;
                    if (product.UnitsReserved == null)
                        product.UnitsReserved = 0;

                    product.LastModifiedTime = DateTime.Now;//timestamp

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
                //TempData["td_serverMessage"] = "Updated";
                //return RedirectToAction(nameof(Index));
                //return RedirectToAction(nameof(ProductDetailForShop));
                //return RedirectToAction("ProductDetailForShop", product.ProductId);
                return RedirectToAction("ProductDetailForShop//" + product.ProductId);
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
        public async Task<IActionResult> ProductList(int? cat, string catName, string search)
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

            if (search != null && search.Length >= 1)
            {
                if (search.Length >= 10)
                    search = search.Substring(0, 10);
                var q2 = _context.Products
                    .Where(x => x.Picture != null
                    //&& x.Discontinued == false  
                    && (x.ProductName.Contains(search) || x.Description.Contains(search))
                    )
                    //.Include(p => p.Category)
                    .Take(30)
                    ;
                var q2List = await q2.ToListAsync();
                ViewBag.searchKey = search;
                return View(q2List);
            }

            var veganNewWorldContext =
                _context.Products
                .Where(p => p.CategoryId == cat && p.Picture != null) //::cat id
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
            ViewData["UserLevel"] = _ms.GetMySession("UserLevel", HttpContext.Session);
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
                            if (query.UnitsReserved != null)
                            {
                                _stock = (short)(query.UnitsInStock - query.UnitsReserved);
                            }
                            else
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
                            .Select(x => new {x.ProductId, x.UnitsInStock, x.UnitsReserved })
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
                                {
                                    //::
                                    if(q.UnitsReserved != null)
                                    {
                                        sc.Stock = (short)(q.UnitsInStock- q.UnitsReserved);
                                        //for 
                                        if(q.UnitsInStock <= 0 ||  q.UnitsReserved <0)
                                        {
                                            sc.Stock = 0;
                                        }
                                    }
                                    else
                                        sc.Stock = (short)q.UnitsInStock;
                                }                                    
                            }
                            Debug.WriteLine("\n EOD ");
                        }
                        #endregion

                        if (shoppingCarts.Count <= 0)
                        {
                            TempData["td_serverWarning"] = "訂單是空的，請選擇商品";
                        }
                        //else
                          //  TempData["td_serverInfo"] = "取得資料" + shoppingCarts.Count;
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
                                .Select(x=> new { x.ProductId, x.UnitsInStock, x.UnitsReserved }).First();                                
                                //.Find(_pid);
                                if (query != null)
                                {
                                    if (query.UnitsReserved != null)
                                    {
                                        found.Stock = (short)
                                            (query.UnitsInStock - query.UnitsReserved);

                                        //::try
                                        if (query.UnitsReserved < 0 || query.UnitsInStock <= 0)
                                            found.Stock = 0;
                                    }
                                    else
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


        //::api for get StockReserved 2B
        public async Task<IActionResult> GetStockReserved(int? id)
        {
            string _result = "tbc", _detail = "tbc", productName = "", picture = "";
            short stock = 0, reserved = 0, unitPrice = 0;
            //check user level

            //::check pid
            int _pid;
            if (id == 0 || id == null)
            {
                _result = "FAIL"; _detail = "id or key value is null";
            }
            else
            {
                try
                {
                    _pid = (int)id;
                    //string pidJSON = null;
                    var query = await _context.Products
                      .Where(x => x.ProductId == _pid && x.Picture != null)
                      .AsNoTracking()
                      .Select(x => new { x.ProductId, x.UnitsInStock, x.UnitsReserved,
                          x.ProductName, x.UnitPrice, x.Picture })
                      .FirstOrDefaultAsync();

                    if(query != null)
                    {
                        stock = (short)query.UnitsInStock;
                        reserved = (short)query.UnitsReserved;
                        productName = query.ProductName;
                        unitPrice = (short) query.UnitPrice;
                        picture = query.Picture;
                        _result = "PASS"; _detail = "";
                    }
                    else
                    {
                        _result = "FAIL"; _detail = "product data is null";
                    }

                }
                catch (Exception ex)
                {
                    _result = "ERROR"; _detail = "" + ex.ToString();
                }

            }
            var res = new { result = _result, detail = _detail, stock, reserved, productName, unitPrice, picture };
            return Json(res);
        }

        //::api for get add new product item rder for 2B
        public async Task<IActionResult> GetProductReadyList(string search)
        {
            string _result = "tbc", _detail = "tbc";
            string key = "";
            var q0 = _context.Products
                .Take(30) //top limit
                .Where(x => (x.UnitsInStock - x.UnitsReserved) > 0 &&
                    x.Discontinued == false
                    );
            if(search != null && search.Length >= 1)
            {
                if (search.Length >= 10)
                    search = search.Substring(0, 10);

                q0 = _context.Products
                .Where(x => (x.UnitsInStock - x.UnitsReserved) > 0 &&
                    x.Discontinued == false &&
                    (x.ProductName.Contains(search) || x.Description.Contains(search))
                    )
                .Take(30); //top limit;
                key = search;
            }
            int rowCount = q0.Select(x => new {x.UnitsInStock }).Count(); // x.UnitsReserved,
            var ps = await q0.ToArrayAsync();
            var res = new { result = _result, detail = _detail, key, rowCount, ps};
            return Json(res);
        }

        //::for level 2B Shop
        public async Task<IActionResult> ProductDetailForShop(int? id)
        {
            //::check Shop
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                //return Content("You have no right to access this");
                return RedirectToAction("Login", "Customers");
            }

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

            //var currentUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}{HttpContext.Request.QueryString}";
            //ViewData["currentUrl"] = currentUrl;
            //ViewData["currentPath"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
            ViewData["currentHost"] = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            ViewData["currentBase"] = $"{HttpContext.Request.PathBase}";
            //ViewData["currentBody"] = $"{HttpContext.Request.Body }";
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> EditForShop(int? id)
        {
            //::check admin
            //if (!_ms.CheckAdmin(HttpContext.Session))
            //    return Content("You have no right to access this page");
            //if (!_ms.LoginPrecheck(HttpContext.Session))
            //    return RedirectToAction("Login", "Customers");

            //::check 2B
            //::check Shop
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                //return Content("You have no right to access this");
                return RedirectToAction("Login", "Customers");
            }

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
        public async Task<IActionResult> EditForShop(int id,
            [Bind("ProductId,ProductName,CategoryId,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued,Picture,Description,UnitsReserved,LastModifiedTime"
                )] Product product)
        {
            //::check Shop
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                //return Content("You have no right to access this");
                return RedirectToAction("Login", "Customers");
            }

            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //::precheck data be updated by another 
                    var originalProduct = await _context.Products
                        .AsNoTracking() //KEY
                        .Where(x => x.ProductId == id)
                        .Select(x => new { x.ProductId, x.LastModifiedTime })
                        .FirstOrDefaultAsync();
                    if (originalProduct == null)
                    {
                        return Content("Error: original data is null ");
                    }

                    //::precheck then auto fine tune
                    if (product.UnitsOnOrder == null)
                        product.UnitsOnOrder = 0;
                    if (product.UnitsInStock == null)
                        product.UnitsInStock = 0;
                    if (product.ReorderLevel == null)
                        product.ReorderLevel = 0;
                    if (product.UnitsReserved == null)
                        product.UnitsReserved = 0;

                    //::check timeStamp or RowVersion
                    if (originalProduct.LastModifiedTime.ToString() != product.LastModifiedTime.ToString())
                    {
                        //return Content("TimeStamp is not match! Someone changed data at the same time");
                        string msg = "注意! 有其它用戶也在修改資料，發生衝突。(建議按[取消]並先另記錄目前的數據)";
                        TempData["td_serverWarning"] = msg;
                        ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "CategoryId", "CategoryName", product.CategoryId);
                        return View(product);
                        //EditForShop
                    }
                    product.LastModifiedTime = DateTime.Now;
                    // _context.Entry(entity).OriginalValues["RowVersion"] 

                    //Debug.WriteLine(" " + _context.Entry(product).OriginalValues["UnitsReserved"]);
                    //Debug.WriteLine(" " + _context.Entry(product).OriginalValues["UnitsOnOrder"]);

                    _context.Update(product);
                    //_context.Products.Update(product); // indicate to product from View
                    //_context.Entry(product).State = EntityState.Modified;
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
                //TempData["td_serverMessage"] = "Updated";
                //return RedirectToAction(nameof(Index));
                //return RedirectToAction(nameof(ProductDetailForShop));
                //return RedirectToAction("ProductDetailForShop", product.ProductId);
                //return RedirectToAction("ProductDetailForShop//" + product.ProductId);
                //return RedirectToAction($"ProductDetailForShop?id=" + product.ProductId);
                return RedirectToAction("ProductDetailForShop", new { id = product.ProductId });
            }
            //ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "CategoryId", "CategoryId", product.CategoryId);
            ViewData["CategoryId"] = new SelectList(_context.Set<Category>(), "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        //::API for shop, add stock value quick
        public async Task<IActionResult> QuickAddStock(int pid, int qty)
        {
            string _result = "tbc", _detail = "tbc";
            int _NewStock = 0; string _timeStamp = "";

            //::check Shop
            string UserLevel = _ms.GetMySession("UserLevel", HttpContext.Session);
            if (UserLevel != "2B" && UserLevel != "1A")
            {
                _result = "FAIL"; _detail = "you have no right to access it";
                var res0 = new
                {
                    result = _result,
                    detail = _detail,
                    NewStock = _NewStock,
                    timeStamp = _timeStamp
                };
                return Json(res0);
            }

            //::check pid and qty (add stock)
            //int _pid;
            if (pid == 0 || pid == null || qty == null || qty == 0)
            {
                //var res = new { result = "FAIL", detail = "id is null" };
                //return Json(res);
                _result = "FAIL"; _detail = "id or key value is null";
            }
            else if (qty <= 0 || qty > 100)
            {
                _result = "FAIL"; _detail = "acc qty is over range";
            }
            else
            {
                //::query p
                var p = await _context.Products.Where(x => x.ProductId == pid)
                    .FirstOrDefaultAsync();
                if(p != null)
                {

                    var oldp = await _context.Products.Where(x => x.ProductId == pid)
                        .Select(x => new {x.ProductId, x.LastModifiedTime})
                        .FirstOrDefaultAsync();

                    if(oldp.LastModifiedTime.ToString() != p.LastModifiedTime.ToString())
                    {
                        _result = "fail";
                        _detail = "可能同時有人在修改資料";
                        //error
                        var res0 = new
                        {
                            result = _result,
                            detail = _detail,
                            NewStock = _NewStock,
                            timeStamp = _timeStamp
                        };
                        return Json(res0);
                    }

                    //::update p (add stock...)
                    p.UnitsInStock += (short)qty;
                    //p.UnitsOnOrder -= (short)qty; //not now
                    p.LastModifiedTime = DateTime.Now;
                    _context.Update(p);
                    await _context.SaveChangesAsync();
                    //::get new stock, and timestamp
                    _NewStock = (int)p.UnitsInStock;                    
                    _timeStamp = p.LastModifiedTime.ToString();                    
                    _result = "PASS"; _detail = "done";
                }
                else
                {
                    _result = "FAIL"; _detail = "can not find data";
                }                
            }
            var res = new {
                result = _result, detail = _detail,
                NewStock = _NewStock, timeStamp = _timeStamp
            };
            return Json(res);
        }


        public async Task<IActionResult> Analysis(int id)
        {
            var p = await _context.Products.FindAsync(id);

            DateTime specificDate = new DateTime(2024, 6, 4, 12, 0, 0);

            var ods = await _context.OrderDetails
                .Where(x => x.ProductId == id && x.OrderId == x.Order.OrderId
                    && ((int)x.Order.Status < 20 || x.Order.Status == null)
                    && x.Order.OrderDate > specificDate)
                .Include(x => x.Order)
                .OrderByDescending(x => x.Order.OrderDate)
                .ToListAsync();
            ViewData["p"] = p;
            ViewData["ods"] = ods;
            return View(ods);
        }

    }
}
