using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EmployeeManagementSystem.Models
{
    
    public class Employee
    {
        

        [Required]
        [Range(1, 999999, ErrorMessage = "Enter Valid Employee Id") ]
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Employee name is required.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Enter a valid name (only letters and spaces are allowed).")]
        public string EmployeeName { get; set; }

        public string DepartmentName {  get; set; }

        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        //[Column("dob")] 
        public DateTime DateOfBirth { get; set; }

        [Required (ErrorMessage ="Gender is required")]
        public string Gender { get; set; }

        [Required]
        public string Address { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }

    }
}