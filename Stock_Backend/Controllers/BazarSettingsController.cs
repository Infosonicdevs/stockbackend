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
    public class BazarSettingsController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/BazarSetting")]
        public HttpResponseMessage GetBazarSetting()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from View_bazar_settg");
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception ex) 
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //[Route("api/BazarSetting")]
        //[HttpPost]
        //public HttpResponseMessage PostBazarSetting([FromBody]BAZAR_SETTING bazar_setting)
        //{
        //    try
        //    {
        //        db.Connect();

        //        if (db.IsValidUser(bazar_setting.Created_by))
        //        {
        //            SqlCommand cmd = new SqlCommand("Sp_Bazar_Setting", db.cn);
        //            cmd.CommandType = System.Data.CommandType.StoredProcedure;

        //            cmd.Parameters.AddWithValue("@Pur_l_id", bazar_setting.Pur_id);
        //            cmd.Parameters.AddWithValue("@Sale_l_id", bazar_setting.Sale_id);
        //            cmd.Parameters.AddWithValue("@Redeem_l_id", bazar_setting.Redeem_id);
        //            cmd.Parameters.AddWithValue("@Cash_return_l_id", bazar_setting.Cash_return_id);
        //            cmd.Parameters.AddWithValue("@Transfer_l_id", bazar_setting.Transfer_id);
        //            cmd.Parameters.AddWithValue("@RoundOff_l_id", bazar_setting.Round_Off_id);
        //            cmd.Parameters.AddWithValue("@Hamali_l_id", bazar_setting.Hamali_id);
        //            cmd.Parameters.AddWithValue("@Transport_l_id", bazar_setting.Transport_id);
        //            cmd.Parameters.AddWithValue("@Commi_l_id", bazar_setting.Commi_id);
        //            cmd.Parameters.AddWithValue("@Udhar_id", bazar_setting.Udhar_id);
        //            cmd.Parameters.AddWithValue("@Ma_ses_l_id", bazar_setting.Ma_ses_id);
        //            cmd.Parameters.AddWithValue("@Cash_submit_l_id", bazar_setting.Cash_submit_id);
        //            cmd.Parameters.AddWithValue("@Tcs_l_id", bazar_setting.TCS_id);
        //            cmd.Parameters.AddWithValue("@UPI_l_id", bazar_setting.UPI_id);
        //            cmd.Parameters.AddWithValue("@kharedi_parat_l_id", bazar_setting.Kharedi_parat_id);
        //            cmd.Parameters.AddWithValue("@Vyapari_yene_l_id", bazar_setting.Vyapari_yene_id);
        //            cmd.Parameters.AddWithValue("@Net_disc_l_id", bazar_setting.Net_disc_id);
        //            cmd.Parameters.AddWithValue("@txt", 1);
        //            cmd.ExecuteNonQuery();
        //            db.Disconnect();
        //            return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
        //        }
        //        db.Disconnect();
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");
        //    }
        //    catch(Exception ex)
        //    {
        //        db.Disconnect();
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        [Route("api/BazarSetting")]
        [HttpPut]
        public HttpResponseMessage PutBazarSetting([FromBody] List<Bazar_settg> bz_settgs)
        {
            try
            {
                int count=0;
                db.Connect();
                if (bz_settgs == null || !bz_settgs.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Request data is empty.");
                }
                if (db.IsAdmin(bz_settgs.First().User))
                {
                    foreach (var settg in bz_settgs)
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_Led_Setting", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Bz_id", settg.Bz_id);
                        cmd.Parameters.AddWithValue("@L_id", settg.L_id);
                        cmd.Parameters.AddWithValue("@txt", 2);
                        count += cmd.ExecuteNonQuery();
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Settings Updated : "+count);
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User!!");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
