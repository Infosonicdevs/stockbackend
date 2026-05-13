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
    public class StockDistributionController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/stock-distribution-report")]
        [HttpGet]
        public HttpResponseMessage GetStockDistribution(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                db.Connect();

                string query = @"
SELECT 
    SD_id,
    Date,
    Outlet_id,
    Outlet_name,
    Outlet_code,
    Stock_id,
    Stock_no,
    Barcode,
    Stock_name,
    Unit_name,
    Quantity,
    Amount,
    MRP
FROM VIEW_STOCK_DISTRIBUTION
WHERE Date >= @FromDate 
AND Date < DATEADD(DAY, 1, @ToDate)
ORDER BY Date, Outlet_name, Stock_name";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@FromDate", FromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", ToDate.Date);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);

                var list = dt.AsEnumerable()
                    .GroupBy(x => new
                    {
                        Date = x["Date"],
                        Outlet_id = x["Outlet_id"],
                        Outlet_name = x["Outlet_name"],
                        Outlet_code = x["Outlet_code"]
                    })
                    .Select(g => new
                    {
                        Date = g.Key.Date,
                        Outlet_id = g.Key.Outlet_id,
                        Outlet_name = g.Key.Outlet_name,
                        Outlet_code = g.Key.Outlet_code,

                        Stocks = g.Select(s => new
                        {
                            SD_id = s["SD_id"],
                            Stock_id = s["Stock_id"],
                            Stock_no = s["Stock_no"],
                            Barcode = s["Barcode"],
                            Stock_name = s["Stock_name"],
                            Unit_name = s["Unit_name"],
                            Quantity = s["Quantity"],
                            MRP = s["MRP"],
                            Amount = s["Amount"]
                        }).ToList()
                    }).ToList();

                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}