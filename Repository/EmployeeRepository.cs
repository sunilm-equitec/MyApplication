using Dapper;
using EmployeeManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace EmployeeManagementSystem.Repository
{
    public class EmployeeRepository
 
    {
        private readonly string connectionstring = ConfigurationManager.ConnectionStrings["EmployeeManagement"].ConnectionString;

        /// <summary>
        /// Get All Employee Data
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Employee> GetAllEmployees()
        {
            using (var con = new SqlConnection(connectionstring))
            {
                
                return con.Query<Employee>("getAllEmployees", commandType: CommandType.StoredProcedure).ToList();
                
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Add employees data"></param>
        /// <returns></returns>
        public bool AddEmployee(Employee employee, out string msg)
        {
            msg = string.Empty;
            using (var c = new SqlConnection(connectionstring))
            {
                c.Open();
                string checkempID = "select count(1) from Employees where EmployeeID = @EmployeeId";
                int Idcount= c.ExecuteScalar<int>(checkempID, new {employee.EmployeeID} );

                string checkmob = "SELECT COUNT(1) FROM Employees WHERE PhoneNumber = @PhoneNumber";
                int mobcount = c.ExecuteScalar<int>(checkmob, new { employee.PhoneNumber });

                if (mobcount > 0)
                {
                    msg = "Mobile Number already exist";
                    return false;
                }

                if(Idcount >0)
                {
                    msg = "Employee ID already exist";
                    return false;
                }
                c.Execute("usp_AddEmployee", new
                {
                    employee.EmployeeID,
                    employee.EmployeeName,
                    employee.DepartmentID,
                    employee.DateOfBirth,
                    employee.Gender,
                    employee.Address,
                    employee.PhoneNumber
                }, commandType: CommandType.StoredProcedure);
                
            }
            return true;
        }   
            
        


        /// <summary>
        /// Update Employee Data using Id
        /// </summary>
        /// <param name="Update Employee Data using Id"></param>
        public bool UpdateEmployee(Employee emp)
        {
         
            using (var c = new SqlConnection(connectionstring))
            {
                c.Open();
                string checkSql = "Select COUNT(1) from Employees where PhoneNumber = @PhoneNumber AND EmployeeID != @EmployeeID";
                int count = c.ExecuteScalar<int>(checkSql, new { emp.PhoneNumber, emp.EmployeeID });

                if (count > 0)
                {
                    return false; // Duplicate mobile number exists
                }

                c.Execute("usp_UpdateEmployeeData", new {
                emp.EmployeeID,
                emp.EmployeeName,
                emp.DepartmentID,
                emp.DateOfBirth,
                emp.Gender,
                emp.Address,
                emp.PhoneNumber}, commandType: CommandType.StoredProcedure);

                return true;
            }
            
        }

        /// <summary>
        /// Delte employee using Id
        /// </summary>
        /// <param name="id"></param>
        public void DeleteEmployee(int id)
        {
            using (var connection = new SqlConnection(connectionstring))
            {
                connection.Open();

                connection.Execute("usp_SoftDeleteEmployee",  new { EmployeeID= id }, commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// Get single record of employee by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Employee GetEmployeeById(int id)
        {
            using (var connection = new SqlConnection(connectionstring))
            {
                connection.Open();
                return connection.QueryFirstOrDefault<Employee>("usp_GetEmployeeByID", new { EmployeeId=id },
                    commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// Get deleted employee data using backup table
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Employee> BackupEmployee()
        {
            using (var con = new SqlConnection(connectionstring))
            {
                con.Open();
               
                return con.Query<Employee>("usp_GetActiveEmployees", commandType: CommandType.StoredProcedure);
               
            }
        }

        
        /// <summary>
        /// Delete Data from backup permanantly
        /// </summary>
        /// <param name="id"></param>
        public void DeleteBackupData(int id)
        {
            using (var con = new SqlConnection(connectionstring))
            {
                con.Open();
                con.ExecuteScalar<Employee>("usp_DeleteBackupData", new {@EmployeeID=id },  commandType: CommandType.StoredProcedure);
            }
        }
        
        /// <summary>
        /// Retrive data from backup table
        /// </summary>
        /// <param name="Id"></param>
        public void RetriveDeleteData(int id)
        {
            using (var con = new SqlConnection(connectionstring))
            {
                con.Open();
                con.ExecuteScalar("usp_RestoreEmployee", new { EmployeeID = id }, commandType: CommandType.StoredProcedure);
            }   
        }
    }
}