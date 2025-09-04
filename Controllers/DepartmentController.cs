using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using HR_Administration_System.Models;
using HR_Administration_System.Attributes;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;

namespace HR_Administration_System.Controllers
{
    [Authorize]
    public class DepartmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Department
        public async Task<ActionResult> Index()
        {
            var currentUserId = User.Identity.GetUserId();
            var currentUser = await db.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == currentUserId);

            IQueryable<Department> departments;

            if (User.IsInRole("HRAdministrator"))
            {
                // HR Admin can see all departments
                departments = db.Departments.Include(d => d.Manager);
            }
            else if (User.IsInRole("Manager") && currentUser?.EmployeeId != null)
            {
                // Managers can see departments they manage
                departments = db.Departments
                    .Where(d => d.ManagerId == currentUser.EmployeeId)
                    .Include(d => d.Manager);
            }
            else
            {
                // Regular employees can see departments they work in
                if (currentUser?.EmployeeId != null)
                {
                    var employeeDepartmentIds = db.EmployeeDepartments
                        .Where(ed => ed.EmployeeId == currentUser.EmployeeId && ed.IsActive)
                        .Select(ed => ed.DepartmentId);

                    departments = db.Departments
                        .Where(d => employeeDepartmentIds.Contains(d.Id))
                        .Include(d => d.Manager);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
            }

            return View(await departments.ToListAsync());
        }

