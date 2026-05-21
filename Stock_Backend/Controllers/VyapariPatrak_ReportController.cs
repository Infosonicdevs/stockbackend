using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class Trading_AccountController : ApiController
    {
        DbClass db = new DbClass();

        class LedgerItem
        {
            public int Ledger_id { get; set; }
            public string Ledger_name { get; set; }
            public decimal Curr { get; set; }
        }

        class SubgroupItem
        {
            public int Subgroup_id { get; set; }
            public string Subgroup_name { get; set; }
            public decimal Curr { get; set; }
            public List<LedgerItem> Ledgers { get; set; } = new List<LedgerItem>();
        }

        class GroupItem
        {
            public int Group_id { get; set; }
            public string Group_name { get; set; }
            public decimal Curr { get; set; }
            public List<SubgroupItem> Subgroups { get; set; } = new List<SubgroupItem>();
        }

        [Route("api/vyapari-patrak")]
        [HttpGet]
        public HttpResponseMessage GetTrading(DateTime FromDate, DateTime ToDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                // INCOME - Selected date range
                string incomeQuery = @"
SELECT
    lg.L_group_id, lg.L_group_name, lg.Seqno AS Group_Seqno,
    ls.Ledger_subgroup_id, ls.Ledger_subgroup_name, ls.Seqno AS Subgroup_Seqno,
    l.Ledger_id, l.Ledger_name,
    ISNULL(SUM(CASE 
        WHEN CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate 
        AND td.CrDr_id = 1 
        THEN td.Amount ELSE 0 END), 0) AS Curr_Amt
FROM LEDGER_GROUP lg
INNER JOIN LEDGER_SUBGROUP ls ON ls.Ledger_group_id = lg.L_group_id
INNER JOIN LEDGER l ON l.Ledger_subgroup_id = ls.Ledger_subgroup_id
LEFT JOIN TRANS_DETAILS td ON td.L_id = l.Ledger_id
LEFT JOIN TRANS t ON t.Trans_id = td.Trans_id AND t.Status = 1
WHERE lg.crdr_id = 1 AND lg.Patrak_id = 3
GROUP BY lg.L_group_id, lg.L_group_name, lg.Seqno,
         ls.Ledger_subgroup_id, ls.Ledger_subgroup_name, ls.Seqno,
         l.Ledger_id, l.Ledger_name
ORDER BY lg.Seqno, ls.Seqno, l.Ledger_name";

                // EXPENSE - Selected date range
                string expenseQuery = @"
SELECT
    lg.L_group_id, lg.L_group_name, lg.Seqno AS Group_Seqno,
    ls.Ledger_subgroup_id, ls.Ledger_subgroup_name, ls.Seqno AS Subgroup_Seqno,
    l.Ledger_id, l.Ledger_name,
    ISNULL(SUM(CASE 
        WHEN CAST(t.Trans_date AS DATE) BETWEEN @FromDate AND @ToDate 
        AND td.CrDr_id = 2 
        THEN td.Amount ELSE 0 END), 0) AS Curr_Amt
FROM LEDGER_GROUP lg
INNER JOIN LEDGER_SUBGROUP ls ON ls.Ledger_group_id = lg.L_group_id
INNER JOIN LEDGER l ON l.Ledger_subgroup_id = ls.Ledger_subgroup_id
LEFT JOIN TRANS_DETAILS td ON td.L_id = l.Ledger_id
LEFT JOIN TRANS t ON t.Trans_id = td.Trans_id AND t.Status = 1
WHERE lg.Patrak_id = 3
GROUP BY lg.L_group_id, lg.L_group_name, lg.Seqno,
         ls.Ledger_subgroup_id, ls.Ledger_subgroup_name, ls.Seqno,
         l.Ledger_id, l.Ledger_name
ORDER BY lg.Seqno, ls.Seqno, l.Ledger_name";

                // OPENING STOCK - FromDate before Expense total
                // AKHER SHILLAK MAAL = < FromDate  (Purchase - Sales)
                string closingStockQuery = @"
SELECT 
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END), 0) -
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END), 0) AS Closing_Amt
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON td.Trans_id = t.Trans_id
INNER JOIN LEDGER l ON l.Ledger_id = td.L_id
INNER JOIN LEDGER_SUBGROUP ls ON ls.Ledger_subgroup_id = l.Ledger_subgroup_id
INNER JOIN LEDGER_GROUP lg ON lg.L_group_id = ls.Ledger_group_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) < @FromDate
AND lg.Patrak_id = 3";

                // AARAMBHI SHILLAK MAAL = <= ToDate 
                string openingStockQuery = @"
SELECT 
    ISNULL(SUM(CASE WHEN td.CrDr_id = 2 THEN td.Amount ELSE 0 END), 0) -
    ISNULL(SUM(CASE WHEN td.CrDr_id = 1 THEN td.Amount ELSE 0 END), 0) AS Opening_Amt
