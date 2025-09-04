using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace HR_Administration_System.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public int? ManagerId { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        public virtual Employee Manager { get; set; }
        public virtual ICollection<EmployeeDepartment> EmployeeDepartments { get; set; }
    }
}