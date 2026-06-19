
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class TrialBalanceController : ApiController
    {
        DbClass db = new DbClass();

        [HttpGet]
        [Route("api/trialbalance")]
        public HttpResponseMessage GetTrialBalance(DateTime FromDate, DateTime ToDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                string department = "All";

                if (Outlet_id != null)
                {
                    object dep = db.ExecuteScalar(
                        "SELECT Outlet_name FROM OUTLET WHERE Outlet_id = " + Outlet_id
                    );

                    if (dep != null)
                        department = dep.ToString();
                }

                // ==================  OPENING BALANCE ==================
                string openingQuery = @"
SELECT 
    CAST(t.Trans_date AS DATE) AS Trans_Day,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END),0) AS CR,
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END),0) AS DR
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
INNER JOIN LEDGER l ON td.L_id = l.Ledger_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) < @FromDate";

                if (Outlet_id != null)
                    openingQuery += " AND t.Outlet_id = @Outlet_id";

                openingQuery += " GROUP BY CAST(t.Trans_date AS DATE)";

                SqlCommand cmdOpen = new SqlCommand(openingQuery, db.cn);
                cmdOpen.Parameters.AddWithValue("@FromDate", FromDate.Date);
                if (Outlet_id != null)
                    cmdOpen.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dtOpen = new DataTable();
                new SqlDataAdapter(cmdOpen).Fill(dtOpen);

                // Every day |CR - DR| cumulative add 
                decimal Opening_Balance = 0;
                foreach (DataRow r in dtOpen.Rows)
                {
                    decimal cr = Convert.ToDecimal(r["CR"]);
                    decimal dr = Convert.ToDecimal(r["DR"]);
                    Opening_Balance += Math.Abs(cr - dr);  // everyday Math.Abs
                }

                //decimal Closing_Balance = Opening_Balance + Math.Abs(Today_CR - Today_DR);

                // ================== CURRENT PERIOD ==================
                string query = @"
SELECT
    td.L_id,
    l.Ledger_name,
    l.Ledger_name_EN,

    SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END) AS Credit,
    SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END) AS Debit

FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
INNER JOIN LEDGER l ON td.L_id = l.Ledger_id

WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE)
BETWEEN @FromDate AND @ToDate";

                if (Outlet_id != null)
                    query += " AND t.Outlet_id = @Outlet_id";

                query += @"
GROUP BY
    td.L_id,
    l.Ledger_name,
    l.Ledger_name_EN";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);

                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                // ==================  BUILD RESPONSE ==================
                List<object> Credit_List = new List<object>();
                List<object> Debit_List = new List<object>();

                decimal Total_Credit = 0;
                decimal Total_Debit = 0;

                foreach (DataRow row in dt.Rows)
                {
                    decimal credit = Convert.ToDecimal(row["Credit"]);
                    decimal debit = Convert.ToDecimal(row["Debit"]);

                    if (credit > 0)
                    {
                        Credit_List.Add(new
                        {
                            Ledger_id = row["L_id"],
                            Ledger_name = row["Ledger_name"].ToString(),
                            Ledger_name_EN = row["Ledger_name_EN"].ToString(),
                            Amount = credit
                        });

                        Total_Credit += Math.Abs(credit);
                    }

                    if (debit > 0)
                    {
                        Debit_List.Add(new
                        {
                            Ledger_id = row["L_id"],
                            Ledger_name = row["Ledger_name"].ToString(),
                            Ledger_name_EN = row["Ledger_name_EN"].ToString(),
                            Amount = debit
                        });

                        Total_Debit += Math.Abs(debit);
                    }
                }
                // ================== CLOSING BALANCE ==================
                decimal Closing_Balance = Opening_Balance + Math.Abs(Total_Credit - Total_Debit);

                // Grand Totals
                decimal Grand_Total_Credit = Total_Credit + Opening_Balance;  // Credit + Opening
                decimal Grand_Total_Debit = Total_Debit + Closing_Balance;  // Debit + Closing

                db.Disconnect();

                // ================== FINAL OUTPUT ==================
                var result = new
                {
                    Header = new
                    {
                        FromDate = FromDate.ToString("dd-MM-yyyy"),
                        ToDate = ToDate.ToString("dd-MM-yyyy"),
                        Department = department
                    },

                    Opening_Balance = Opening_Balance,

                    Credit_List = Credit_List,
                    Debit_List = Debit_List,

                    Summary = new
                    {
                        Total_Credit = Total_Credit,
                        Total_Debit = Total_Debit,
                        Closing_Balance = Closing_Balance,
                        Grand_Total_Credit = Grand_Total_Credit,           // Credit + Opening
                        Grand_Total_Debit = Grand_Total_Debit             // Debit + Closing
                    }
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();

                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    ex.Message
                );
            }
        }
    }
}