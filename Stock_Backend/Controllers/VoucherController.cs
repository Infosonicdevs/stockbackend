using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;

namespace Stock_Backend.Controllers
{
    public class VoucherController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/VoucherTrans")]
        public HttpResponseMessage GetVoucherTrans(DateTime Login_Date)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("SELECT TRANS.*, TRANS_TYPE.Type_name FROM TRANS left join TRANS_TYPE on TRANS.Trans_type_id = TRANS_TYPE.Type_id WHERE TRANS.Trans_type_id IN(5, 6, 7) AND TRANS.Status = 1 AND TRANS.Trans_date = '" + Login_Date.ToString("MM/dd/yyyy") + "'");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/VoucherTrans")]
        public HttpResponseMessage GetVoucherTrans(int Trans_id, DateTime Login_Date)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("SELECT TRANS.*, TRANS_TYPE.Type_name FROM TRANS left join TRANS_TYPE on TRANS.Trans_type_id = TRANS_TYPE.Type_id WHERE TRANS.Trans_type_id IN(5, 6, 7) AND TRANS.Status = 1 AND TRANS.Trans_date = '" + Login_Date.ToString("MM/dd/yyyy") + "' and TRANS.Trans_id = " + Trans_id);
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
        [Route("api/Voucher")]
        public HttpResponseMessage PostVoucher([FromBody] VOUCHER voucher)
        {
            try
            {
                int Trans_id = 0;
                db.Connect();

                if (!db.IsValidUser(voucher.Trans.Created_by))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");
                }

                if (voucher.Trans == null)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Trans request");
                }

                if (voucher.Trans_Details == null || voucher.Trans_Details.Count == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Trans Detail request");
                }

