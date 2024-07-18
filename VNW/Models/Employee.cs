using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;// for NotMapped

namespace VNW.Models
{
    public partial class Employee
    {
        public Employee()
        {
            //EmployeeTerritories = new HashSet<EmployeeTerritories>();
            //InverseReportsToNavigation = new HashSet<Employees>();
            //Orders = new HashSet<Orders>();
        }
        //::PK        
        public int Id { get; set; }
        //public string EmployeeId { get; set; }
        [Display(Name = "郵件帳號")]
        public string Email { set; get; }

        [Display(Name = "密碼")]
        public string PasswordEncoded { set; get; }
        [Display(Name = "姓名")]
        public string Name { get; set; }
        [Display(Name = "職稱")]
        public string Title { get; set; }
        
        //[Display(Name = "層級")]
        //public int Level { get; set; }

        //public string LastName { get; set; }
        //public string FirstName { get; set; }        
        //public string TitleOfCourtesy { get; set; }
        //public DateTime? BirthDate { get; set; }
        //public DateTime? HireDate { get; set; }
        //public string Address { get; set; }
        //public string City { get; set; }
        //public string Region { get; set; }
        //public string PostalCode { get; set; }
        //public string Country { get; set; }
        [Display(Name = "分機")]
        public string Extension { get; set; }
        //[Display(Name = "家裡電話")]
        //public string HomePhone { get; set; }
        //public byte[] Photo { get; set; }
        //public string Notes { get; set; }
        [Display(Name = "上司")]
        public int? ReportsTo { get; set; }
        [Display(Name = "相片")]
        public string PhotoPath { get; set; }

        //::NP
        [NotMapped]
        [Display(Name = "上司")]
        public Employee ReportsToNavigation { get; set; }
        [NotMapped]
        [Display(Name = "下屬")]
        public ICollection<Employee> InverseReportsToNavigation { get; set; }

        //[Display(Name = "接手的訂單")]                
        //public ICollection<Order> Orders { get; set; }

        //[Display(Name = "管區")]
        //public ICollection<EmployeeTerritories> EmployeeTerritories { get; set; }
    }
}
