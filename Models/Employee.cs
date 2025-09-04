using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace HR_Administration_System.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string Telephone { get; set; }

        public int? ManagerId { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public virtual Employee Manager { get; set; }
        public virtual ICollection<Employee> Subordinates { get; set; }
        public virtual ICollection<EmployeeDepartment> EmployeeDepartments { get; set; }

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}