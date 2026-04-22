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
    public class Day_CloseController : ApiController
    {
        DbClass db = new DbClass();

        [HttpPost]
        [Route("api/dayclose")]
        public HttpResponseMessage GetDayClose(DayCloseModel model)
        {
            try
            {
                db.Connect();

                DateTime date = (model.Trans_date == DateTime.MinValue)
                                ? DateTime.Now.Date
                                : model.Trans_date;

                //Last Total
                string lastQuery = @"
SELECT 
ISNULL(SUM(CASE WHEN Trans_code IN ('C','S') THEN Trans_amt ELSE 0 END),0) -
ISNULL(SUM(CASE WHEN Trans_code='D' THEN Trans_amt ELSE 0 END),0)
FROM Trans
WHERE Outlet_id=@Outlet_id 
AND CAST(Trans_date AS DATE) < @Date";

                SqlCommand cmd1 = new SqlCommand(lastQuery, db.cn);
                cmd1.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);
                cmd1.Parameters.AddWithValue("@Date", date);

                decimal lastTotal = Convert.ToDecimal(cmd1.ExecuteScalar());

                //GET COUNTERS
                string counterQuery = @"SELECT ac.Counter_id,ac.Emp_id,ac.Status,
                                     (SELECT ISNULL(SUM(Trans_amt),0)
                                     FROM Trans
                                     WHERE Outlet_id = @Outlet_id
                                     AND CAST(Trans_date AS DATE) = @Date
                                     AND Trans_code IN ('C','S')) AS Total_CR,

                                     (SELECT ISNULL(SUM(Trans_amt),0)
                                     FROM Trans
                                     WHERE Outlet_id = @Outlet_id
                                     AND CAST(Trans_date AS DATE) = @Date
                                     AND Trans_code = 'D') AS Total_DR

                                     FROM ASSIGN_COUNTER ac
                                     WHERE CAST(ac.Login_date AS DATE) = @Date";

                SqlCommand cmd6 = new SqlCommand(counterQuery, db.cn);
                cmd6.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);
                cmd6.Parameters.AddWithValue("@Date", date);

                DataTable counterTable = new DataTable();
                new SqlDataAdapter(cmd6).Fill(counterTable);

                // Today CR
                string crQuery = @"SELECT ISNULL(SUM(Trans_amt),0)
                   FROM Trans
                   WHERE Outlet_id=@Outlet_id 
                   AND CAST(Trans_date AS DATE)=@Date 
                   AND Trans_code IN ('C','S')";

                SqlCommand cmd2 = new SqlCommand(crQuery, db.cn);
                cmd2.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);
                cmd2.Parameters.AddWithValue("@Date", date);

                decimal todayCR = Convert.ToDecimal(cmd2.ExecuteScalar());

                // Today DR
                string drQuery = @"SELECT ISNULL(SUM(Trans_amt),0)
                   FROM Trans
                   WHERE Outlet_id=@Outlet_id 
                   AND CAST(Trans_date AS DATE)=@Date 
                   AND Trans_code IN ('D')";

                SqlCommand cmd3 = new SqlCommand(drQuery, db.cn);
                cmd3.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);
                cmd3.Parameters.AddWithValue("@Date", date);

                decimal todayDR = Convert.ToDecimal(cmd3.ExecuteScalar());

                decimal todayTotal = todayCR - todayDR;
                decimal finalTotal = lastTotal + todayTotal;

                // Employee Login/Logout Count
                string loginQuery = @"SELECT COUNT(*) FROM ASSIGN_COUNTER 
                             WHERE Status='O' AND Login_date=@Date";

                SqlCommand cmd4 = new SqlCommand(loginQuery, db.cn);
                cmd4.Parameters.AddWithValue("@Date", date);

                int totalLogin = Convert.ToInt32(cmd4.ExecuteScalar());

                string logoutQuery = @"SELECT COUNT(*) FROM ASSIGN_COUNTER 
                              WHERE Status='C' AND Login_date=@Date";

                SqlCommand cmd5 = new SqlCommand(logoutQuery, db.cn);
                cmd5.Parameters.AddWithValue("@Date", date);

                int totalLogout = Convert.ToInt32(cmd5.ExecuteScalar());

                var result = new
                {
                    LastTotal = lastTotal,
                    TodayCR = todayCR,
                    TodayDR = todayDR,
                    TodayTotal = todayTotal,
                    FinalTotal = finalTotal,
                    TotalLogin = totalLogin,
                    TotalLogout = totalLogout,
                    Counters = counterTable
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);

                db.Disconnect();

                model.LastTotal = lastTotal;
                model.TodayCR = todayCR;
                model.TodayDR = todayDR;
                model.TodayTotal = todayTotal;
                model.FinalTotal = finalTotal;
                model.TotalLogin = totalLogin;
                model.TotalLogout = totalLogout;

                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut]
        [Route("api/dayclose/close")]
        public HttpResponseMessage CloseDay(DayCloseModel model)
        {
            try
            {
                db.Connect();

                DateTime date = model.Trans_date;

                // Logout all employees
                string updateQuery = @"
        UPDATE ASSIGN_COUNTER
        SET Status='C',
            Log_out_time = CONVERT(VARCHAR, GETDATE(), 108),
            Log_out_date = CAST(GETDATE() AS DATE),
            Closing_bal = @Closing
        WHERE Status='O' AND Login_date=@Date";

                SqlCommand cmd = new SqlCommand(updateQuery, db.cn);
                cmd.Parameters.AddWithValue("@Closing", model.FinalTotal);
                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = date.Date;

                cmd.ExecuteNonQuery();

                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, "Day Closed Successfully");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
