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
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
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
                return RedirectToAction(nameof(Index));
            }
            //ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId",
                "OrderId", orderDetail.OrderId);

            ViewData["ProductId"] = new SelectList(_context.Products, "ProductName","ProductId"
                , orderDetail.ProductId);
            //"ProductId", orderDetail.ProductId);
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
                //.Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderId == oid
                       && m.ProductId == pid
                );

            if (orderDetail == null)
            {
                return NotFound();
            }
            //ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", 
                "OrderId", orderDetail.OrderId);

            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId",
                "ProductName", orderDetail.ProductId);
                //"ProductId", orderDetail.ProductId);
            return View(orderDetail);
        }

        // POST: OrderDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,ProductId,UnitPrice,Quantity,Discount")] OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderId)
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
            return RedirectToAction(nameof(Index));
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails.Any(e => e.OrderId == id);
        }
    }
}
