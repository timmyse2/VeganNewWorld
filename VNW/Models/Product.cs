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

        [Display(Name = "庫存")]
        public short? UnitsInStock { get; set; }

        [Display(Name = "訂購量")]
        public short? UnitsOnOrder { get; set; }

        [Display(Name = "安全庫存量(不夠請補貨)")] //續訂級別
        public short? ReorderLevel { get; set; }

        [Display(Name = "已下架")]
        public bool Discontinued { get; set; }

        [Display(Name = "圖例")]
        public string Picture { get; set; }

        [Display(Name = "描述")] //new       
        public string Description { get; set; }

        //::NP
        [Display(Name = "分類")]
        public Category Category { get; set; }
        //[Display(Name = "供應商")]
        //public Suppliers Supplier { get; set; }
        [Display(Name = "明細")]
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
