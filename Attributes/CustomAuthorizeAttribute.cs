using HR_Administration_System.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HR_Administration_System.Attributes
{
    public class EmployeeAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            var user = httpContext.User;

            // HR Administrators can access everything
            if (user.IsInRole("HRAdministrator"))
                return true;

            // Get the employee ID from the route data
            var routeData = httpContext.Request.RequestContext.RouteData;
            var employeeIdParam = routeData.Values["id"]?.ToString() ??
                                  httpContext.Request.QueryString["id"];

            if (string.IsNullOrEmpty(employeeIdParam))
                return true; // Let the action handle missing ID

            if (!int.TryParse(employeeIdParam, out int requestedEmployeeId))
                return false;

            var userId = user.Identity.GetUserId();
            using (var context = new ApplicationDbContext())
            {
                var currentUser = context.Users.Include("Employee").FirstOrDefault(u => u.Id == userId);

                if (currentUser?.EmployeeId == null)
                    return false;

                // Employees can only access their own data
                if (user.IsInRole("Employee"))
                {
                    return currentUser.EmployeeId == requestedEmployeeId;
                }

                // Managers can access their department employees
                if (user.IsInRole("Manager"))
                {
                    var managerDepartments = context.Departments
                        .Where(d => d.ManagerId == currentUser.EmployeeId)
                        .Select(d => d.Id)
                        .ToList();

                    var employeeInDepartment = context.EmployeeDepartments
                        .Any(ed => ed.EmployeeId == requestedEmployeeId &&
                                   managerDepartments.Contains(ed.DepartmentId) &&
                                   ed.IsActive);

                    return employeeInDepartment || currentUser.EmployeeId == requestedEmployeeId;
                }
            }

            return false;
        }
    }

    public class HRAdminOnlyAttribute : AuthorizeAttribute
    {
        public HRAdminOnlyAttribute()
        {
            Roles = "HRAdministrator";
        }
    }
}