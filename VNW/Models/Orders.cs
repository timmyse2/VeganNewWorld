﻿using System;
using System.Collections.Generic;

namespace VNW.Models
{
    public partial class Orders
    {
        public Orders()
        {
            //OrderDetails = new HashSet<OrderDetails>();
        }

        //pk
        public int OrderId { get; set; }

        //fk
        public string CustomerId { get; set; }
        //public int? EmployeeId { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }

        public int? ShipVia { get; set; }
        public decimal? Freight { get; set; }
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        //public string ShipRegion { get; set; }
        public string ShipPostalCode { get; set; }
        public string ShipCountry { get; set; }

        //np
        public Customers Customer { get; set; }
        //public Employees Employee { get; set; }
        //public Shippers ShipViaNavigation { get; set; }
        public ICollection<OrderDetails> OrderDetails { get; set; }
    }
}
