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

                // Ledger-wise grouped list
                string query = @"
SELECT 
    l.Ledger_id,
    l.Ledger_no,
    l.Ledger_name,
 td.CrDr_id,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END), 0) AS CR_Amount,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END), 0) AS DR_Amount
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
INNER JOIN LEDGER l ON td.L_id = l.Ledger_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate";

                if (Outlet_id != null)
                    query += " AND t.Outlet_id = @Outlet_id";

                query += " GROUP BY l.Ledger_id, l.Ledger_no, l.Ledger_name, td.CrDr_id ORDER BY l.Ledger_no";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                // Summary
                string sumQuery = @"
SELECT 
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate
AND td.L_id != 0";
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

                // Opening
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

                var list = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        Ledger_id = row["Ledger_id"],
                        Ledger_no = row["Ledger_no"],
                        Ledger_name = row["Ledger_name"],
                        CrDr_id = row["CrDr_id"],
                        CR_Amount = row["CR_Amount"],
                        DR_Amount = row["DR_Amount"]
                    });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    List = list,
                    Summary = new
                    {
                        Total_CR = Today_CR,
                        Total_DR = Today_DR,
                        Grand_Total = Math.Abs(Today_CR - Today_DR),
                        Opening_Balance = Opening_Balance,
                        Closing_Balance = Closing_Balance
                    }
                });
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

                // Ledger-wise grouped
                string query = @"
SELECT 
    l.Ledger_id,
    l.Ledger_no,
    l.Ledger_name,
 td.CrDr_id,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END), 0) AS CR_Amount,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END), 0) AS DR_Amount
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
INNER JOIN LEDGER l ON td.L_id = l.Ledger_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate";

                if (Outlet_id != null)
                    query += " AND t.Outlet_id = @Outlet_id";

                query += " GROUP BY l.Ledger_id, l.Ledger_no, l.Ledger_name, td.CrDr_id ORDER BY l.Ledger_no";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                // Summary
                string sumQuery = @"
SELECT 
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate
AND td.L_id != 0";

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

                // Opening
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

                var list = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        Ledger_id = row["Ledger_id"],
                        Ledger_no = row["Ledger_no"],
                        Ledger_name = row["Ledger_name"],
                        CrDr_id = row["CrDr_id"],
                        CR_Amount = row["CR_Amount"],
                        DR_Amount = row["DR_Amount"]
                    });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    List = list,
                    Summary = new
                    {
                        Total_CR = Today_CR,
                        Total_DR = Today_DR,
                        Grand_total = Math.Abs(Today_CR - Today_DR),
                        Opening_Balance = Opening_Balance,
                        Closing_Balance = Closing_Balance
                    }
                });
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

                // All entries with Ledger info
                string query = @"
SELECT 
    l.Ledger_id,
    l.Ledger_no,
    l.Ledger_name,
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
    ISNULL(c.First_name + ' ' + ISNULL(c.Middle_name,'') + ' ' + c.Last_name, '') AS Customer_name,
    CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END AS CR_Amount,
    CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END AS DR_Amount
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
INNER JOIN LEDGER l ON td.L_id = l.Ledger_id
LEFT JOIN CUSTOMER c ON td.Cust_id = c.Cust_id
LEFT JOIN TRANS_TYPE vt ON t.Trans_type_id = vt.Type_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate";

                if (Outlet_id != null)
                    query += " AND t.Outlet_id = @Outlet_id";

                query += " ORDER BY l.Ledger_no, t.Trans_id";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                // Summary
                string sumQuery = @"
SELECT 
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) = @FromDate
AND td.L_id != 0";

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

                // Opening
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

                //  Ledger-wise grouped
                var grouped = new List<object>();
                var ledgerGroups = dt.AsEnumerable()
                    .GroupBy(r => new {
                        Ledger_id = r["Ledger_id"],
                        Ledger_no = r["Ledger_no"],
                        Ledger_name = r["Ledger_name"]
                    });

                foreach (var group in ledgerGroups)
                {
                    var entries = new List<object>();
                    foreach (DataRow row in group)
                    {
                        entries.Add(new
                        {
                            Trans_id = row["Trans_id"],
                            Trans_date = row["Trans_date"],
                            Trans_no = row["Trans_no"],
                            Type_name = row["Type_name"],
                            Trans_Type = row["Trans_Type"],
                            Amount = row["Amount"],
                            CrDr_id = row["CrDr_id"],
                            Narr = row["Narr"],
                            Customer_name = row["Customer_name"],
                            CR_Amount = row["CR_Amount"],
                            DR_Amount = row["DR_Amount"]
                        });
                    }

                    grouped.Add(new
                    {
                        Ledger_id = group.Key.Ledger_id,
                        Ledger_no = group.Key.Ledger_no,
                        Ledger_name = group.Key.Ledger_name,
                        Total_CR = group.Sum(r => Convert.ToDecimal(r["CR_Amount"])),
                        Total_DR = group.Sum(r => Convert.ToDecimal(r["DR_Amount"])),
                        Entries = entries
                    });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Details = grouped,
                    Summary = new
                    {
                        Total_DR = Today_DR,
                        Total_CR = Today_CR,
                        Grand_total = Math.Abs(Today_CR - Today_DR),
                        Opening_Balance = Opening_Balance,
                        Closing_Balance = Closing_Balance
                    }
                });
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}