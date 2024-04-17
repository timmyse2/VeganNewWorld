using System;
using System.Collections.Generic;

namespace VNW.Models
{
    public partial class OrderDetails
    {
        //pk fk
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public decimal UnitPrice { get; set; }
        public short Quantity { get; set; }
        public float Discount { get; set; }

        //np
        public Orders Order { get; set; }
        public Products Product { get; set; }
    }
}
