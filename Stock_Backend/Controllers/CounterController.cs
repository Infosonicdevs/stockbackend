using Stock_Backend.Models;
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
    public class CounterController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/Counter")]
        public HttpResponseMessage GetCounter()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_COUNTER");
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
        [Route("api/Counter")]
        public HttpResponseMessage SaveCounter([FromBody] Counter counter)
        {
            try
            {
                db.Connect();

                if (db.IsValidUser(counter.User))
                {
                    if (db.IsExists("select * from COUNTER where Counter_name='" + counter.Counter_name + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Counter name already exists!");
                    }
                    else if (db.IsExists("select * from COUNTER where Computer_name='" + counter.Computer_name + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Computer already assigned!");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Counter", db.cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Counter_name", counter.Counter_name);
                        cmd.Parameters.AddWithValue("@Outlet_id", counter.Outlet_id);
                        cmd.Parameters.AddWithValue("@Computer_name", counter.Computer_name);
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


        [HttpPut]
        [Route("api/Counter")]
        public HttpResponseMessage EditCounter([FromBody] Counter counter)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(counter.User))
                {
                    if (db.IsExists("select * from COUNTER where Counter_name='" + counter.Counter_name + "' and Counter_id!=" + counter.Counter_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Counter name already exists!");
                    }
                    else if (db.IsExists("select * from COUNTER where Computer_name='" + counter.Computer_name + "' and Counter_id!=" + counter.Counter_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Computer already assigned!");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Counter", db.cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Counter_id", counter.Counter_id);
                        cmd.Parameters.AddWithValue("@Counter_name", counter.Counter_name);
                        cmd.Parameters.AddWithValue("@Outlet_id", counter.Outlet_id);
                        cmd.Parameters.AddWithValue("@Computer_name", counter.Computer_name);
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
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPost]
        [Route("api/DelCounter")]
        public HttpResponseMessage DeleteCounter([FromBody] Counter counter)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(counter.User))
                {
                    SqlCommand cmd = new SqlCommand("Sp_Counter_dlt", db.cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Counter_id", counter.Counter_id);
                    cmd.ExecuteNonQuery();

                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record deleted");
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
    }
}
