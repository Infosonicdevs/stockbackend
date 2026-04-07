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
    public class UnitController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/Unit")]
        public HttpResponseMessage GetUnit()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from UNIT");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Unit")]
        public HttpResponseMessage GetUnit(int Unit_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from UNIT where Unit_id = " + Unit_id);
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Unit")]
        public HttpResponseMessage PostUnit([FromBody]UNIT unit)
        {
            try
            {
                db.Connect();
                if(db.IsValidUser(unit.Created_by))
                {
                    if(!db.IsExists("Select * from UNIT where Unit_name = '" + unit.Unit_name + "'"))
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Unit", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Unit_name", unit.Unit_name);
                        cmd.Parameters.AddWithValue("@multiple_factor", unit.multiple_factor);
                        cmd.Parameters.AddWithValue("@txt", 1);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unit name already exists");
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

        [Route("api/Unit")]
        public HttpResponseMessage PutUnit([FromBody]UNIT unit)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(unit.Modified_by))
                {
                    if (!db.IsExists("Select * from UNIT where Unit_name = '" + unit.Unit_name + "' and Unit_id != " + unit.Unit_id))
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Unit", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Unit_id", unit.Unit_id);
                        cmd.Parameters.AddWithValue("@Unit_name", unit.Unit_name);
                        cmd.Parameters.AddWithValue("@multiple_factor", unit.multiple_factor);
                        cmd.Parameters.AddWithValue("@txt", 2);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unit name is already exits");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/DelUnit")]
        [HttpPost]
        public HttpResponseMessage DeleteUnit([FromBody]UNIT unit)
        {
            try
            {
                db.Connect();
                if(db.IsAdmin(unit.Modified_by))
                {
                    SqlCommand cmd = new SqlCommand("Sp_Unit_dlt", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Unit_id", unit.Unit_id);
                    cmd.ExecuteNonQuery();
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, "Invalid User");
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