        // GET: Department/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Department department = await db.Departments
                .Include(d => d.Manager)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return HttpNotFound();
            }

            // Check access rights
            var currentUserId = User.Identity.GetUserId();
            var currentUser = await db.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == currentUserId);

            bool hasAccess = false;

            if (User.IsInRole("HRAdministrator"))
            {
                hasAccess = true;
            }
            else if (User.IsInRole("Manager") && currentUser?.EmployeeId != null)
            {
                // Managers can see departments they manage
                hasAccess = department.ManagerId == currentUser.EmployeeId;
            }
            else if (currentUser?.EmployeeId != null)
            {
                // Employees can see departments they work in
                hasAccess = db.EmployeeDepartments
                    .Any(ed => ed.DepartmentId == id &&
                               ed.EmployeeId == currentUser.EmployeeId &&
                               ed.IsActive);
            }

            if (!hasAccess)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            // Get employees in this department
            ViewBag.DepartmentEmployees = await db.EmployeeDepartments
                .Where(ed => ed.DepartmentId == id && ed.IsActive)
                .Include(ed => ed.Employee)
                .Select(ed => ed.Employee)
                .Distinct()
                .ToListAsync();

            return View(department);
        }

        // GET: Department/Create
        [HRAdminOnly]
        public ActionResult Create()
        {
            PopulateManagerDropDownList();
            return View();
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HRAdminOnly]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,ManagerId,Status")] Department department)
        {
            if (ModelState.IsValid)
            {
                db.Departments.Add(department);
                await db.SaveChangesAsync();

                // Check if the manager needs Manager role
                if (department.ManagerId.HasValue)
                {
                    await UpdateManagerRole(department.ManagerId.Value);
                }

                TempData["Success"] = "Department created successfully.";
                return RedirectToAction("Index");
            }

            PopulateManagerDropDownList(department.ManagerId);
            return View(department);
        }

        // GET: Department/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Department department = await db.Departments.FindAsync(id);
            if (department == null)
            {
                return HttpNotFound();
            }

            // Check access rights
            var currentUserId = User.Identity.GetUserId();
            var currentUser = await db.Users.Include(u => u.Employee).FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (!User.IsInRole("HRAdministrator"))
            {
                // Only HR Admin can edit departments
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            PopulateManagerDropDownList(department.ManagerId);
            return View(department);
        }

        // POST: Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HRAdminOnly]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,ManagerId,Status")] Department department)
        {
            if (ModelState.IsValid)
            {
                var oldDepartment = await db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == department.Id);

                db.Entry(department).State = EntityState.Modified;
                await db.SaveChangesAsync();

                // Update manager roles if manager changed
                if (oldDepartment.ManagerId != department.ManagerId)
                {
                    if (department.ManagerId.HasValue)
                    {
                        await UpdateManagerRole(department.ManagerId.Value);
                    }

                    // Check if old manager still manages any departments
                    if (oldDepartment.ManagerId.HasValue)
                    {
                        await CheckAndRemoveManagerRole(oldDepartment.ManagerId.Value);
                    }
                }

                TempData["Success"] = "Department updated successfully.";
                return RedirectToAction("Index");
            }

            PopulateManagerDropDownList(department.ManagerId);
            return View(department);
        }

        // GET: Department/Delete/5
        [HRAdminOnly]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Department department = await db.Departments
                .Include(d => d.Manager)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return HttpNotFound();
            }

            // Check if department has active employees
            var activeEmployeeCount = await db.EmployeeDepartments
                .CountAsync(ed => ed.DepartmentId == id && ed.IsActive);

            ViewBag.ActiveEmployeeCount = activeEmployeeCount;

            return View(department);
        }

        // POST: Department/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [HRAdminOnly]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Department department = await db.Departments.FindAsync(id);

            // Soft delete by setting status to Inactive
            department.Status = "Inactive";
            db.Entry(department).State = EntityState.Modified;

            // Deactivate all employee-department relationships
            var employeeDepartments = db.EmployeeDepartments.Where(ed => ed.DepartmentId == id && ed.IsActive);
            foreach (var ed in employeeDepartments)
            {
                ed.IsActive = false;
                ed.EndDate = DateTime.Now;
            }

            await db.SaveChangesAsync();

            // Check if manager still manages other departments
            if (department.ManagerId.HasValue)
            {
                await CheckAndRemoveManagerRole(department.ManagerId.Value);
            }

            TempData["Success"] = "Department deactivated successfully.";
            return RedirectToAction("Index");
        }

        // GET: Department/AssignEmployees/5
        [HRAdminOnly]
        public async Task<ActionResult> AssignEmployees(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Department department = await db.Departments.FindAsync(id);
            if (department == null)
            {
                return HttpNotFound();
            }

            ViewBag.DepartmentName = department.Name;
            ViewBag.DepartmentId = id;

            // Get all active employees
            var allEmployees = await db.Employees
                .Where(e => e.Status == "Active")
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            // Get currently assigned employees
            var assignedEmployeeIds = await db.EmployeeDepartments
                .Where(ed => ed.DepartmentId == id && ed.IsActive)
                .Select(ed => ed.EmployeeId)
                .ToListAsync();

            ViewBag.AssignedEmployeeIds = assignedEmployeeIds;

            return View(allEmployees);
        }

        // POST: Department/AssignEmployees/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HRAdminOnly]
        public async Task<ActionResult> AssignEmployees(int id, int[] selectedEmployees)
        {
            var department = await db.Departments.FindAsync(id);
            if (department == null)
            {
                return HttpNotFound();
            }

            // Get current assignments
            var currentAssignments = await db.EmployeeDepartments
                .Where(ed => ed.DepartmentId == id)
                .ToListAsync();

            // Deactivate employees no longer selected
            foreach (var assignment in currentAssignments.Where(ca => ca.IsActive))
            {
                if (selectedEmployees == null || !selectedEmployees.Contains(assignment.EmployeeId))
                {
                    assignment.IsActive = false;
                    assignment.EndDate = DateTime.Now;
                }
            }

            // Add or reactivate selected employees
            if (selectedEmployees != null)
            {
                foreach (var employeeId in selectedEmployees)
                {
                    var existingAssignment = currentAssignments
                        .FirstOrDefault(ca => ca.EmployeeId == employeeId);

                    if (existingAssignment == null)
                    {
                        // Create new assignment
                        db.EmployeeDepartments.Add(new EmployeeDepartment
                        {
                            DepartmentId = id,
                            EmployeeId = employeeId,
                            StartDate = DateTime.Now,
                            IsActive = true
                        });
                    }
                    else if (!existingAssignment.IsActive)
                    {
                        // Reactivate existing assignment
                        existingAssignment.IsActive = true;
                        existingAssignment.StartDate = DateTime.Now;
                        existingAssignment.EndDate = null;
                    }
                }
            }

            await db.SaveChangesAsync();
            TempData["Success"] = "Department employees updated successfully.";
            return RedirectToAction("Details", new { id = id });
        }

        #region Helper Methods

        private void PopulateManagerDropDownList(object selectedManager = null)
        {
            var managers = db.Employees
                .Where(e => e.Status == "Active")
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => new
                {
                    Id = e.Id,
                    FullName = e.FirstName + " " + e.LastName
                });

            ViewBag.ManagerId = new SelectList(managers, "Id", "FullName", selectedManager);
        }

        private async Task UpdateManagerRole(int employeeId)
        {
            var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (employee != null && !string.IsNullOrEmpty(employee.ApplicationUserId))
            {
                var userManager = new ApplicationUserManager(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(db));

                // Add Manager role if not already assigned
                if (!await userManager.IsInRoleAsync(employee.ApplicationUserId, "Manager"))
                {
                    await userManager.AddToRoleAsync(employee.ApplicationUserId, "Manager");
                }
            }
        }

        private async Task CheckAndRemoveManagerRole(int employeeId)
        {
            // Check if employee still manages any active departments
            bool stillManages = await db.Departments
                .AnyAsync(d => d.ManagerId == employeeId && d.Status == "Active");

            if (!stillManages)
            {
                var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
                if (employee != null && !string.IsNullOrEmpty(employee.ApplicationUserId))
                {
                    var userManager = new ApplicationUserManager(new Microsoft.AspNet.Identity.EntityFramework.UserStore<ApplicationUser>(db));

                    // Remove Manager role but keep Employee role
                    if (await userManager.IsInRoleAsync(employee.ApplicationUserId, "Manager"))
                    {
                        await userManager.RemoveFromRoleAsync(employee.ApplicationUserId, "Manager");
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}