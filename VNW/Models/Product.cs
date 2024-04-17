using System;
using System.Collections.Generic;

namespace VNW.Models
{
    public partial class Product
    {
        public Product()
        {
            //OrderDetails = new HashSet<OrderDetails>();
        }

        //pk
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        //fk
        //public int? SupplierId { get; set; }
        public int? CategoryId { get; set; }

        public string QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }
        public short? ReorderLevel { get; set; }
        public bool Discontinued { get; set; }

        //np
        public Category Category { get; set; }
        //public Suppliers Supplier { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
