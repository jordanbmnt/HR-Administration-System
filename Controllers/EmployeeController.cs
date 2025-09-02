using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HR_Administration_System.Models;

namespace HR_Administration_System.Controllers
{
    public class EmployeeController : Controller
    {
        private EmployeeDBContext db = new EmployeeDBContext();

        // GET: Employee
        public ActionResult Index()
        {
            return View(db.Employees.ToList());
        }

        // GET: Employee/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employee/FormFilter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FormFilter([Bind(Include = "Department,Manager,Status")] EmployeeFilter employeeFilter)
        {
            if (ModelState.IsValid)
            {
                //Filter with EmployeeFilter data
                return RedirectToAction("Index");
            }

            return View();
        }

        // GET: Employee/CreateEdit/5
        public ActionResult CreateEdit(int? id)
        {
            if (id == null)
            {
                return View();
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employee/CreateEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit([Bind(Include = "Id,FirstName,LastName,Email,Telephone,Manager,Status")] Employee employee)
        {
            if (employee.Id == 0) // new employee
            {
                db.Employees.Add(employee);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // existing employee → fetch and update
            var employeeExists = db.Employees.Find(employee.Id);
            if (employeeExists == null)
            {
                return HttpNotFound();
            }

            employeeExists.FirstName = employee.FirstName;
            employeeExists.LastName = employee.LastName;
            employeeExists.Email = employee.Email;
            employeeExists.Telephone = employee.Telephone;
            employeeExists.Manager = employee.Manager;
            employeeExists.Status = employee.Status;

            db.SaveChanges();
            return RedirectToAction("Index");
        }


        // GET: Employee/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Employee employee = db.Employees.Find(id);
            db.Employees.Remove(employee);
            db.SaveChanges();
            return RedirectToAction("Index");
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
