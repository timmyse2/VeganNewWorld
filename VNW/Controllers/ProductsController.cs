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
using VNW.Common;

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
            if (!_ms.CheckAdmin(HttpContext.Session))
                return Content("You have no right to access this page");

            if (!LoginPrecheck())
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
            if (!LoginPrecheck())
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
            if (!LoginPrecheck())
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
            if (!LoginPrecheck())
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


        //::api for adding p.id in cookie 
        public async Task<IActionResult> AddProductInOrder(int? pid)
        {
            //check pid

            //cookie p.id list
            //count
            //do not re-add item

            //::FAIL case

            //::PASS case
            //return Json("");

            if(true)
            {
                var res = new { result = "PASS", detail = "pid=" + pid };
                return Json(res);
            }

            else
            {
                var res = new { result = "FAIL", detail = "xxx" };
                return Json(res);
            }
            
            //return Content("result:'pass'");
        }


        ////::my api for session
        //public bool SetMySession(string key, string val)
        //{
        //    try
        //    {
        //        //::string to byte[]
        //        byte[] bv = System.Text.Encoding.Default.GetBytes(val); ;
        //        HttpContext.Session.Set(key, bv);
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        ////::my api for session
        //public string GetMySession(string key)
        //{
        //    string _str = null;
        //    try
        //    {
        //        byte[] bv = null;
        //        HttpContext.Session.TryGetValue(key, out bv);
        //        //::byte[] to string
        //        _str = System.Text.Encoding.Default.GetString(bv);
        //        //System.Diagnostics.Debug.WriteLine(" ss" + _str.Length);
        //    }
        //    catch
        //    {
        //    }
        //    return _str;
        //}
    }
}
