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
    public class StockrateController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/StockRate")]
        public HttpResponseMessage GetStockRates()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("SELECT * FROM STOCK_RATE");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("api/StockRate")]
        public HttpResponseMessage PostStockRate([FromBody] STOCK_RATE stockRate)
        {
            try
            {
                db.Connect();
                if (!db.IsValidUser(stockRate.User_name))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");
                }

                SqlCommand cmd = new SqlCommand("dbo.Sp_Stock_rate", db.cn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Stock_id", stockRate.Stock_id);
                cmd.Parameters.AddWithValue("@MRP", stockRate.MRP);
                cmd.Parameters.AddWithValue("@Discount", stockRate.Discount);
                cmd.Parameters.AddWithValue("@Rate", stockRate.Rate);
                cmd.Parameters.AddWithValue("@Change_date", stockRate.Change_date);
                cmd.Parameters.AddWithValue("@Sequence_no", stockRate.Sequence_no);
                cmd.Parameters.AddWithValue("@On_from", stockRate.On_form);
                cmd.Parameters.AddWithValue("@txt", 1); 

                cmd.ExecuteNonQuery();
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, "Stock Rate Saved");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("api/StockRate")]
        public HttpResponseMessage PutStockRate([FromBody] STOCK_RATE stockRate)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(stockRate.User_name))
                {
                    SqlCommand cmd = new SqlCommand("Sp_Stock_rate", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Rate_id", stockRate.Rate_id);
                    cmd.Parameters.AddWithValue("@Stock_id", stockRate.Stock_id);
                    cmd.Parameters.AddWithValue("@MRP", stockRate.MRP);
                    cmd.Parameters.AddWithValue("@Discount", stockRate.Discount);
                    cmd.Parameters.AddWithValue("@Rate", stockRate.Rate);
                    cmd.Parameters.AddWithValue("@Change_date", DateTime.Now.Date);
                    cmd.Parameters.AddWithValue("@Sequence_no", stockRate.Sequence_no);
                    cmd.Parameters.AddWithValue("@On_from", stockRate.On_form);
                    cmd.Parameters.AddWithValue("@txt", 1); 

                    cmd.ExecuteNonQuery();
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Stock Rate Updated");
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

        [Route("api/DelStockRate")]
        [HttpPost]
        public HttpResponseMessage DeleteStockRate([FromBody] STOCK_RATE stockRate)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(stockRate.User_name))
                {
                    SqlCommand cmd = new SqlCommand("Sp_Stock_rate_dlt", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Rate_id", stockRate.Rate_id);
                    cmd.ExecuteNonQuery();
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Stock Rate Deleted");
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


    }
}


