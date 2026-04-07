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
    public class GSTSlabsController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/GSTSlab")]
        [HttpGet]
        public HttpResponseMessage GetGSTSlab()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_GST_SLAB");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/GSTSlab")]
        [HttpGet]
        public HttpResponseMessage GetGSTSlab(int GST_SLAB_Id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_GST_SLAB where Id = " + GST_SLAB_Id);
                if(result.Rows.Count == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.NotFound, $"GST_SLAB with Id {GST_SLAB_Id} not found");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK,result);
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/GSTSlab")]
        [HttpPost]
        public HttpResponseMessage PostGSTSlab([FromBody] GST_SLAB gst_slab)
        {
            try
            {
                db.Connect();

                if (db.IsValidUser(gst_slab.Created_by))
                {
                    if (db.IsExists("select * from GST_SLAB where Tax_code = " + gst_slab.Tax_code))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Tax_code already exists");
                    }
                    else if (db.IsExists("select * from GST_SLAB where Heading = '" + gst_slab.Heading + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Heading already exists");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_gst_slab", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Tax_code", gst_slab.Tax_code);
                        cmd.Parameters.AddWithValue("@Heading", gst_slab.Heading);
                        cmd.Parameters.AddWithValue("@CGST_per", gst_slab.CGST_per);
                        cmd.Parameters.AddWithValue("@SGST_per", gst_slab.SGST_per);
                        cmd.Parameters.AddWithValue("@IGST_per", gst_slab.IGST_per);
                        cmd.Parameters.AddWithValue("@CGST_l_id", gst_slab.CGST_l_id);
                        cmd.Parameters.AddWithValue("@SGST_l_id", gst_slab.SGST_l_id);
                        cmd.Parameters.AddWithValue("@IGST_l_id", gst_slab.IGST_l_id);
                        cmd.Parameters.AddWithValue("@txt", 1);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, "Invalid user");
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("api/GSTSlab")]
        [HttpPut]
        public HttpResponseMessage PutGSTSlab([FromBody] GST_SLAB gst_slab)
        {
            try
            {
                db.Connect();

                if(db.IsAdmin(gst_slab.Modified_by))
                {
                    if (db.IsExists("select * from GST_SLAB where Tax_code = " + gst_slab.Tax_code + " and Id != " + gst_slab.Id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Tax_code already exists");
                    }
                    else if(db.IsExists("select * from GST_SLAB where Heading = '" + gst_slab.Heading + "' and Id != " + gst_slab.Id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Heading already exists");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_gst_slab", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id", gst_slab.Id);
                        cmd.Parameters.AddWithValue("@Tax_code", gst_slab.Tax_code);
                        cmd.Parameters.AddWithValue("@Heading", gst_slab.Heading);
                        cmd.Parameters.AddWithValue("@CGST_per", gst_slab.CGST_per);
                        cmd.Parameters.AddWithValue("@SGST_per", gst_slab.SGST_per);
                        cmd.Parameters.AddWithValue("@IGST_per", gst_slab.IGST_per);
                        cmd.Parameters.AddWithValue("@CGST_l_id", gst_slab.CGST_l_id);
                        cmd.Parameters.AddWithValue("@SGST_l_id", gst_slab.SGST_l_id);
                        cmd.Parameters.AddWithValue("@IGST_l_id", gst_slab.IGST_l_id);
                        cmd.Parameters.AddWithValue("@txt", 2);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, "Invalid user");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/DelGSTSlab")]
        [HttpPost]
        public HttpResponseMessage DeleteGSTSlab(GST_SLAB gst_slab)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(gst_slab.Modified_by))
                {
                    SqlCommand cmd = new SqlCommand("Sp_gst_slab_dlt", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", gst_slab.Id);
                    cmd.ExecuteNonQuery();
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, "Invalid user");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
