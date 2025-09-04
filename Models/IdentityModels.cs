using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace HR_Administration_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            if (EmployeeId.HasValue)
            {
                userIdentity.AddClaim(new Claim("EmployeeId", EmployeeId.Value.ToString()));
            }

            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        // Add DbSets for your entities
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<EmployeeDepartment> EmployeeDepartments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Employee self-referencing relationship
            modelBuilder.Entity<Employee>()
                .HasOptional(e => e.Manager)
                .WithMany(e => e.Subordinates)
                .HasForeignKey(e => e.ManagerId)
                .WillCascadeOnDelete(false);

            // Configure Employee-ApplicationUser relationship
            modelBuilder.Entity<Employee>()
                .HasOptional(e => e.ApplicationUser)
                .WithOptionalPrincipal()
                .Map(m => m.MapKey("ApplicationUserId"));

            // Configure Department-Manager relationship
            modelBuilder.Entity<Department>()
                .HasOptional(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId)
                .WillCascadeOnDelete(false);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}