using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class PrifixController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/Prifix")]
        public HttpResponseMessage GetPrifix()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from PREFIX_MASTER order by Prefix");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }

        }


        [Route("api/Prifix")]   
        public HttpResponseMessage GetPrifix(int P_id)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from PREFIX_MASTER where Prefix_id=" + P_id);
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }


        [Route("api/Prifix")]
        public HttpResponseMessage PutPrifix([FromBody] PREFIX_MASTER prefix)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(prefix.User_name))
                {
                    if (!db.IsExists("Select * from PREFIX_MASTER where Prefix='" + prefix.Prefix + "' and Prefix_id != " + prefix.Prefix_id))
                    {
                        db.Execute("update PREFIX_MASTER set Prefix ='" + prefix.Prefix + "', Prefix_RL = '" + prefix.Prefix_RL + "' where Prefix_id= " + prefix.Prefix_id + "");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already exists!");
                }
                
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }


        [Route("api/Prifix")]
        public HttpResponseMessage PostPrifix([FromBody] PREFIX_MASTER prefix)
        {
            try
            {

                db.Connect();
                if (db.IsValidUser(prefix.User_name))
                {
                    if (!db.IsExists("Select * from PREFIX_MASTER where Prefix='" + prefix.Prefix + "'"))
                    {
                        db.Execute("insert into PREFIX_MASTER(Prefix,Prefix_RL) values ('" + prefix.Prefix + "', '" + prefix.Prefix_RL + "')");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already exists!");
                }

                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NotFound, ex.Message);

            }

        }


        [HttpPost]
        [Route("api/DelPrifix")]
        public HttpResponseMessage deletePrifix([FromBody] PREFIX_MASTER prefix)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(prefix.User_name))
                {
                    db.Execute("delete from PREFIX_MASTER where Prefix_id = " + prefix.Prefix_id + "");
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record deleted");
                }
                
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NotFound, ex.Message);

            }

        }

    }
}
