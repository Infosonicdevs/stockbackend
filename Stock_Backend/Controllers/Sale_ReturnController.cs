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
    public class Sale_ReturnController : ApiController
    {

        DbClass db = new DbClass();

        [Route("api/Salereturn")]
        public HttpResponseMessage GetSale()
        {
            try
            {
                db.Connect();
                var result = db.GetTable(@"SELECT    
                                            SR.*
                                        FROM SALE_RETURN SR
                                        WHERE SR.Status = 1");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Salereturn")]
        public HttpResponseMessage GetSale(int Sale_Rtn_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable(@"SELECT *
                                        FROM SALE_RETURN SR
                                        INNER JOIN SALE_RETURN_DETAILS SRD ON SRD.Return_id = SR.Sale_Rtn_id
                                        WHERE SR.Sale_Rtn_id = " + Sale_Rtn_id + " AND SR.Status = 1 ");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Salereturn")]
        [HttpPost]
        public HttpResponseMessage PostSaleReturn([FromBody] SaleReturnModel request)
        {
            try
            {
                db.Connect();

                if (!db.IsValidUser(request.Created_by))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");

                if (request == null || request.DETAILS.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Data");

                SqlCommand cmdSetting = new SqlCommand("SELECT Ledger_id FROM LEDGER_SETTING WHERE Purpose = 'Sale Return'", db.cn);
                object saleL = cmdSetting.ExecuteScalar();
                if (saleL == null || saleL == DBNull.Value)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sale Return account not found in Ledger Setting");
                }
                int Sale_l_id = Convert.ToInt32(saleL);

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
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_Sale_Return", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        // MAIN
                        cmd.Parameters.AddWithValue("@Outlet_id", request.Outlet_id);
                        cmd.Parameters.AddWithValue("@Return_date", request.Return_date);
                        cmd.Parameters.AddWithValue("@Sale_id", request.Sale_id);
                        cmd.Parameters.AddWithValue("@Total_quantity", request.Total_quantity);
                        cmd.Parameters.AddWithValue("@Total_Disc", request.Total_Disc);
                        cmd.Parameters.AddWithValue("@Total_amt", request.Total_amt);
                        cmd.Parameters.AddWithValue("@Round_off", request.Round_off);
                        cmd.Parameters.AddWithValue("@Roundoff_id", Round_Off_id);
                        cmd.Parameters.AddWithValue("@Bill_amt", request.Bill_amt);
                        cmd.Parameters.AddWithValue("@Sale_l_id", Sale_l_id);

                        cmd.Parameters.AddWithValue("@Total", request.Total);
                        cmd.Parameters.AddWithValue("@Total_CGST", request.Total_CGST);
                        cmd.Parameters.AddWithValue("@CGST_L_id", gst_slab_result.Rows[0]["CGST_l_id"]);
                        cmd.Parameters.AddWithValue("@Total_SGST", request.Total_SGST);
                        cmd.Parameters.AddWithValue("@SGST_L_id", gst_slab_result.Rows[0]["SGST_l_id"]);
                        cmd.Parameters.AddWithValue("@Total_IGST", request.Total_IGST);
                        cmd.Parameters.AddWithValue("@IGST_L_id", gst_slab_result.Rows[0]["IGST_l_id"]);

                        cmd.Parameters.AddWithValue("@User", request.Created_by);
                        cmd.Parameters.AddWithValue("@txt", 1);

                        // TRANS
                        cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                        cmd.Parameters.AddWithValue("@Trans_type_id", 4);
                        cmd.Parameters.AddWithValue("@trans_code", "SR");

                        // TRANS DETAILS
                        cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                        cmd.Parameters.AddWithValue("@Status", "1");
                        cmd.Parameters.AddWithValue("@L_id", Transfer_ledger);
                        cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);
                        cmd.Parameters.AddWithValue("@Card_no", request.Card_no ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Narr", "Sale Return");

                        cmd.Parameters.AddWithValue("@Dr_point", request.Dr_point ?? 0);
                        cmd.Parameters.AddWithValue("@Bal", request.Bal ?? 0);
                        cmd.Parameters.AddWithValue("@Point_amt", request.Point_amt ?? 0);

                        SqlParameter outParam = new SqlParameter("@Return_id", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outParam);

                        cmd.ExecuteNonQuery();
                        Return_id = Convert.ToInt32(outParam.Value);

                        // DETAILS
                        foreach (var item in request.DETAILS)
                        {
                            var gst = gst_slab_details_all.AsEnumerable()
                                            .FirstOrDefault(r => r.Field<int>("Stock_id") == item.Stock_id);
                            if (gst != null)
                            {
                                SqlCommand cmdDet = new SqlCommand("Sp_Bazar_Sale_Return_Details", db.cn, transaction);
                                cmdDet.CommandType = CommandType.StoredProcedure;

                                cmdDet.Parameters.AddWithValue("@Return_id", Return_id);
                                cmdDet.Parameters.AddWithValue("@Stock_id", item.Stock_id);
                                cmdDet.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmdDet.Parameters.AddWithValue("@MRP", item.MRP);
                                cmdDet.Parameters.AddWithValue("@Disc", item.Disc);
                                cmdDet.Parameters.AddWithValue("@Rate", item.Rate);
                                cmdDet.Parameters.AddWithValue("@Amount", item.Amount);
                                cmdDet.Parameters.AddWithValue("@Taxable_amt", item.Taxable_amt);

                                cmdDet.Parameters.AddWithValue("@CGST_per", Convert.ToDecimal(gst["CGST_per"]));
                                cmdDet.Parameters.AddWithValue("@SGST_per", Convert.ToDecimal(gst["SGST_per"]));
                                cmdDet.Parameters.AddWithValue("@IGST_per", Convert.ToDecimal(gst["IGST_per"]));

                                cmdDet.Parameters.AddWithValue("@CGST_amt", item.CGST_amt);
                                cmdDet.Parameters.AddWithValue("@SGST_amt", item.SGST_amt);
                                cmdDet.Parameters.AddWithValue("@IGST_amt", item.IGST_amt);

                                cmdDet.ExecuteNonQuery();
                            }
                            else
                            {
                                throw new Exception($"GST slab not found for Stock_id {item.Stock_id}");
                            }
                        }

                        transaction.Commit();
                        db.Disconnect();

                        return Request.CreateResponse(HttpStatusCode.OK, "Sale Return Inserted");
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

        [Route("api/Salereturn")]
        [HttpPut]
        public HttpResponseMessage PutSaleReturn([FromBody] SaleReturnModel request)
        {
            try
            {
                db.Connect();

                if (!db.IsAdmin(request.Created_by))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Only Admin Can Update");
                }

                if (request == null)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sale Return is required");
                }

                if (request.DETAILS == null || request.DETAILS.Count == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Details required");
                }

                if (request.update_return_id == null)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Update ID missing");
                }

                SqlCommand cmdSetting = new SqlCommand("SELECT Ledger_id FROM LEDGER_SETTING WHERE Purpose = 'Sale Return'", db.cn);
                object saleL = cmdSetting.ExecuteScalar();
                if (saleL == null || saleL == DBNull.Value)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sale Return account not found in Ledger Setting");
                }
                int Sale_l_id = Convert.ToInt32(saleL);

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

                int Trans_id = 0;
                // Trans_id fetch from DB
                var dt = db.GetTable("SELECT TOP 1 Trans_id FROM TRANS_DETAILS WHERE Master_id = " + request.update_return_id);
                if (dt.Rows.Count > 0)
                {
                    Trans_id = Convert.ToInt32(dt.Rows[0]["Trans_id"]);
                }
                else
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Trans_id not found");
                }

                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_Sale_Return", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        //  MODE
                        cmd.Parameters.AddWithValue("@txt", 2);

                        // IDS
                        cmd.Parameters.AddWithValue("@update_return_id", request.update_return_id);
                        cmd.Parameters.AddWithValue("@update_trans_id", Trans_id);

                        // MAIN
                        cmd.Parameters.AddWithValue("@Outlet_id", request.Outlet_id);
                        cmd.Parameters.AddWithValue("@Return_date", request.Return_date);
                        cmd.Parameters.AddWithValue("@Sale_id", request.Sale_id);
                        cmd.Parameters.AddWithValue("@Total_quantity", request.Total_quantity);
                        cmd.Parameters.AddWithValue("@Total_Disc", request.Total_Disc);
                        cmd.Parameters.AddWithValue("@Total_amt", request.Total_amt);
                        cmd.Parameters.AddWithValue("@Round_off", request.Round_off);
                        cmd.Parameters.AddWithValue("@Roundoff_id", Round_Off_id);
                        cmd.Parameters.AddWithValue("@Bill_amt", request.Bill_amt);
                        cmd.Parameters.AddWithValue("@Sale_l_id", Sale_l_id);

                        cmd.Parameters.AddWithValue("@Total", request.Total);
                        cmd.Parameters.AddWithValue("@Total_CGST", request.Total_CGST);
                        cmd.Parameters.AddWithValue("@CGST_L_id", gst_slab_result.Rows[0]["CGST_l_id"]);
                        cmd.Parameters.AddWithValue("@Total_SGST", request.Total_SGST);
                        cmd.Parameters.AddWithValue("@SGST_L_id", gst_slab_result.Rows[0]["SGST_l_id"]);
                        cmd.Parameters.AddWithValue("@Total_IGST", request.Total_IGST);
                        cmd.Parameters.AddWithValue("@IGST_L_id", gst_slab_result.Rows[0]["IGST_l_id"]);

                        cmd.Parameters.AddWithValue("@User", request.Created_by);

                        // TRANS
                        cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                        cmd.Parameters.AddWithValue("@Trans_type_id", 4);
                        cmd.Parameters.AddWithValue("@trans_code", "SR");

                        // TRANS DETAILS
                        cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                        cmd.Parameters.AddWithValue("@Status", "1");
                        cmd.Parameters.AddWithValue("@L_id", Transfer_ledger);
                        cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);
                        cmd.Parameters.AddWithValue("@Card_no", request.Card_no ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Narr", "Sale Return");

                        cmd.Parameters.AddWithValue("@Dr_point", request.Dr_point ?? 0);
                        cmd.Parameters.AddWithValue("@Bal", request.Bal ?? 0);
                        cmd.Parameters.AddWithValue("@Point_amt", request.Point_amt ?? 0);

                        //  OUTPUT
                        SqlParameter outParam = new SqlParameter("@Return_id", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outParam);

                        cmd.ExecuteNonQuery();

                        //  use update_return_id
                       
                        Return_id = Convert.ToInt32(request.update_return_id);

                        // DELETE OLD DETAILS
                        SqlCommand cmdDelete = new SqlCommand(
                            "DELETE FROM SALE_RETURN_DETAILS WHERE Return_id = @Return_id",
                            db.cn,
                            transaction
                        );
                        cmdDelete.Parameters.AddWithValue("@Return_id", Return_id);
                        cmdDelete.ExecuteNonQuery();

                        // INSERT NEW DETAILS
                        foreach (var item in request.DETAILS)
                        {
                            var gst = gst_slab_details_all.AsEnumerable()
                                            .FirstOrDefault(r => r.Field<int>("Stock_id") == item.Stock_id);
                            if (gst != null)
                            {
                                SqlCommand cmdDet = new SqlCommand("Sp_Bazar_Sale_Return_Details", db.cn, transaction);
                                cmdDet.CommandType = CommandType.StoredProcedure;

                                cmdDet.Parameters.AddWithValue("@Return_id", Return_id);
                                cmdDet.Parameters.AddWithValue("@Stock_id", item.Stock_id);
                                cmdDet.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmdDet.Parameters.AddWithValue("@MRP", item.MRP);
                                cmdDet.Parameters.AddWithValue("@Disc", item.Disc);
                                cmdDet.Parameters.AddWithValue("@Rate", item.Rate);
                                cmdDet.Parameters.AddWithValue("@Amount", item.Amount);
                                cmdDet.Parameters.AddWithValue("@Taxable_amt", item.Taxable_amt);

                                cmdDet.Parameters.AddWithValue("@CGST_per", Convert.ToDecimal(gst["CGST_per"]));
                                cmdDet.Parameters.AddWithValue("@SGST_per", Convert.ToDecimal(gst["SGST_per"]));
                                cmdDet.Parameters.AddWithValue("@IGST_per", Convert.ToDecimal(gst["IGST_per"]));

                                cmdDet.Parameters.AddWithValue("@CGST_amt", item.CGST_amt);
                                cmdDet.Parameters.AddWithValue("@SGST_amt", item.SGST_amt);
                                cmdDet.Parameters.AddWithValue("@IGST_amt", item.IGST_amt);

                                cmdDet.ExecuteNonQuery();
                            }
                            else
                            {
                                throw new Exception($"GST slab not found for Stock_id {item.Stock_id}");
                            }
                        }

                        transaction.Commit();
                        db.Disconnect();

                        return Request.CreateResponse(HttpStatusCode.OK, "Sale Return Updated Successfully");
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

        [Route("api/Salereturn/delete")]
        [HttpPost]
        public HttpResponseMessage DeleteSaleReturn(dynamic request)
        {
            try
            {
                db.Connect();
                int Return_id = request.Return_id;
                string Reason = request.Reason;
                string User = request.User;

                if (!db.IsAdmin(User))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Only Admin Can Delete");
                }

                if (Return_id == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid IDs");
                }

                int Trans_id = 0;
                // Trans_id fetch from DB
                var dt = db.GetTable("SELECT TOP 1 Trans_id FROM TRANS_DETAILS WHERE Master_id = " + request.Return_id);
                if (dt.Rows.Count > 0)
                {
                    Trans_id = Convert.ToInt32(dt.Rows[0]["Trans_id"]);
                }
                else
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Trans_id not found");
                }

                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_sale_rtn_dlt", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Return_id", Return_id);
                        cmd.Parameters.AddWithValue("@Trans_id", Trans_id);
                        cmd.Parameters.AddWithValue("@Reason", Reason ?? "Deleted");

                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        db.Disconnect();

                        return Request.CreateResponse(HttpStatusCode.OK, "Sale Return Deleted ");
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
