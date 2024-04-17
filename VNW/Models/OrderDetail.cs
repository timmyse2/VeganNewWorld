using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;// for [Key]

namespace VNW.Models
{
    public partial class OrderDetail
    {
        //pk fk
        //[Key]
        public int OrderId { get; set; }
        //[Key]
        public int ProductId { get; set; }

        public decimal UnitPrice { get; set; }
        public short Quantity { get; set; }
        public float Discount { get; set; }

        //np
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
