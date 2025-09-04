using System;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using HR_Administration_System.Models;

namespace HR_Administration_System
{
    public class DbInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            // Create roles
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!roleManager.RoleExists("HRAdministrator"))
            {
                roleManager.Create(new IdentityRole("HRAdministrator"));
            }

            if (!roleManager.RoleExists("Manager"))
            {
                roleManager.Create(new IdentityRole("Manager"));
            }

            if (!roleManager.RoleExists("Employee"))
            {
                roleManager.Create(new IdentityRole("Employee"));
            }

            // Create admin user
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var adminUser = userManager.FindByEmail("hradmin@test.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "hradmin@test.com",
                    Email = "hradmin@test.com",
                    EmailConfirmed = true
                };

                var result = userManager.Create(adminUser, "TestPass1234");
                if (result.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, "HRAdministrator");
                }
            }

            context.SaveChanges();
        }
    }
}