using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HR_Administration_System.Models
{
    public class EmployeeFilter
    {
        public string Department { get; set; }
        public string Status { get; set; }
        public string Manager { get; set; }
    }
}