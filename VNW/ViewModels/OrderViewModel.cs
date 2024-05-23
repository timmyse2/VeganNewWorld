using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
//using System.ComponentModel; // Description
using VNW.Common; //enum
using VNW.Models; //enum or other model class

namespace VNW.ViewModels
{
    public class OrderViewModel
    {
        //::PK
        [Display(Name = "訂單編號")]
        public int OrderId { get; set; }

        //::FK
        [Display(Name = "客戶")]
        public string CustomerId { get; set; }
        //[Display(Name = "店家")] //員工
        //public int? EmployeeId { get; set; }

        [Display(Name = "訂購時間")]
        public DateTime? OrderDate { get; set; }
        [Display(Name = "需求時間")]
        public DateTime? RequiredDate { get; set; }
        [Display(Name = "寄送時間")]
        public DateTime? ShippedDate { get; set; }

        [Display(Name = "運送方式")]
        public int? ShipVia { get; set; }

        [Display(Name = "運費")]
        public decimal? Freight { get; set; }

        [Display(Name = "出貨地名")]
        public string ShipName { get; set; }
        [Display(Name = "地址")]
        public string ShipAddress { get; set; }
        [Display(Name = "城市")]
        public string ShipCity { get; set; }
        //[Display(Name = "出貨區域")]
        //public string ShipRegion { get; set; 
        [Display(Name = "郵遞區號")]
        public string ShipPostalCode { get; set; }
        [Display(Name = "國家")]
        public string ShipCountry { get; set; }

        //::new
        [Display(Name = "發票資訊")]
        public InvoiceEnum? Invoice { get; set; }
        [Display(Name = "付款方式")]
        public PayEnum? Payment { get; set; }
        [Display(Name = "總額")]
        public decimal? TotalPriceSum { get; set; }

        //::Products + Shopping Cart
        int Pid;
        string PName;
        int Qty;
        int Price;
        int Stock;
        int OnOrder;
        int Discort;
    }
}
