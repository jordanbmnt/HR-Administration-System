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

            Employee employee = await db.Employees.FindAsync(id);
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
                .Include(e => e.