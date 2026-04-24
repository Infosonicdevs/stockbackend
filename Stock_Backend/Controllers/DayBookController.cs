using Stock_Backend.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class Day_BookController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/Daybook/Main")]
        [HttpGet]
        public HttpResponseMessage GetMainDaybook(DateTime FromDate, DateTime ToDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                string query = @"
SELECT 
    t.Trans_id,
    t.Trans_date,
    t.Trans_no,
    td.Amount,
    td.CrDr_id,
    td.Narr
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate";

                if (Outlet_id != null)
                    query += " AND t.Outlet_id = @Outlet_id";

                query += " ORDER BY t.Trans_date";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);
                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                // SUMMARY
                string sumQuery = @"
SELECT 
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate";

                if (Outlet_id != null)
                    sumQuery += " AND t.Outlet_id = @Outlet_id";

                SqlCommand cmd2 = new SqlCommand(sumQuery, db.cn);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd2.Parameters.AddWithValue("@ToDate", ToDate.Date);
                if (Outlet_id != null)
                    cmd2.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt2 = new DataTable();
                new SqlDataAdapter(cmd2).Fill(dt2);

                db.Disconnect();

                var result = new
                {
                    List = dt,
                    Summary = new
                    {
                        Total_CR = Convert.ToDecimal(dt2.Rows[0]["Total_CR"]),
                        Total_DR = Convert.ToDecimal(dt2.Rows[0]["Total_DR"]),
                        Grand_Total = Convert.ToDecimal(dt2.Rows[0]["Total_CR"]) -
                                      Convert.ToDecimal(dt2.Rows[0]["Total_DR"])
                    }
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


      
        [Route("api/daybook/general")]
        [HttpGet]
        public HttpResponseMessage GetGeneral(DateTime FromDate, DateTime ToDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                string query = @"SELECT 
    t.Trans_id,
    t.Trans_date,
    t.Trans_no,
    td.CrDr_id,
    td.Amount,
    l.Ledger_name,
    CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END AS CR_Amount,
    CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END AS DR_Amount
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
INNER JOIN LEDGER l ON td.L_id = l.Ledger_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate";

                if (Outlet_id != null)
                    query += " AND t.Outlet_id = @Outlet_id";

                query += " ORDER BY t.Trans_date, t.Trans_id";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);
                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                // TOTAL
                string sumQuery = @"SELECT 
                                  ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
                                  ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR
                                  FROM TRANS t
                                  INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
                                  WHERE t.Status = 1
                                  AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate";

                if (Outlet_id != null)
                    sumQuery += " AND t.Outlet_id = @Outlet_id";

                SqlCommand cmd2 = new SqlCommand(sumQuery, db.cn);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd2.Parameters.AddWithValue("@ToDate", ToDate.Date);
                if (Outlet_id != null)
                    cmd2.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt2 = new DataTable();
                new SqlDataAdapter(cmd2).Fill(dt2);

                db.Disconnect();

                var result = new
                {
                    List = dt,
                    Summary = new
                    {
                        Total_CR = Convert.ToDecimal(dt2.Rows[0]["Total_CR"]),
                        Total_DR = Convert.ToDecimal(dt2.Rows[0]["Total_DR"]),
                    }
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/daybook/details")]
        [HttpGet]
        public HttpResponseMessage GetDetails(DateTime FromDate, DateTime ToDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();
                string query = @"SELECT t.Trans_id,t.Trans_date,t.Trans_no,td.Amount,td.CrDr_id,td.Narr,l.Ledger_name,
                               ISNULL(c.First_name + ' ' + ISNULL(c.Middle_name,'') + ' ' + c.Last_name, '') AS Customer_name,
                               CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END AS CR_Amount,
                               CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END AS DR_Amount
                               FROM TRANS t
                               INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
                               INNER JOIN LEDGER l ON td.L_id = l.Ledger_id
                               LEFT JOIN CUSTOMER c ON td.Cust_id = c.Cust_id
                               WHERE t.Status = 1
                               AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate";

                //  Outlet filter
                if (Outlet_id != null)
                {
                    query += " AND t.Outlet_id = @Outlet_id";
                }

                query += " ORDER BY t.Trans_date";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);

                if (Outlet_id != null)
                {
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // ===== TOTAL =====
                string sumQuery = @"
        SELECT 
            ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR,
            ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
            ISNULL(SUM(td.Amount),0) AS Grand_Total
        FROM TRANS t
        INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
        WHERE t.Status = 1
        AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate";

                if (Outlet_id != null)
                {
                    sumQuery += " AND t.Outlet_id = @Outlet_id";
                }

                SqlCommand cmd2 = new SqlCommand(sumQuery, db.cn);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd2.Parameters.AddWithValue("@ToDate", ToDate.Date);

                if (Outlet_id != null)
                {
                    cmd2.Parameters.AddWithValue("@Outlet_id", Outlet_id);
                }

                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                DataTable dt2 = new DataTable();
                da2.Fill(dt2);

                db.Disconnect();

                var result = new
                {
                    Details = dt,
                    Summary = new
                    {
                        Total_DR = Convert.ToDecimal(dt2.Rows[0]["Total_DR"]),
                        Total_CR = Convert.ToDecimal(dt2.Rows[0]["Total_CR"]),
                    }
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("api/daybook/balance")]
        [HttpGet]
        public HttpResponseMessage GetFullBalance(DateTime FromDate, DateTime ToDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                // OPENING BALANCE — daily wise ABS cumulative
                string openingQuery = @"
                SELECT 
                 CAST(t.Trans_date AS DATE) AS TransDate,
                 ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS CR,
                 ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS DR
                 FROM TRANS t
                 INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
                 WHERE t.Status = 1
                 AND td.CashTrans = 'C'
                 AND CAST(t.Trans_date AS DATE) < @FromDate";

                if (Outlet_id != null)
                    openingQuery += " AND t.Outlet_id = @Outlet_id";

                openingQuery += " GROUP BY CAST(t.Trans_date AS DATE) ORDER BY TransDate";

                SqlCommand cmd1 = new SqlCommand(openingQuery, db.cn);
                cmd1.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd1.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt1 = new DataTable();
                new SqlDataAdapter(cmd1).Fill(dt1);

                // Per day ABS add 
                decimal Opening_Balance = 0;
                foreach (DataRow row in dt1.Rows)
                {
                    decimal cr = Convert.ToDecimal(row["CR"]);
                    decimal dr = Convert.ToDecimal(row["DR"]);
                    Opening_Balance += Math.Abs(cr - dr);
                }

                // TODAY TRANSACTIONS
                string todayQuery = @"
                SELECT 
                ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS CR,
                ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS DR
                FROM TRANS t
                INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
                WHERE t.Status = 1
                AND td.CashTrans = 'C'
                AND CAST(t.Trans_date AS DATE) = @FromDate";

                if (Outlet_id != null)
                    todayQuery += " AND t.Outlet_id = @Outlet_id";

                SqlCommand cmd2 = new SqlCommand(todayQuery, db.cn);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd2.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt2 = new DataTable();
                new SqlDataAdapter(cmd2).Fill(dt2);

                decimal Today_CR = Convert.ToDecimal(dt2.Rows[0]["CR"]);
                decimal Today_DR = Convert.ToDecimal(dt2.Rows[0]["DR"]);

                // CALCULATION
                decimal Today_Difference = Math.Abs(Today_CR - Today_DR);
                decimal Closing_Balance = Opening_Balance + Today_Difference;

                var result = new
                {
                    Opening_Balance = Opening_Balance,
                    Today_CR = Today_CR,
                    Today_DR = Today_DR,
                    Today_Difference = Today_Difference,
                    Cl_Balance = Closing_Balance
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

    }
}