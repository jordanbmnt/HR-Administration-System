# HR Administration System

An ASP.NET MVC5 web application for managing employees, including features to create, edit, activate/deactivate, and list employees.

---

## Features
- Create new employees
- Edit employee details
- Toggle employee **Active/Inactive** status
- View all employees in a structured list
- Responsive design with Bootstrap

---

## Requirements

Before running this project, ensure you have the following installed:

- [Visual Studio 2019 or later](https://visualstudio.microsoft.com/) with:
  - **ASP.NET and Web Development** workload
- [.NET Framework 4.7.2 or later](https://dotnet.microsoft.com/en-us/download/dotnet-framework)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or a full SQL Server instance
- [Entity Framework 6](https://www.nuget.org/packages/EntityFramework/)

---

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/hr-administration-system.git
   cd hr-administration-system

2. **Open in Visual Studio**

   * Open the `.sln` file in Visual Studio.

3. **Configure the database**

   * Open `Web.config`.
   * Update the `connectionStrings` section to point to your SQL Server instance:

     ```xml
     <connectionStrings>
       <add name="DefaultConnection" 
            connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=HR_Admin_DB;Integrated Security=True"
            providerName="System.Data.SqlClient" />
     </connectionStrings>
     ```

4. **Run migrations (if enabled)**
   If the project uses Entity Framework migrations:

   * Open **Package Manager Console** in Visual Studio.
   * Run:

     ```powershell
     Update-Database
     ```

   Otherwise, the database will be created automatically on first run.

5. **Run the project**

   * Press `F5` or click **Start** in Visual Studio.
   * The app will open in your browser at:

     ```
     https://localhost:xxxx/
     ```

---

## Usage

* Navigate to **Employees** from the navbar.
* Use the **Create** button to add new employees.
* Use the **Edit** button to modify employee details.
* Use the **Active/Inactive** button to toggle employee status.
* All changes are saved in the SQL Server database.

---

## Project Structure

* `Controllers/` → Contains MVC controllers (`EmployeeController.cs`, etc.)
* `Models/` → Entity classes (e.g., `Employee.cs`)
* `Views/` → Razor views (`.cshtml` files for UI)
* `App_Data/` → Local database files (if using LocalDB)
* `Scripts/` → JavaScript files, including jQuery and Bootstrap
* `Content/` → CSS and static assets

---

## Technologies Used

* ASP.NET MVC5
* Entity Framework 6 (Code First)
* SQL Server
* Bootstrap (for styling and responsive design)
* jQuery

---

## Troubleshooting

* **Error:** *"Attaching an entity of type failed because another entity of the same type already has the same primary key value."*

  * Ensure you are fetching the entity from the database before updating (see `CreateEdit` method in `EmployeeController`).

* **Database not found**

  * Verify your SQL Server connection string in `Web.config`.

* **Bootstrap not rendering correctly**

  * Ensure `_Layout.cshtml` includes the Bootstrap CSS/JS references.

---

## License

This project is for learning/demo purposes. You may modify and use it freely.

```
