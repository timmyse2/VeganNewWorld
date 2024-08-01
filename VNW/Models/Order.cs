using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel; // Description

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

        //::<new col><28 May 2025>
        [Display(Name = "狀態")]
        public OrderStatusEnum? Status { get; set; }
        [Display(Name = "支付方式")]
        public VNW.Common.PayEnum? Payment { get; set; }
        //public int? Payment { get; set; }

        //::for Optimistic Concurrency Control
        public byte[] TimeStamp { get; set; } //RowVersion

        //::NP
        [Display(Name = "客戶")]
        public Customer Customer { get; set; }
        //[Display(Name = "負責員工")]
        //public Employees Employee { get; set; }
        //public Shippers ShipViaNavigation { get; set; }
        [Display(Name = "訂單明細")]
        public ICollection<OrderDetail> OrderDetails { get; set; }

        //public List<string> shipViaTypes = new List<string>() {
        //    "宅配",
        //    "超商取貨",            
        //    "魔女宅急便",
        //};
    }

    public enum ShipViaTypeEnum
    {
        [Display(Name = "宅配")]
        [Description("宅配")]
        Home, //宅配
        [Display(Name = "超商取貨")]
        [Description("超商取貨")]
        Shop, //超商取貨
        [Display(Name = "魔女宅急便")]
        [Description("魔女宅急便")]
        Witch, //魔女宅急便
    }
    public enum OrderStatusEnum
    {
        [Display(Name = "尚未接單")]
        None = 00, //or null

        //::in shop side
        [Display(Name = "店家處理中")]
        Got = 10,
        [Display(Name = "已出貨")]
        Shipped = 20,

        //::out of shop
        [Display(Name = "運送中")]
        Shipping = 30,
        [Display(Name = "已完成")]
        Finish = 100,

        //::other events
        [Display(Name = "取消中")]
        Canceling = 240,
        [Display(Name = "已取消")]        
        Cancelled = 250,
    }

}
