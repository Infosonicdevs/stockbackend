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
    public class PurchaseTransactionController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/PurchaseTransaction")]
        public HttpResponseMessage GetPurchase()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_PURCHASE");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public List<BazaSetting> GetBazarSetting()
        {
            try
            {
                var result = db.GetTable("select Pur_id, Round_Off_id, Hamali_id, Commi_id, Transport_id, Ma_ses_id, Tcs_id, Transfer_id, Net_Disc_id from VIEW_BAZAR_SETTING");

                var list = new List<BazaSetting>();

                foreach (DataRow row in result.Rows)
                {
                    list.Add(new BazaSetting
                    {
                        Pur_id = Convert.ToDecimal(row["Pur_id"]),
                        Round_Off_id = Convert.ToDecimal(row["Round_Off_id"]),
                        Hamali_id = Convert.ToDecimal(row["Hamali_id"]),
                        Commi_id = Convert.ToDecimal(row["Commi_id"]),
                        Transport_id = Convert.ToDecimal(row["Transport_id"]),
                        Ma_ses_id = Convert.ToDecimal(row["Ma_ses_id"]),
                        Tcs_id = Convert.ToDecimal(row["Tcs_id"]),
                        Net_Disc_id = Convert.ToDecimal(row["Net_Disc_id"]),
                        Transfer_id = Convert.ToDecimal(row["Transfer_id"])
                    });
                }

                return list;
            }
            catch
            {
                return new List<BazaSetting>();
            }
        }


        [Route("api/PurchaseTransaction")]
        [HttpPost]
        public HttpResponseMessage PostPurchaseTransaction([FromBody]PURCHASE_Transaction request)
        {
            try
            {
                db.Connect();
                if (db.IsValidUser(request.Created_by))
                {
                    if (request == null)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Purchase Transaction is required");
                    }
                    else if (request.PURCHASE_DETAILS == null || request.PURCHASE_DETAILS.Count == 0)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Purchase Transaction Details are required");
                    }
                    else
                    {
                        int Invoice_id = 0;
                        List<BazaSetting> bazar_setting_id = GetBazarSetting();
                        var gst_slab_result = db.GetTable("select top 1 * from VIEW_GST_SLAB order by Id desc");
                        var gst_slab_details_all = db.GetTable("select VIEW_GST_SLAB.CGST_per, VIEW_GST_SLAB.SGST_per, VIEW_GST_SLAB.IGST_per, VIEW_STOCK_DETAILS.Stock_id from VIEW_GST_SLAB left join VIEW_STOCK_DETAILS ON VIEW_GST_SLAB.Id = VIEW_STOCK_DETAILS.Slab_id");

                        using (SqlTransaction transaction = db.cn.BeginTransaction())
                        {
                            try
                            {
                                if (bazar_setting_id.Count > 0 && gst_slab_result.Rows.Count > 0)
                                {
                                    SqlCommand cmd = new SqlCommand("Sp_Bazar_Purchase", db.cn, transaction);

                                    //Purchase
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@Invoice_date", Convert.ToDateTime(request.Invoice_date.ToString("MM-dd-yyyy")));
                                    cmd.Parameters.AddWithValue("@Bill_date", Convert.ToDateTime(request.Bill_date.ToString("MM-dd-yyyy")));
                                    cmd.Parameters.AddWithValue("@Vend_id", request.Vend_id);
                                    cmd.Parameters.AddWithValue("@Roundoff", request.RoundOFF);
                                    cmd.Parameters.AddWithValue("@Roundoff_id", bazar_setting_id[0].Round_Off_id);
                                    cmd.Parameters.AddWithValue("@Final_amt", request.Final_amt);
                                    cmd.Parameters.AddWithValue("@Parvana_no", request.Parvana_no);
                                    cmd.Parameters.AddWithValue("@Truck_no", request.Truck_no);
                                    cmd.Parameters.AddWithValue("@Commi_amt", request.Commi_amt);
                                    cmd.Parameters.AddWithValue("@Commi_id", bazar_setting_id[0].Commi_id);
                                    cmd.Parameters.AddWithValue("@Diff_amt", request.Diff_amt);
                                    cmd.Parameters.AddWithValue("@State_id", request.State_id);
                                    cmd.Parameters.AddWithValue("@Hamali", request.Hamali);
                                    cmd.Parameters.AddWithValue("@Hamali_id", bazar_setting_id[0].Hamali_id);
                                    cmd.Parameters.AddWithValue("@Transport_amt", request.Transport_amt);
                                    cmd.Parameters.AddWithValue("@Transport_id", bazar_setting_id[0].Transport_id);
                                    cmd.Parameters.AddWithValue("@Ma_ses_amt", request.Ma_ses_amt);
                                    cmd.Parameters.AddWithValue("@Ma_ses_id", bazar_setting_id[0].Ma_ses_id);
                                    cmd.Parameters.AddWithValue("@Tcs_amt", request.TCS_amt);
                                    cmd.Parameters.AddWithValue("@Tcs_id", bazar_setting_id[0].Tcs_id);
                                    cmd.Parameters.AddWithValue("@Credit_note", request.Credit_note);
                                    cmd.Parameters.AddWithValue("@Credit_note_id", bazar_setting_id[0].Pur_id);
                                    cmd.Parameters.AddWithValue("@Pavati_no", request.Pavati_no);
                                    cmd.Parameters.AddWithValue("@invoice_no", request.Invoice_no);
                                    cmd.Parameters.AddWithValue("@net_disc", request.Net_disc);
                                    cmd.Parameters.AddWithValue("@txt", 1);

                                    // Trans
                                    cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                                    cmd.Parameters.AddWithValue("@Trans_type_id", request.Trans_type_id);
                                    cmd.Parameters.AddWithValue("@Trans_code", request.trans_code);

                                    //Trans Details
                                    cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                                    cmd.Parameters.AddWithValue("@Pur_L_id", bazar_setting_id[0].Pur_id);
                                    cmd.Parameters.AddWithValue("@Amount", request.Amount);
                                    cmd.Parameters.AddWithValue("@CGST_id", gst_slab_result.Rows[0]["CGST_l_id"]);
                                    cmd.Parameters.AddWithValue("@CGST_amt", request.CGST_amt);
                                    cmd.Parameters.AddWithValue("@SGST_id", gst_slab_result.Rows[0]["SGST_l_id"]);
                                    cmd.Parameters.AddWithValue("@SGST_amt", request.SGST_amt);
                                    cmd.Parameters.AddWithValue("@IGST_id", gst_slab_result.Rows[0]["IGST_l_id"]);
                                    cmd.Parameters.AddWithValue("@IGST_amt", request.IGST_amt);
                                    cmd.Parameters.AddWithValue("@User", request.Created_by);
                                    cmd.Parameters.AddWithValue("@Status", '1');
                                    cmd.Parameters.AddWithValue("@Narr", "Stock Purchase");
                                    cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);
                                    cmd.Parameters.AddWithValue("@Card_no", request.Card_no);
                                    cmd.Parameters.AddWithValue("@Invert", 'G');
                                    cmd.Parameters.AddWithValue("@Outlet_id", request.Outlet_id);
                                    cmd.Parameters.AddWithValue("@Net_dis_l_id ", bazar_setting_id[0].Net_Disc_id);
                                    if (request.CashTrans == 'T')
                                    {
                                        cmd.Parameters.AddWithValue("@Transfer_id", gst_slab_result.Rows[0]["Transfer_id"]);
                                    }
                                    else if (request.CashTrans == 'C')
                                    {
                                        cmd.Parameters.AddWithValue("@Transfer_id", 0);
                                    }
                                    SqlParameter par_invoice_id = new SqlParameter("@Invoice_id", System.Data.SqlDbType.Int); // Define parameter
                                    par_invoice_id.Direction = System.Data.ParameterDirection.Output;   // Set direction of parameter to output
                                    cmd.Parameters.Add(par_invoice_id);
                                    cmd.ExecuteNonQuery();

                                    Invoice_id = Convert.ToInt32(cmd.Parameters["@Invoice_id"].Value);
                                }


                                foreach (var purchase in request.PURCHASE_DETAILS)
                                {
                                    var gst = gst_slab_details_all.AsEnumerable()
                                            .FirstOrDefault(r => r.Field<int>("Stock_id") == purchase.Stock_id);

                                    if (gst != null)
                                    {
                                        SqlCommand cmd = new SqlCommand("Sp_Bazar_Pur_Details", db.cn, transaction);

                                        //Purchase Detail
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.AddWithValue("@Invoice_id", Invoice_id);
                                        cmd.Parameters.AddWithValue("@Stock_id", purchase.Stock_id);
                                        cmd.Parameters.AddWithValue("@Price", purchase.Price);
                                        cmd.Parameters.AddWithValue("@Quantity", purchase.Quantity);
                                        cmd.Parameters.AddWithValue("@CGST_per", Convert.ToDecimal(gst["CGST_per"]));
                                        cmd.Parameters.AddWithValue("@SGST_per", Convert.ToDecimal(gst["SGST_per"]));
                                        cmd.Parameters.AddWithValue("@IGST_per", Convert.ToDecimal(gst["IGST_per"]));
                                        cmd.Parameters.AddWithValue("@CGST_amt", purchase.CGST_amt);
                                        cmd.Parameters.AddWithValue("@SGST_amt", purchase.SGST_amt);
                                        cmd.Parameters.AddWithValue("@IGST_amt", purchase.IGST_amt);
                                        cmd.Parameters.AddWithValue("@Disc_amt", purchase.Disc_amt);
                                        cmd.Parameters.AddWithValue("@Mrp", purchase.MRP);
                                        cmd.Parameters.AddWithValue("@Total", purchase.Total);
                                        cmd.Parameters.AddWithValue("@Mode", purchase.Mode);

                                        //GS_INWARD 
                                        cmd.Parameters.AddWithValue("@Date", Convert.ToDateTime(request.Invoice_date.ToString("MM-dd-yyyy")));
                                        cmd.Parameters.AddWithValue("@Amount", purchase.Total);
                                        cmd.Parameters.AddWithValue("@Is_new", '1');
                                        cmd.Parameters.AddWithValue("@Return_id", 0);

                                        cmd.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        throw new Exception($"GST slab not found for Stock_id {purchase.Stock_id}");
                                    }
                                }                              
                                transaction.Commit();
                                db.Disconnect();
                                return Request.CreateResponse(HttpStatusCode.OK, "Purchase Transaction Record Inserted");
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                db.Disconnect();
                                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                            }
                        }

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



        [Route("api/PurchaseTransaction")]
        [HttpPut]
        public HttpResponseMessage PutPurchaseTransaction([FromBody] PURCHASE_Transaction request)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(request.Modified_by))
                {
                    if (request == null)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Purchase Transaction is required");
                    }
                    else if (request.PURCHASE_DETAILS == null || request.PURCHASE_DETAILS.Count == 0)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Purchase Transaction Details are required");
                    }
                    else
                    {
                        int Invoice_id = 0;
                        List<BazaSetting> bazar_setting_id = GetBazarSetting();
                        var gst_slab_result = db.GetTable("select top 1 * from VIEW_GST_SLAB order by Id desc");
                        var gst_slab_details_all = db.GetTable("select VIEW_GST_SLAB.CGST_per, VIEW_GST_SLAB.SGST_per, VIEW_GST_SLAB.IGST_per, VIEW_STOCK_DETAILS.Stock_id from VIEW_GST_SLAB left join VIEW_STOCK_DETAILS ON VIEW_GST_SLAB.Id = VIEW_STOCK_DETAILS.Slab_id");

                        using (SqlTransaction transaction = db.cn.BeginTransaction())
                        {
                            try
                            {
                                if (bazar_setting_id.Count > 0 && gst_slab_result.Rows.Count > 0)
                                {
                                    SqlCommand cmd = new SqlCommand("Sp_Bazar_Purchase", db.cn, transaction);

                                    //Purchase
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@update_invoice_id", request.Invoice_id);
                                    cmd.Parameters.AddWithValue("@Invoice_date", Convert.ToDateTime(request.Invoice_date.ToString("MM-dd-yyyy")));
                                    cmd.Parameters.AddWithValue("@Bill_date", Convert.ToDateTime(request.Bill_date.ToString("MM-dd-yyyy")));
                                    cmd.Parameters.AddWithValue("@Vend_id", request.Vend_id);
                                    cmd.Parameters.AddWithValue("@Roundoff", request.RoundOFF);
                                    cmd.Parameters.AddWithValue("@Roundoff_id", bazar_setting_id[0].Round_Off_id);
                                    cmd.Parameters.AddWithValue("@Final_amt", request.Final_amt);
                                    cmd.Parameters.AddWithValue("@Parvana_no", request.Parvana_no);
                                    cmd.Parameters.AddWithValue("@Truck_no", request.Truck_no);
                                    cmd.Parameters.AddWithValue("@Commi_amt", request.Commi_amt);
                                    cmd.Parameters.AddWithValue("@Commi_id", bazar_setting_id[0].Commi_id);
                                    cmd.Parameters.AddWithValue("@Diff_amt", request.Diff_amt);
                                    cmd.Parameters.AddWithValue("@State_id", request.State_id);
                                    cmd.Parameters.AddWithValue("@Hamali", request.Hamali);
                                    cmd.Parameters.AddWithValue("@Hamali_id", bazar_setting_id[0].Hamali_id);
                                    cmd.Parameters.AddWithValue("@Transport_amt", request.Transport_amt);
                                    cmd.Parameters.AddWithValue("@Transport_id", bazar_setting_id[0].Transport_id);
                                    cmd.Parameters.AddWithValue("@Ma_ses_amt", request.Ma_ses_amt);
                                    cmd.Parameters.AddWithValue("@Ma_ses_id", bazar_setting_id[0].Ma_ses_id);
                                    cmd.Parameters.AddWithValue("@Tcs_amt", request.TCS_amt);
                                    cmd.Parameters.AddWithValue("@Tcs_id", bazar_setting_id[0].Tcs_id);
                                    cmd.Parameters.AddWithValue("@Credit_note", request.Credit_note);
                                    cmd.Parameters.AddWithValue("@Credit_note_id", bazar_setting_id[0].Pur_id);
                                    cmd.Parameters.AddWithValue("@Pavati_no", request.Pavati_no);
                                    cmd.Parameters.AddWithValue("@invoice_no", request.Invoice_no);
                                    cmd.Parameters.AddWithValue("@net_disc", request.Net_disc);
                                    cmd.Parameters.AddWithValue("@txt", 2);

                                    // Trans
                                    cmd.Parameters.AddWithValue("@update_trans_id", request.Trans_id);
                                    cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                                    cmd.Parameters.AddWithValue("@Trans_type_id", request.Trans_type_id);
                                    cmd.Parameters.AddWithValue("@Trans_code", request.trans_code);
                                    cmd.Parameters.AddWithValue("@Modify_reason", request.Modify_reason);


                                    //Trans Details
                                    cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                                    cmd.Parameters.AddWithValue("@Pur_L_id", bazar_setting_id[0].Pur_id);
                                    cmd.Parameters.AddWithValue("@Amount", request.Amount);
                                    cmd.Parameters.AddWithValue("@CGST_id", gst_slab_result.Rows[0]["CGST_l_id"]);
                                    cmd.Parameters.AddWithValue("@CGST_amt", request.CGST_amt);
                                    cmd.Parameters.AddWithValue("@SGST_id", gst_slab_result.Rows[0]["SGST_l_id"]);
                                    cmd.Parameters.AddWithValue("@SGST_amt", request.SGST_amt);
                                    cmd.Parameters.AddWithValue("@IGST_id", gst_slab_result.Rows[0]["IGST_l_id"]);
                                    cmd.Parameters.AddWithValue("@IGST_amt", request.IGST_amt);
                                    cmd.Parameters.AddWithValue("@User", request.Modified_by);
                                    cmd.Parameters.AddWithValue("@Status", '1');
                                    cmd.Parameters.AddWithValue("@Narr", "Stock Purchase");
                                    cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);
                                    cmd.Parameters.AddWithValue("@Card_no", request.Card_no);
                                    cmd.Parameters.AddWithValue("@Invert", 'G');
                                    cmd.Parameters.AddWithValue("@Outlet_id", request.Outlet_id);
                                    cmd.Parameters.AddWithValue("@Net_dis_l_id ", bazar_setting_id[0].Net_Disc_id);
                                    if (request.CashTrans == 'T')
                                    {
                                        cmd.Parameters.AddWithValue("@Transfer_id", gst_slab_result.Rows[0]["Transfer_id"]);
                                    }
                                    else if (request.CashTrans == 'C')
                                    {
                                        cmd.Parameters.AddWithValue("@Transfer_id", 0);
                                    }
                                    SqlParameter par_invoice_id = new SqlParameter("@Invoice_id", System.Data.SqlDbType.Int); // Define parameter
                                    par_invoice_id.Direction = System.Data.ParameterDirection.Output;   // Set direction of parameter to output
                                    cmd.Parameters.Add(par_invoice_id);
                                    cmd.ExecuteNonQuery();

                                    Invoice_id = Convert.ToInt32(cmd.Parameters["@Invoice_id"].Value);
                                }

                                if (Invoice_id > 0)
                                {
                                    SqlCommand cmdDelete = new SqlCommand(
                                          "DELETE from PURCHASE_DETAILS WHERE Invoice_id = @Invoice_id",
                                          db.cn,
                                          transaction
                                      );

                                    cmdDelete.Parameters.AddWithValue("@Invoice_id", Invoice_id);
                                    cmdDelete.ExecuteNonQuery();
                                }

                                foreach (var purchase in request.PURCHASE_DETAILS)
                                {
                                    var gst = gst_slab_details_all.AsEnumerable()
                                            .FirstOrDefault(r => r.Field<int>("Stock_id") == purchase.Stock_id);

                                    if (gst != null)
                                    {
                                        SqlCommand cmd = new SqlCommand("Sp_Bazar_Pur_Details", db.cn, transaction);

                                        //Purchase Detail
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.AddWithValue("@Invoice_id", Invoice_id);
                                        cmd.Parameters.AddWithValue("@Stock_id", purchase.Stock_id);
                                        cmd.Parameters.AddWithValue("@Price", purchase.Price);
                                        cmd.Parameters.AddWithValue("@Quantity", purchase.Quantity);
                                        cmd.Parameters.AddWithValue("@CGST_per", Convert.ToDecimal(gst["CGST_per"]));
                                        cmd.Parameters.AddWithValue("@SGST_per", Convert.ToDecimal(gst["SGST_per"]));
                                        cmd.Parameters.AddWithValue("@IGST_per", Convert.ToDecimal(gst["IGST_per"]));
                                        cmd.Parameters.AddWithValue("@CGST_amt", purchase.CGST_amt);
                                        cmd.Parameters.AddWithValue("@SGST_amt", purchase.SGST_amt);
                                        cmd.Parameters.AddWithValue("@IGST_amt", purchase.IGST_amt);
                                        cmd.Parameters.AddWithValue("@Disc_amt", purchase.Disc_amt);
                                        cmd.Parameters.AddWithValue("@Mrp", purchase.MRP);
                                        cmd.Parameters.AddWithValue("@Total", purchase.Total);
                                        cmd.Parameters.AddWithValue("@Mode", purchase.Mode);

                                        //GS_INWARD 
                                        cmd.Parameters.AddWithValue("@Date", Convert.ToDateTime(request.Invoice_date.ToString("MM-dd-yyyy")));
                                        cmd.Parameters.AddWithValue("@Amount", purchase.Total);
                                        cmd.Parameters.AddWithValue("@Is_new", '1');
                                        cmd.Parameters.AddWithValue("@Return_id", 0);

                                        cmd.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        throw new Exception($"GST slab not found for Stock_id {purchase.Stock_id}");
                                    }
                                }                             
                                transaction.Commit();
                                db.Disconnect();
                                return Request.CreateResponse(HttpStatusCode.OK, "Purchase Transaction Record Updated");
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                db.Disconnect();
                                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                            }
                        }

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
