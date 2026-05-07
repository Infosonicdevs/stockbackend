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
    public class StockBalanceController : ApiController
    {
        DbClass db = new DbClass();

        #region StockBalance
        [Route("api/StockBalance")]
        public HttpResponseMessage GetStockBalance(int? Outlet_id = null)
        {
            try
            {
                db.Connect();
                string query = "Select * from VIEW_STOCK_BALANCE";
                if (Outlet_id != null)
                    query += " WHERE Outlet_id = " + Outlet_id;
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

        [Route("api/StockBalance")]
        public HttpResponseMessage GetStockBalance(int Bal_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_STOCK_BALANCE where Bal_id = " + Bal_id);
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/StockBalance")]
        public HttpResponseMessage PostStockBalance([FromBody] STOCK_BALANCE sTOCK_BALANCE)
        {
            try
            {
                db.Connect();

                if (db.IsValidUser(sTOCK_BALANCE.Created_By))
                {
                    if (db.IsExists("Select * from STOCK_BALANCE where Stock_id = "
        + sTOCK_BALANCE.Stock_id +
        " AND Outlet_id = " + sTOCK_BALANCE.Outlet_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Stock balance already exists");
                    }
                    else if (sTOCK_BALANCE.Quantity <= 0)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Plese enter the Quantity ");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Stock_balance", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Stock_id", sTOCK_BALANCE.Stock_id);
                        cmd.Parameters.AddWithValue("@Quantity", sTOCK_BALANCE.Quantity);
                        cmd.Parameters.AddWithValue("@Amount", sTOCK_BALANCE.Amount);
                        cmd.Parameters.AddWithValue("@MRP", 0);
                        cmd.Parameters.AddWithValue("@Disc", 0);
                        cmd.Parameters.AddWithValue("@Pur_amt", 0);
                        cmd.Parameters.AddWithValue("@Rate", 0);
                        cmd.Parameters.AddWithValue("@Outlet_id", sTOCK_BALANCE.Outlet_id);
                        cmd.Parameters.AddWithValue("@txt", 1);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/StockBalance")]
        public HttpResponseMessage PutStockBalance([FromBody] STOCK_BALANCE sTOCK_BALANCE)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(sTOCK_BALANCE.Modified_By))
                {
                    if (db.IsExists("Select * from STOCK_BALANCE where Stock_id = "
     + sTOCK_BALANCE.Stock_id +
     " AND Outlet_id = " + sTOCK_BALANCE.Outlet_id +
     " AND Bal_id != " + sTOCK_BALANCE.Bal_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Stock balance already exists");
                    }
                    else if (sTOCK_BALANCE.Quantity <= 0)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Please enter the Quantity ");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Stock_balance", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Bal_id", sTOCK_BALANCE.Bal_id);
                        cmd.Parameters.AddWithValue("@Stock_id", sTOCK_BALANCE.Stock_id);
                        cmd.Parameters.AddWithValue("@Quantity", sTOCK_BALANCE.Quantity);
                        cmd.Parameters.AddWithValue("@Amount", sTOCK_BALANCE.Amount);
                        cmd.Parameters.AddWithValue("@MRP", 0);
                        cmd.Parameters.AddWithValue("@Disc", 0);
                        cmd.Parameters.AddWithValue("@Pur_amt", 0);
                        cmd.Parameters.AddWithValue("@Rate", 0);
                        cmd.Parameters.AddWithValue("@Outlet_id", sTOCK_BALANCE.Outlet_id);
                        cmd.Parameters.AddWithValue("@txt", 2);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/DelStockBalance")]
        [HttpPost]
        public HttpResponseMessage DeleteStockBalance([FromBody] STOCK_BALANCE sTOCK_BALANCE)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(sTOCK_BALANCE.Modified_By))
                {

                    SqlCommand cmd = new SqlCommand("Sp_Stock_balance_dlt", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Bal_id", sTOCK_BALANCE.Bal_id);
                    cmd.ExecuteNonQuery();
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion
    }
}
