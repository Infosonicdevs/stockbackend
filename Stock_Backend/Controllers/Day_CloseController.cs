using Stock_Backend.Models;
using System;
using System.Data;
using System.Data.SqlClient;
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
                                : model.Trans_date.Date;

                //  LAST TOTAL (OPENING)
                // OPENING BALANCE — daily wise ABS cumulative (same as GetFullBalance)
                string lastQuery = @"
SELECT 
    CAST(t.Trans_date AS DATE) AS TransDate,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS CR,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS DR
FROM Trans t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Outlet_id = @Outlet_id
AND t.Status = 1
AND td.CashTrans = 'C'
AND CAST(t.Trans_date AS DATE) < @Date
GROUP BY CAST(t.Trans_date AS DATE)
ORDER BY TransDate";

                SqlCommand cmd1 = new SqlCommand(lastQuery, db.cn);
                cmd1.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);
                cmd1.Parameters.AddWithValue("@Date", date);

                DataTable dt1 = new DataTable();
                new SqlDataAdapter(cmd1).Fill(dt1);

                // Per day ABS add — cumulative opening balance
                decimal lastTotal = 0;
                foreach (DataRow row in dt1.Rows)
                {
                    decimal cr = Convert.ToDecimal(row["CR"]);
                    decimal dr = Convert.ToDecimal(row["DR"]);
                    lastTotal += Math.Abs(cr - dr);
                }

         

                // TODAY CR / DR
                string todayQuery = @"
SELECT 
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS CR,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS DR
FROM Trans t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Outlet_id = @Outlet_id
AND CAST(t.Trans_date AS DATE) = @Date";

                SqlCommand cmd2 = new SqlCommand(todayQuery, db.cn);
                cmd2.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);
                cmd2.Parameters.AddWithValue("@Date", date);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd2).Fill(dt);

                decimal todayCR = Convert.ToDecimal(dt.Rows[0]["CR"]);
                decimal todayDR = Convert.ToDecimal(dt.Rows[0]["DR"]);

                decimal todayTotal = Math.Abs(todayCR - todayDR);
                decimal finalTotal = lastTotal + todayTotal;

                //  LOGIN / LOGOUT COUNT
                string loginQuery = @"
SELECT COUNT(*) FROM ASSIGN_COUNTER ac
INNER JOIN COUNTER c ON ac.Counter_id = c.Counter_id
WHERE ac.Status = 'O' 
AND CAST(ac.Login_date AS DATE) = @Date
AND c.Outlet_id = @Outlet_id";

                SqlCommand cmd3 = new SqlCommand(loginQuery, db.cn);
                cmd3.Parameters.AddWithValue("@Date", date);
                cmd3.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);

                int totalLogin = Convert.ToInt32(cmd3.ExecuteScalar());

                string logoutQuery = @"
SELECT COUNT(*) FROM ASSIGN_COUNTER ac
INNER JOIN COUNTER c ON ac.Counter_id = c.Counter_id
WHERE ac.Status = 'C' 
AND CAST(ac.Login_date AS DATE) = @Date
AND c.Outlet_id = @Outlet_id";

                SqlCommand cmd4 = new SqlCommand(logoutQuery, db.cn);
                cmd4.Parameters.AddWithValue("@Date", date);
                cmd4.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);
                int totalLogout = Convert.ToInt32(cmd4.ExecuteScalar());

                //  COUNTER LIST
                string counterQuery = @"
SELECT 
    ac.Counter_id,
    c.Counter_name,
    ac.Emp_id,
    ac.Status,
    ac.Login_date,
    ac.Log_out_date,
    ac.Closing_bal
FROM ASSIGN_COUNTER ac
INNER JOIN COUNTER c ON ac.Counter_id = c.Counter_id
WHERE CAST(ac.Login_date AS DATE) = @Date
AND c.Outlet_id = @Outlet_id";

                SqlCommand cmd5 = new SqlCommand(counterQuery, db.cn);
                cmd5.Parameters.AddWithValue("@Date", date);
                cmd5.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);



                DataTable counterList = new DataTable();
                new SqlDataAdapter(cmd5).Fill(counterList);

                db.Disconnect();

                //  FINAL 
                var result = new
                {
                    Outlet_id = model.Outlet_id,
                    Trans_date = date,

                    LastTotal = lastTotal,
                    TodayCR = todayCR,
                    TodayDR = todayDR,
                    TodayTotal = todayTotal,
                    FinalTotal = finalTotal,

                    TotalLogin = totalLogin,
                    TotalLogout = totalLogout,

                    Counters = counterList
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // CLOSE DAY
        [HttpPut]
        [Route("api/dayclose/close")]
        public HttpResponseMessage CloseDay(DayCloseModel model)
        {
            try
            {
                db.Connect();

                DateTime date = model.Trans_date.Date;

                string updateQuery = @"
UPDATE ASSIGN_COUNTER
SET Status='C',
    Log_out_time = CONVERT(VARCHAR, GETDATE(), 108),
    Log_out_date = CAST(GETDATE() AS DATE),
    Closing_bal = @Closing
WHERE Status='O' 
AND CAST(Login_date AS DATE)=@Date";

                SqlCommand cmd = new SqlCommand(updateQuery, db.cn);
                cmd.Parameters.AddWithValue("@Closing", model.FinalTotal);
                cmd.Parameters.AddWithValue("@Date", date);

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