using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class ProfitLossController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/profitloss")]
        [HttpGet]
        public HttpResponseMessage GetProfitLoss(DateTime FromDate, DateTime ToDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                string outletFilter = Outlet_id != null ? " AND t.Outlet_id = @Outlet_id" : "";

                string query = @"
SELECT 
    lg.L_group_id,
    lg.L_group_name,
    l.Ledger_id,
    l.Ledger_name,

    SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END) AS CreditAmt,
    SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END) AS DebitAmt

FROM LEDGER_GROUP lg
INNER JOIN LEDGER l ON l.Ledger_group_id = lg.L_group_id
LEFT JOIN TRANS_DETAILS td ON td.L_id = l.Ledger_id
LEFT JOIN TRANS t ON t.Trans_id = td.Trans_id

WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate
" + outletFilter + @"

GROUP BY 
    lg.L_group_id,
    lg.L_group_name,
    l.Ledger_id,
    l.Ledger_name
";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);

                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                // GROUP → SUBGROUP STRUCTURE
                var list = dt.AsEnumerable()
      .GroupBy(x => new
      {
          GroupId = x["L_group_id"],
          GroupName = x["L_group_name"]
      })
      .Select(g => new
      {
          Group_id = g.Key.GroupId,
          Group_name = g.Key.GroupName,

          SubGroups = g.Select(s => new
          {
              Ledger_id = s["Ledger_id"],
              Ledger_name = s["Ledger_name"],

              Credit = Convert.ToDecimal(s["CreditAmt"]),
              Debit = Convert.ToDecimal(s["DebitAmt"])
          }).ToList()
      })
      .Where(g => g.SubGroups.Any(x => x.Credit > 0 || x.Debit > 0))
      .ToList();

                // TOTAL CALCULATION
                decimal totalCr = dt.AsEnumerable().Sum(x => Convert.ToDecimal(x["CreditAmt"]));
                decimal totalDr = dt.AsEnumerable().Sum(x => Convert.ToDecimal(x["DebitAmt"]));


                // CURRENT YEAR PROFIT / LOSS
                decimal diff = totalCr - totalDr;

                decimal currentProfit = 0;
                decimal currentLoss = 0;

                if (diff >= 0)
                    currentProfit = diff;
                else
                    currentLoss = Math.Abs(diff);

                // FINAL TOTAL (ADD PROFIT/LOSS)
                decimal finalCr = totalCr + currentProfit;
                decimal finalDr = totalDr + currentLoss;

                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    FromDate = FromDate.ToString("dd/MM/yyyy"),
                    ToDate = ToDate.ToString("dd/MM/yyyy"),
                    Outlet_id = Outlet_id,

                    List = list,

                    Total_Credit = totalCr,
                    Total_Debit = totalDr,

                    Current_Year_Profit = currentProfit,
                    Current_Year_Loss = currentLoss,

                    Final_Total_Credit = finalCr,
                    Final_Total_Debit = finalDr
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