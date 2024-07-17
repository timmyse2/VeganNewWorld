using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VNW.Models
{
    public partial class Employee
    {
        public Employee()
        {
        }
        //::PK
        [Display(Name = "客戶ID")]
        public string EmployeeId { get; set; }

        [Display(Name = "密碼")]
        public string PasswordEncoded { set; get; }
        [Display(Name = "姓名")]
        public string Name { get; set; }
        [Display(Name = "職稱")]
        public string ContactTitle { get; set; }        
        [Display(Name = "電話")]
        public string Phone { get; set; }
        [Display(Name = "層級")]
        public int Level { get; set; }		
    }
}
