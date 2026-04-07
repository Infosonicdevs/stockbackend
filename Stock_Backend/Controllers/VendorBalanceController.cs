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
    public class VendorBalanceController : ApiController
    {
        DbClass db = new DbClass();
        #region VendorBalance

        [Route("api/VendorBalance")]
        public HttpResponseMessage GetVendorBalance()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("SELECT * FROM VIEW_VENDOR_BAL");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/VendorBalance")]
        public HttpResponseMessage GetVendorBalance(int Opn_bal_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("SELECT * FROM VIEW_VENDOR_BAL WHERE Opn_bal_id = " + Opn_bal_id);
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/VendorBalance")]
        public HttpResponseMessage PostVendorBalance([FromBody]VENDOR_BAL vENDOR_BAL)
        {
            try
            {
                db.Connect();
                if (db.IsValidUser(vENDOR_BAL.Created_by))
                {
                    if (db.IsExists("Select * from Vendor_Bal where Vend_Id = " + vENDOR_BAL.Vend_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Vendor balance already exists");
                    }
                    else if (vENDOR_BAL.Amount <= 0)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Please enter the Amount");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Vendor_Bal", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Vend_Id ", vENDOR_BAL.Vend_id);
                        cmd.Parameters.AddWithValue("@Outlet_id ", vENDOR_BAL.Outlet_id);
                        cmd.Parameters.AddWithValue("@Amount ", vENDOR_BAL.Amount);
                        cmd.Parameters.AddWithValue("@txt", 1);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User!");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/VendorBalance")]
        public HttpResponseMessage PutVendorBalance([FromBody]VENDOR_BAL vENDOR_BAL)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(vENDOR_BAL.Modified_by))
                {
                    if (db.IsExists("Select * from Vendor_Bal where Vend_Id = " + vENDOR_BAL.Vend_id + " and Opn_bal_id != " + vENDOR_BAL.Opn_bal_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Vendor balance already exists");
                    }
                    else if (vENDOR_BAL.Amount <= 0)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Please enter the Amount");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Vendor_Bal", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@opn_bal_id", vENDOR_BAL.Opn_bal_id);
                        cmd.Parameters.AddWithValue("@Vend_Id ", vENDOR_BAL.Vend_id);
                        cmd.Parameters.AddWithValue("@Outlet_id", vENDOR_BAL.Outlet_id);
                        cmd.Parameters.AddWithValue("@Amount ", vENDOR_BAL.Amount);
                        cmd.Parameters.AddWithValue("@txt", 2);
                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User!");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("api/DelVendorBalance")]
        public HttpResponseMessage DeleteVendorBalance([FromBody]VENDOR_BAL vENDOR_BAL)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(vENDOR_BAL.Modified_by))
                {                  
                    db.Execute("DELETE FROM Vendor_Bal WHERE Opn_bal_id = " + vENDOR_BAL.Opn_bal_id);
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User!");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion

        #region VendorInfo
        [Route("api/VendorInfo")]
        public HttpResponseMessage GetVendorInfo()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select Vend_id, Vend_name from VENDOR_INFO");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion
    }
}
