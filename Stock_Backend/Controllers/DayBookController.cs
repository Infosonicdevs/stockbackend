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
        public HttpResponseMessage GetMainDaybook(DateTime FromDate, DateTime ToDate)
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
        AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate
        ORDER BY t.Trans_date";



                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // SUMMARY
                string sumQuery = @"
        SELECT 
            ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR,
            ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
            ISNULL(SUM(td.Amount),0) AS Grand_Total
        FROM TRANS t
        INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
        WHERE t.Status = 1
        AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate";

                SqlCommand cmd2 = new SqlCommand(sumQuery, db.cn);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd2.Parameters.AddWithValue("@ToDate", ToDate.Date);

                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                DataTable dt2 = new DataTable();
                da2.Fill(dt2);

                db.Disconnect();

                var result = new
                {
                    List = dt,
                    Summary = new
                    {
                        Total_DR = dt2.Rows[0]["Total_DR"],
                        Total_CR = dt2.Rows[0]["Total_CR"],
                        Grand_Total = dt2.Rows[0]["Grand_Total"]
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


        [Route("api/Daybook/Branch")]
        [HttpGet]
        public HttpResponseMessage GetBranchDaybook(DateTime FromDate, DateTime ToDate, int Outlet_id)
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
        AND t.Outlet_id = @Outlet_id
        AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate
        ORDER BY t.Trans_date";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);
                cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // SUMMARY
                string sumQuery = @"
        SELECT 
            ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR,
            ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
            ISNULL(SUM(td.Amount),0) AS Grand_Total
        FROM TRANS t
        INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
        WHERE t.Status = 1
        AND t.Outlet_id = @Outlet_id
        AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate";

                SqlCommand cmd2 = new SqlCommand(sumQuery, db.cn);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd2.Parameters.AddWithValue("@ToDate", ToDate.Date);
                cmd2.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                DataTable dt2 = new DataTable();
                da2.Fill(dt2);

                db.Disconnect();

                var result = new
                {
                    List = dt,
                    Summary = new
                    {
                        Total_DR = dt2.Rows[0]["Total_DR"],
                        Total_CR = dt2.Rows[0]["Total_CR"],
                        Grand_Total = dt2.Rows[0]["Grand_Total"]
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
        public HttpResponseMessage GetGeneral(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                db.Connect();

                string query = @"
        SELECT 
            t.Trans_id,
            t.Trans_date,
            t.Trans_no,
            l.Ledger_name
        FROM TRANS t
        INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
        INNER JOIN LEDGER l ON td.L_id = l.Ledger_id
        WHERE t.Status = 1
        AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate
        GROUP BY t.Trans_id, t.Trans_date, t.Trans_no, l.Ledger_name
        ORDER BY t.Trans_date";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // TOTAL
                string sumQuery = @"
        SELECT 
            ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR,
            ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
            ISNULL(SUM(td.Amount),0) AS Grand_Total
        FROM TRANS t
        INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
        WHERE t.Status = 1
        AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate";

                SqlCommand cmd2 = new SqlCommand(sumQuery, db.cn);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd2.Parameters.AddWithValue("@ToDate", ToDate.Date);

                SqlDataAdapter da2 = new SqlDataAdapter(cmd2);
                DataTable dt2 = new DataTable();
                da2.Fill(dt2);

                db.Disconnect();

                var result = new
                {
                    List = dt,
                    Summary = new
                    {
                        Total_DR = Convert.ToDecimal(dt2.Rows[0]["Total_DR"]),
                        Total_CR = Convert.ToDecimal(dt2.Rows[0]["Total_CR"]),
                        Grand_Total = Convert.ToDecimal(dt2.Rows[0]["Grand_Total"])
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
        public HttpResponseMessage GetDetails(int Trans_id)
        {
            try
            {
                db.Connect();

                string query = @"SELECT td.Trans_id,td.Amount,td.CrDr_id,td.Narr,l.Ledger_name, 
                               ISNULL(c.First_name + ' ' + ISNULL(c.Middle_name,'') + ' ' + c.Last_name, '') AS Customer_name
                               FROM TRANS_DETAILS td INNER JOIN LEDGER l ON td.L_id = l.Ledger_id
                               LEFT JOIN CUSTOMER c ON td.Cust_id = c.Cust_id
                               WHERE td.Trans_id = @Trans_id";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@Trans_id", Trans_id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // TOTAL
                string sumQuery = @"
        SELECT 
            ISNULL(SUM(CASE WHEN CrDr_id = 2 THEN Amount ELSE 0 END),0) AS Total_DR,
            ISNULL(SUM(CASE WHEN CrDr_id = 1 THEN Amount ELSE 0 END),0) AS Total_CR,
            ISNULL(SUM(Amount),0) AS Grand_Total
        FROM TRANS_DETAILS
        WHERE Trans_id = @Trans_id";

                SqlCommand cmd2 = new SqlCommand(sumQuery, db.cn);
                cmd2.Parameters.AddWithValue("@Trans_id", Trans_id);

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
                        Grand_Total = Convert.ToDecimal(dt2.Rows[0]["Grand_Total"])
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

    }
}