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
    public class VendorInfoController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/Supplier")]
        public HttpResponseMessage GetSupplier()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_VENDOR_INFO");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Supplier")]
        public HttpResponseMessage GetSupplier(int Vend_id)
        {
            try
            {
                db.Connect();
                var reuslt = db.GetTable("Select * from VIEW_VENDOR_INFO where Vend_id = " + Vend_id);
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, reuslt);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Supplier")]
        public HttpResponseMessage PostSupplier([FromBody]VENDOR_INFO vendoInfo)
        {
            try
            {
                db.Connect();
                if (db.IsValidUser(vendoInfo.Created_by))
                {
                    if (db.IsExists("select * from VENDOR_INFO where Vend_name = '" + vendoInfo.Vend_name + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Vendor name already exists");
                    }
                    else if (db.IsExists("select * from VENDOR_INFO where Vend_code = '" + vendoInfo.Vend_code + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Vendor code already exists");
                    }
                    else if (db.IsExists("select * from VENDOR_INFO where Contact_no = '" + vendoInfo.Contact_no + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Mobile number already exists");
                    }

                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Vendor_Info", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Vend_name", vendoInfo.Vend_name);
                        cmd.Parameters.AddWithValue("@Vend_code", vendoInfo.Vend_code);
                        cmd.Parameters.AddWithValue("@Address", vendoInfo.Address);
                        cmd.Parameters.AddWithValue("@City_id", vendoInfo.City_id);
                        cmd.Parameters.AddWithValue("@Taluka_id", vendoInfo.Taluka_id);
                        cmd.Parameters.AddWithValue("@Dist_id", vendoInfo.Dist_id);
                        cmd.Parameters.AddWithValue("@State_id", vendoInfo.State_id);
                        cmd.Parameters.AddWithValue("@Contact_no", vendoInfo.Contact_no);
                        cmd.Parameters.AddWithValue("@GST_no", vendoInfo.GST_no);
                        cmd.Parameters.AddWithValue("@Email", vendoInfo.Email);
                        cmd.Parameters.AddWithValue("@Bank_name", vendoInfo.Bank_name);
                        cmd.Parameters.AddWithValue("@Bank_acc_no", vendoInfo.Bank_acc_no);
                        cmd.Parameters.AddWithValue("@Bank_branch", vendoInfo.Bank_branch);
                        cmd.Parameters.AddWithValue("@IFSC_code", vendoInfo.IFSC_code);
                        cmd.Parameters.AddWithValue("@Opn_bal", vendoInfo.Opn_bal);
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

        [Route("api/Supplier")]
        public HttpResponseMessage PutSupplier([FromBody]VENDOR_INFO vendoInfo)
        {
            try
            {
                db.Connect();
                if(db.IsAdmin(vendoInfo.Modified_by))
                {
                    if (db.IsExists("select * from VENDOR_INFO where Vend_name = '" + vendoInfo.Vend_name + "' and Vend_id != " + vendoInfo.Vend_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Vendor name already exists");
                    }
                    else if (db.IsExists("select * from VENDOR_INFO where Vend_code = '" + vendoInfo.Vend_code + "' and Vend_id != " + vendoInfo.Vend_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Vendor code already exists");
                    }
                    else if (db.IsExists("select * from VENDOR_INFO where Contact_no = '" + vendoInfo.Contact_no + "' and Vend_id !=" + vendoInfo.Vend_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Mobile number already exists");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Vendor_Info", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Vend_id", vendoInfo.Vend_id);
                        cmd.Parameters.AddWithValue("@Vend_name", vendoInfo.Vend_name);
                        cmd.Parameters.AddWithValue("@Vend_code", vendoInfo.Vend_code);
                        cmd.Parameters.AddWithValue("@Address", vendoInfo.Address);
                        cmd.Parameters.AddWithValue("@City_id", vendoInfo.City_id);
                        cmd.Parameters.AddWithValue("@Taluka_id", vendoInfo.Taluka_id);
                        cmd.Parameters.AddWithValue("@Dist_id", vendoInfo.Dist_id);
                        cmd.Parameters.AddWithValue("@State_id", vendoInfo.State_id);
                        cmd.Parameters.AddWithValue("@Contact_no", vendoInfo.Contact_no);
                        cmd.Parameters.AddWithValue("@GST_no", vendoInfo.GST_no);
                        cmd.Parameters.AddWithValue("@Email", vendoInfo.Email);
                        cmd.Parameters.AddWithValue("@Bank_name", vendoInfo.Bank_name);
                        cmd.Parameters.AddWithValue("@Bank_acc_no", vendoInfo.Bank_acc_no);
                        cmd.Parameters.AddWithValue("@Bank_branch", vendoInfo.Bank_branch);
                        cmd.Parameters.AddWithValue("@IFSC_code", vendoInfo.IFSC_code);
                        cmd.Parameters.AddWithValue("@Opn_bal", vendoInfo.Opn_bal);
                        cmd.Parameters.AddWithValue("@txt", 2);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }

                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/DelSupplier")]
        [HttpPost]
        public HttpResponseMessage DeleteSupplier([FromBody] VENDOR_INFO vendorInfo)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(vendorInfo.Modified_by))
                {
                    if (db.IsExists("select * from Vendor_Bal where Vend_id = " + vendorInfo.Vend_id + ""))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Vendor has Balance,Can't Delete!");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Vendor_Info_dlt", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Vend_id", vendorInfo.Vend_id);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted");
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
    }
}
