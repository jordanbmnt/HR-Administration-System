using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using SQLite.CodeFirst;

namespace HR_Administration_System.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Manager { get; set; }
        public string Status { get; set; }
    }

    public class EmployeeDBContext : DbContext
    {
        public EmployeeDBContext() : base("name=EmployeeDBContext")
        {
        }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<EmployeeDBContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}