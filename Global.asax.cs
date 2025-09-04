using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using HR_Administration_System.Models;
using System.Data.Entity;

namespace HR_Administration_System
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Initialize SQLite databases
            InitializeDatabases();
        }

        private void InitializeDatabases()
        {
            try
            {
                // Force Entity Framework to initialize the databases
                using (var employeeContext = new EmployeeDBContext())
                {
                    employeeContext.Database.Initialize(true);

                    // Optional: Add seed data if database is empty
                    if (!employeeContext.Employees.Any())
                    {
                        // Add some sample employees
                        employeeContext.Employees.Add(new Employee
                        {
                            FirstName = "John",
                            LastName = "Doe",
                            Email = "john.doe@example.com",
                            Telephone = "555-0001",
                            Manager = "Jane Smith",
                            Status = "Active"
                        });

                        employeeContext.Employees.Add(new Employee
                        {
                            FirstName = "Alice",
                            LastName = "Johnson",
                            Email = "alice.johnson@example.com",
                            Telephone = "555-0002",
                            Manager = "Jane Smith",
                            Status = "Active"
                        });

                        employeeContext.SaveChanges();
                    }
                }

                using (var departmentContext = new DepartmentDBContext())
                {
                    departmentContext.Database.Initialize(true);

                    // Optional: Add seed data if database is empty
                    if (!departmentContext.Departments.Any())
                    {
                        // Add some sample departments
                        departmentContext.Departments.Add(new Department
                        {
                            Name = "Human Resources",
                            Manager = "Jane Smith",
                            Status = "Active"
                        });

                        departmentContext.Departments.Add(new Department
                        {
                            Name = "Information Technology",
                            Manager = "Bob Wilson",
                            Status = "Active"
                        });

                        departmentContext.Departments.Add(new Department
                        {
                            Name = "Sales",
                            Manager = "Tom Anderson",
                            Status = "Active"
                        });

                        departmentContext.SaveChanges();
                    }
                }

                // Log successful initialization
                System.Diagnostics.Debug.WriteLine("SQLite databases initialized successfully.");
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // You can also write to a log file if needed
                string errorMessage = $"[{DateTime.Now}] Database initialization error: {ex.Message}\n{ex.StackTrace}\n\n";
                string logPath = Server.MapPath("~/App_Data/errors.log");
                System.IO.File.AppendAllText(logPath, errorMessage);
            }
        }
    }
}