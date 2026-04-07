using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using System.Web.Http.Cors;
using Stock_Backend.Models;

namespace Stock_Backend.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CityController : ApiController
    {
        DbClass db = new DbClass();

        //To Get all Cities
        [Route("api/City")]
        public HttpResponseMessage GetCity()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_CITY_MASTER order by City");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }

        }

        //To Get City by Id
        [Route("api/City")]   //"api/Ledger?L_id=1"
        public HttpResponseMessage GetCity(int C_id)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from VIEW_CITY_MASTER where City_id=" + C_id);
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }

        //To Update City
        [HttpPut]
        [Route("api/City")]
        public HttpResponseMessage EditCity([FromBody] City_Master city)
        {
            try
            {
                db.Connect();
                
                if (db.IsAdmin(city.User_name))
                {
                    DataTable dt = db.GetTable("select * from CITY_MASTER where City ='"+ city.City +"' and City_id !="+ city.City_id +"");
                    if (dt.Rows.Count == 0)
                    {
                       db.Execute("update CITY_MASTER set Taluka_id =" + city.Taluka_id + ", City ='" + city.City + "', City_RL =N'" + city.City_RL + "' where City_id= " + city.City_id + "");


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

        //To Save City
        [Route("api/City")]
        public HttpResponseMessage PostCity([FromBody] City_Master city)
        {
            try
            {

                db.Connect();
                if (db.IsValidUser(city.User_name))
                {
                    if (!db.IsExists("Select * from CITY_MASTER where City='" + city.City + "'"))
                    { 

                        db.Execute("insert into CITY_MASTER(Taluka_id, City,City_RL ) values (" + city.Taluka_id + ",'" + city.City + "',N'" + city.City_RL + "')");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already exists!");

                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NotFound, ex.Message);

            }

        }

        //To Delete City
        [HttpPost]
        [Route("api/DelCity")]
        public HttpResponseMessage deleteCity([FromBody] City_Master city)
        {
            try
            {
                db.Connect();
                
                if (db.IsAdmin(city.User_name))
                {
                    db.Execute("delete from CITY_MASTER where City_id = " + city.City_id + "");
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

        //To Get City by name
        [Route("api/City")]   
        public HttpResponseMessage GetCity(string name)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from VIEW_CITY_MASTER where City='" + name +"'");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }


        //get City from Taluka id
        [Route("api/GetCity")]
        public HttpResponseMessage GetCityByTaluka(int Taluka_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_CITY_MASTER where Taluka_id =" + Taluka_id + " order by City");
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
