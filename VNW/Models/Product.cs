using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VNW.Models
{
    public partial class Product
    {
        public Product()
        {
            //OrderDetails = new HashSet<OrderDetails>();
        }

        //::PK
        [Key]
        [Display(Name = "商品編號")]
        public int ProductId { get; set; }

        [Display(Name = "商品名")]
        public string ProductName { get; set; }

        //::FK
        //[Display(Name = "供應商")]
        //public int? SupplierId { get; set; }
        [Display(Name = "分類")]
        public int? CategoryId { get; set; }

        [Display(Name = "份量單位")]
        public string QuantityPerUnit { get; set; }

        [Display(Name = "單價")]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "店內庫存")]
        public short? UnitsInStock { get; set; }

        [Display(Name = "向供應商的訂購量")] //採購流程, 向供應商下訂單
        public short? UnitsOnOrder { get; set; }

        [Display(Name = "安全庫存量")] //續訂級別 (不夠請補貨)
        public short? ReorderLevel { get; set; }

        [Display(Name = "已下架無法販售")]
        public bool Discontinued { get; set; }

        [Display(Name = "圖例")]
        public string Picture { get; set; }

        [Display(Name = "描述")] //new       
        public string Description { get; set; }

        //::prepare UnitsReserved for more complex order system
        [Display(Name = "預訂購量")] //預留標記
        public short? UnitsReserved { get; set; }

        //::NP
        [Display(Name = "分類")]
        public Category Category { get; set; }
        //[Display(Name = "供應商")]
        //public Suppliers Supplier { get; set; }
        [Display(Name = "明細")]
        public ICollection<OrderDetail> OrderDetails { get; set; }

        //::for Optimistic Concurrency Control
        //public byte[] TimeStamp { get; set; } //RowVersion
        public DateTime LastModifiedTime { get; set; }
    }
}
