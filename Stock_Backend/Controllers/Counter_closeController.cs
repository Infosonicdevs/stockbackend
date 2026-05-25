using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class Counter_closeController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/CounterClose/GetData")]
        [HttpGet]
        public HttpResponseMessage GetCounterData(int Counter_id, DateTime LoginDate)
        {
            try
            {
                db.Connect();

                // Counter Info
                string counterQuery = @"
SELECT 
    ac.Id,
    ac.Counter_id,
    ac.Emp_id,
    ac.Login_date,
    ac.Login_time,
    ac.Is_closed,
    ac.Opn_bal,
    ac.Closing_bal,
    ac.Log_out_date,
    ac.Log_out_time,
    c.Counter_name,
    e.Emp_name
FROM ASSIGN_COUNTER ac
INNER JOIN COUNTER c ON c.Counter_id = ac.Counter_id
INNER JOIN EMPLOYEE_INFO e ON e.Emp_id = ac.Emp_id
WHERE ac.Counter_id = @Counter_id
AND CAST(ac.Login_date AS DATE) = @LoginDate
AND (ac.Is_closed = 0 OR ac.Is_closed IS NULL)";

                SqlCommand cmd = new SqlCommand(counterQuery, db.cn);
                cmd.Parameters.AddWithValue("@Counter_id", Counter_id);
                cmd.Parameters.AddWithValue("@LoginDate", LoginDate.Date);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Counter not found");
                }

                var row = dt.Rows[0];

                // Sales data - from SALE table  live
                string salesQuery = @"
SELECT
    ISNULL(MIN(s.Pavati_no), 0)      AS Bill_from,
    ISNULL(MAX(s.Pavati_no), 0)      AS Bill_to,
    ISNULL(SUM(s.Final_amt), 0)      AS Total_sale,
    ISNULL(SUM(s.Receive_cash), 0)   AS Cash_sale,
    ISNULL(SUM(s.UPI_amt), 0)        AS Upi_pay,
    ISNULL(SUM(s.Redeem_amt), 0)     AS Cust_points,
    ISNULL(SUM(s.Return_cash), 0)    AS Cash_return
FROM SALE s
WHERE s.Status = 1
AND s.Counter_id = @Counter_id
AND CAST(s.Sale_date AS DATE) = @LoginDate";

                SqlCommand cmdSales = new SqlCommand(salesQuery, db.cn);
                cmdSales.Parameters.AddWithValue("@Counter_id", Counter_id);
                cmdSales.Parameters.AddWithValue("@LoginDate", LoginDate.Date);

                DataTable dtSales = new DataTable();
                new SqlDataAdapter(cmdSales).Fill(dtSales);

                db.Disconnect();

                var sales = dtSales.Rows[0];

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    // Counter Info
                    Id = row["Id"],
                    Counter_id = row["Counter_id"],
                    Counter_name = row["Counter_name"],
                    Emp_id = row["Emp_id"],
                    Emp_name = row["Emp_name"],
                    Login_date = row["Login_date"],
                    Login_time = row["Login_time"],
                    Is_closed = row["Is_closed"],
                    Opn_bal = row["Opn_bal"],
                    Closing_bal = row["Closing_bal"],
                    Log_out_date = row["Log_out_date"],
                    Log_out_time = row["Log_out_time"],

                    // Sales - from SALE  live
                    Bill_from = sales["Bill_from"],
                    Bill_to = sales["Bill_to"],
                    Total_sale = sales["Total_sale"],
                    Cash_sale = sales["Cash_sale"],
                    Card_pay = 0,              // manually enter
                    Upi_pay = sales["Upi_pay"],
                    Cust_points = sales["Cust_points"],
                    Cash_return = sales["Cash_return"],
                    Office_return = 0               // manually enter
                });
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


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
