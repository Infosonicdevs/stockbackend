using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class Stock_BookController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/stockbook")]
        [HttpGet]
        public HttpResponseMessage GetStockBook(int Stock_id, DateTime FromDate, DateTime ToDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                // OPENING STOCK
                string openingQuery = @"
SELECT 
    ISNULL((SELECT SUM(pd.Total) FROM PURCHASE_DETAILS pd 
             INNER JOIN PURCHASE p ON p.Invoice_id = pd.Invoice_id
             WHERE pd.Stock_id = @Stock_id 
             AND CAST(p.Invoice_date AS DATE) < @FromDate), 0)
    -
    ISNULL((SELECT SUM(sd.Amount) FROM SALE_DETAILS sd 
             INNER JOIN SALE s ON s.Sale_id = sd.Sale_Rtn_id
             WHERE sd.Stock_id = @Stock_id 
             AND CAST(s.Sale_date AS DATE) < @FromDate
             AND s.Status = 1), 0)
    AS Opening_Amt";

                SqlCommand cmd1 = new SqlCommand(openingQuery, db.cn);
                cmd1.Parameters.AddWithValue("@Stock_id", Stock_id);
                cmd1.Parameters.AddWithValue("@FromDate", FromDate.Date);
                decimal Opening_Amt = Math.Abs(Convert.ToDecimal(cmd1.ExecuteScalar()));

                // PURCHASE LIST
                string purchaseQuery = @"
SELECT 
    p.Invoice_id,
    p.Invoice_date AS Trans_date,
    'Purchase' AS Type,
    pd.Quantity,
    pd.Price AS Rate,
    pd.Total AS Amount
FROM PURCHASE p
INNER JOIN PURCHASE_DETAILS pd ON p.Invoice_id = pd.Invoice_id
WHERE pd.Stock_id = @Stock_id
AND CAST(p.Invoice_date AS DATE) BETWEEN @FromDate AND @ToDate";

                if (Outlet_id != null)
                    purchaseQuery += " AND p.Outlet_id = @Outlet_id";

                purchaseQuery += " ORDER BY p.Invoice_date";

                SqlCommand cmd2 = new SqlCommand(purchaseQuery, db.cn);
                cmd2.Parameters.AddWithValue("@Stock_id", Stock_id);
                cmd2.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd2.Parameters.AddWithValue("@ToDate", ToDate.Date);
                if (Outlet_id != null)
                    cmd2.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dtPurchase = new DataTable();
                new SqlDataAdapter(cmd2).Fill(dtPurchase);

                // SALE LIST
                string saleQuery = @"
SELECT 
    s.Sale_id,
    s.Sale_date AS Trans_date,
    'Sale' AS Type,
    sd.Quantity,
    sd.Rate,
    sd.Amount
FROM SALE s
INNER JOIN SALE_DETAILS sd ON s.Sale_id = sd.Sale_Rtn_id
WHERE sd.Stock_id = @Stock_id
AND CAST(s.Sale_date AS DATE) BETWEEN @FromDate AND @ToDate
AND s.Status = 1";

                if (Outlet_id != null)
                    saleQuery += " AND s.Outlet_id = @Outlet_id";

                saleQuery += " ORDER BY s.Sale_date";

                SqlCommand cmd3 = new SqlCommand(saleQuery, db.cn);
                cmd3.Parameters.AddWithValue("@Stock_id", Stock_id);
                cmd3.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd3.Parameters.AddWithValue("@ToDate", ToDate.Date);
                if (Outlet_id != null)
                    cmd3.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                DataTable dtSale = new DataTable();
                new SqlDataAdapter(cmd3).Fill(dtSale);

                // TOTALS
                decimal Total_Purchase_Qty = 0, Total_Purchase_Amt = 0;
                foreach (DataRow row in dtPurchase.Rows)
                {
                    Total_Purchase_Qty += Convert.ToDecimal(row["Quantity"]);
                    Total_Purchase_Amt += Convert.ToDecimal(row["Amount"]);
                }

                decimal Total_Sale_Qty = 0, Total_Sale_Amt = 0;
                foreach (DataRow row in dtSale.Rows)
                {
                    Total_Sale_Qty += Convert.ToDecimal(row["Quantity"]);
                    Total_Sale_Amt += Convert.ToDecimal(row["Amount"]);
                }

                decimal Closing_Amt = Math.Abs( Opening_Amt + Total_Sale_Amt - Total_Purchase_Amt);

                db.Disconnect();

                var result = new
                {
                    Stock_id = Stock_id,
                    FromDate = FromDate.Date,
                    ToDate = ToDate.Date,
                    Opening_Amt = Opening_Amt,
                    Purchase = dtPurchase,
                    Sale = dtSale,
                    Summary = new
                    {
                        Total_Purchase_Qty = Total_Purchase_Qty,
                        Total_Purchase_Amt = Total_Purchase_Amt,
                        Total_Sale_Qty = Total_Sale_Qty,
                        Total_Sale_Amt = Total_Sale_Amt,
                        Closing_Amt = Closing_Amt 
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
