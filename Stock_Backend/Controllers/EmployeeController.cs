using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class EmployeeController : ApiController
    {
        DbClass db = new DbClass();
 

        [Route("api/Employee")]
        public HttpResponseMessage GetEmployee()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_EMPLOYEE_INFO");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }

        }


        [Route("api/Employee")]   
        public HttpResponseMessage GetEmployee(int E_id)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from VIEW_EMPLOYEE_INFO where Emp_id=" + E_id);
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }

        [Route("api/Emp")]
        public HttpResponseMessage GetEmpByEmpName(string EmpName)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_EMPLOYEE_INFO where Emp_name like '%" + EmpName + "%' ");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }
        }

        [Route("api/Employee")]
        public HttpResponseMessage PutEmployee([FromBody] Employee_Info Employee)
        {
            try
            {
                db.Connect();
                
                if (db.IsAdmin(Employee.Modified_by))
                {
                    if (db.IsExists("Select * from EMPLOYEE_INFO where Cust_id = " + Employee.Cust_id + " and Emp_id != " + Employee.Emp_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer Id already Exists!");
                    }
                    else if(db.IsExists("Select * from EMPLOYEE_INFO where Emp_code = '" + Employee.Emp_code + "' and Emp_id != " + Employee.Emp_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Employee code already Exists!");
                    }
                    else if (db.IsExists("select * from EMPLOYEE_INFO  where Mobile_no = '" + Employee.Mobile_no + "' and Emp_id !=" + Employee.Emp_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Mobile number already exists");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Employee_Info", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Emp_id", Employee.Emp_id);
                        cmd.Parameters.AddWithValue("@Emp_code", Employee.Emp_code);
                        cmd.Parameters.AddWithValue("@Designation_id", Employee.Designation_id);
                        cmd.Parameters.AddWithValue("@DOJ", Employee.DOJ.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@Qualification", Employee.Qualification);
                        cmd.Parameters.AddWithValue("@E_mail", Employee.E_mail);
                        cmd.Parameters.AddWithValue("@Status", Employee.Status);
                        cmd.Parameters.AddWithValue("@DOR", Employee.DOR);
                        cmd.Parameters.AddWithValue("@Reason", Employee.Reason);
                        cmd.Parameters.AddWithValue("@Outlet_id", Employee.Outlet_id);
                        cmd.Parameters.AddWithValue("@DOB", Employee.DOB);
                        cmd.Parameters.AddWithValue("@Mobile_no", Employee.Mobile_no);
                        cmd.Parameters.AddWithValue("@Emp_name", Employee.Emp_name);
                        cmd.Parameters.AddWithValue("@Cust_id", Employee.Cust_id);
                        cmd.Parameters.AddWithValue("@txt", 2);

                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }


        [Route("api/Employee")]
        public HttpResponseMessage PostEmployee([FromBody] Employee_Info Employee)
        {
            try
            {
                db.Connect();
                if (db.IsValidUser(Employee.Created_by))
                {
                    if (db.IsExists("Select * from EMPLOYEE_INFO where Cust_id = " + Employee.Cust_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer Id already Exists!");
                    }
                    else if(db.IsExists("Select * from EMPLOYEE_INFO where Emp_code = '" + Employee.Emp_code + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Employee code already Exists!");
                    }
                    else if (db.IsExists("select * from EMPLOYEE_INFO  where Mobile_no = '" + Employee.Mobile_no + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Mobile number already exists");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Employee_Info", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Emp_code", Employee.Emp_code);
                        cmd.Parameters.AddWithValue("@Designation_id", Employee.Designation_id);
                        cmd.Parameters.AddWithValue("@DOJ", Employee.DOJ.ToString("MM/dd/yyyy"));
                        cmd.Parameters.AddWithValue("@Qualification", Employee.Qualification);
                        cmd.Parameters.AddWithValue("@E_mail", Employee.E_mail);
                        cmd.Parameters.AddWithValue("@Status", Employee.Status);
                        cmd.Parameters.AddWithValue("@DOR", Employee.DOR);
                        cmd.Parameters.AddWithValue("@Reason", Employee.Reason);
                        cmd.Parameters.AddWithValue("@Outlet_id", Employee.Outlet_id);
                        cmd.Parameters.AddWithValue("@DOB", Employee.DOB);
                        cmd.Parameters.AddWithValue("@Mobile_no", Employee.Mobile_no);
                        cmd.Parameters.AddWithValue("@Emp_name", Employee.Emp_name);
                        cmd.Parameters.AddWithValue("@Cust_id", Employee.Cust_id);
                        cmd.Parameters.AddWithValue("@txt", 1);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                   
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NotFound, ex.Message);

            }

        }


        [HttpPost]
        [Route("api/DelEmployee")]
        public HttpResponseMessage deleteEmployee([FromBody] Employee_Info Employee)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(Employee.Modified_by))
                {
                    db.Execute("delete from EMPLOYEE_INFO where Emp_id = " + Employee.Emp_id + "");
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record deleted");
                }
                
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NotFound, ex.Message);

            }

        }

    }
}
