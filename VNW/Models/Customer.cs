using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VNW.Models
{
    public partial class Customer
    {
        public Customer()
        {
            ////CustomerCustomerDemo = new HashSet<CustomerCustomerDemo>();
            //Orders = new HashSet<Orders>();
        }
        //::PK
        [Display(Name = "客戶ID")]
        public string CustomerId { get; set; }

        [Display(Name = "公司組織名稱")]
        public string CompanyName { get; set; }
        [Display(Name = "名稱")]
        public string ContactName { get; set; }
        //[Display(Name = "職稱")]
        //public string ContactTitle { get; set; }
        [Display(Name = "地址")]
        public string Address { get; set; }
        //[Display(Name = "城市")]
        public string City { get; set; }
        //public string Region { get; set; }
        //[Display(Name = "區號")]
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        //public string Fax { get; set; }

        ////public ICollection<CustomerCustomerDemo> CustomerCustomerDemo { get; set; }
        [Display(Name = "訂單")]
        public ICollection<Order> Orders { get; set; }
    }
}
