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
    public class StockSubGroupController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/StockSubGroup")]
        public HttpResponseMessage GetStockSubGroup()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_STOCK_SUBGROUP");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);   
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/StockSubGroup")]
        public HttpResponseMessage GetStockSubGroup(int Subgroup_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_STOCK_SUBGROUP where Subgroup_id = " + Subgroup_id);
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/StockSubGroup")]
        public HttpResponseMessage PostStockSubGroup([FromBody]STOCK_SUBGROUP stock_subgroup)
        {
            try
            {
                db.Connect();
                if(db.IsValidUser(stock_subgroup.Created_by))
                {
                    if (db.IsExists("Select * from STOCK_SUBGROUP where Subgroup_name = '" + stock_subgroup.Subgroup_name + "' and Group_id = " + stock_subgroup.Group_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Stock subgroup already exists in this group");
                    }
                    SqlCommand cmd = new SqlCommand("Sp_Stock_Subgroup", db.cn);

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Group_id", stock_subgroup.Group_id);
                    cmd.Parameters.AddWithValue("@Subgroup_name", stock_subgroup.Subgroup_name);
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

        [Route("api/StockSubGroup")]
        public HttpResponseMessage PutStockSubGroup([FromBody] STOCK_SUBGROUP stock_subgroup)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(stock_subgroup.Modified_by))
                {
                    if (db.IsExists("Select * from STOCK_SUBGROUP where Subgroup_name = '" + stock_subgroup.Subgroup_name + "' and Group_id = " + stock_subgroup.Group_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Stock subgroup already exists in this group");
                    }
                    SqlCommand cmd = new SqlCommand("Sp_Stock_Subgroup", db.cn);

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Subgroup_id", stock_subgroup.Subgroup_id);
                    cmd.Parameters.AddWithValue("@Group_id", stock_subgroup.Group_id);
                    cmd.Parameters.AddWithValue("@Subgroup_name", stock_subgroup.Subgroup_name);
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

        [Route("api/DelStockSubGroup")]
        [HttpPost]
        public HttpResponseMessage DeleteStockSubGroup([FromBody] STOCK_SUBGROUP stock_subgroup)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(stock_subgroup.Modified_by))
                {
                    SqlCommand cmd = new SqlCommand("Sp_Stock_Subgroup_dlt", db.cn);

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Subgroup_id", stock_subgroup.Subgroup_id);
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
