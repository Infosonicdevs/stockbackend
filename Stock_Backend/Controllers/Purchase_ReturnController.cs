using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class Purchase_ReturnController : ApiController
    {

        DbClass db = new DbClass();

        [Route("api/Purchaseretun")]
        public HttpResponseMessage GetSale()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from PURCHASE_RETURN where Status = 1 ");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }




        [Route("api/Purchasereturn")]
        [HttpPost]
        public HttpResponseMessage PostPurchaseReturn([FromBody] PurchaseReturnModel request)
        {
            try
            {
                db.Connect();

                if (!db.IsValidUser(request.User))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");

                if (request == null || request.DETAILS.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Data");

                int Return_id = 0;

                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_pur_Return", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        // MAIN
                        cmd.Parameters.AddWithValue("@Return_date", request.Return_date);
                        cmd.Parameters.AddWithValue("@Invoice_id", request.Invoice_id);
                        cmd.Parameters.AddWithValue("@Pavati_no", request.Pavati_no);
                        cmd.Parameters.AddWithValue("@Total_quantity", request.Total_quantity);
                        cmd.Parameters.AddWithValue("@Total", request.Total);
                        cmd.Parameters.AddWithValue("@Total_CGST", request.Total_CGST);
                        cmd.Parameters.AddWithValue("@CGST_L_id", request.CGST_L_id);
                        cmd.Parameters.AddWithValue("@Total_SGST", request.Total_SGST);
                        cmd.Parameters.AddWithValue("@SGST_L_id", request.SGST_L_id);
                        cmd.Parameters.AddWithValue("@Total_IGST", request.Total_IGST);
                        cmd.Parameters.AddWithValue("@IGST_L_id", request.IGST_L_id);
                        cmd.Parameters.AddWithValue("@Total_Disc", request.Total_Disc);
                        cmd.Parameters.AddWithValue("@Total_amt", request.Total_amt);
                        cmd.Parameters.AddWithValue("@Round_off", request.Round_off);
                        cmd.Parameters.AddWithValue("@Roundoff_id", request.Roundoff_id);
                        cmd.Parameters.AddWithValue("@Bill_amt", request.Bill_amt);
                        cmd.Parameters.AddWithValue("@User", request.User);
                        cmd.Parameters.AddWithValue("@Purchase_id", request.Purchase_id);
                        cmd.Parameters.AddWithValue("@net_disc", request.net_disc);
                        cmd.Parameters.AddWithValue("@net_disc_id", request.net_disc_id);
                        cmd.Parameters.AddWithValue("@txt", 1);

                        // TRANS
                        cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                        cmd.Parameters.AddWithValue("@Trans_type_id", request.Trans_type_id);
                        cmd.Parameters.AddWithValue("@trans_code", request.trans_code);

                        // TRANS DETAILS
                        cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);
                        cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                        cmd.Parameters.AddWithValue("@Card_no", request.Card_no);
                        cmd.Parameters.AddWithValue("@Status", request.Status);
                        cmd.Parameters.AddWithValue("@L_id", request.L_id);
                        cmd.Parameters.AddWithValue("@Narr", request.Narr);

                        // OPTIONAL
                        cmd.Parameters.AddWithValue("@Bank_id", (object)request.Bank_id ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Cheque_no", (object)request.Cheque_no ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Cheque_date", (object)request.Cheque_date ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Effective_date", (object)request.Effective_date ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Serial_no", (object)request.Serial_no ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Reason", (object)request.Reason ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Cheque_Status", (object)request.Cheque_Status ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@BankOrUdhar", (object)request.BankOrUdhar ?? DBNull.Value);

                        SqlParameter outParam = new SqlParameter("@Return_id", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outParam);

                        cmd.ExecuteNonQuery();
                        Return_id = Convert.ToInt32(outParam.Value);

                        // DETAILS LOOP
                        foreach (var item in request.DETAILS)
                        {
                            SqlCommand cmdDet = new SqlCommand("Sp_Bazar_Pur_Details", db.cn, transaction);
                            cmdDet.CommandType = CommandType.StoredProcedure;

                            cmdDet.Parameters.AddWithValue("@Invoice_id", request.Invoice_id);
                            cmdDet.Parameters.AddWithValue("@Stock_id", item.Stock_id);
                            cmdDet.Parameters.AddWithValue("@Price", item.Price);
                            cmdDet.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmdDet.Parameters.AddWithValue("@CGST_per", item.CGST_per);
                            cmdDet.Parameters.AddWithValue("@SGST_per", item.SGST_per);
                            cmdDet.Parameters.AddWithValue("@IGST_per", item.IGST_per);
                            cmdDet.Parameters.AddWithValue("@CGST_amt", item.CGST_amt);
                            cmdDet.Parameters.AddWithValue("@SGST_amt", item.SGST_amt);
                            cmdDet.Parameters.AddWithValue("@IGST_amt", item.IGST_amt);
                            cmdDet.Parameters.AddWithValue("@Disc_amt", item.Disc_amt);
                            cmdDet.Parameters.AddWithValue("@Mrp", item.Mrp);
                            cmdDet.Parameters.AddWithValue("@Total", item.Total);

                            cmdDet.Parameters.AddWithValue("@Mode", '2'); // RETURN

                            cmdDet.Parameters.AddWithValue("@Date", request.Return_date);
                            cmdDet.Parameters.AddWithValue("@Amount", item.Total);
                            cmdDet.Parameters.AddWithValue("@Is_new", '2');
                            cmdDet.Parameters.AddWithValue("@Return_id", Return_id);

                            cmdDet.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Purchase Return Inserted");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Purchasereturn")]
        [HttpPut]
        public HttpResponseMessage PutPurchaseReturn([FromBody] PurchaseReturnModel request)
        {
            try
            {
                db.Connect();

                if (!db.IsAdmin(request.User))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");
                }

                if (request == null)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Purchase Return is required");
                }

                if (request.DETAILS == null || request.DETAILS.Count == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Return Details are required");
                }

                if (request.update_return_id == null || request.update_trans_id == null)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Update IDs missing");
                }

                int Return_id = 0;

                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_pur_Return", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@txt", 2);

                        // IDS
                        cmd.Parameters.AddWithValue("@update_return_id", request.update_return_id);
                        cmd.Parameters.AddWithValue("@update_trans_id", request.update_trans_id);

                        //  MAIN
                        cmd.Parameters.AddWithValue("@Return_date", request.Return_date);
                        cmd.Parameters.AddWithValue("@Invoice_id", request.Invoice_id);
                        cmd.Parameters.AddWithValue("@Pavati_no", request.Pavati_no);
                        cmd.Parameters.AddWithValue("@Total_quantity", request.Total_quantity);
                        cmd.Parameters.AddWithValue("@Total", request.Total);
                        cmd.Parameters.AddWithValue("@Total_CGST", request.Total_CGST);
                        cmd.Parameters.AddWithValue("@CGST_L_id", request.CGST_L_id);
                        cmd.Parameters.AddWithValue("@Total_SGST", request.Total_SGST);
                        cmd.Parameters.AddWithValue("@Total_IGST", request.Total_IGST);
                        cmd.Parameters.AddWithValue("@SGST_L_id", request.SGST_L_id);
                        cmd.Parameters.AddWithValue("@IGST_L_id", request.IGST_L_id);
                        cmd.Parameters.AddWithValue("@Total_Disc", request.Total_Disc);
                        cmd.Parameters.AddWithValue("@Total_amt", request.Total_amt);
                        cmd.Parameters.AddWithValue("@Round_off", request.Round_off);
                        cmd.Parameters.AddWithValue("@Roundoff_id", request.Roundoff_id);
                        cmd.Parameters.AddWithValue("@Bill_amt", request.Bill_amt);
                        cmd.Parameters.AddWithValue("@User", request.User);
                        cmd.Parameters.AddWithValue("@Purchase_id", request.Purchase_id);
                        cmd.Parameters.AddWithValue("@net_disc", request.net_disc);
                        cmd.Parameters.AddWithValue("@net_disc_id", request.net_disc_id);

                        //  TRANS
                        cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                        cmd.Parameters.AddWithValue("@Trans_type_id", request.Trans_type_id);
                        cmd.Parameters.AddWithValue("@trans_code", request.trans_code);
                        cmd.Parameters.AddWithValue("@Modify_reason", request.Modify_reason ?? (object)DBNull.Value);

                        //  TRANS DETAILS
                        cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);
                        cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                        cmd.Parameters.AddWithValue("@Card_no", request.Card_no ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", request.Status);
                        cmd.Parameters.AddWithValue("@L_id", request.L_id);
                        cmd.Parameters.AddWithValue("@Narr", request.Narr ?? "Purchase Return");

                        // CHEQUE
                        cmd.Parameters.AddWithValue("@Bank_id", request.Bank_id ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Cheque_no", request.Cheque_no ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Cheque_date", request.Cheque_date ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Effective_date", request.Effective_date ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Serial_no", request.Serial_no ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Reason", request.Reason ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Cheque_Status", request.Cheque_Status ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@BankOrUdhar", request.BankOrUdhar ?? (object)DBNull.Value);

                        // OUTPUT ONLY 
                        SqlParameter outParam = new SqlParameter("@Return_id", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outParam);

                        //  DEBUG LOG
                        System.Diagnostics.Debug.WriteLine("TXT: 2");
                        System.Diagnostics.Debug.WriteLine("Return ID: " + request.update_return_id);
                        System.Diagnostics.Debug.WriteLine("Trans ID: " + request.update_trans_id);

                        cmd.ExecuteNonQuery();

                        Return_id = Convert.ToInt32(outParam.Value);

                        //  DELETE OLD DETAILS
                        SqlCommand cmdDelete = new SqlCommand(
                            "DELETE FROM PURCHASE_DETAILS WHERE return_id = @Return_id",
                            db.cn,
                            transaction
                        );
                        cmdDelete.Parameters.AddWithValue("@Return_id", Return_id);
                        cmdDelete.ExecuteNonQuery();

                        //  INSERT NEW DETAILS
                        foreach (var item in request.DETAILS)
                        {
                            SqlCommand cmdDetail = new SqlCommand("Sp_Bazar_Pur_Details", db.cn, transaction);
                            cmdDetail.CommandType = CommandType.StoredProcedure;

                            cmdDetail.Parameters.AddWithValue("@Invoice_id", request.Invoice_id);
                            cmdDetail.Parameters.AddWithValue("@Stock_id", item.Stock_id);
                            cmdDetail.Parameters.AddWithValue("@Price", item.Price);
                            cmdDetail.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmdDetail.Parameters.AddWithValue("@CGST_per", item.CGST_per);
                            cmdDetail.Parameters.AddWithValue("@SGST_per", item.SGST_per);
                            cmdDetail.Parameters.AddWithValue("@IGST_per", item.IGST_per);
                            cmdDetail.Parameters.AddWithValue("@CGST_amt", item.CGST_amt);
                            cmdDetail.Parameters.AddWithValue("@SGST_amt", item.SGST_amt);
                            cmdDetail.Parameters.AddWithValue("@IGST_amt", item.IGST_amt);
                            cmdDetail.Parameters.AddWithValue("@Disc_amt", item.Disc_amt);
                            cmdDetail.Parameters.AddWithValue("@Mrp", item.Mrp);
                            cmdDetail.Parameters.AddWithValue("@Total", item.Total);

                            cmdDetail.Parameters.AddWithValue("@Mode", item.Mode);
                            cmdDetail.Parameters.AddWithValue("@Date", item.Date);
                            cmdDetail.Parameters.AddWithValue("@Amount", item.Amount);
                            cmdDetail.Parameters.AddWithValue("@Is_new", item.Is_new);
                            cmdDetail.Parameters.AddWithValue("@Return_id", Return_id);

                            cmdDetail.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        db.Disconnect();

                        return Request.CreateResponse(HttpStatusCode.OK, "Purchase Return Updated Successfully ");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/DelPurchasereturn")]
        [HttpPost]
        public HttpResponseMessage DeletePurchaseReturn([FromBody] dynamic data)
        {
            try
            {
                db.Connect();


                string user = Convert.ToString(data.User);

                if (!db.IsAdmin(user))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Only Admin Can Delete");
                }


                int Return_id = Convert.ToInt32(data.Return_id);
                int Trans_id = Convert.ToInt32(data.Trans_id);
                string Reason = Convert.ToString(data.Reason);

                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_pur_rtn_dlt", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Return_id", Return_id);
                        cmd.Parameters.AddWithValue("@Trans_id", Trans_id);
                        cmd.Parameters.AddWithValue("@Reason", Reason ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        db.Disconnect();

                        return Request.CreateResponse(HttpStatusCode.OK, "Purchase Return  Deleted Successfully");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}
