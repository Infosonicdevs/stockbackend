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
    public class StockDistController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/Gsinward")]
        public HttpResponseMessage GetGsInward()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("SELECT * FROM Gs_inward");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/GetByBarcode/{barcode}")]
        [HttpGet]
        public HttpResponseMessage GetByBarcode(string barcode)
        {
            try
            {
                db.Connect();

                var result = db.GetTable("SELECT * FROM view_gs_inward WHERE Barcode = '" + barcode + "'");

                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("api/StockDist")]
        public HttpResponseMessage GetStockDist()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("SELECT * FROM STOCK_DISTRIBUTION");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/StockDistribution")]
        public HttpResponseMessage PostStockDistribution([FromBody] List<STOCK_DISTRIBUTION> stocks)
        {

            if (stocks == null || !stocks.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "List is empty. Can not save.");
            }
            try
            {
                db.Connect();
                int count = 0;
                if (db.IsValidUser(stocks.First().Created_by))
                {
                    foreach (var stock in stocks)
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Stock_distribution", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Batch_no", stock.Batch_no);
                        cmd.Parameters.AddWithValue("@Outlet_id", stock.Outlet_id);
                        cmd.Parameters.AddWithValue("@Date", stock.Date);
                        cmd.Parameters.AddWithValue("@Invert", stock.Invert);
                        cmd.Parameters.AddWithValue("@Gs_pur_id", stock.GS_pur_id);
                        cmd.Parameters.AddWithValue("@Stock_id", stock.Stock_id);
                        cmd.Parameters.AddWithValue("@pur_amt", stock.Pur_amt);
                        cmd.Parameters.AddWithValue("@MRP", stock.MRP);
                        cmd.Parameters.AddWithValue("@Quantity", stock.Quantity);
                        cmd.Parameters.AddWithValue("@Amount", stock.Amount);
                        cmd.Parameters.AddWithValue("@Is_new", stock.Is_new);
                        cmd.Parameters.AddWithValue("@User", stock.Created_by);
                        cmd.Parameters.AddWithValue("@txt", 1);

                        count += cmd.ExecuteNonQuery();
                    }
                    
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted :"+count);
                }

                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("api/StockDistribution")]
        public HttpResponseMessage PutStockDistribution([FromBody] List<STOCK_DISTRIBUTION> stocks)
        {
            if (stocks == null || !stocks.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "List is empty. Can not update.");
            }

            try
            {
         
                db.Connect();
                int count = 0;

                if (db.IsAdmin(stocks.First().Modified_by))
                {
                    //  Delete 
                    SqlCommand delCmd = new SqlCommand("DELETE FROM STOCK_DISTRIBUTION WHERE Batch_no=@Batch_no ", db.cn);
                    delCmd.Parameters.AddWithValue("@Batch_no", stocks.First().Batch_no);
                    delCmd.ExecuteNonQuery();

                    foreach (var stock in stocks)
                    {
                        //  Insert 
                        SqlCommand cmd = new SqlCommand("Sp_Stock_distribution", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Batch_no", stock.Batch_no);
                        cmd.Parameters.AddWithValue("@Outlet_id", stock.Outlet_id);
                        cmd.Parameters.AddWithValue("@Date", stock.Date);
                        cmd.Parameters.AddWithValue("@Invert", stock.Invert);
                        cmd.Parameters.AddWithValue("@Gs_pur_id", stock.GS_pur_id);
                        cmd.Parameters.AddWithValue("@Stock_id", stock.Stock_id);
                        cmd.Parameters.AddWithValue("@pur_amt", stock.Pur_amt);
                        cmd.Parameters.AddWithValue("@MRP", stock.MRP);
                        cmd.Parameters.AddWithValue("@Quantity", stock.Quantity);
                        cmd.Parameters.AddWithValue("@Amount", stock.Amount);
                        cmd.Parameters.AddWithValue("@Is_new", stock.Is_new);
                        cmd.Parameters.AddWithValue("@User", stock.Modified_by);

                        cmd.Parameters.AddWithValue("@txt", 1);

                        count += cmd.ExecuteNonQuery();
                    }

                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Batch Updated: " + count);
                }

                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("api/BatchNo")]

        public HttpResponseMessage GetMaxBatchNo()
        {
            try
            {
                db.Connect();
                var result = Convert.ToInt32(db.ExecuteScalar("SELECT COALESCE(MAX(Batch_no), 0) + 1 FROM STOCK_DISTRIBUTION"));
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
         }


        [Route("api/StockDist")]
        public HttpResponseMessage GetStockDist(string BatchNo)
        {
            try
            {
                db.Connect();
                string query = @"WITH LatestRate AS (
                                SELECT 
                                    Stock_id,
                                    MRP,
                                    Discount,
                                    ROW_NUMBER() OVER (
                                        PARTITION BY Stock_id 
                                        ORDER BY Sequence_no DESC
                                    ) AS rn
                                FROM [Stock].[dbo].[STOCK_RATE]
                            )

                            SELECT 
                                sd.*,
                                lr.Discount AS Discount
                            FROM 
                                [Stock].[dbo].[STOCK_DISTRIBUTION] sd
                            LEFT JOIN 
                                LatestRate lr 
                                ON sd.Stock_id = lr.Stock_id AND lr.rn = 1
                            WHERE Batch_no = @BatchNo";
                var result = db.GetTable(query, new SqlParameter("@BatchNo", BatchNo));
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/StockDistList")]
        public HttpResponseMessage GetStockDistList()
        {
            try
            {
                db.Connect();
                string query = @"WITH LatestRate AS (
                                SELECT 
                                    Stock_id,
                                    MRP,
                                    Discount,
                                    ROW_NUMBER() OVER (
                                        PARTITION BY Stock_id 
                                        ORDER BY Sequence_no DESC
                                    ) AS rn
                                FROM [Stock].[dbo].[STOCK_RATE]
                            )

                            SELECT 
                                sd.Outlet_id,
                                sd.Batch_no,
                                CAST(sd.Date AS DATE) AS Date,
                                SUM(sd.Quantity) AS Total_Quantity,
                                SUM(sd.Amount) AS Total_Amount,
                                MAX(lr.MRP) AS MRP,
                                MAX(lr.Discount) AS Discount
                            FROM 
                                [Stock].[dbo].[STOCK_DISTRIBUTION] sd
                            LEFT JOIN 
                                LatestRate lr 
                                ON sd.Stock_id = lr.Stock_id AND lr.rn = 1
                            GROUP BY 
                                sd.Outlet_id,
                                sd.Batch_no,
                                CAST(sd.Date AS DATE)
                            ORDER BY 
                                sd.Batch_no DESC;";
                var result = db.GetTable(query);
                db.Disconnect();
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
