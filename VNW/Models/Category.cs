using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;// for NotMapped

namespace VNW.Models
{
    public partial class Category
    {
        //public Category()
        //{
        //    Products = new HashSet<Product>();
        //}

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        //public byte[] Picture { get; set; }


        //::NP
        public ICollection<Product> Products { get; set; }

        //::not map to DB, for view only
        [NotMapped]
        public int CatCount { get; set; }
    }
}
