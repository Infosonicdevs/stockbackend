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
    public class OutletController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/Outlet")]
        public HttpResponseMessage GetOutlet()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_OUTLET");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Outlet")]
        public HttpResponseMessage GetOutlet(int Outlet_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_OUTLET where Outlet_id = " + Outlet_id);
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Outlet")]
        public HttpResponseMessage PostOutlet([FromBody] OUTLET outlet)
        {
            try
            {
                db.Connect();
                if (db.IsValidUser(outlet.Created_by))
                {               
                    if (outlet.Is_main_branch == 1 && db.IsExists("Select * from OUTLET where Is_main_branch=1"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Main Branch already exists");
                    }
                    else if(db.IsExists("select * from OUTLET where Outlet_code = '" + outlet.Outlet_code + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Branch code already exists");
                    }
                    else if (db.IsExists("select * from OUTLET where Outlet_name = '" + outlet.Outlet_name + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Branch name already exists");
                    }
                    else if (db.IsExists("select * from OUTLET  where Contact_no = '" + outlet.Contact_no + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Mobile number already exists");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_outlet", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Outlet_code", outlet.Outlet_code);
                        cmd.Parameters.AddWithValue("@Outlet_name", outlet.Outlet_name);
                        cmd.Parameters.AddWithValue("@Outlet_add", outlet.Outlet_add);
                        cmd.Parameters.AddWithValue("@City_id", outlet.City_id);
                        cmd.Parameters.AddWithValue("@Dist_id", outlet.District_id);
                        cmd.Parameters.AddWithValue("@Taluka_id", outlet.Taluka_id);
                        cmd.Parameters.AddWithValue("@State_id", outlet.State_id);
                        cmd.Parameters.AddWithValue("@Contact_no", outlet.Contact_no);
                        cmd.Parameters.AddWithValue("@Short_name", outlet.Short_name);
                        cmd.Parameters.AddWithValue("@user", outlet.Created_by);
                        cmd.Parameters.AddWithValue("@Is_main_branch", outlet.Is_main_branch);
                        cmd.Parameters.AddWithValue("@txt", 1);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
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

        [Route("api/Outlet")]
        public HttpResponseMessage PutOutlet([FromBody] OUTLET outlet)
        {
            try
            {
                db.Connect();
                if(db.IsAdmin(outlet.Modified_by))
                {
                    if (outlet.Is_main_branch == 1 && db.IsExists("Select * from OUTLET where Is_main_branch=1"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Main Branch already exists");
                    }
                    else if (db.IsExists("select * from OUTLET where Outlet_code = '" + outlet.Outlet_code + "' and Outlet_id != " + outlet.Outlet_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Branch code already exists");
                    }
                    else if (db.IsExists("select * from OUTLET where Outlet_name = '" + outlet.Outlet_name + "' and Outlet_id != " + outlet.Outlet_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Branch name already exists");
                    }
                    else if (db.IsExists("select * from OUTLET  where Contact_no = '" + outlet.Contact_no + "' and Outlet_id !=" + outlet.Outlet_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Mobile number already exists");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_outlet", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Outlet_id", outlet.Outlet_id);
                        cmd.Parameters.AddWithValue("@Outlet_code", outlet.Outlet_code);
                        cmd.Parameters.AddWithValue("@Outlet_name", outlet.Outlet_name);
                        cmd.Parameters.AddWithValue("@Outlet_add", outlet.Outlet_add);
                        cmd.Parameters.AddWithValue("@City_id", outlet.City_id);
                        cmd.Parameters.AddWithValue("@Dist_id", outlet.District_id);
                        cmd.Parameters.AddWithValue("@Taluka_id", outlet.Taluka_id);
                        cmd.Parameters.AddWithValue("@State_id", outlet.State_id);
                        cmd.Parameters.AddWithValue("@Contact_no", outlet.Contact_no);
                        cmd.Parameters.AddWithValue("@Short_name", outlet.Short_name);
                        cmd.Parameters.AddWithValue("@user", outlet.Modified_by);                  
                        cmd.Parameters.AddWithValue("@Is_main_branch", outlet.Is_main_branch);
                        cmd.Parameters.AddWithValue("@txt", 2);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
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

        [Route("api/DelOutlet")]
        [HttpPost]
        public HttpResponseMessage DeleteOutlet([FromBody] OUTLET outlet)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(outlet.Modified_by))
                {
                    SqlCommand cmd = new SqlCommand("Sp_Bazar_outlet_dlt", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Outlet_id", outlet.Outlet_id);
                    cmd.ExecuteNonQuery();
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record deleted");
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
