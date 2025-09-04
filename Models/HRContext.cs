using System.Data.Entity;
using SQLite.CodeFirst;

namespace HR_Administration_System.Models
{
    public class HRContext : DbContext
    {
        public HRContext() : base("name=EmployeeDBContext")
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<HRContext>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}