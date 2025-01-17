using Dapper;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Repository;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeRepository employeeRepository = new EmployeeRepository();

        public EmployeeController(EmployeeRepository employeeRepository)
        {
            this.employeeRepository = employeeRepository;
        }

        public EmployeeController()
        {

        }

        /// <summary>
        /// Add Employee Information
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddEmployee()
        {
            try
            {
                var str = ConfigurationManager.ConnectionStrings["EmployeeManagement"].ConnectionString;
                using (var connection = new SqlConnection(str))
                {
                    string sql = "Select * from Department";
                    var e = connection.Query<Employee>(sql);
                    ViewBag.Department = e;
                }
                return View();
            }
            catch (SqlException sql)
            {
                TempData["data"] = "Department data not fetch";
            }
            return View();
        }


        [HttpPost]
        public ActionResult AddEmployee(Employee emp)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string msg;
                    bool isAdded = employeeRepository.AddEmployee(emp, out msg);

                    if (isAdded)
                    {
                        TempData["SuccessMessage"] = "Employee Added successfully";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = msg;
                        return RedirectToAction("AddEmployee");   
                    }

                }

                return View(emp); // Return the view if model validation fail
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "try again later";
                return RedirectToAction("Index");
            }
        }


        [HttpGet]
        public ActionResult UpdateEmployee(int id)
        {
            try
            {
                var employee = employeeRepository.GetEmployeeById(id);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Unable to fetch Employee record";
                }

                var str = ConfigurationManager.ConnectionStrings["EmployeeManagement"].ConnectionString;

                using (var connection = new SqlConnection(str))
                {
                    connection.Open();

                    string query = "SELECT DepartmentID, DepartmentName FROM Department";
                    var departments = connection.Query(query).ToList();

                    
                    ViewBag.Department = departments.Select(d => new
                    {
                        DepartmentID = d.DepartmentID,
                        DepartmentName = d.DepartmentName
                    }).ToList();
                }

                return View(employee); 
            }
            catch (SqlException sql)
            {
                
                TempData["ErrorMessage"] = "Database connection error. Please try again later.";
            }
            

            return RedirectToAction("Index"); // Redirect in case of error
        }

        [HttpPost]
        public ActionResult UpdateEmployee(Employee emp)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isUpdated = employeeRepository.UpdateEmployee(emp);

                    if (isUpdated)
                    {
                        TempData["SuccessMessage"] = "Employee updated successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Mobile number {emp.PhoneNumber} already exists. Please use another mobile number.";
                    }

                    return RedirectToAction("Index");
                }

                return View(emp);
            }
            catch (SqlException sql)
            {
               
                TempData["ErrorMessage"] = "Database connection error";
            }
            
            return RedirectToAction("Index"); 
        }


        public ActionResult Delete(int id)
        {
            try
            {
                employeeRepository.DeleteEmployee(id);
                
                TempData["SuccessMessage"] = "Employee record deleted successfully";
            }
            catch(Exception e)
            {
                TempData["SuccessMessage"] = "Record not deleted";
            }
            return RedirectToAction("Index");

        }

        public ActionResult GetEmployeeById(int id)
        {
            try
            {
                var e = employeeRepository.GetEmployeeById(id);
                return View(e);
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Employee record unable to fetch";
            }
            return RedirectToAction("Index");       
        }

        public ActionResult GetDeleteEmployee()
        {
            try
            {
                var emp = employeeRepository.BackupEmployee();
                return View(emp);
            }
            catch(SqlException sql)
            {
                TempData["ErrorMessage"] = "data not recovred";
            }
            return RedirectToAction("Index");
        }

        public ActionResult RetriveData(int id)
        {
            try
            {
                employeeRepository.RetriveDeleteData(id);
                TempData["SuccessMessage"] = "Data recover Sucessfully";
            }
            catch(Exception e)
            {
                TempData["ErrorMessage"] = "Data not recovered";
            }
            return RedirectToAction("GetDeleteEmployee");
        }

        public ActionResult DeleteEmployeePermanent(int id)
        {
            try
            {
                employeeRepository.DeleteBackupData(id);
                TempData["SuccessMessage"] = "Record permanently deleted";
            }
            catch(Exception e)
            {
                TempData["ErrorMessage"] = "Record not deleted";
            }
            return RedirectToAction("GetDeleteEmployee");
        }

        public ActionResult DownloadExcelMainList()
        {
            try
            {
                // Fetch the data from the database
                var employees = employeeRepository.GetAllEmployees();


                using (var package = new ExcelPackage())
                {
                    // Create a new worksheet
                    var worksheet = package.Workbook.Worksheets.Add("Employees");

                    // column name
                    worksheet.Cells[1, 1].Value = "EmployeeID";
                    worksheet.Cells[1, 2].Value = "Employee Name";
                    worksheet.Cells[1, 3].Value = "Age";
                    worksheet.Cells[1, 4].Value = "Mobile Number";

                    // Add data rows
                    int row = 2;
                    foreach (var employee in employees)
                    {
                        worksheet.Cells[row, 1].Value = employee.EmployeeID;
                        worksheet.Cells[row, 2].Value = employee.EmployeeName;
                        worksheet.Cells[row, 3].Value = employee.DateOfBirth.ToString("yyyy-MM-dd");
                        worksheet.Cells[row, 4].Value = employee.PhoneNumber;
                        row++;
                    }

                    //Autofit columns
                    worksheet.Cells.AutoFitColumns();

                    // Set the response type and download the file
                    var stream = new MemoryStream(package.GetAsByteArray());
                    stream.Position = 0;
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employees.xlsx");
                    
                }
                
            }
            
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Error while generating Excel";
                return RedirectToAction("Index");
            }
        }


        public ActionResult DownloadExcelBackupData()
        {
            try
            {
               
                var employees = employeeRepository.BackupEmployee();


                using (var package = new ExcelPackage())
                {
                 
                    var worksheet = package.Workbook.Worksheets.Add("Employees"); //to make excel

                    worksheet.Cells[1, 1].Value = "EmployeeID";
                    worksheet.Cells[1, 2].Value = "Employee Name";
                    worksheet.Cells[1, 3].Value = "Age";
                    worksheet.Cells[1, 4].Value = "Mobile Number";

                    // Add data rows
                    int row = 2;
                    foreach (var employee in employees)
                    {
                        worksheet.Cells[row, 1].Value = employee.EmployeeID;
                        worksheet.Cells[row, 2].Value = employee.EmployeeName;
                        worksheet.Cells[row, 3].Value = employee.DateOfBirth.ToString("yyyy-MM-dd");
                        worksheet.Cells[row, 4].Value = employee.PhoneNumber;
                        row++;
                    }
                    //Autofit columns
                    worksheet.Cells.AutoFitColumns();


                    var stream = new MemoryStream(package.GetAsByteArray());
                    stream.Position = 0;
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BackupEmployees.xlsx");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error while generating excel";
                return RedirectToAction("Index");    
            }
        }


        public ActionResult Index(int pageNumber = 1, int pageSize = 6)
        {
            var str = ConfigurationManager.ConnectionStrings["EmployeeManagement"].ConnectionString;
            using (var conn = new SqlConnection(str))
            {
                conn.Open();
                
                var parameters = new DynamicParameters();
                parameters.Add("@PageNumber", pageNumber);
                parameters.Add("@PageSize", pageSize);
                int count = (pageNumber - 1) * pageSize + 1;
                ViewBag.c = count;
                using (var multi = conn.QueryMultiple("usp_PagingEmployee", parameters, commandType: CommandType.StoredProcedure))
                {
                    var employee = multi.Read<Employee>().ToList();
                    var totalRecords = multi.Read<int>().FirstOrDefault();

                    ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                    ViewBag.CurrentPage = pageNumber;
                    return View(employee);
                }
            }
        }
    }
}