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
    public class Vendor_ReportController : ApiController
    {
        DbClass db = new DbClass();

        // HELPER METHOD -  methods same logic 
        private List<object> ProcessVendor(
            int vid, DataRow vrow, decimal openingBase,
            int? Outlet_id, DateTime? fromDate, DateTime? toDate,
            ref decimal totalOpening, ref decimal totalPurchase,
            ref decimal totalPaid, ref decimal totalBalance)
        {
            var list = new List<object>();

            // From_date  Opening Balance
            if (fromDate.HasValue)
            {
                string prevPurQuery = @"
SELECT ISNULL(SUM(p.Final_amt), 0)
FROM PURCHASE p
WHERE p.Vend_id = @Vend_id
AND p.Status = '1'
AND CAST(p.Invoice_date AS DATE) < @From_date";
                if (Outlet_id != null) prevPurQuery += " AND p.Outlet_id = @Outlet_id";

                SqlCommand prevPurCmd = new SqlCommand(prevPurQuery, db.cn);
                prevPurCmd.Parameters.AddWithValue("@Vend_id", vid);
                prevPurCmd.Parameters.AddWithValue("@From_date", fromDate.Value.Date);
                if (Outlet_id != null) prevPurCmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);
                decimal prevPurchase = Convert.ToDecimal(prevPurCmd.ExecuteScalar());

                string prevPaidQuery = @"
SELECT ISNULL(SUM(td.Amount), 0)
FROM TRANS_DETAILS td
INNER JOIN TRANS t ON td.Trans_id = t.Trans_id
WHERE td.CrDr_id = 2
AND td.Status = '1'
AND t.Trans_type_id = 7
AND t.Status = 1
AND td.Cust_id = @Vend_id
AND CAST(t.Trans_date AS DATE) < @From_date";

                SqlCommand prevPaidCmd = new SqlCommand(prevPaidQuery, db.cn);
                prevPaidCmd.Parameters.AddWithValue("@Vend_id", vid);
                prevPaidCmd.Parameters.AddWithValue("@From_date", fromDate.Value.Date);
                decimal prevPaid = Convert.ToDecimal(prevPaidCmd.ExecuteScalar());

                openingBase = openingBase + prevPurchase - prevPaid;
            }

            // Invoice Query
            string invoiceQuery = @"
SELECT 
    p.Invoice_id,
    p.Invoice_no,
    CAST(p.Invoice_date AS DATE) AS Invoice_date,
    p.Final_amt AS Purchase
FROM PURCHASE p
WHERE p.Vend_id = @Vend_id
AND p.Status = '1'";

            if (Outlet_id != null) invoiceQuery += " AND p.Outlet_id = @Outlet_id";
            if (fromDate.HasValue) invoiceQuery += " AND CAST(p.Invoice_date AS DATE) >= @From_date";
            if (toDate.HasValue) invoiceQuery += " AND CAST(p.Invoice_date AS DATE) <= @To_date";
            invoiceQuery += " ORDER BY p.Invoice_date, p.Invoice_id";

            SqlCommand invoiceCmd = new SqlCommand(invoiceQuery, db.cn);
            invoiceCmd.Parameters.AddWithValue("@Vend_id", vid);
            if (Outlet_id != null) invoiceCmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);
            if (fromDate.HasValue) invoiceCmd.Parameters.AddWithValue("@From_date", fromDate.Value.Date);
            if (toDate.HasValue) invoiceCmd.Parameters.AddWithValue("@To_date", toDate.Value.Date);

            DataTable dtInvoices = new DataTable();
            new SqlDataAdapter(invoiceCmd).Fill(dtInvoices);

            // Payment Query - date wise individual rows
            string paymentQuery = @"
SELECT 
    CAST(t.Trans_date AS DATE) AS Pay_date,
    td.Amount
