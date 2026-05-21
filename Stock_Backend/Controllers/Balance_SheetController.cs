
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
    public class Balance_SheetController : ApiController
    {
        DbClass db = new DbClass();
        [Route("api/taleband")]
        [HttpGet]
        public HttpResponseMessage GetTaleband(DateTime FromDate, DateTime ToDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                // Previous year same date
                DateTime prevDate = new DateTime(FromDate.Year - 1, FromDate.Month, FromDate.Day).AddDays(-1);

                string query = @"
SELECT 
    lg.L_group_id,
    lg.L_group_name,
    lg.Seqno,
    lg.crdr_id,

    l.Ledger_id,
    l.Ledger_name,

    -- Previous Year
    ISNULL(SUM(
        CASE 
            WHEN CAST(t.Trans_date AS DATE) <= @PrevDate
            THEN 
                CASE 
                    WHEN td.CrDr_id = 1 THEN td.Amount * -1
                    ELSE td.Amount
                END
            ELSE 0
        END
    ),0) AS Prev_Amt,

    -- Current Range
    ISNULL(SUM(
        CASE 
            WHEN CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate
            THEN 
                CASE 
                    WHEN td.CrDr_id = 1 THEN td.Amount * -1
                    ELSE td.Amount
                END
            ELSE 0
        END
    ),0) AS Curr_Amt

FROM LEDGER_GROUP lg
INNER JOIN LEDGER l ON l.Ledger_group_id = lg.L_group_id
LEFT JOIN TRANS_DETAILS td ON td.L_id = l.Ledger_id
LEFT JOIN TRANS t ON t.Trans_id = td.Trans_id

WHERE t.Status = 1
";

                if (Outlet_id != null)
                    query += " AND t.Outlet_id = @Outlet_id";

                query += @"
GROUP BY 
    lg.L_group_id,
    lg.L_group_name,
    lg.Seqno,
    lg.crdr_id,
    l.Ledger_id,
    l.Ledger_name

ORDER BY lg.Seqno";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);
                cmd.Parameters.AddWithValue("@PrevDate", prevDate.Date);

                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);


                //  DEBIT SIDE
                // DEBIT SIDE - Transaction DR 
                var debit = dt.AsEnumerable()
                    .Where(r => Convert.ToDecimal(r["Curr_Amt"]) > 0) // DR > 0
                    .GroupBy(r => new {
                        GroupId = r["L_group_id"],
                        GroupName = r["L_group_name"]
                    })
                    .Select(g => new {
                        Group_id = g.Key.GroupId,
                        Group_name = g.Key.GroupName,
                        SubGroups = g.Select(x => new {
                            Sub_group_id = x["Ledger_id"],
                            Sub_group_name = x["Ledger_name"],
                            Prev_Amt = Math.Abs(Convert.ToDecimal(x["Prev_Amt"])),
                            Curr_Amt = Math.Abs(Convert.ToDecimal(x["Curr_Amt"]))
                        }).ToList()
                    }).ToList();

                // CREDIT SIDE - Transaction CR 
                var credit = dt.AsEnumerable()
                    .Where(r => Convert.ToDecimal(r["Curr_Amt"]) < 0) // CR < 0
                    .GroupBy(r => new {
                        GroupId = r["L_group_id"],
                        GroupName = r["L_group_name"]
                    })
                    .Select(g => new {
                        Group_id = g.Key.GroupId,
                        Group_name = g.Key.GroupName,
                        SubGroups = g.Select(x => new {
                            Sub_group_id = x["Ledger_id"],
                            Sub_group_name = x["Ledger_name"],
                            Prev_Amt = Math.Abs(Convert.ToDecimal(x["Prev_Amt"])),
                            Curr_Amt = Math.Abs(Convert.ToDecimal(x["Curr_Amt"]))
                        }).ToList()
                    }).ToList();

                //  TOTAL
                decimal debitPrev = debit.Sum(g => g.SubGroups.Sum(s => s.Prev_Amt));
                decimal debitCurr = debit.Sum(g => g.SubGroups.Sum(s => s.Curr_Amt));

                decimal creditPrev = credit.Sum(g => g.SubGroups.Sum(s => s.Prev_Amt));
                decimal creditCurr = credit.Sum(g => g.SubGroups.Sum(s => s.Curr_Amt));

                db.Disconnect();

                var result = new
                {
                    FromDate = FromDate.ToString("dd/MM/yyyy"),
                    ToDate = ToDate.ToString("dd/MM/yyyy"),
                    PrevDate = prevDate.ToString("dd/MM/yyyy"),

                    Debit = new
                    {
                        List = debit,
                        Total_Prev = debitPrev,
                        Total_Curr = debitCurr
                    },

                    Credit = new
                    {
                        List = credit,
                        Total_Prev = creditPrev,
                        Total_Curr = creditCurr
                    },

                    Difference = new
                    {
                        Prev = Math.Abs(debitPrev - creditPrev),
                        Curr = Math.Abs(debitCurr - creditCurr)
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
