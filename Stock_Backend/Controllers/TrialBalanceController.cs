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

                // =========================
                // HEADER
                // =========================

                string department = "All";

                if (Outlet_id != null)
                {
                    object dep = db.ExecuteScalar("SELECT Outlet_name FROM OUTLET WHERE Outlet_id = " + Outlet_id);
                    if (dep != null)
                        department = dep.ToString();
                }

                // =========================
                // OPENING BALANCE
                // =========================

                string openingQuery = @"
SELECT
    td.L_id,
    l.Ledger_name,
    l.Ledger_name_EN,

    ISNULL(SUM(CASE
        WHEN td.CrDr_id = 1 THEN td.Amount
        ELSE -td.Amount
    END),0) AS Opening_Balance

FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
INNER JOIN LEDGER l ON td.L_id = l.Ledger_id

WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) < @FromDate";

                if (Outlet_id != null)
                    openingQuery += " AND t.Outlet_id = @Outlet_id";

                openingQuery += @"
GROUP BY
    td.L_id,
    l.Ledger_name,
    l.Ledger_name_EN";

                SqlCommand cmd1 = new SqlCommand(openingQuery, db.cn);

                cmd1.Parameters.AddWithValue("@FromDate", FromDate.Date);

                if (Outlet_id != null)
                    cmd1.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dtOpening = new DataTable();
                new SqlDataAdapter(cmd1).Fill(dtOpening);

                // =========================
                // CURRENT TRANSACTIONS
                // =========================

                string transQuery = @"
SELECT
    td.L_id,
    l.Ledger_name,
    l.Ledger_name_EN,

    ISNULL(SUM(CASE
        WHEN td.CrDr_id = 1 THEN td.Amount
        ELSE -td.Amount
    END),0) AS Current_Balance

FROM TRANS t
INNER JOIN TRANS_DETAILS td ON t.Trans_id = td.Trans_id
INNER JOIN LEDGER l ON td.L_id = l.Ledger_id

WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE)
BETWEEN @FromDate AND @ToDate";

                if (Outlet_id != null)
                    transQuery += " AND t.Outlet_id = @Outlet_id";

                transQuery += @"
GROUP BY
    td.L_id,
    l.Ledger_name,
    l.Ledger_name_EN";

                SqlCommand cmd2 = new SqlCommand(transQuery, db.cn);

                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd2.Parameters.AddWithValue("@ToDate", ToDate.Date);

                if (Outlet_id != null)
                    cmd2.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dtTrans = new DataTable();
                new SqlDataAdapter(cmd2).Fill(dtTrans);

                // =========================
                // FINAL LIST
                // =========================

                List<object> Credit_List = new List<object>();
                List<object> Debit_List = new List<object>();

                decimal Total_Credit = 0;
                decimal Total_Debit = 0;

                var ledgerIds = dtOpening.AsEnumerable()
                    .Select(x => Convert.ToInt32(x["L_id"]))
                    .Union(
                        dtTrans.AsEnumerable()
                        .Select(x => Convert.ToInt32(x["L_id"]))
                    )
                    .Distinct();

                foreach (var ledgerId in ledgerIds)
                {
                    decimal opening = 0;
                    decimal current = 0;

                    var openRow = dtOpening.AsEnumerable()
                        .FirstOrDefault(x => Convert.ToInt32(x["L_id"]) == ledgerId);

                    if (openRow != null)
                        opening = Convert.ToDecimal(openRow["Opening_Balance"]);

                    var transRow = dtTrans.AsEnumerable()
                        .FirstOrDefault(x => Convert.ToInt32(x["L_id"]) == ledgerId);

                    if (transRow != null)
                        current = Convert.ToDecimal(transRow["Current_Balance"]);

                    decimal finalBalance = opening + current;

                    string ledgerName = "";

                    if (openRow != null)
                        ledgerName = openRow["Ledger_name"].ToString();
                    else if (transRow != null)
                        ledgerName = transRow["Ledger_name"].ToString();

                    string ledgerNameEN = "";

                    if (openRow != null)
                        ledgerNameEN = openRow["Ledger_name_EN"].ToString();
                    else if (transRow != null)
                        ledgerNameEN = transRow["Ledger_name_EN"].ToString();

                    if (finalBalance > 0)
                    {
                        Credit_List.Add(new
                        {
                            Ledger_id = ledgerId,
                            Ledger_name = ledgerName,
                            Ledger_name_EN = ledgerNameEN,
                            Amount = Math.Abs(finalBalance)
                        });

                        Total_Credit += Math.Abs(finalBalance);
                    }
                    else if (finalBalance < 0)
                    {
                        Debit_List.Add(new
                        {
                            Ledger_id = ledgerId,
                            Ledger_name = ledgerName,
                            Ledger_name_EN = ledgerNameEN,
                            Amount = Math.Abs(finalBalance)
                        });

                        Total_Debit += Math.Abs(finalBalance);
                    }
                }

                db.Disconnect();

                var result = new
                {
                    Header = new
                    {
                        FromDate = FromDate.ToString("dd-MM-yyyy"),
                        ToDate = ToDate.ToString("dd-MM-yyyy"),
                        Department = department
                    },

                    Credit_List = Credit_List,
                    Debit_List = Debit_List,

                    Summary = new
                    {
                        Total_Credit = Total_Credit,
                        Total_Debit = Total_Debit
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