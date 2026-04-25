using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class AssignCounterController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/AssignCounter")]
        public HttpResponseMessage GetAssignCounter()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("SELECT * FROM VIEW_ASSIGN_COUNTER");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }

        }

        [HttpPost]
        [Route("api/AssignCounter")]
        public HttpResponseMessage PostAssignCounter([FromBody] AssignCounter model)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(model.User_name))
                {
                    if (db.IsExists("SELECT * FROM ASSIGN_COUNTER WHERE Counter_id = " + model.Counter_id + " AND CAST(Login_date AS DATE) = '" + model.Login_date.ToString("yyyy-MM-dd") + "' AND ISNULL(Is_closed,0) = 0"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Counter already running for this date! Please close first.");
                    }
                    else if (db.IsExists("SELECT * FROM ASSIGN_COUNTER WHERE Emp_id = " + model.Emp_id + " AND CAST(Login_date AS DATE) = '" + model.Login_date.ToString("yyyy-MM-dd") + "' AND ISNULL(Is_closed,0) = 0"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Employee already working on this date!");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("dbo.Sp_Assign_Counter", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Counter_id", model.Counter_id);
                        cmd.Parameters.AddWithValue("@Emp_id", model.Emp_id);
                        cmd.Parameters.AddWithValue("@Login_date", model.Login_date);
                        cmd.Parameters.AddWithValue("@Status", model.Status);
                        cmd.Parameters.AddWithValue("@Opn_bal", model.Opn_bal);
                        cmd.Parameters.AddWithValue("@Closing_bal", model.Closing_bal);
                        cmd.Parameters.AddWithValue("@Login_time", model.Login_time);
                        cmd.Parameters.AddWithValue("@txt", 1);

                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Counter Assigned Successfully!");
                    
                }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User!");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        [HttpPut]
        [Route("api/AssignCounter")]
        public HttpResponseMessage EditAssignCounter([FromBody] AssignCounter model)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(model.User_name))
                {
                    if (db.IsExists("Select * from VIEW_ASSIGN_COUNTER where Counter_id='" + model.Counter_id + "' and Login_date!='" + model.Login_date.ToString("MM/dd/yyyy") + "' and Status=1 and Id !=" + model.Id + ""))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "This counter is already loged on another day please logout!");
                    }
                    else if (db.IsExists("Select * from VIEW_ASSIGN_COUNTER where Counter_id='" + model.Counter_id + "' and Status=1 and Id !=" + model.Id + ""))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Counter already Active!");
                    }
                    else if (db.IsExists("Select * from VIEW_ASSIGN_COUNTER where Emp_id='" + model.Emp_id + "' and Login_date='" + model.Login_date.ToString("MM/dd/yyyy") + "' and Status=1 and Id !=" + model.Id + ""))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Employee already logged on Date!");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Assign_Counter", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Id", model.Id);
                        cmd.Parameters.AddWithValue("@Counter_id", model.Counter_id);
                        cmd.Parameters.AddWithValue("@Emp_id", model.Emp_id);
                        cmd.Parameters.AddWithValue("@Status", model.Status);
                        cmd.Parameters.AddWithValue("@Opn_bal", model.Opn_bal);
                        cmd.Parameters.AddWithValue("@Closing_bal", model.Closing_bal);
                        cmd.Parameters.AddWithValue("@Log_out_time", model.Log_out_time);
                        cmd.Parameters.AddWithValue("@Log_out_date", model.Log_out_date);
                        cmd.Parameters.AddWithValue("@txt", 2);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Updated Successfully!");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User!");

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPost]
        [Route("api/DelAssignCounter")]
        public HttpResponseMessage DeleteAssignCounter([FromBody] AssignCounter model)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(model.User_name))
                {
                    SqlCommand cmd = new SqlCommand("dbo.Sp_Assign_Counter_dlt", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    db.Disconnect();
                    if (rowsAffected > 0)
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted!");
                    else
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Record Not Found!");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User!");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }

}


