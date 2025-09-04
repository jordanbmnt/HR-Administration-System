using System;
using System.ComponentModel.DataAnnotations;

namespace HR_Administration_System.Models
{
    public class EmployeeDepartment
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}