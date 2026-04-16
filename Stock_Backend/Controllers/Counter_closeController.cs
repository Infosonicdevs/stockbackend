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
    public class Counter_closeController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/CounterClose")]
        [HttpPost]
        public HttpResponseMessage CounterClose([FromBody] CounterCloseModel request)
        {
            try
            {
                db.Connect();

                if (request == null)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Data");
                }

                // User validation
                if (!db.IsValidUser(request.User))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");
                }

                // Fetch counter assign details
                string query = @"SELECT Id, Emp_id, Is_closed 
                                 FROM ASSIGN_COUNTER 
                                 WHERE Id=@Id AND Counter_id=@Counter_id";

                SqlCommand checkCmd = new SqlCommand(query, db.cn);
                checkCmd.Parameters.AddWithValue("@Id", request.Id);
                checkCmd.Parameters.AddWithValue("@Counter_id", request.Counter_id);

                SqlDataAdapter da = new SqlDataAdapter(checkCmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                //  Counter not found
                if (dt.Rows.Count == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Counter not found");
                }

                //  Already closed check
                if (dt.Rows[0]["Is_closed"].ToString() == "1")
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Counter already closed");
                }

                //  Employe validation 
                int dbEmpId = Convert.ToInt32(dt.Rows[0]["Emp_id"]);

                if (dbEmpId != request.Emp_id)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You are not allowed to close this counter");
                }

                // MAIN TRANSACTION
                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("Sp_counter_close", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Id", request.Id);
                        cmd.Parameters.AddWithValue("@Counter_id", request.Counter_id);

                        cmd.Parameters.AddWithValue("@Bill_from", request.Bill_from);
                        cmd.Parameters.AddWithValue("@Bill_to", request.Bill_to);

                        cmd.Parameters.AddWithValue("@Total_sale", request.Total_sale);
                        cmd.Parameters.AddWithValue("@Cash_sale", request.Cash_sale);
                        cmd.Parameters.AddWithValue("@Card_pay", request.Card_pay);
                        cmd.Parameters.AddWithValue("@Upi_pay", request.Upi_pay);

                        cmd.Parameters.AddWithValue("@Cust_points", request.Cust_points);
                        cmd.Parameters.AddWithValue("@Cash_return", request.Cash_return);
                        cmd.Parameters.AddWithValue("@Office_return", request.Office_return);

                        cmd.Parameters.AddWithValue("@Logout_date", request.Logout_date);
                        cmd.Parameters.AddWithValue("@Logout_time",
                            request.Logout_time ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        db.Disconnect();

                        return Request.CreateResponse(HttpStatusCode.OK, "Counter Closed Successfully");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}
