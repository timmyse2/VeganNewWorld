using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;// for [Key]

namespace VNW.Models
{
    public partial class OrderDetail
    {
        //PK, FK
        //[Key]
        [Display(Name = "訂單編號")]
        public int OrderId { get; set; }
        //[Key]
        [Display(Name = "產品編號")]
        public int ProductId { get; set; }

        [Display(Name = "單價")]
        public decimal UnitPrice { get; set; }
        [Display(Name = "數量")]
        public short Quantity { get; set; }
        [Display(Name = "折扣")]
        public float Discount { get; set; }
        public byte[] RowVersion { get; set; }

        //NP
        [Display(Name = "購物車訂單")]
        public Order Order { get; set; }
        [Display(Name = "產品")]
        public Product Product { get; set; }
    }
}
