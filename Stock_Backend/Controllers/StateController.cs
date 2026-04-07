using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;


namespace Stock_Backend.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]

    public class StateController : ApiController
    {
        DbClass db = new DbClass();

        //to get all states
        [Route("api/State")]
        public HttpResponseMessage GetState()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from STATE_MASTER order by State");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }

        }

        //To get State by state_id
        [Route("api/State")]   
        public HttpResponseMessage GetState(int S_id)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from STATE_MASTER where State_id=" + S_id);
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }

        //To Update a State
        [HttpPut]
        [Route("api/State")]
        public HttpResponseMessage EditState([FromBody] State_Master state)
        {
            try
            {
                db.Connect();
               
                    if (db.IsAdmin(state.User_name))
                    {
                        DataTable dt1 = db.GetTable("Select * from STATE_MASTER where State ='" + state.State + "' and State_id!=" + state.State_id + "");
                        if (dt1.Rows.Count == 0)
                        {
                            db.Execute("update STATE_MASTER set State ='" + state.State + "',State_RL =N'" + state.State_RL + "' where State_id= " + state.State_id + "");
                            db.Disconnect();
                            return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                        }
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already Exists");
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

        //To save a state
        [Route("api/State")]
        public HttpResponseMessage PostState([FromBody] State_Master state)
        {
            try
            {

                db.Connect();
                if (db.IsValidUser(state.User_name))
                {
                    if(! db.IsExists("select * from STATE_MASTER where State='"+state.State+"'"))
                    { 

                        db.Execute("insert into STATE_MASTER(State,State_RL) values ('" + state.State + "',N'" + state.State_RL + "')");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already Exists");
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

        //To delete a state
        [HttpPost]
        [Route("api/DelState")]
        public HttpResponseMessage deleteState([FromBody] State_Master state)
        {
            try
            {
                db.Connect();
                
                    if (db.IsAdmin(state.User_name))
                    {
                        var State_id = state.State_id;
                        if (db.GetTable("Select * from DIST_MASTER where State_id=" + State_id + "").Rows.Count > 0)
                        {
                            db.Disconnect();
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Record can not be deleted");
                        }
                        else
                        {
                            db.Execute("delete from STATE_MASTER where State_id = " + State_id + "");
                            db.Disconnect();
                            return Request.CreateResponse(HttpStatusCode.OK, "Record deleted");
                        }
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

        //To get state by its name
        [Route("api/State")]
        public HttpResponseMessage GetState(string name)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from STATE_MASTER where State='" + name +"'");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }
        }


    }
}
