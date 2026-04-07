using Finex_api.Models;
using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class YearController : ApiController
    {
        DbClass db = new DbClass();

        //To Get all Years
        [Route("api/Year")]
        public HttpResponseMessage GetYear()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from YEAR_INFO order by Year_id");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }

        }

        //Save Year 
        [Route("api/Year")]
        public HttpResponseMessage SaveYear([FromBody] YEAR_INFO Year)
        {
            try
            {
                db.Connect();
                if (db.IsValidUser(Year.User_name))
                {
                    if(Year.Status == 1 && db.IsExists("select * from Year_Info where status=1"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Current financial year already active");
                    }
                    if (!db.IsExists("select * from Year_info where End_date>'"+Year.Start_date.ToString("MM/dd/yyyy")+"'"))
                    {
                        Year.Year_id = Convert.ToInt32(db.ExecuteScalar("select coalesce(MAX(year_id),0)+1 from YEAR_INFO "));
                        db.Execute("insert into YEAR_INFO (Year_id,Start_date,End_date,status,Password) values ("+Year.Year_id+",'"+Year.Start_date.ToString("MM/dd/yyyy")+"','"+Year.End_date.ToString("MM/dd/yyyy")+"',"+Year.Status+",'"+Year.Password+"')");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid date selection!");
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


        //Save Year 
        [HttpPut]
        [Route("api/Year")]
        public HttpResponseMessage EditYear([FromBody] YEAR_INFO Year)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(Year.User_name))
                {
                    if (Year.Status == 1 && db.IsExists("select * from Year_Info where status=1") && db.IsExists("select * from Year_Info where Year_id != " + Year.Year_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Current financial year already active");
                    }
                    if (Year.Password == "")
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Password must not be empty!");
                    }

                    if (!db.IsExists("select * from Year_info where End_date>'" + Year.Start_date.ToString("MM/dd/yyyy") + "' and Year_id < "+Year.Year_id+""))
                    {
                        db.Execute("update YEAR_INFO set Start_date='"+Year.Start_date.ToString("MM/dd/yyyy")+"',End_date='"+Year.End_date.ToString("MM/dd/yyyy")+"',Status="+Year.Status+",Password='"+Year.Password+ "' where Year_id="+Year.Year_id+"");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Update");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Date selection!");
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

        [Route("api/CurrentYear")]
        public HttpResponseMessage GetCurrentFinancialYear()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from Year_Info where status=1");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
