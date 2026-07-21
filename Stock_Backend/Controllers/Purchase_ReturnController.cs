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

                if (!db.IsValidUser(request.Created_by))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");

                if (request == null || request.DETAILS.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Data");

                SqlCommand cmdSetting = new SqlCommand("SELECT Ledger_id FROM LEDGER_SETTING WHERE Purpose = 'Purchase Return'", db.cn);
                object purL = cmdSetting.ExecuteScalar();
                if (purL == null || purL == DBNull.Value)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Purchase Return account not found in Ledger Setting");
                }
                int Pur_L_id = Convert.ToInt32(purL);

                SqlCommand cmdRound = new SqlCommand("SELECT L_id FROM Bazar_Settg WHERE Purpose='Round off account'", db.cn);
                int Round_Off_id = Convert.ToInt32(cmdRound.ExecuteScalar());

                SqlCommand cmdNetDisc = new SqlCommand("SELECT L_id FROM Bazar_Settg WHERE Purpose='Net discount'", db.cn);
                int Net_Disc_id = Convert.ToInt32(cmdNetDisc.ExecuteScalar());
                var gst_slab_result = db.GetTable("select top 1 * from VIEW_GST_SLAB order by Id desc");
                var gst_slab_details_all = db.GetTable("select VIEW_GST_SLAB.CGST_per, VIEW_GST_SLAB.SGST_per, VIEW_GST_SLAB.IGST_per, VIEW_STOCK_DETAILS.Stock_id from VIEW_GST_SLAB left join VIEW_STOCK_DETAILS ON VIEW_GST_SLAB.Id = VIEW_STOCK_DETAILS.Slab_id");

                int Transfer_ledger = 0;
                int Return_id = 0;

                if (request.CashTrans == 'T')
                {
                    string query = "SELECT Transfer_Ledger FROM OUTLET WHERE Outlet_id=" + request.Outlet_id;
                    SqlCommand cmdTransfer = new SqlCommand(query, db.cn);

                    var transfer = cmdTransfer.ExecuteScalar();

                    if (transfer == null || transfer == DBNull.Value)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Transfer account not found in Ledger Setting");
                    }
                    else
                    {
                        Transfer_ledger = Convert.ToInt32(transfer);
                    }
                }

                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_pur_Return", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        // MAIN
                        cmd.Parameters.AddWithValue("@Outlet_id", request.Outlet_id);
                        cmd.Parameters.AddWithValue("@Return_date", request.Return_date);
                        cmd.Parameters.AddWithValue("@Invoice_id", request.Invoice_id);
                        cmd.Parameters.AddWithValue("@Pavati_no", "");
                        cmd.Parameters.AddWithValue("@Total_quantity", request.Total_quantity);
                        cmd.Parameters.AddWithValue("@Total", request.Total);
                        cmd.Parameters.AddWithValue("@Total_CGST", request.Total_CGST);
                        cmd.Parameters.AddWithValue("@CGST_L_id", gst_slab_result.Rows[0]["CGST_l_id"]);
                        cmd.Parameters.AddWithValue("@Total_SGST", request.Total_SGST);
                        cmd.Parameters.AddWithValue("@SGST_L_id", gst_slab_result.Rows[0]["SGST_l_id"]);
                        cmd.Parameters.AddWithValue("@Total_IGST", request.Total_IGST);
                        cmd.Parameters.AddWithValue("@IGST_L_id", gst_slab_result.Rows[0]["IGST_l_id"]);
                        cmd.Parameters.AddWithValue("@Total_Disc", request.Total_Disc);
                        cmd.Parameters.AddWithValue("@Total_amt", request.Total_amt);
                        cmd.Parameters.AddWithValue("@Round_off", request.Round_off);
                        cmd.Parameters.AddWithValue("@Roundoff_id", Round_Off_id);
                        cmd.Parameters.AddWithValue("@Bill_amt", request.Bill_amt);
                        cmd.Parameters.AddWithValue("@User", request.Created_by);
                        cmd.Parameters.AddWithValue("@Purchase_id", Pur_L_id);
                        cmd.Parameters.AddWithValue("@net_disc", request.net_disc);
                        cmd.Parameters.AddWithValue("@net_disc_id", Net_Disc_id);
                        cmd.Parameters.AddWithValue("@txt", 1);

                        // TRANS
                        cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                        cmd.Parameters.AddWithValue("@Trans_type_id", 3);
                        cmd.Parameters.AddWithValue("@trans_code", request.trans_code);

                        // TRANS DETAILS
                        cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);
                        cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                        cmd.Parameters.AddWithValue("@Card_no", request.Card_no);
                        cmd.Parameters.AddWithValue("@Status", '1');
                        cmd.Parameters.AddWithValue("@L_id", Transfer_ledger);
                        cmd.Parameters.AddWithValue("@Narr", "Purchase Return");

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
                            var gst = gst_slab_details_all.AsEnumerable()
                                            .FirstOrDefault(r => r.Field<int>("Stock_id") == item.Stock_id);
                            if (gst != null) 
                            {
                                SqlCommand cmdDet = new SqlCommand("Sp_Bazar_Pur_Rtn_Details", db.cn, transaction);
                                cmdDet.CommandType = CommandType.StoredProcedure;

                                cmdDet.Parameters.AddWithValue("@Invoice_id", request.Invoice_id);
                                cmdDet.Parameters.AddWithValue("@Stock_id", item.Stock_id);
                                cmdDet.Parameters.AddWithValue("@Price", item.Price);
                                cmdDet.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmdDet.Parameters.AddWithValue("@CGST_per", Convert.ToDecimal(gst["CGST_per"]));
                                cmdDet.Parameters.AddWithValue("@SGST_per", Convert.ToDecimal(gst["SGST_per"]));
                                cmdDet.Parameters.AddWithValue("@IGST_per", Convert.ToDecimal(gst["IGST_per"]));
                                cmdDet.Parameters.AddWithValue("@CGST_amt", item.CGST_amt);
                                cmdDet.Parameters.AddWithValue("@SGST_amt", item.SGST_amt);
                                cmdDet.Parameters.AddWithValue("@IGST_amt", item.IGST_amt);
                                cmdDet.Parameters.AddWithValue("@Disc_amt", item.Disc_amt);
                                cmdDet.Parameters.AddWithValue("@Mrp", item.Mrp);
                                cmdDet.Parameters.AddWithValue("@Total", item.Total);

                                cmdDet.Parameters.AddWithValue("@Date", request.Return_date);
                                cmdDet.Parameters.AddWithValue("@Amount", item.Total);
                                cmdDet.Parameters.AddWithValue("@Is_new", '2'); // RETURN
                                cmdDet.Parameters.AddWithValue("@Return_id", Return_id);

                                cmdDet.ExecuteNonQuery();
                            }
                            else
                            {
                                throw new Exception($"GST slab not found for Stock_id {item.Stock_id}");
                            }
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

                if (!db.IsValidUser(request.Created_by))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");

                if (request == null || request.DETAILS.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Data");

                SqlCommand cmdSetting = new SqlCommand("SELECT Ledger_id FROM LEDGER_SETTING WHERE Purpose = 'Purchase Return'", db.cn);
                object purL = cmdSetting.ExecuteScalar();
                if (purL == null || purL == DBNull.Value)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Purchase Return account not found in Ledger Setting");
                }
                int Pur_L_id = Convert.ToInt32(purL);

                SqlCommand cmdRound = new SqlCommand("SELECT L_id FROM Bazar_Settg WHERE Purpose='Round off account'", db.cn);
                int Round_Off_id = Convert.ToInt32(cmdRound.ExecuteScalar());

                SqlCommand cmdNetDisc = new SqlCommand("SELECT L_id FROM Bazar_Settg WHERE Purpose='Net discount'", db.cn);
                int Net_Disc_id = Convert.ToInt32(cmdNetDisc.ExecuteScalar());
                var gst_slab_result = db.GetTable("select top 1 * from VIEW_GST_SLAB order by Id desc");
                var gst_slab_details_all = db.GetTable("select VIEW_GST_SLAB.CGST_per, VIEW_GST_SLAB.SGST_per, VIEW_GST_SLAB.IGST_per, VIEW_STOCK_DETAILS.Stock_id from VIEW_GST_SLAB left join VIEW_STOCK_DETAILS ON VIEW_GST_SLAB.Id = VIEW_STOCK_DETAILS.Slab_id");

                int Transfer_ledger = 0;
                int Return_id = 0;

                if (request.CashTrans == 'T')
                {
                    string query = "SELECT Transfer_Ledger FROM OUTLET WHERE Outlet_id=" + request.Outlet_id;
                    SqlCommand cmdTransfer = new SqlCommand(query, db.cn);

                    var transfer = cmdTransfer.ExecuteScalar();

                    if (transfer == null || transfer == DBNull.Value)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Transfer account not found in Ledger Setting");
                    }
                    else
                    {
                        Transfer_ledger = Convert.ToInt32(transfer);
                    }
                }

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
                        cmd.Parameters.AddWithValue("@Outlet_id", request.Outlet_id);
                        cmd.Parameters.AddWithValue("@Return_date", request.Return_date);
                        cmd.Parameters.AddWithValue("@Invoice_id", request.Invoice_id);
                        cmd.Parameters.AddWithValue("@Pavati_no", "");
                        cmd.Parameters.AddWithValue("@Total_quantity", request.Total_quantity);
                        cmd.Parameters.AddWithValue("@Total", request.Total);
                        cmd.Parameters.AddWithValue("@Total_CGST", request.Total_CGST);
                        cmd.Parameters.AddWithValue("@CGST_L_id", gst_slab_result.Rows[0]["CGST_l_id"]);
                        cmd.Parameters.AddWithValue("@Total_SGST", request.Total_SGST);
                        cmd.Parameters.AddWithValue("@SGST_L_id", gst_slab_result.Rows[0]["SGST_l_id"]);
                        cmd.Parameters.AddWithValue("@Total_IGST", request.Total_IGST);
                        cmd.Parameters.AddWithValue("@IGST_L_id", gst_slab_result.Rows[0]["IGST_l_id"]);
                        cmd.Parameters.AddWithValue("@Total_Disc", request.Total_Disc);
                        cmd.Parameters.AddWithValue("@Total_amt", request.Total_amt);
                        cmd.Parameters.AddWithValue("@Round_off", request.Round_off);
                        cmd.Parameters.AddWithValue("@Roundoff_id", Round_Off_id);
                        cmd.Parameters.AddWithValue("@Bill_amt", request.Bill_amt);
                        cmd.Parameters.AddWithValue("@User", request.Created_by);
                        cmd.Parameters.AddWithValue("@Purchase_id", Pur_L_id);
                        cmd.Parameters.AddWithValue("@net_disc", request.net_disc);
                        cmd.Parameters.AddWithValue("@net_disc_id", Net_Disc_id);

                        //  TRANS
                        cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                        cmd.Parameters.AddWithValue("@Trans_type_id", 3);
                        cmd.Parameters.AddWithValue("@trans_code", request.trans_code);
                        cmd.Parameters.AddWithValue("@Modify_reason", request.Modify_reason ?? (object)DBNull.Value);

                        //  TRANS DETAILS
                        cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);
                        cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                        cmd.Parameters.AddWithValue("@Card_no", request.Card_no ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", '1');
                        cmd.Parameters.AddWithValue("@L_id", Transfer_ledger);
                        cmd.Parameters.AddWithValue("@Narr", "Purchase Return");

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
                            "DELETE FROM PURCHASE_RETURN_DETAILS WHERE return_id = @Return_id",
                            db.cn,
                            transaction
                        );
                        cmdDelete.Parameters.AddWithValue("@Return_id", Return_id);
                        cmdDelete.ExecuteNonQuery();

                        //  INSERT NEW DETAILS
                        foreach (var item in request.DETAILS)
                        {
                            var gst = gst_slab_details_all.AsEnumerable()
                                            .FirstOrDefault(r => r.Field<int>("Stock_id") == item.Stock_id);
                            if (gst != null)
                            {
                                SqlCommand cmdDet = new SqlCommand("Sp_Bazar_Pur_Rtn_Details", db.cn, transaction);
                                cmdDet.CommandType = CommandType.StoredProcedure;

                                cmdDet.Parameters.AddWithValue("@Invoice_id", request.Invoice_id);
                                cmdDet.Parameters.AddWithValue("@Stock_id", item.Stock_id);
                                cmdDet.Parameters.AddWithValue("@Price", item.Price);
                                cmdDet.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmdDet.Parameters.AddWithValue("@CGST_per", Convert.ToDecimal(gst["CGST_per"]));
                                cmdDet.Parameters.AddWithValue("@SGST_per", Convert.ToDecimal(gst["SGST_per"]));
                                cmdDet.Parameters.AddWithValue("@IGST_per", Convert.ToDecimal(gst["IGST_per"]));
                                cmdDet.Parameters.AddWithValue("@CGST_amt", item.CGST_amt);
                                cmdDet.Parameters.AddWithValue("@SGST_amt", item.SGST_amt);
                                cmdDet.Parameters.AddWithValue("@IGST_amt", item.IGST_amt);
                                cmdDet.Parameters.AddWithValue("@Disc_amt", item.Disc_amt);
                                cmdDet.Parameters.AddWithValue("@Mrp", item.Mrp);
                                cmdDet.Parameters.AddWithValue("@Total", item.Total);

                                cmdDet.Parameters.AddWithValue("@Date", request.Return_date);
                                cmdDet.Parameters.AddWithValue("@Amount", item.Total);
                                cmdDet.Parameters.AddWithValue("@Is_new", '2'); // RETURN
                                cmdDet.Parameters.AddWithValue("@Return_id", Return_id);

                                cmdDet.ExecuteNonQuery();
                            }
                            else
                            {
                                throw new Exception($"GST slab not found for Stock_id {item.Stock_id}");
                            }
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
