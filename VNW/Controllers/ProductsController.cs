using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VNW.Models;

using VNW.ViewModels;//
using System.Diagnostics;
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
        public IActionResult AddProductInOrder(int? pid, string pname, string img, int? price)
        {
            //::check pid
            int _pid;
            if (pid == 0 || pid == null)
            {
                var res = new { result = "FAIL", detail = "id is null", prodCount=0 };
                return Json(res);
            }
            _pid = (int)pid;

            try
            {
                string pidJSON = null;
                List<VNW.ViewModels.ShoppingCart> shoppingCarts = new List<VNW.ViewModels.ShoppingCart>();
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
                    }
                    else
                    {
                        isUpdateData = true;
                        //Debug.WriteLine("not find ");
                    }
                }
                //Debug.WriteLine("shoppingCarts.Count " + shoppingCarts.Count);
                //ViewBag.shoppingCartsCount = shoppingCarts.Count;

                if(isUpdateData)
                {
                    shoppingCarts.Add(new VNW.ViewModels.ShoppingCart
                    {
                        Pid = _pid,
                        Qty = 1,
                        Name = pname,
                        Price = (int) price,
                        Img =  img //"default.jpg"
                    });
                    pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                }

                HttpContext.Response.Cookies.Append("pidJSON", pidJSON);
                var res2 = new { result = "PASS", detail = pidJSON, prodCount = shoppingCarts.Count };            
                return Json(res2);
            }
            catch
            {
                var res2 = new { result = "Err", detail = "", prodCount=0 };
                return Json(res2);
            }
        }

        //class ShoppingCart
        //{
        //    public int Pid;
        //    public int Qty;
        //    public int Price;
        //    public string Img;
        //    public string Name;
        //}

        //::get data from Cookie - API for testing
        public IActionResult GetShoppingCart()
        {
            try
            {
                string pidJSON = null;
                List<VNW.ViewModels.ShoppingCart> shoppingCarts = new List<VNW.ViewModels.ShoppingCart>();
                pidJSON = HttpContext.Request.Cookies["pidJSON"];
                if (pidJSON == null)
                {
                    var res1 = new { result = "PASS", detail = "no data", prodCount = 0 };
                    return Json(res1);
                }
                else
                {
                    shoppingCarts = JsonConvert.DeserializeObject<List<VNW.ViewModels.ShoppingCart>>(pidJSON);
                    pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                    //Debug.WriteLine("shoppingCarts.Count " + shoppingCarts.Count);
                    //ViewBag.shoppingCartsCount = shoppingCarts.Count;
                    HttpContext.Response.Cookies.Append("pidJSON", pidJSON);
                    var res2 = new { result = "PASS", detail = pidJSON, prodCount = shoppingCarts.Count };
                    return Json(res2);
                }
            }
            catch
            {
                var res2 = new { result = "Err", detail = "", prodCount=0 };
                return Json(res2);
            }
        }

        //::api for remove product from shopping cart
        public IActionResult RemoveShoppingCart(int? pid)
        {
            //if (!_ms.LoginPrecheck(HttpContext.Session))
            //    return RedirectToAction("Login", "Customers");
            //::check pid
            int _pid = 0;
            if (pid == 0 || pid == null)
            {
                var res = new { result = "FAIL", detail = "id is null", prodCount=0 };
                return Json(res);
            }
            _pid = (int)pid;

            try
            {
                string pidJSON = null;
                List<VNW.ViewModels.ShoppingCart> shoppingCarts = new List<VNW.ViewModels.ShoppingCart>();
                pidJSON = HttpContext.Request.Cookies["pidJSON"];
                if (pidJSON == null)
                {
                    var res1 = new { result = "empty", detail = "", count = 0 };
                    return Json(res1);
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
                    }
                    else
                    {
                        //pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                    }
                    var res2 = new { result = "PASS", detail = pidJSON, prodCount = shoppingCarts.Count };
                    return Json(res2);
                }
            }
            catch
            {
                var res2 = new { result = "Err", detail = "", prodCount=0 };
                return Json(res2);
            }

        }

        //::for end user, Shopping Cart|Step|Prepare Order
        public IActionResult PrepareOrder()
        //public async Task<IActionResult> PrepareOrder()
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
                    //await Task.Run(()=> {
                        shoppingCarts = JsonConvert.DeserializeObject<List<VNW.ViewModels.ShoppingCart>>(pidJSON);
                        pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                        //Debug.WriteLine("shoppingCarts.Count " + shoppingCarts.Count);
                        //ViewBag.shoppingCartsCount = shoppingCarts.Count;
                        HttpContext.Response.Cookies.Append("pidJSON", pidJSON);
                        //var res2 = new { result = "PASS", detail = pidJSON, prodCount = shoppingCarts.Count };
                        //return Json(res2);
                        //TempData["td_serverMessage"] = "";

                        if (shoppingCarts.Count <= 0)
                        {
                            TempData["td_serverWarning"] = "訂單是空的，請選擇商品";
                        }
                        else
                            TempData["td_serverInfo"] = "取得資料" + shoppingCarts.Count;
                    //});
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

        //::for end user, update qty
        public IActionResult UpdateQty(int? pid, int? qty)
        {
            //::check pid
            int _pid;
            if (pid == 0 || pid == null || qty == null || qty==0)
            {
                var res = new { result = "FAIL", detail = "id is null"};
                return Json(res);
            }
            _pid = (int)pid;

            try
            {
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
                        pidJSON = JsonConvert.SerializeObject(shoppingCarts);
                        HttpContext.Response.Cookies.Append("pidJSON", pidJSON);
                        var res1 = new { result = "PASS", detail = found};
                        return Json(res1);
                    }
                    else
                    {
                        //::not found
                    }
                }
                //HttpContext.Response.Cookies.Append("pidJSON", pidJSON);
                var res2 = new { result = "NG", detail = "no match data"};
                return Json(res2);
            }
            catch
            {
                var res2 = new { result = "Err", detail = ""};
                return Json(res2);
            }
        }

    }
}
