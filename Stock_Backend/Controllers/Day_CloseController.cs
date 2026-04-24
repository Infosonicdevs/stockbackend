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
                // OPENING BALANCE — daily 
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

                // Per day ABS add —  opening balance
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
WHERE (ac.Is_closed = 0 OR ac.Is_closed IS NULL)
AND CAST(ac.Login_date AS DATE) = @Date
AND c.Outlet_id = @Outlet_id";

                SqlCommand cmd3 = new SqlCommand(loginQuery, db.cn);
                cmd3.Parameters.AddWithValue("@Date", date);
                cmd3.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);

                int totalLogin = Convert.ToInt32(cmd3.ExecuteScalar());

                string logoutQuery = @"
SELECT COUNT(*) FROM ASSIGN_COUNTER ac
INNER JOIN COUNTER c ON ac.Counter_id = c.Counter_id
WHERE ac.Is_closed = 1
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
    ac.Opn_bal,
    ac.Closing_bal,
    ISNULL(SUM(s.Receive_cash), 0) AS Total_CR,
    ISNULL(SUM(s.Return_cash), 0) AS Total_DR,
    ISNULL(SUM(s.Receive_cash), 0) - ISNULL(SUM(s.Return_cash), 0) AS Net_Total
FROM ASSIGN_COUNTER ac
INNER JOIN COUNTER c ON ac.Counter_id = c.Counter_id
LEFT JOIN SALE s ON s.Counter_id = ac.Counter_id
    AND CAST(s.Sale_date AS DATE) = @Date
    AND s.Status = 1
WHERE CAST(ac.Login_date AS DATE) = @Date
AND c.Outlet_id = @Outlet_id
GROUP BY 
    ac.Counter_id, c.Counter_name, ac.Emp_id, ac.Status,
    ac.Login_date, ac.Log_out_date, ac.Opn_bal, ac.Closing_bal";

                SqlCommand cmd5 = new SqlCommand(counterQuery, db.cn);
                cmd5.Parameters.AddWithValue("@Date", date);
                cmd5.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);

       

                DataTable counterList = new DataTable();
                new SqlDataAdapter(cmd5).Fill(counterList);

                var counterResult = new System.Collections.Generic.List<object>();
                foreach (DataRow row in counterList.Rows)
                {
                    decimal opn_bal = Convert.ToDecimal(row["Opn_bal"]);
                    decimal net_total = Convert.ToDecimal(row["Net_Total"]);

                    counterResult.Add(new
                    {
                        Counter_id = row["Counter_id"],
                        Counter_name = row["Counter_name"],
                        Emp_id = row["Emp_id"],
                        Status = row["Status"],
                        Login_date = row["Login_date"],
                        Log_out_date = row["Log_out_date"],
                        Opn_bal = opn_bal,
                        Total_CR = row["Total_CR"],
                        Total_DR = row["Total_DR"],
                        Net_Total = net_total,
                        Closing_bal = opn_bal + net_total  // ← Opening + Net_Total
                    });
                }

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
                    Counters = counterResult  
                };

                db.Disconnect();

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

                string checkQuery = @"
                     SELECT COUNT(*) FROM ASSIGN_COUNTER ac
                     INNER JOIN COUNTER c ON ac.Counter_id = c.Counter_id
                     WHERE (ac.Is_closed = 0 OR ac.Is_closed IS NULL)
                     AND CAST(ac.Login_date AS DATE) = @Date
                     AND c.Outlet_id = @Outlet_id";

                SqlCommand checkCmd = new SqlCommand(checkQuery, db.cn);
                checkCmd.Parameters.AddWithValue("@Date", date);
                checkCmd.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);

                int activeCounters = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (activeCounters > 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest,
                        "Counters are not logged out. Please logout all counters first.");
                }

                string updateQuery = @"
UPDATE ac
SET ac.Is_closed = 1,
    ac.Log_out_time = CONVERT(VARCHAR, GETDATE(), 108),
    ac.Log_out_date = CAST(GETDATE() AS DATE),
    ac.Closing_bal = @Closing
FROM ASSIGN_COUNTER ac
INNER JOIN COUNTER c ON ac.Counter_id = c.Counter_id
WHERE (ac.Is_closed = 0 OR ac.Is_closed IS NULL)
AND CAST(ac.Login_date AS DATE) = @Date
AND c.Outlet_id = @Outlet_id";

                SqlCommand cmd = new SqlCommand(updateQuery, db.cn);
                cmd.Parameters.AddWithValue("@Closing", model.FinalTotal);
                cmd.Parameters.AddWithValue("@Date", date);
                cmd.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);
                cmd.ExecuteNonQuery();

                //TEMP_TBL Status = 0
                string tempQuery = @"
UPDATE TOP(1) TEMP_TBL 
SET Status = 0 
WHERE CAST(Date AS DATE) = @Date
AND Vibhag_id = @Outlet_id
AND Status = 1";

                SqlCommand cmdTemp = new SqlCommand(tempQuery, db.cn);
                cmdTemp.Parameters.AddWithValue("@Date", date);
                cmdTemp.Parameters.AddWithValue("@Outlet_id", model.Outlet_id);
                cmdTemp.ExecuteNonQuery();

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