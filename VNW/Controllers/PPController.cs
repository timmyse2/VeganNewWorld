﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VNW.Models;

//using Microsoft.AspNetCore.Mvc;

namespace VNW.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PPController : ControllerBase
    {
        private readonly VeganNewWorldContext _context;

        public PPController(VeganNewWorldContext context)
        {
            _context = context;
        }

        // GET: api/PP
        [HttpGet]
        //public IEnumerable<Product> GetProducts()
        //public async Task<IEnumerable<Product>> GetProducts()
        public async Task<IActionResult> GetProducts()
        {
            var query = _context.Products.Take(10);
            int count = await query.CountAsync();
            var products = await query.ToListAsync();
            //return _context.Products;
            //return ps;
            return Ok(new {result="PASS", count, products});
        }

        //GET: api/PP/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }
            //return Ok(product);
            return Ok(new { result = "PASS", product });
        }
        //[HttpGet]
        //public IActionResult GetProducts([FromQuery] string category = null, [FromQuery] int? minPrice = null, [FromQuery] int? maxPrice = null)
        //{
        //    //var products = GetFilteredProducts(category, minPrice, maxPrice);
        //    //return Ok(products);
        //    return Ok(new { category, minPrice, maxPrice });
        //}

            // GET: api/PP/ABC/1/100
        [HttpGet("{category}/{minPrice}/{maxPrice}")]
        //[HttpGet("{category}")]
        //[HttpGet]
        public async Task<IActionResult> Cate(int category, decimal? minPrice, decimal? maxPrice)
        {
            var ps = _context.Products.Where(p =>
                p.UnitPrice >= minPrice && p.UnitPrice <= maxPrice && p.CategoryId == category);
            int ps_count = ps.Count();

            var ps_result = await ps.ToListAsync();
            return Ok(new { category, minPrice, maxPrice, ps_count, ps_result });
            //return Content(" " + category);
            //return JsonResult(new {category });
        }

        //GET: api/PP/girl/5/
        [HttpGet("{name}/{id}")]
        //public async Task<IActionResult> SearchPS2([FromRoute] string Name, [FromRoute] int id)
        public async Task<IActionResult> SearchPS2([FromRoute] string Name, [FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var products = await _context.Products
                .Where(p => p.ProductId == id || p.ProductName.Contains(Name))
                .ToListAsync();

            if (products == null)
            {
                return NotFound();
            }

            return Ok(products);
        }

        // PUT: api/PP/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct([FromRoute] int id, [FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string result = "PASS";   
            return Ok(new { result, id, product });

            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/PP
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //_context.Products.Add(product);
            //await _context.SaveChangesAsync();
            //string result = "PASS";
            return CreatedAtAction("GetProduct", new {id = product.ProductId }, product);
            //return Ok(new { result});
        }

        //:: try to add Patch action, updated from Put
        // Patch: api/PP/8
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchProduct([FromRoute] int id, [FromBody] Product product)
        {
            string result = "";
            string detail = "";

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.ProductId)
            {
                return BadRequest();
            }

            var p = await _context.Products.FindAsync(id);
            if(p == null)
            {
                return NotFound();
            }
            //p.CategoryId = product.CategoryId;
            p.ReorderLevel = product.ReorderLevel;
            p.LastModifiedTime = DateTime.Now;

            _context.Entry(p).State = EntityState.Modified;
            //_context.Update(p);
            await _context.SaveChangesAsync();

            result = "PASS";
            detail = "";
            return Ok(new {result, detail , data_updated = p});
        }

        // DELETE: api/PP/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            //_context.Products.Remove(product);
            //await _context.SaveChangesAsync();

            return Ok(product);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}