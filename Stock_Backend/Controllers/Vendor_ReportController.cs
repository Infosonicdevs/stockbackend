using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class Vendor_ReportController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/vendor/report")]
        [HttpGet]
        public HttpResponseMessage GetVendorReport(DateTime FromDate, DateTime ToDate, int? Outlet_id = null, int? Vend_id = null)
        {
            try
            {
                db.Connect();

                //  VENDOR INFO
                string vendorQuery = @"
SELECT 
    v.Vend_id,
    v.Vend_name,
    v.Contact_no,
    ISNULL(v.Opn_bal, 0) AS Opening_Base
FROM VENDOR_INFO v
WHERE 1=1";

                if (Vend_id != null)
                    vendorQuery += " AND v.Vend_id = @Vend_id";

                vendorQuery += " ORDER BY v.Vend_name";

                SqlCommand vendorCmd = new SqlCommand(vendorQuery, db.cn);
                if (Vend_id != null)
                    vendorCmd.Parameters.AddWithValue("@Vend_id", Vend_id);

                DataTable dtVendors = new DataTable();
                new SqlDataAdapter(vendorCmd).Fill(dtVendors);

                decimal totalOpening = 0, totalPurchase = 0, totalPaid = 0, totalBalance = 0;
                var list = new List<object>();

                foreach (DataRow vrow in dtVendors.Rows)
                {
                    int vid = Convert.ToInt32(vrow["Vend_id"]);
                    decimal openingBase = Convert.ToDecimal(vrow["Opening_Base"]);

                    // All invoices in range — Invoice_id wise ORDER
                    string invoiceQuery = @"
SELECT 
    p.Invoice_id,
    CAST(p.Invoice_date AS DATE) AS Invoice_date,
    p.Final_amt AS Purchase
FROM PURCHASE p
WHERE p.Vend_id = @Vend_id
AND p.Status = '1'
AND CAST(p.Invoice_date AS DATE) BETWEEN @FromDate AND @ToDate";

                    if (Outlet_id != null)
                        invoiceQuery += " AND p.Outlet_id = @Outlet_id";

                    invoiceQuery += " ORDER BY p.Invoice_id";

                    SqlCommand invoiceCmd = new SqlCommand(invoiceQuery, db.cn);
                    invoiceCmd.Parameters.AddWithValue("@Vend_id", vid);
                    invoiceCmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                    invoiceCmd.Parameters.AddWithValue("@ToDate", ToDate.Date);
                    if (Outlet_id != null)
                        invoiceCmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);

                    DataTable dtInvoices = new DataTable();
                    new SqlDataAdapter(invoiceCmd).Fill(dtInvoices);

                    //  Opening = openingBase + all purchases before first invoice in range
                    // i.e. all purchases with Invoice_id < first Invoice_id in range
                    decimal runningOpening = openingBase;

                    if (dtInvoices.Rows.Count > 0)
                    {
                        int firstInvoiceId = Convert.ToInt32(dtInvoices.Rows[0]["Invoice_id"]);

                        // Previous Purchase (Invoice_id < first invoice in range)
                        string prevPurchaseQuery = @"
SELECT ISNULL(SUM(p.Final_amt), 0)
FROM PURCHASE p
WHERE p.Vend_id = @Vend_id
AND p.Status = '1'
AND (
    CAST(p.Invoice_date AS DATE) < @FromDate
    OR (CAST(p.Invoice_date AS DATE) = @FromDate AND p.Invoice_id < @FirstInvoiceId)
)";

                        SqlCommand prevPurCmd = new SqlCommand(prevPurchaseQuery, db.cn);
                        prevPurCmd.Parameters.AddWithValue("@Vend_id", vid);
                        prevPurCmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                        prevPurCmd.Parameters.AddWithValue("@FirstInvoiceId", firstInvoiceId);
                        decimal prevPurchase = Convert.ToDecimal(prevPurCmd.ExecuteScalar());

                        // Previous Paid (Invoice_id < first invoice in range)
                        string prevPaidQuery = @"
SELECT ISNULL(SUM(td.Amount), 0)
FROM TRANS_DETAILS td
WHERE td.CrDr_id = 1
AND td.Status = '1'
AND td.Master_id IN (
    SELECT Invoice_id FROM PURCHASE 
    WHERE Vend_id = @Vend_id
    AND Status = '1'
    AND (
        CAST(Invoice_date AS DATE) < @FromDate
        OR (CAST(Invoice_date AS DATE) = @FromDate AND Invoice_id < @FirstInvoiceId)
    )
)";

                        SqlCommand prevPaidCmd = new SqlCommand(prevPaidQuery, db.cn);
                        prevPaidCmd.Parameters.AddWithValue("@Vend_id", vid);
                        prevPaidCmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                        prevPaidCmd.Parameters.AddWithValue("@FirstInvoiceId", firstInvoiceId);
                        decimal prevPaid = Convert.ToDecimal(prevPaidCmd.ExecuteScalar());

                        runningOpening = openingBase + prevPurchase - prevPaid;
                        if (runningOpening < 0) runningOpening = Math.Abs(runningOpening);
                    }

                    bool openingAdded = false;

                    // Invoice-wise running balance
                    foreach (DataRow irow in dtInvoices.Rows)
                    {
                        int invoiceId = Convert.ToInt32(irow["Invoice_id"]);
                        decimal purchase = Convert.ToDecimal(irow["Purchase"]);

                        // Paid for this invoice
                        string paidQuery = @"
SELECT ISNULL(SUM(td.Amount), 0)
FROM TRANS_DETAILS td
WHERE td.CrDr_id = 1
AND td.Status = '1'
AND td.Master_id = @Invoice_id";

                        SqlCommand paidCmd = new SqlCommand(paidQuery, db.cn);
                        paidCmd.Parameters.AddWithValue("@Invoice_id", invoiceId);
                        decimal paid = Convert.ToDecimal(paidCmd.ExecuteScalar());
                        decimal closing = runningOpening + purchase - paid;

                        // display value
                        decimal displayClosing = Math.Abs(closing);

                        list.Add(new
                        {
                            Vend_id = vid,
                            Vend_name = vrow["Vend_name"],
                            Contact_no = vrow["Contact_no"],
                            Date = Convert.ToDateTime(irow["Invoice_date"]).ToString("yyyy-MM-dd"),
                            Invoice_id = invoiceId,
                            Opening_Bal = Math.Abs(runningOpening),
                            Total_Purchase = purchase,
                            Total_Paid = paid,
                            Closing_Bal = displayClosing
                        });

                        // totals
                        if (!openingAdded)
                        {
                            totalOpening += Math.Abs(runningOpening); // Opn_bal + prev calc
                            openingAdded = true;
                        }
                        totalPurchase += purchase;
                        totalPaid += paid;
                        totalBalance = closing;

                        //  next opening
                        runningOpening = closing;
                    }

                    // No invoices in range
                    if (dtInvoices.Rows.Count == 0)
                    {
                        // Calculate opening for display
                        string prevPurchaseQuery = @"
SELECT ISNULL(SUM(p.Final_amt), 0)
FROM PURCHASE p
WHERE p.Vend_id = @Vend_id
AND p.Status = '1'
AND CAST(p.Invoice_date AS DATE) < @FromDate";

                        SqlCommand prevPurCmd = new SqlCommand(prevPurchaseQuery, db.cn);
                        prevPurCmd.Parameters.AddWithValue("@Vend_id", vid);
                        prevPurCmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                        decimal prevPurchase = Convert.ToDecimal(prevPurCmd.ExecuteScalar());

                        string prevPaidQuery = @"
SELECT ISNULL(SUM(td.Amount), 0)
FROM TRANS_DETAILS td
WHERE td.CrDr_id = 1
AND td.Status = '1'
AND td.Master_id IN (
    SELECT Invoice_id FROM PURCHASE 
    WHERE Vend_id = @Vend_id
    AND Status = '1'
    AND CAST(Invoice_date AS DATE) < @FromDate
)";

                        SqlCommand prevPaidCmd = new SqlCommand(prevPaidQuery, db.cn);
                        prevPaidCmd.Parameters.AddWithValue("@Vend_id", vid);
                        prevPaidCmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                        decimal prevPaid = Convert.ToDecimal(prevPaidCmd.ExecuteScalar());

                        decimal opening = openingBase + prevPurchase - prevPaid;
                        if (opening < 0) opening = Math.Abs(opening);

                        list.Add(new
                        {
                            Vend_id = vid,
                            Vend_name = vrow["Vend_name"],
                            Contact_no = vrow["Contact_no"],
                            Date = "",
                            Invoice_id = 0,
                            Opening_Bal = opening,
                            Total_Purchase = 0m,
                            Total_Paid = 0m,
                            Closing_Bal = opening
                        });

                        totalOpening += opening;
                        totalBalance = opening;
                    }
                }

                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    List = list,
                    Summary = new
                    {
                        Total_Opening = totalOpening,
                        Total_Purchase = totalPurchase,
                        Total_Paid = totalPaid,
                        Total_Closing = Math.Abs(totalBalance)
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