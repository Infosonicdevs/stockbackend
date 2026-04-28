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

        // ================= MAIN =================
        [Route("api/Daybook/Main")]
        [HttpGet]
        public HttpResponseMessage GetMainDaybook(DateTime FromDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                string query = @"
SELECT 
    t.Trans_id,
    t.Trans_date,
    t.Trans_no,
    vt.Type_name, 
CASE 
    WHEN td.CashTrans = 'C' THEN 'Cash'
    WHEN td.CashTrans = 'T' THEN 'Transfer'
END AS Trans_Type,
    td.Amount,
    td.CrDr_id,
    td.Narr
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
LEFT JOIN VIEW_TRANS vt ON t.Trans_id = vt.Trans_id 
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate";

                if (Outlet_id != null)
                    query += " AND t.Outlet_id = @Outlet_id";

                query += " ORDER BY t.Trans_date";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                // ===== TODAY SUMMARY =====
                string sumQuery = @"
SELECT 
ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate";

                if (Outlet_id != null)
                    sumQuery += " AND t.Outlet_id = @Outlet_id";

                SqlCommand cmd2 = new SqlCommand(sumQuery, db.cn);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd2.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt2 = new DataTable();
                new SqlDataAdapter(cmd2).Fill(dt2);

                decimal Today_CR = Convert.ToDecimal(dt2.Rows[0]["Total_CR"]);
                decimal Today_DR = Convert.ToDecimal(dt2.Rows[0]["Total_DR"]);

                // ===== OPENING =====
                string openingQuery = @"
SELECT 
ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS CR,
ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND td.CashTrans = 'C'
AND CAST(t.Trans_date AS DATE) < @FromDate";

                if (Outlet_id != null)
                    openingQuery += " AND t.Outlet_id = @Outlet_id";

                SqlCommand cmdOpen = new SqlCommand(openingQuery, db.cn);
                cmdOpen.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmdOpen.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dtOpen = new DataTable();
                new SqlDataAdapter(cmdOpen).Fill(dtOpen);

                decimal Opening_Balance = Math.Abs(
                    Convert.ToDecimal(dtOpen.Rows[0]["CR"]) -
                    Convert.ToDecimal(dtOpen.Rows[0]["DR"])
                );

                decimal Today_Diff = Math.Abs(Today_CR - Today_DR);
                decimal Closing_Balance = Opening_Balance + Today_Diff;

                decimal totalCR = Convert.ToDecimal(dt2.Rows[0]["Total_CR"]);
                decimal totalDR = Convert.ToDecimal(dt2.Rows[0]["Total_DR"]);

                decimal grandTotal = Math.Abs(totalCR - totalDR);

                db.Disconnect();

                var result = new
                {
                    List = dt,
                    Summary = new
                    {
                        Total_CR = Today_CR,
                        Total_DR = Today_DR,
                        Grand_Total = grandTotal,
                        Opening_Balance = Opening_Balance,
                        Closing_Balance = Closing_Balance
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


        // ================= GENERAL =================
        [Route("api/daybook/general")]
        [HttpGet]
        public HttpResponseMessage GetGeneral(DateTime FromDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                string query = @"SELECT 
t.Trans_id,
t.Trans_date,
t.Trans_no,
    vt.Type_name, 
CASE 
    WHEN td.CashTrans = 'C' THEN 'Cash'
    WHEN td.CashTrans = 'T' THEN 'Transfer'
END AS Trans_Type,
td.CrDr_id,
td.Amount,
l.Ledger_name,
CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END AS CR_Amount,
CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END AS DR_Amount
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
INNER JOIN LEDGER l ON td.L_id = l.Ledger_id
LEFT JOIN VIEW_TRANS vt ON t.Trans_id = vt.Trans_id 
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate";

                if (Outlet_id != null)
                    query += " AND t.Outlet_id = @Outlet_id";

                query += " ORDER BY t.Trans_date, t.Trans_id";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                string sumQuery = @"SELECT 
ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate";

                if (Outlet_id != null)
                    sumQuery += " AND t.Outlet_id = @Outlet_id";

                SqlCommand cmd2 = new SqlCommand(sumQuery, db.cn);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd2.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt2 = new DataTable();
                new SqlDataAdapter(cmd2).Fill(dt2);

                decimal Today_CR = Convert.ToDecimal(dt2.Rows[0]["Total_CR"]);
                decimal Today_DR = Convert.ToDecimal(dt2.Rows[0]["Total_DR"]);

                // OPENING
                string openingQuery = @"
SELECT 
ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS CR,
ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND td.CashTrans = 'C'
AND CAST(t.Trans_date AS DATE) < @FromDate";

                if (Outlet_id != null)
                    openingQuery += " AND t.Outlet_id = @Outlet_id";

                SqlCommand cmdOpen = new SqlCommand(openingQuery, db.cn);
                cmdOpen.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmdOpen.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dtOpen = new DataTable();
                new SqlDataAdapter(cmdOpen).Fill(dtOpen);

                decimal Opening_Balance = Math.Abs(
                    Convert.ToDecimal(dtOpen.Rows[0]["CR"]) -
                    Convert.ToDecimal(dtOpen.Rows[0]["DR"])
                );

                decimal Closing_Balance = Opening_Balance + Math.Abs(Today_CR - Today_DR);

                db.Disconnect();

                var result = new
                {
                    List = dt,
                    Summary = new
                    {
                        Total_CR = Today_CR,
                        Total_DR = Today_DR,
                        Opening_Balance = Opening_Balance,
                        Closing_Balance = Closing_Balance
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


        // ================= DETAILS =================
        [Route("api/daybook/details")]
        [HttpGet]
        public HttpResponseMessage GetDetails(DateTime FromDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                string query = @"SELECT 
t.Trans_id,
t.Trans_date,
t.Trans_no,
vt.Type_name,
CASE 
    WHEN td.CashTrans = 'C' THEN 'Cash'
    WHEN td.CashTrans = 'T' THEN 'Transfer'
END AS Trans_Type,
td.Amount,
td.CrDr_id,
td.Narr,
l.Ledger_name,
ISNULL(c.First_name + ' ' + ISNULL(c.Middle_name,'') + ' ' + c.Last_name, '') AS Customer_name,
CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END AS CR_Amount,
CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END AS DR_Amount
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
INNER JOIN LEDGER l ON td.L_id = l.Ledger_id
LEFT JOIN CUSTOMER c ON td.Cust_id = c.Cust_id
LEFT JOIN VIEW_TRANS vt ON t.Trans_id = vt.Trans_id 
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate";

                if (Outlet_id != null)
                    query += " AND t.Outlet_id = @Outlet_id";

                query += " ORDER BY t.Trans_date";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                string sumQuery = @"
SELECT 
ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate";

                if (Outlet_id != null)
                    sumQuery += " AND t.Outlet_id = @Outlet_id";

                SqlCommand cmd2 = new SqlCommand(sumQuery, db.cn);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd2.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt2 = new DataTable();
                new SqlDataAdapter(cmd2).Fill(dt2);

                decimal Today_CR = Convert.ToDecimal(dt2.Rows[0]["Total_CR"]);
                decimal Today_DR = Convert.ToDecimal(dt2.Rows[0]["Total_DR"]);

                // OPENING
                string openingQuery = @"
SELECT 
ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS CR,
ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND td.CashTrans = 'C'
AND CAST(t.Trans_date AS DATE) < @FromDate";

                if (Outlet_id != null)
                    openingQuery += " AND t.Outlet_id = @Outlet_id";

                SqlCommand cmdOpen = new SqlCommand(openingQuery, db.cn);
                cmdOpen.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmdOpen.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dtOpen = new DataTable();
                new SqlDataAdapter(cmdOpen).Fill(dtOpen);

                decimal Opening_Balance = Math.Abs(
                    Convert.ToDecimal(dtOpen.Rows[0]["CR"]) -
                    Convert.ToDecimal(dtOpen.Rows[0]["DR"])
                );

                decimal Closing_Balance = Opening_Balance + Math.Abs(Today_CR - Today_DR);

                db.Disconnect();

                var result = new
                {
                    Details = dt,
                    Summary = new
                    {
                        Total_DR = Today_DR,
                        Total_CR = Today_CR,
                        Opening_Balance = Opening_Balance,
                        Closing_Balance = Closing_Balance
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