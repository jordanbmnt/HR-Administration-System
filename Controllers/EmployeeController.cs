using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using HR_Administration_System.Models;
using HR_Administration_System.Attributes;
using HR_Administration_System.Services;
using Microsoft.AspNet.Identity;

namespace HR_Administration_System.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserService userService;

        public EmployeeController()
        {
            userService = new UserService(db);
        }

        // GET: Employee
        public async Task<ActionResult> Index()
        {
            var currentUserId = User.Identity.GetUserId();
            var currentUser = await db.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == currentUserId);

            IQueryable<Employee> employees;

            if (User.IsInRole("HRAdministrator"))
            {
                // HR Admin can see all employees
                employees = db.Employees.Include(e => e.Manager);
            }
            else if (User.IsInRole("Manager") && currentUser?.EmployeeId != null)
            {
                // Managers can see employees in their departments
                var managerDepartments = db.Departments
                    .Where(d => d.ManagerId == currentUser.EmployeeId)
                    .Select(d => d.Id)
                    .ToList();

                var employeeIds = db.EmployeeDepartments
                    .Where(ed => managerDepartments.Contains(ed.DepartmentId) && ed.IsActive)
                    .Select(ed => ed.EmployeeId)
                    .Distinct();

                employees = db.Employees
                    .Where(e => employeeIds.Contains(e.Id) || e.Id == currentUser.EmployeeId)
                    .Include(e => e.Manager);
            }
            else if (currentUser?.EmployeeId != null)
            {
                // Regular employees can only see themselves
                employees = db.Employees
                    .Where(e => e.Id == currentUser.EmployeeId)
                    .Include(e => e.Manager);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(await employees.ToListAsync());
        }

        // GET: Employee/Details/5
        [EmployeeAuthorize]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Employee employee = await db.Employees
                .Include(e => e.Manager)
                .Include(e => e.EmployeeDepartments.Select(ed => ed.Department))
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return HttpNotFound();
            }

            return View(employee);
        }

        // GET: Employee/Create
        [HRAdminOnly]
        public ActionResult Create()
        {
            ViewBag.ManagerId = new SelectList(db.Employees.Where(e => e.Status == "Active"), "Id", "FullName");
            return View();
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HRAdminOnly]
        public async Task<ActionResult> Create([Bind(Include = "Id,FirstName,LastName,Email,Telephone,ManagerId,Status")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingEmployee = await db.Employees.FirstOrDefaultAsync(e => e.Email == employee.Email);
                if (existingEmployee != null)
                {
                    ModelState.AddModelError("Email", "An employee with this email already exists.");
                    ViewBag.ManagerId = new SelectList(db.Employees.Where(e => e.Status == "Active"), "Id", "FullName", employee.ManagerId);
                    return View(employee);
                }

                db.Employees.Add(employee);
                await db.SaveChangesAsync();

                // Create user account for the employee
                bool isManager = db.Departments.Any(d => d.ManagerId == employee.Id);
                var result = await userService.CreateUserForEmployee(employee, isManager);

                if (!result.Succeeded)
                {
                    // Log the error or show a message
                    TempData["Warning"] = $"Employee created but user account creation failed: {string.Join(", ", result.Errors)}";
                }
                else
                {
                    TempData["Success"] = "Employee and user account created successfully. Default password is: Password123#";
                }

                return RedirectToAction("Index");
            }

            ViewBag.ManagerId = new SelectList(db.Employees.Where(e => e.Status == "Active"), "Id", "FullName", employee.ManagerId);
            return View(employee);
        }

        // GET: Employee/Edit/5
        [EmployeeAuthorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Employee employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            // Only HR Admin can edit manager and status fields
            if (User.IsInRole("HRAdministrator"))
            {
                ViewBag.ManagerId = new SelectList(db.Employees.Where(e => e.Status == "Active" && e.Id != id), "Id", "FullName", employee.ManagerId);
            }
            else
            {
                ViewBag.IsReadOnly = true;
            }

            return View(employee);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EmployeeAuthorize]
        public async Task<ActionResult> Edit([Bind(Include = "Id,FirstName,LastName,Email,Telephone,ManagerId,Status,ApplicationUserId")] Employee employee)
        {
            var existingEmployee = await db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == employee.Id);

            if (existingEmployee == null)
            {
                return HttpNotFound();
            }

            // Non-HR administrators cannot change manager or status
            if (!User.IsInRole("HRAdministrator"))
            {
                employee.ManagerId = existingEmployee.ManagerId;
                employee.Status = existingEmployee.Status;
                ModelState.Remove("ManagerId");
                ModelState.Remove("Status");
            }

            // Preserve ApplicationUserId
            employee.ApplicationUserId = existingEmployee.ApplicationUserId;

            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                await db.SaveChangesAsync();

                TempData["Success"] = "Employee updated successfully.";
                return RedirectToAction("Index");
            }

            if (User.IsInRole("HRAdministrator"))
            {
                ViewBag.ManagerId = new SelectList(db.Employees.Where(e => e.Status == "Active" && e.Id != employee.Id), "Id", "FullName", employee.ManagerId);
            }
            else
            {
                ViewBag.IsReadOnly = true;
            }

            return View(employee);
        }

        // GET: Employee/Delete/5
        [HRAdminOnly]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Employee employee = await db.Employees
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return HttpNotFound();
            }

            // Check if employee is a manager
            var departmentsManaged = await db.Departments
                .CountAsync(d => d.ManagerId == id && d.Status == "Active");

            var subordinatesCount = await db.Employees
                .CountAsync(e => e.ManagerId == id && e.Status == "Active");

            ViewBag.DepartmentsManaged = departmentsManaged;
            ViewBag.SubordinatesCount = subordinatesCount;

            return View(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [HRAdminOnly]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Employee employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == id);

            // Soft delete by setting status to Inactive
            employee.Status = "Inactive";
            db.Entry(employee).State = EntityState.Modified;

            // Deactivate all employee-department relationships
            var employeeDepartments = db.EmployeeDepartments
                .Where(ed => ed.EmployeeId == id && ed.IsActive);

            foreach (var ed in employeeDepartments)
            {
                ed.IsActive = false;
                ed.EndDate = DateTime.Now;
            }

            // Deactivate the user account
            if (!string.IsNullOrEmpty(employee.ApplicationUserId))
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == employee.ApplicationUserId);
                if (user != null)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEndDateUtc = DateTime.MaxValue; // Permanently locked out
                }
            }

            await db.SaveChangesAsync();

            TempData["Success"] = "Employee deactivated successfully.";
            return RedirectToAction("Index");
        }

        // GET: Employee/ResetPassword/5
        [HRAdminOnly]
        public async Task<ActionResult> ResetPassword(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Employee employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            ViewBag.EmployeeName = employee.FullName;
            ViewBag.EmployeeEmail = employee.Email;

            return View();
        }

        // POST: Employee/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HRAdminOnly]
        public async Task<ActionResult> ResetPassword(int id)
        {
            Employee employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrEmpty(employee.ApplicationUserId))
            {
                TempData["Error"] = "This employee does not have a user account.";
                return RedirectToAction("Details", new { id = id });
            }

            var userManager = new ApplicationUserManager(
                new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(db));

            // Generate password reset token
            var token = await userManager.GeneratePasswordResetTokenAsync(employee.ApplicationUserId);

            // Reset to default password
            var result = await userManager.ResetPasswordAsync(
                employee.ApplicationUserId,
                token,
                "Password123#");

            if (result.Succeeded)
            {
                TempData["Success"] = $"Password reset successfully for {employee.FullName}. New password is: Password123#";
            }
            else
            {
                TempData["Error"] = $"Failed to reset password: {string.Join(", ", result.Errors)}";
            }

            return RedirectToAction("Details", new { id = id });
        }

        // GET: Employee/MyProfile
        public async Task<ActionResult> MyProfile()
        {
            var currentUserId = User.Identity.GetUserId();
            var currentUser = await db.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (currentUser?.EmployeeId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden,
                    "No employee profile associated with your account.");
            }

            return RedirectToAction("Details", new { id = currentUser.EmployeeId });
        }

        // GET: Employee/Subordinates/5
        public async Task<ActionResult> Subordinates(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            // Check if user can view subordinates
            var currentUserId = User.Identity.GetUserId();
            var currentUser = await db.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (!User.IsInRole("HRAdministrator") &&
                !User.IsInRole("Manager") &&
                currentUser?.EmployeeId != id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var subordinates = await db.Employees
                .Where(e => e.ManagerId == id && e.Status == "Active")
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            ViewBag.ManagerName = employee.FullName;
            return View(subordinates);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}