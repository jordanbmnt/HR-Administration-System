using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using HR_Administration_System.Models;

namespace HR_Administration_System.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
        }

        public async Task<IdentityResult> CreateUserForEmployee(Employee employee, bool isManager = false)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(employee.Email);
            if (existingUser != null)
            {
                return IdentityResult.Failed($"User with email {employee.Email} already exists");
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = employee.Email,
                Email = employee.Email,
                EmailConfirmed = true,
                EmployeeId = employee.Id
            };

            // Create user with default password
            var result = await _userManager.CreateAsync(user, "Password123#");

            if (result.Succeeded)
            {
                // Update employee with ApplicationUserId
                employee.ApplicationUserId = user.Id;
                _context.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                await _context.SaveChangesAsync();

                // Assign role
                if (isManager)
                {
                    await _userManager.AddToRoleAsync(user.Id, "Manager");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user.Id, "Employee");
                }
            }

            return result;
        }
    }
}