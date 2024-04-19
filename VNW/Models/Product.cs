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
        public int ProductId { get; set; }

        [Display(Name = "商品名")]
        public string ProductName { get; set; }

        //::FK
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

        [Display(Name = "續訂級別")]
        public short? ReorderLevel { get; set; }

        [Display(Name = "已下架")]
        public bool Discontinued { get; set; }

        [Display(Name = "圖例")]
        public string Picture { get; set; }

        [Display(Name = "描述")]        
        public string Description { get; set; }

        //::NP
        [Display(Name = "分類")]
        public Category Category { get; set; }
        //public Suppliers Supplier { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
