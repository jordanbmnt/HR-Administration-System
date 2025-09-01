using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HR_Administration_System.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Manager { get; set; }
        public string Status { get; set; }
    }
}