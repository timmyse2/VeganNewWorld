using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VNW.Models
{
    public partial class Order
    {
        public Order()
        {
            //OrderDetails = new HashSet<OrderDetails>();
        }

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

        //::NP
        [Display(Name = "客戶")]
        public Customer Customer { get; set; }
        //[Display(Name = "員工")]
        //public Employees Employee { get; set; }
        //public Shippers ShipViaNavigation { get; set; }
        [Display(Name = "訂單明細")]
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