FROM TRANS_DETAILS td
INNER JOIN TRANS t ON td.Trans_id = t.Trans_id
WHERE td.CrDr_id = 2
AND td.Status = '1'
AND t.Trans_type_id = 7
AND t.Status = 1
AND td.Cust_id = @Vend_id";

            if (fromDate.HasValue) paymentQuery += " AND CAST(t.Trans_date AS DATE) >= @From_date";
            if (toDate.HasValue) paymentQuery += " AND CAST(t.Trans_date AS DATE) <= @To_date";
            paymentQuery += " ORDER BY t.Trans_date";

            SqlCommand payCmd = new SqlCommand(paymentQuery, db.cn);
            payCmd.Parameters.AddWithValue("@Vend_id", vid);
            if (fromDate.HasValue) payCmd.Parameters.AddWithValue("@From_date", fromDate.Value.Date);
            if (toDate.HasValue) payCmd.Parameters.AddWithValue("@To_date", toDate.Value.Date);

            DataTable dtPayments = new DataTable();
            new SqlDataAdapter(payCmd).Fill(dtPayments);

            // Invoice + Payment  merge 
            var allRows = new List<(DateTime Date, string Type, int Id, string InvoiceNo, decimal Amount)>();

            foreach (DataRow irow in dtInvoices.Rows)
            {
                allRows.Add((
                    Convert.ToDateTime(irow["Invoice_date"]),
                    "PURCHASE",
                    Convert.ToInt32(irow["Invoice_id"]),
                    irow["Invoice_no"].ToString(),
                    Convert.ToDecimal(irow["Purchase"])
                ));
            }

            foreach (DataRow prow in dtPayments.Rows)
            {
                allRows.Add((
                    Convert.ToDateTime(prow["Pay_date"]),
                    "PAYMENT",
                    0,
                    "Payment",
                    Convert.ToDecimal(prow["Amount"])
                ));
            }

            // Date wise sort - Purchase , Payment  same date 
            allRows = allRows
                .OrderBy(x => x.Date)
                .ThenBy(x => x.Type == "PURCHASE" ? 0 : 1)
                .ToList();

            decimal runningOpening = openingBase;
            bool openingAdded = false;

            if (allRows.Count == 0)
            {
                // NO INVOICE + NO PAYMENT CASE
                list.Add(new
                {
                    Vend_id = vid,
                    Vend_name = vrow["Vend_name"],
                    Contact_no = vrow["Contact_no"],
                    Date = "",
                    Invoice_id = 0,
                    Invoice_no = "",
                    Row_Type = "NONE",
                    Opening_Bal = Math.Abs(openingBase),
                    Total_Purchase = 0m,
                    Total_Paid = 0m,
                    Closing_Bal = Math.Abs(openingBase),
                    Closing_Type = openingBase >= 0 ? "Payable" : "Receivable"
                });

                totalOpening += Math.Abs(openingBase);
                totalBalance = openingBase;
                return list;
            }

            foreach (var row in allRows)
            {
                if (!openingAdded)
                {
                    totalOpening += Math.Abs(runningOpening);
                    openingAdded = true;
                }

                if (row.Type == "PURCHASE")
                {
                    decimal closing = runningOpening + row.Amount;
                    string closingType = closing > 0 ? "Payable" : closing < 0 ? "Receivable" : "Settled";

                    list.Add(new
                    {
                        Vend_id = vid,
                        Vend_name = vrow["Vend_name"],
                        Contact_no = vrow["Contact_no"],
                        Date = row.Date.ToString("yyyy-MM-dd"),
                        Invoice_id = row.Id,
                        Invoice_no = row.InvoiceNo,
                        Row_Type = "PURCHASE",
                        Opening_Bal = Math.Abs(runningOpening),
                        Total_Purchase = row.Amount,
                        Total_Paid = 0m,
                        Closing_Bal = Math.Abs(closing),
                        Closing_Type = closingType
                    });

                    totalPurchase += row.Amount;
                    runningOpening = closing;
                    totalBalance = closing;
                }
                else // PAYMENT
                {
                    decimal closing = runningOpening - row.Amount;
                    string closingType = closing > 0 ? "Payable" : closing < 0 ? "Receivable" : "Settled";

                    list.Add(new
                    {
                        Vend_id = vid,
                        Vend_name = vrow["Vend_name"],
                        Contact_no = vrow["Contact_no"],
                        Date = row.Date.ToString("yyyy-MM-dd"),
                        Invoice_id = 0,
                        Invoice_no = "Payment",
                        Row_Type = "PAYMENT",
                        Opening_Bal = Math.Abs(runningOpening),
                        Total_Purchase = 0m,
                        Total_Paid = row.Amount,
                        Closing_Bal = Math.Abs(closing),
                        Closing_Type = closingType
                    });

                    totalPaid += row.Amount;
                    runningOpening = closing;
                    totalBalance = closing;
                }
            }

            return list;
        }

        [Route("api/allvendor/report")]
        [HttpGet]
        public HttpResponseMessage GetAllVendorReport(int? Outlet_id = null, string From_date = null, string To_date = null)
        {
            try
            {
                db.Connect();

                DateTime? fromDate = null, toDate = null;
                if (!string.IsNullOrEmpty(From_date)) fromDate = DateTime.Parse(From_date);
                if (!string.IsNullOrEmpty(To_date)) toDate = DateTime.Parse(To_date);

                string vendorQuery = @"
SELECT 
    v.Vend_id,
    v.Vend_name,
    v.Contact_no,
    ISNULL(v.Opn_bal, 0) + ISNULL(vb.Amount, 0) AS Opening_Base
FROM VENDOR_INFO v
LEFT JOIN VENDOR_BAL vb 
    ON v.Vend_id = vb.Vend_id
ORDER BY v.Vend_name";

                SqlCommand vendorCmd = new SqlCommand(vendorQuery, db.cn);
                DataTable dtVendors = new DataTable();
                new SqlDataAdapter(vendorCmd).Fill(dtVendors);

                decimal totalOpening = 0, totalPurchase = 0, totalPaid = 0, totalBalance = 0;
                var list = new List<object>();

                foreach (DataRow vrow in dtVendors.Rows)
                {
                    int vid = Convert.ToInt32(vrow["Vend_id"]);
                    decimal openingBase = Convert.ToDecimal(vrow["Opening_Base"]);

                    var vendorRows = ProcessVendor(
                        vid, vrow, openingBase, Outlet_id, fromDate, toDate,
                        ref totalOpening, ref totalPurchase, ref totalPaid, ref totalBalance);

                    list.AddRange(vendorRows);
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

        [Route("api/vendor/report")]
        [HttpGet]
        public HttpResponseMessage GetVendorReport(int? Outlet_id = null, int? Vend_id = null, string From_date = null, string To_date = null)
        {
            try
            {
                db.Connect();

                DateTime? fromDate = null, toDate = null;
                if (!string.IsNullOrEmpty(From_date)) fromDate = DateTime.Parse(From_date);
                if (!string.IsNullOrEmpty(To_date)) toDate = DateTime.Parse(To_date);

                string vendorQuery = @"
SELECT 
    v.Vend_id,
    v.Vend_name,
    v.Contact_no,
    ISNULL(v.Opn_bal, 0) + ISNULL(SUM(vb.Amount), 0) AS Opening_Base
FROM VENDOR_INFO v
LEFT JOIN VENDOR_BAL vb 
    ON v.Vend_id = vb.Vend_id
WHERE 1=1";

                if (Vend_id != null) vendorQuery += " AND v.Vend_id = @Vend_id";

                vendorQuery += @"
GROUP BY 
    v.Vend_id, v.Vend_name, v.Contact_no, v.Opn_bal
ORDER BY 
    v.Vend_name";

                SqlCommand vendorCmd = new SqlCommand(vendorQuery, db.cn);
                if (Vend_id != null) vendorCmd.Parameters.AddWithValue("@Vend_id", Vend_id);

                DataTable dtVendors = new DataTable();
                new SqlDataAdapter(vendorCmd).Fill(dtVendors);

                decimal totalOpening = 0, totalPurchase = 0, totalPaid = 0, totalBalance = 0;
                var list = new List<object>();

                foreach (DataRow vrow in dtVendors.Rows)
                {
                    int vid = Convert.ToInt32(vrow["Vend_id"]);
                    decimal openingBase = Convert.ToDecimal(vrow["Opening_Base"]);

                    var vendorRows = ProcessVendor(
                        vid, vrow, openingBase, Outlet_id, fromDate, toDate,
                        ref totalOpening, ref totalPurchase, ref totalPaid, ref totalBalance);

                    list.AddRange(vendorRows);
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