using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;// for [Key]

namespace VNW.ViewModels
{
    public class ProductsViewModel
    {
        [Key]        
        public int ProductId { get; set; }
        //[Display(Name = "商品名")]
        public string ProductName { get; set; }
        //[Display(Name = "單價")]
        public decimal? UnitPrice { get; set; }
        //[Display(Name = "份量單位")]
        public string QuantityPerUnit { get; set; }
        //[Display(Name = "分類ID")]
        public int? CategoryId { get; set; }
        [Display(Name = "庫存")]
        public short? UnitsInStock { get; set; }
        public string Picture { get; set; }
    }

    //::
    public class TopProduct
    {
        public int Pid { get; set; }
        public string Name { get; set; }
        //public int Sum { get; set; }
        public int Qty { get; set; } //long
        public int Count { get; set; }
    }

}
