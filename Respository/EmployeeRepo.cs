using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace EmployeeManagementSystem.Respository
{
    public class EmployeeRepo
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["EmployeeMg"].ConnectionString;
    }
}