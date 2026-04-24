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
    public class Ledger_reportController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/ledger/report")]
        [HttpGet]
        public HttpResponseMessage GetLedgerReport(int Ledger_id, DateTime FromDate, DateTime ToDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                // OPENING BALANCE
                string openingQuery = @"
SELECT 
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) -
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0)
    AS Opening_Balance
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND td.L_id = @Ledger_id
AND CAST(t.Trans_date AS DATE) < @FromDate";

                if (Outlet_id != null)
                    openingQuery += " AND t.Outlet_id = @Outlet_id";

                SqlCommand cmd1 = new SqlCommand(openingQuery, db.cn);
                cmd1.Parameters.AddWithValue("@Ledger_id", Ledger_id);
                cmd1.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmd1.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                decimal Opening_Balance = Math.Abs(Convert.ToDecimal(cmd1.ExecuteScalar()));

                // TRANSACTIONS LIST
                string listQuery = @"
SELECT 
    t.Trans_id,
    t.Trans_date,
    t.Trans_no,
    td.Amount,
    td.CrDr_id,
    td.Narr,
    l.Ledger_name,
    CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END AS CR_Amount,
    CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END AS DR_Amount
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
INNER JOIN LEDGER l ON td.L_id = l.Ledger_id
WHERE t.Status = 1
AND td.L_id = @Ledger_id
AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate";

                if (Outlet_id != null)
                    listQuery += " AND t.Outlet_id = @Outlet_id";

                listQuery += " ORDER BY t.Trans_date, t.Trans_id";

                SqlCommand cmd2 = new SqlCommand(listQuery, db.cn);
                cmd2.Parameters.AddWithValue("@Ledger_id", Ledger_id);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd2.Parameters.AddWithValue("@ToDate", ToDate.Date);
                if (Outlet_id != null)
                    cmd2.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd2).Fill(dt);

                // TOTAL
                string totalQuery = @"
SELECT 
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS Total_CR,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS Total_DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
WHERE t.Status = 1
AND td.L_id = @Ledger_id
AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate";

                if (Outlet_id != null)
                    totalQuery += " AND t.Outlet_id = @Outlet_id";

                SqlCommand cmd3 = new SqlCommand(totalQuery, db.cn);
                cmd3.Parameters.AddWithValue("@Ledger_id", Ledger_id);
                cmd3.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd3.Parameters.AddWithValue("@ToDate", ToDate.Date);
                if (Outlet_id != null)
                    cmd3.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt2 = new DataTable();
                new SqlDataAdapter(cmd3).Fill(dt2);
                decimal Total_CR = Convert.ToDecimal(dt2.Rows[0]["Total_CR"]);
                decimal Total_DR = Convert.ToDecimal(dt2.Rows[0]["Total_DR"]);
                decimal Net_Total = Math.Abs(Total_CR - Total_DR);
                decimal Closing_Balance = Opening_Balance + Net_Total;



                db.Disconnect();

                var result = new
                {
                    Opening_Balance = Opening_Balance,
                    List = dt,
                    Summary = new
                    {
                        Total_CR = Total_CR,
                        Total_DR = Total_DR,
                        Net_Total = Net_Total,
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
