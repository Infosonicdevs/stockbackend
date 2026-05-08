using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class PurchaseRegisterController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/purchase/register")]
        [HttpGet]
        public HttpResponseMessage GetPurchaseRegister(DateTime FromDate, DateTime ToDate, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                string query = @"
SELECT 
    CAST(p.Invoice_date AS DATE) AS Invoice_Date,
    COUNT(DISTINCT p.Invoice_id) AS Total_Pavati,
    ISNULL(SUM(p.Final_amt) - SUM(pd_gst.Total_CGST) - SUM(pd_gst.Total_SGST) - SUM(pd_gst.Total_IGST) - SUM(p.Roundoff), 0) AS Sale_Amount,
    ISNULL(SUM(pd_gst.Total_CGST), 0) AS Total_CGST,
    ISNULL(SUM(pd_gst.Total_SGST), 0) AS Total_SGST,
    ISNULL(SUM(pd_gst.Total_IGST), 0) AS Total_IGST,
    ISNULL(SUM(p.Roundoff), 0) AS Total_Roundoff,
    ISNULL(SUM(p.Final_amt), 0) AS Bill_Amount
FROM PURCHASE p
INNER JOIN (
    SELECT 
        Invoice_id,
        SUM(CGST_amt) AS Total_CGST,
        SUM(SGST_amt) AS Total_SGST,
        SUM(IGST_amt) AS Total_IGST
    FROM PURCHASE_DETAILS
    GROUP BY Invoice_id
) pd_gst ON pd_gst.Invoice_id = p.Invoice_id
WHERE p.Status = '1'
AND CAST(p.Invoice_date AS DATE) BETWEEN @FromDate AND @ToDate";
                if (Outlet_id != null)
                    query += " AND p.Outlet_id = @Outlet_id";

                query += @"
GROUP BY CAST(p.Invoice_date AS DATE)
ORDER BY CAST(p.Invoice_date AS DATE)";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);
                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                System.Data.DataTable dt = new System.Data.DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                decimal grandCGST = 0, grandSGST = 0, grandIGST = 0, grandSale = 0,
                        grandRoundoff = 0, grandBill = 0;
                int grandPavati = 0;
                var list = new List<object>();

                foreach (System.Data.DataRow row in dt.Rows)
                {
                    int pavati = Convert.ToInt32(row["Total_Pavati"]);
                    decimal sale = Convert.ToDecimal(row["Sale_Amount"]);
                    decimal cgst = Convert.ToDecimal(row["Total_CGST"]);
                    decimal sgst = Convert.ToDecimal(row["Total_SGST"]);
                    decimal igst = Convert.ToDecimal(row["Total_IGST"]);
                    decimal roundoff = Convert.ToDecimal(row["Total_Roundoff"]);
                    decimal bill = Convert.ToDecimal(row["Bill_Amount"]);

                    grandPavati += pavati;
                    grandSale += sale;
                    grandCGST += cgst;
                    grandSGST += sgst;
                    grandIGST += igst;
                    grandRoundoff += roundoff;
                    grandBill += bill;

                    list.Add(new
                    {
                        Invoice_Date = row["Invoice_Date"],
                        Total_Pavati = pavati,
                        Sale_Amount = sale,
                        Total_CGST = cgst,
                        Total_SGST = sgst,
                        Total_IGST = igst,
                        Total_Roundoff = roundoff,
                        Bill_Amount = bill
                    });
                }

                db.Disconnect();

                var result = new
                {
                    List = list,
                    Summary = new
                    {
                        Grand_Pavati = grandPavati,
                        Grand_CGST = grandCGST,
                        Grand_SGST = grandSGST,
                        Grand_IGST = grandIGST,
                        Grand_Roundoff = grandRoundoff,
                        Grand_Bill = grandBill
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