FROM TRANS t
INNER JOIN TRANS_DETAILS td ON td.Trans_id = t.Trans_id
INNER JOIN LEDGER l ON l.Ledger_id = td.L_id
INNER JOIN LEDGER_SUBGROUP ls ON ls.Ledger_subgroup_id = l.Ledger_subgroup_id
INNER JOIN LEDGER_GROUP lg ON lg.L_group_id = ls.Ledger_group_id
WHERE t.Status = 1
AND CAST(t.Trans_date AS DATE) <= @ToDate
AND lg.Patrak_id = 3";

                DataTable FillData(string query)
                {
                    SqlCommand cmd = new SqlCommand(query, db.cn);
                    cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);
                    DataTable dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    return dt;
                }

                decimal FillScalar(string query)
                {
                    SqlCommand cmd = new SqlCommand(query, db.cn);
                    cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                    cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);
                    object result = cmd.ExecuteScalar();
                    return result == null || result == DBNull.Value ? 0 : Math.Abs(Convert.ToDecimal(result));
                }

                List<GroupItem> BuildNestedList(DataTable dt)
                {
                    var groupDict = new Dictionary<int, GroupItem>();
                    var groupList = new List<GroupItem>();

                    foreach (DataRow row in dt.Rows)
                    {
                        decimal curr = Math.Abs(Convert.ToDecimal(row["Curr_Amt"]));

                        int groupId = Convert.ToInt32(row["L_group_id"]);
                        int subgroupId = Convert.ToInt32(row["Ledger_subgroup_id"]);

                        if (!groupDict.ContainsKey(groupId))
                        {
                            var grp = new GroupItem
                            {
                                Group_id = groupId,
                                Group_name = row["L_group_name"].ToString()
                            };
                            groupDict[groupId] = grp;
                            groupList.Add(grp);
                        }

                        GroupItem currentGroup = groupDict[groupId];

                        SubgroupItem currentSub = currentGroup.Subgroups.Find(s => s.Subgroup_id == subgroupId);
                        if (currentSub == null)
                        {
                            currentSub = new SubgroupItem
                            {
                                Subgroup_id = subgroupId,
                                Subgroup_name = row["Ledger_subgroup_name"].ToString()
                            };
                            currentGroup.Subgroups.Add(currentSub);
                        }

                        if (curr != 0)
                        {
                            currentSub.Ledgers.Add(new LedgerItem
                            {
                                Ledger_id = Convert.ToInt32(row["Ledger_id"]),
                                Ledger_name = row["Ledger_name"].ToString(),
                                Curr = curr
                            });

                            currentSub.Curr += curr;
                            currentGroup.Curr += curr;
                        }
                    }

                    groupList.RemoveAll(g => g.Curr == 0);
                    foreach (var g in groupList)
                        g.Subgroups.RemoveAll(s => s.Curr == 0);

                    return groupList;
                }

                DataTable dtIncome = FillData(incomeQuery);
                DataTable dtExpense = FillData(expenseQuery);
                decimal aarambhiShillak = FillScalar(openingStockQuery);   // Opening - before Expense
                decimal akherShillak = FillScalar(closingStockQuery);       // Closing - before Income

                var incomeList = BuildNestedList(dtIncome);
                var expenseList = BuildNestedList(dtExpense);

                // Totals
                decimal totalIncomeCurr = 0;
                decimal totalExpenseCurr = 0;

                foreach (DataRow row in dtIncome.Rows)
                    totalIncomeCurr += Math.Abs(Convert.ToDecimal(row["Curr_Amt"]));

                foreach (DataRow row in dtExpense.Rows)
                    totalExpenseCurr += Math.Abs(Convert.ToDecimal(row["Curr_Amt"]));

                // Profit / Loss
                decimal totalJama = totalIncomeCurr + akherShillak;         // Income + Closing
                decimal totalKharch = totalExpenseCurr + aarambhiShillak;   // Expense + Opening

                decimal profitCurr = totalJama - totalKharch;

                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Income = new
                    {
                        List = incomeList,
                        Total_Curr = totalIncomeCurr
                    },
                    Expense = new
                    {
                        List = expenseList,
                        Total_Curr = totalExpenseCurr
                    },
                    Gross_Profit = new
                    {
                        Curr = profitCurr > 0 ? profitCurr : 0
                    },
                    Gross_Loss = new
                    {
                        Curr = profitCurr < 0 ? Math.Abs(profitCurr) : 0
                    },
                    Summary = new
                    {
                        Total_Jama = totalIncomeCurr,
                        Akher_Shillak_Maal = akherShillak,
                        Total_Kharch = totalExpenseCurr,
                        Aarambhi_Shillak_Maal = aarambhiShillak,
                        Profit = profitCurr > 0 ? profitCurr : 0,
                        Loss = profitCurr < 0 ? Math.Abs(profitCurr) : 0
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