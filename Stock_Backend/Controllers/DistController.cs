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
    public class DistController : ApiController
    {
        DbClass db = new DbClass();

        //To Get all district
        [Route("api/Dist")]
        public HttpResponseMessage GetDist()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from View_Dist_Master order by Dist");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }

        }

        //To Get district by district_id
        [Route("api/Dist")]   
        public HttpResponseMessage GetDist(int D_id)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from View_Dist_Master where Dist_id=" + D_id);
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }

        //To Update a District
        [HttpPut]
        [Route("api/Dist")]
        public HttpResponseMessage EditDist([FromBody] Dist_Master dist)
        {
            try
            {
                db.Connect();
                
                if (db.IsAdmin(dist.User_name))
                {
                    DataTable dt1 = db.GetTable("Select * from DIST_MASTER where Dist ='" + dist.Dist + "' and Dist_id!=" + dist.Dist_id + "");
                    if (dt1.Rows.Count == 0)
                    {
                        db.Execute("update DIST_MASTER set State_id =" + dist.State_id + ", Dist ='" + dist.Dist + "', Dist_RL =N'" + dist.Dist_RL + "' where Dist_id= " + dist.Dist_id + "");
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

        //To save a district
        [Route("api/Dist")]
        public HttpResponseMessage PostDist([FromBody] Dist_Master dist)
        {
            try
            {

                db.Connect();

                if (db.IsValidUser(dist.User_name))
                {
                    if(! db.IsExists("select * from DIST_MASTER where Dist ='"+dist.Dist+"'"))
                    { 
                        db.Execute("insert into Dist_Master(State_id,Dist,Dist_RL) values (" + dist.State_id + ",'" + dist.Dist + "',N'" + dist.Dist_RL + "')");
                        db.Disconnect();  
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already Exists!");

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

        //To delete a district
        [HttpPost]
        [Route("api/DelDist")]
        public HttpResponseMessage deleteDist([FromBody] Dist_Master dist)
        {
            try
            {
                db.Connect();
                
                    if (db.IsAdmin(dist.User_name))
                    {
                        var Dist_id = dist.Dist_id;
                        if(db.GetTable("Select * from Taluka_Master where Dist_id= " + Dist_id + "").Rows.Count>0)
                        {
                            db.Disconnect();
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Record can not be deleted....!");
                        }
                        else
                        {
                            db.Execute("delete from Dist_Master where Dist_id = " + Dist_id + "");
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

        //To get a district by name
        [Route("api/Dist")]
        public HttpResponseMessage GetDist(string name)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from View_Dist_Master where Dist='" + name +"'");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }



        //get Dist from State id
        [Route("api/GetDist")]
        public HttpResponseMessage GetDistByState(int State_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from View_Dist_Master where State_id =" + State_id + " order by Dist");
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