                if (voucher.Trans.Trans_amt != voucher.Trans_Details.Sum(x => x.Amount))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sum of trans detail amount is not matched with trans amount");
                }


                decimal Cr = voucher.Trans_Details .FirstOrDefault(x => x.CrDr_id == 1)?.Amount ?? 0;

                decimal Dr = voucher.Trans_Details .FirstOrDefault(x => x.CrDr_id == 2)?.Amount ?? 0;

                if (Cr != Dr)
                {
                    db.Disconnect();
                    return Request.CreateResponse( HttpStatusCode.BadRequest,"Credit and Debit amount must be equal" );
                }

                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        // Insert Trans
                        SqlCommand cmdTrans = new SqlCommand("Sp_Trans", db.cn, transaction);
                        cmdTrans.CommandType = CommandType.StoredProcedure;

                        cmdTrans.Parameters.AddWithValue("@Outlet_id", voucher.Trans.Outlet_id);
                        cmdTrans.Parameters.AddWithValue("@Trans_date", voucher.Trans.Trans_date);
                        cmdTrans.Parameters.AddWithValue("@Year_id", voucher.Trans.Year_id);
                        cmdTrans.Parameters.AddWithValue("@Trans_amt", voucher.Trans.Trans_amt);
                        cmdTrans.Parameters.AddWithValue("@Trans_type_id", voucher.Trans.Trans_type_id);
                        cmdTrans.Parameters.AddWithValue("@trans_code", '0');
                        cmdTrans.Parameters.AddWithValue("@Status", '1');
                        cmdTrans.Parameters.AddWithValue("@user", voucher.Trans.Created_by);
                        cmdTrans.Parameters.AddWithValue("@txt", 1);

                        SqlParameter par_trans_id = new SqlParameter("@Trans_id", System.Data.SqlDbType.Int); // Define parameter
                        par_trans_id.Direction = System.Data.ParameterDirection.Output;   // Set direction of parameter to output
                        cmdTrans.Parameters.Add(par_trans_id);
                        cmdTrans.ExecuteNonQuery();

                        Trans_id = Convert.ToInt32(cmdTrans.Parameters["@Trans_id"].Value);

                        // Insert Details
                        foreach (var trans_detail in voucher.Trans_Details)
                        {
                            SqlCommand cmdDetail = new SqlCommand("Sp_Voucher_Trans_Details", db.cn, transaction);
                            cmdDetail.CommandType = CommandType.StoredProcedure;

                            cmdDetail.Parameters.AddWithValue("@Trans_id", Trans_id);
                            cmdDetail.Parameters.AddWithValue("@Cash_trans", trans_detail.CashTrans);
                            cmdDetail.Parameters.AddWithValue("@L_id", trans_detail.L_id);

                            if (trans_detail.CrDr_id == 1)
                            {
                                cmdDetail.Parameters.AddWithValue("@CrDr_id", trans_detail.CrDr_id);
                                cmdDetail.Parameters.AddWithValue("@Cr_amt", trans_detail.Amount);
                            }
                            else if (trans_detail.CrDr_id == 2)
                            {
                                cmdDetail.Parameters.AddWithValue("@CrDr_id", trans_detail.CrDr_id);
                                cmdDetail.Parameters.AddWithValue("@Dr_amt", trans_detail.Amount);
                            }
                            cmdDetail.Parameters.AddWithValue("@Cust_id", trans_detail.Cust_id);
                            cmdDetail.Parameters.AddWithValue("@Narr", trans_detail.Narr);
                            cmdDetail.Parameters.AddWithValue("@Cust_name", trans_detail.Cust_name);
                            cmdDetail.Parameters.AddWithValue("@text", 1);

                            cmdDetail.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        db.Disconnect();
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Voucher")]
        public HttpResponseMessage PutVoucher([FromBody] VOUCHER voucher)
        {
            try
            {
                int Trans_id = 0;
                db.Connect();

                if (!db.IsAdmin(voucher.Trans.Modified_by))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");
                }

                if (voucher.Trans == null)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Trans request");
                }

                if (voucher.Trans_Details == null || voucher.Trans_Details.Count == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Trans Detail request");
                }

                if (voucher.Trans.Trans_amt != voucher.Trans_Details.Sum(x => x.Amount))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sum of trans detail amount is not matched with trans amount");
                }
                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        // Insert Trans
                        SqlCommand cmdTrans = new SqlCommand("Sp_Trans", db.cn, transaction);
                        cmdTrans.CommandType = CommandType.StoredProcedure;

                        cmdTrans.Parameters.AddWithValue("@Update_trans_id", voucher.Trans.Trans_id);
                        cmdTrans.Parameters.AddWithValue("@Outlet_id", voucher.Trans.Outlet_id);
                        cmdTrans.Parameters.AddWithValue("@Trans_date", voucher.Trans.Trans_date);
                        cmdTrans.Parameters.AddWithValue("@Year_id", voucher.Trans.Year_id);
                        cmdTrans.Parameters.AddWithValue("@Trans_amt", voucher.Trans.Trans_amt);
                        cmdTrans.Parameters.AddWithValue("@Trans_type_id", voucher.Trans.Trans_type_id);
                        cmdTrans.Parameters.AddWithValue("@trans_code", '0');
                        cmdTrans.Parameters.AddWithValue("@Status", '1');
                        cmdTrans.Parameters.AddWithValue("@user", voucher.Trans.Modified_by);
                        cmdTrans.Parameters.AddWithValue("@Modify_reason", voucher.Trans.Modify_reason);
                        cmdTrans.Parameters.AddWithValue("@txt", 2);

                        SqlParameter par_trans_id = new SqlParameter("@Trans_id", System.Data.SqlDbType.Int); // Define parameter
                        par_trans_id.Direction = System.Data.ParameterDirection.Output;   // Set direction of parameter to output
                        cmdTrans.Parameters.Add(par_trans_id);
                        cmdTrans.ExecuteNonQuery();

                        Trans_id = Convert.ToInt32(cmdTrans.Parameters["@Trans_id"].Value);

                        SqlCommand cmdDelete = new SqlCommand(
                                    "DELETE FROM TRANS_DETAILS WHERE Trans_id = @Trans_id",
                                    db.cn,
                                    transaction
                                );

                        cmdDelete.Parameters.AddWithValue("@Trans_id", Trans_id);
                        cmdDelete.ExecuteNonQuery();

                        // Insert Details
                        foreach (var trans_detail in voucher.Trans_Details)
                        {
                            SqlCommand cmdDetail = new SqlCommand("Sp_Voucher_Trans_Details", db.cn, transaction);
                            cmdDetail.CommandType = CommandType.StoredProcedure;

                            cmdDetail.Parameters.AddWithValue("@Trans_id", Trans_id);
                            cmdDetail.Parameters.AddWithValue("@Cash_trans", trans_detail.CashTrans);
                            cmdDetail.Parameters.AddWithValue("@L_id", trans_detail.L_id);

                            if (trans_detail.CrDr_id == 1)
                            {
                                cmdDetail.Parameters.AddWithValue("@CrDr_id", trans_detail.CrDr_id);
                                cmdDetail.Parameters.AddWithValue("@Cr_amt", trans_detail.Amount);
                            }
                            else if (trans_detail.CrDr_id == 2)
                            {
                                cmdDetail.Parameters.AddWithValue("@CrDr_id", trans_detail.CrDr_id);
                                cmdDetail.Parameters.AddWithValue("@Dr_amt", trans_detail.Amount);
                            }
                            cmdDetail.Parameters.AddWithValue("@Cust_id", trans_detail.Cust_id);
                            cmdDetail.Parameters.AddWithValue("@Narr", trans_detail.Narr);
                            cmdDetail.Parameters.AddWithValue("@Cust_name", trans_detail.Cust_name);
                            cmdDetail.Parameters.AddWithValue("@text", 2);

                            cmdDetail.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        db.Disconnect();
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/DelVoucher")]
        [HttpPost]
        public HttpResponseMessage DeleteVoucher([FromBody]TRANS trans)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(trans.Modified_by))
                {
                    db.Execute("update TRANS set Status=0 where Trans_id = " + trans.Trans_id);
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted");
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

        #region Trans Detail

        [Route("api/VoucherDetail")]
        public HttpResponseMessage GetVoucherDetail(int Trans_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select Trans_id, Trans_no, Trans_date, Amount, Trans_type_id, TYPE_NAME, L_id, Ledger_no, Ledger_name, Cust_id, Cust_name, CrDr_id from VIEW_TRANS where Status=1 and Trans_id = " + Trans_id);
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion
    }
}
