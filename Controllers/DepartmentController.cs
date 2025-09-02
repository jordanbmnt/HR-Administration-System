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
    public class DepartmentController : Controller
    {
        private DepartmentDBContext db = new DepartmentDBContext();

        // GET: Departments
        public ActionResult Index(string status)
        {
            var departments = db.Departments.AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "active" || status == "inactive")
                {
                    departments = departments.Where(e => e.Status == status);
                }
            }
            return View(departments.ToList());
        }

        // GET: Departments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Department/FormFilter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FormFilter([Bind(Include = "Status")] DepartmentFilter departmentFilter)
        {
            if (departmentFilter.Status.Length > 0)
            {
                return RedirectToAction("Index", new
                {
                    status = departmentFilter.Status
                });
            }

            return View();
        }

        // GET: Department/CreateEdit/5
        public ActionResult CreateEdit(int? id)
        {
            if (id == null)
            {
                return View();
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Department/CreateEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit([Bind(Include = "Id,Name,Manager,Status")] Department department)
        {
            if (department.Id == 0) // new department
            {
                db.Departments.Add(department);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // existing department → fetch and update
            var departmentExists = db.Departments.Find(department.Id);
            if (departmentExists == null)
            {
                return HttpNotFound();
            }

            departmentExists.Name = department.Name;
            departmentExists.Manager = department.Manager;
            departmentExists.Status = department.Status;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: Department/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }

            // Toggle status
            department.Status = department.Status == "Active" ? "Inactive" : "Active";

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Departments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Department department = db.Departments.Find(id);
            db.Departments.Remove(department);
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
