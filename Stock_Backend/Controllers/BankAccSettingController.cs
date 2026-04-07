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
    public class BankAccSettingController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/BankAccSetting")]
        public HttpResponseMessage GetBankAccSetting()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_BANK_ACC_SETTING");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpPost]
        [Route("api/BankAccSetting")]
        public HttpResponseMessage SaveBankAccSetting([FromBody] Bank_acc_setting bank_acc)
        {
            try
            {
                db.Connect();
                if (db.IsValidUser(bank_acc.User))
                {
                    if (db.IsExists("select * from BANK_ACC_SETTING where Bank_name='" + bank_acc.Bank_name + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Bank name Already Exists!");
                    }
                    else if (db.IsExists("select * from BANK_ACC_SETTING where Bank_acc_no='" + bank_acc.Bank_acc_no + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Bank acc no Already Exists!");
                    }
                    else if (db.IsExists("select * from BANK_ACC_SETTING where IFSC_code='" + bank_acc.IFSC_code + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "IFSC code Already Exists!");
                    }
                    else if (db.IsExists("select * from BANK_ACC_SETTING where L_id='" + bank_acc.L_id + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Ledger already assigned to Bank!!");
                    }
                    else
                    {

                        SqlCommand cmd = new SqlCommand("Sp_Bank_Acc_Setting", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Bank_name", bank_acc.Bank_name);
                        cmd.Parameters.AddWithValue("@Bank_acc_no", bank_acc.Bank_acc_no);
                        cmd.Parameters.AddWithValue("@Branch", bank_acc.Branch);
                        cmd.Parameters.AddWithValue("@IFSC_code", bank_acc.IFSC_code);
                        cmd.Parameters.AddWithValue("@L_id", bank_acc.L_id);
                        cmd.Parameters.AddWithValue("@Bank_type", bank_acc.Bank_type);
                        cmd.Parameters.AddWithValue("@Customer_no", bank_acc.Customer_no);
                        cmd.Parameters.AddWithValue("@Opn_Amt", bank_acc.Opn_amt);
                        cmd.Parameters.AddWithValue("@Date", bank_acc.Date);
                        cmd.Parameters.AddWithValue("@txt", 1);

                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpPut]
        [Route("api/BankAccSetting")]
        public HttpResponseMessage EditBankAccSetting([FromBody] Bank_acc_setting bank_acc)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(bank_acc.User))
                {
                    if (db.IsExists("select * from BANK_ACC_SETTING where Bank_name='" + bank_acc.Bank_name + "' and Bank_id!="+bank_acc.Bank_id+""))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Bank name Already Exists!");
                    }
                    else if (db.IsExists("select * from BANK_ACC_SETTING where Bank_acc_no='" + bank_acc.Bank_acc_no + "' and Bank_id!="+bank_acc.Bank_id+""))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Bank acc no Already Exists!");
                    }
                    else if (db.IsExists("select * from BANK_ACC_SETTING where IFSC_code='" + bank_acc.IFSC_code + "' and Bank_id!="+bank_acc.Bank_id+""))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "IFSC code Already Exists!");
                    }
                    else if (db.IsExists("select * from BANK_ACC_SETTING where L_id='" + bank_acc.L_id + "' and Bank_id!=" + bank_acc.Bank_id + ""))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Ledger already assigned to Bank!");
                    }
                    else
                    {

                        SqlCommand cmd = new SqlCommand("Sp_Bank_Acc_Setting", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Bank_id", bank_acc.Bank_id);
                        cmd.Parameters.AddWithValue("@Bank_name", bank_acc.Bank_name);
                        cmd.Parameters.AddWithValue("@Bank_acc_no", bank_acc.Bank_acc_no);
                        cmd.Parameters.AddWithValue("@Branch", bank_acc.Branch);
                        cmd.Parameters.AddWithValue("@IFSC_code", bank_acc.IFSC_code);
                        cmd.Parameters.AddWithValue("@L_id", bank_acc.L_id);
                        cmd.Parameters.AddWithValue("@Bank_type", bank_acc.Bank_type);
                        cmd.Parameters.AddWithValue("@Customer_no", bank_acc.Customer_no);
                        cmd.Parameters.AddWithValue("@Opn_Amt", bank_acc.Opn_amt);
                        cmd.Parameters.AddWithValue("@Date", bank_acc.Date);
                        cmd.Parameters.AddWithValue("@txt", 2);

                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpPost]
        [Route("api/DelBankAccSetting")]
        public HttpResponseMessage DeleteBankAccSetting([FromBody] Bank_acc_setting bank_acc)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(bank_acc.User))
                {
                    SqlCommand cmd = new SqlCommand("Sp_Bank_Acc_Setting_dlt", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Bank_id", bank_acc.Bank_id);
                    cmd.ExecuteNonQuery();
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record deleted");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }

    }
}
