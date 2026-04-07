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
    public class TalukaController : ApiController
    {
        DbClass db = new DbClass();

        //To get all Talukas
        [Route("api/Taluka")]
        public HttpResponseMessage GetTaluka()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from View_Taluka_Master order by Taluka");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }

        }

        //To get Taluka by Id
        [Route("api/Taluka")]   
        public HttpResponseMessage GetTaluka(int T_id)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from View_Taluka_Master where Taluka_id=" + T_id);
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }
        }

        //To Update a Taluka
        [HttpPut]
        [Route("api/Taluka")]
        public HttpResponseMessage EditTaluka([FromBody] Taluka_Master taluka)
        {
            try
            {
                db.Connect();
                
                    if (db.IsAdmin(taluka.User_name))
                    {
                        DataTable dt1 = db.GetTable("Select * from TALUKA_MASTER where Taluka ='" + taluka.Taluka + "' and Taluka_id!=" + taluka.Taluka_id + "");
                        if (dt1.Rows.Count == 0)
                        {
                            db.Execute("update TALUKA_MASTER set Dist_id =" + taluka.Dist_id + ",Taluka ='" + taluka.Taluka + "',Taluka_RL =N'" + taluka.Taluka_RL + "' where Taluka_id= " + taluka.Taluka_id + "");
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

        //To save a Taluka
        [Route("api/Taluka")]
        public HttpResponseMessage PostTaluka([FromBody] Taluka_Master taluka)
        {
            try
            {

                db.Connect();
                if (db.IsValidUser(taluka.User_name))
                {
                    if(!db.IsExists("select * from TALUKA_MASTER where Taluka ='"+taluka.Taluka+"'"))
                    { 
                        db.Execute("insert into TALUKA_MASTER(Dist_id,Taluka,Taluka_RL) values (" + taluka.Dist_id + ",'" + taluka.Taluka + "',N'" + taluka.Taluka_RL + "')");
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

        //To delete a Taluka
        [HttpPost]
        [Route("api/DelTaluka")]
        public HttpResponseMessage deleteTaluka([FromBody] Taluka_Master taluka)
        {
            try
            {
                db.Connect();
                
                    if (db.IsAdmin(taluka.User_name))
                    {
                        var Taluka_id = taluka.Taluka_id;
                        if(db.GetTable("Select * from CITY_MASTER where Taluka_id="+ Taluka_id + "").Rows.Count > 0)
                        {
                            db.Disconnect();
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Record can not be deleted");
                        }
                        else
                        {
                            db.Execute("delete from TALUKA_MASTER where Taluka_id = " + Taluka_id + "");
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

        //To get Taluka by name
        [Route("api/Taluka")]
        public HttpResponseMessage GetTaluka(string name)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from VIEW_TALUKA_MASTER where Taluka='" + name +"'");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }
        }



        //get Taluka from District id
        [Route("api/GetTaluka")]
        public HttpResponseMessage GetTalukaByDistrict(int Dist_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_TALUKA_MASTER where Dist_id =" + Dist_id + " order by Taluka");
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
