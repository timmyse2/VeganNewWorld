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

        public OrderDetailsController(VeganNewWorldContext context)
        {
            _context = context;
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index()
        {
            var veganNewWorldContext = _context.OrderDetails.Include(o => o.Order).Include(o => o.Product);
            return View(await veganNewWorldContext.ToListAsync());
        }

        // GET: OrderDetails/Details/5
        //public async Task<IActionResult> Details(int? id)
        public async Task<IActionResult> Details(int? oid, int? pid)
        {
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
            //if (GetMySession("IsAdmin") != "YES")
            if(false)
            {
                TempData["td_serverMessage"] = "權限不足";
                return RedirectToAction("Index");
            }            

            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId");
            //ViewData["ProductId"] = new SelectList(_context.Products, "ProductId",
            //  "ProductName");
            //"ProductId");

            #region
            
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
        public async Task<IActionResult> Edit(int OrderId, [Bind("OrderId,ProductId,UnitPrice,Quantity,Discount")] OrderDetail orderDetail)
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
    }
}
