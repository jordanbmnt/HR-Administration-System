using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace HR_Administration_System.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Manager { get; set; }
        public string Status { get; set; }
    }

    public class DepartmentDBContext : DbContext
    {
        public DbSet<Department> Departments { get; set; }
    }
}