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
    public class StockGroupController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/StockGroup")]
        public HttpResponseMessage GetStockGroup()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from STOCK_GROUP");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/StockGroup")]
        public HttpResponseMessage GetStockGroup(int Group_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from STOCK_GROUP where Group_id = " + Group_id);
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/StockGroup")]
        public HttpResponseMessage PostStockGroup([FromBody] STOCK_GROUP stockGroup)
        {
            try
            {
                db.Connect();
                if (db.IsValidUser(stockGroup.Created_by))
                {
                    if (db.IsExists("Select * from STOCK_GROUP where Group_name = '" + stockGroup.Group_name + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Stock group name already exists");
                    }
                    SqlCommand cmd = new SqlCommand("Sp_Stock_Group", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Group_name", stockGroup.Group_name);
                    cmd.Parameters.AddWithValue("@txt", 1);
                    cmd.ExecuteNonQuery();
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
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

        [Route("api/StockGroup")]
        public HttpResponseMessage PutStockGroup([FromBody] STOCK_GROUP stockGroup)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(stockGroup.Modified_by))
                {
                    if (db.IsExists("Select * from STOCK_GROUP where Group_name = '" + stockGroup.Group_name + "' and Group_id != " + stockGroup.Group_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Stock group name already exists");
                    }
                    SqlCommand cmd = new SqlCommand("Sp_Stock_Group", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Group_id", stockGroup.Group_id);
                    cmd.Parameters.AddWithValue("@Group_name", stockGroup.Group_name);
                    cmd.Parameters.AddWithValue("@txt", 2);
                    cmd.ExecuteNonQuery();
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
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

        [Route("api/DelStockGroup")]
        [HttpPost]
        public HttpResponseMessage DeleteStockGroup([FromBody] STOCK_GROUP stockGroup)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(stockGroup.Modified_by))
                {
                    SqlCommand cmd = new SqlCommand("Sp_Stock_Group_dlt", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Group_id", stockGroup.Group_id);
                    cmd.ExecuteNonQuery();
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted");
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
