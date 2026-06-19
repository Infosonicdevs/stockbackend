using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class SaleRegisterController : ApiController
    {
        DbClass db = new DbClass();

        // API 1 - Main 
        [Route("api/sale/register")]
        [HttpGet]
        public HttpResponseMessage GetSaleRegister(DateTime FromDate, DateTime ToDate, int? Outlet_id = null, int? Counter_id = null)
        {
            try
            {
                db.Connect();

                string query = @"
SELECT 
    CAST(s.Sale_date AS DATE) AS Sale_Date,
    COUNT(DISTINCT s.Sale_id) AS Total_Pavati,
    ISNULL(SUM(s.Final_amt) - SUM(s.Total_CGST) - SUM(s.Total_SGST) - SUM(s.Total_IGST) - SUM(s.Round_off), 0) AS Sale_Amount,
    ISNULL(SUM(s.Total_CGST), 0) AS Total_CGST,
    ISNULL(SUM(s.Total_SGST), 0) AS Total_SGST,
    ISNULL(SUM(s.Total_IGST), 0) AS Total_IGST,
    ISNULL(SUM(s.Round_off), 0) AS Total_Roundoff,
    ISNULL(SUM(s.Final_amt), 0) AS Bill_Amount
FROM SALE s
WHERE s.Status = 1
AND CAST(s.Sale_date AS DATE) BETWEEN @FromDate AND @ToDate";

                if (Outlet_id != null)
                    query += " AND s.Outlet_id = @Outlet_id";

                if (Counter_id != null)
                    query += " AND s.Counter_id = @Counter_id";

                query += @"
GROUP BY CAST(s.Sale_date AS DATE)
ORDER BY CAST(s.Sale_date AS DATE)";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);
                if (Outlet_id != null)
                    cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);
                if (Counter_id != null)
                    cmd.Parameters.AddWithValue("@Counter_id", Counter_id);

                System.Data.DataTable dt = new System.Data.DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                decimal grandSale = 0, grandCGST = 0, grandSGST = 0,
                        grandIGST = 0, grandRoundoff = 0, grandBill = 0;
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
                        Sale_Date = row["Sale_Date"],
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

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    List = list,
                    Summary = new
                    {
                        Grand_Pavati = grandPavati,
                        Grand_Sale = grandSale,
                        Grand_CGST = grandCGST,
                        Grand_SGST = grandSGST,
                        Grand_IGST = grandIGST,
                        Grand_Roundoff = grandRoundoff,
                        Grand_Bill = grandBill
                    }
                });
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // API 2 - Outlet wise (outlet_id + counter_id)
        [Route("api/sale/register/outlet")]
        [HttpGet]
        public HttpResponseMessage GetSaleRegisterByOutlet(DateTime FromDate, DateTime ToDate, int Outlet_id, int? Counter_id = null)
        {
            try
            {
                db.Connect();

                string query = @"
SELECT 
    CAST(s.Sale_date AS DATE) AS Sale_Date,
    s.Counter_id,
    COUNT(DISTINCT s.Sale_id) AS Total_Pavati,
    ISNULL(SUM(s.Final_amt) - SUM(s.Total_CGST) - SUM(s.Total_SGST) - SUM(s.Total_IGST) - SUM(s.Round_off), 0) AS Sale_Amount,
    ISNULL(SUM(s.Total_CGST), 0) AS Total_CGST,
    ISNULL(SUM(s.Total_SGST), 0) AS Total_SGST,
    ISNULL(SUM(s.Total_IGST), 0) AS Total_IGST,
    ISNULL(SUM(s.Round_off), 0) AS Total_Roundoff,
    ISNULL(SUM(s.Final_amt), 0) AS Bill_Amount
FROM SALE s
WHERE s.Status = 1
AND CAST(s.Sale_date AS DATE) BETWEEN @FromDate AND @ToDate
AND s.Outlet_id = @Outlet_id";

                if (Counter_id != null)
                    query += " AND s.Counter_id = @Counter_id";

                query += @"
GROUP BY CAST(s.Sale_date AS DATE), s.Counter_id
ORDER BY CAST(s.Sale_date AS DATE), s.Counter_id";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);
                cmd.Parameters.AddWithValue("@Outlet_id", Outlet_id);
                if (Counter_id != null)
                    cmd.Parameters.AddWithValue("@Counter_id", Counter_id);

                System.Data.DataTable dt = new System.Data.DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                decimal grandSale = 0, grandCGST = 0, grandSGST = 0,
                        grandIGST = 0, grandRoundoff = 0, grandBill = 0;
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
                        Sale_Date = row["Sale_Date"],
                        Counter_id = row["Counter_id"],
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

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    List = list,
                    Summary = new
                    {
                        Grand_Pavati = grandPavati,
                        Grand_Sale = grandSale,
                        Grand_CGST = grandCGST,
                        Grand_SGST = grandSGST,
                        Grand_IGST = grandIGST,
                        Grand_Roundoff = grandRoundoff,
                        Grand_Bill = grandBill
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