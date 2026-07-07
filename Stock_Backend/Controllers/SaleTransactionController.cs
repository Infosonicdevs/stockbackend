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
    public class SaleTransactionController : ApiController
    {
        DbClass db = new DbClass();

        [HttpGet]
        [Route("api/SaleTransaction/StockAvailable")]
        public HttpResponseMessage GetStockAvailable(int Stock_id, int? Outlet_id = null)
        {
            try
            {
                db.Connect();

                string query = @"
        SELECT 
            -- STOCK BALANCE
            ISNULL((SELECT SUM(sb.Quantity) FROM STOCK_BALANCE sb
                     WHERE sb.Stock_id = @Stock_id
                     AND (@Outlet_id IS NULL OR sb.Outlet_id = @Outlet_id)), 0)
            +
            -- GS_INWARD (Purchase  stock)
            ISNULL((SELECT SUM(gs.Quantity) FROM GS_INWARD gs
                     WHERE gs.Stock_id = @Stock_id), 0)
            +
            -- DISTRIBUTION (Distribute  stock)
            ISNULL((SELECT SUM(sd.Quantity) FROM stock_distribution sd
                     WHERE sd.Stock_id = @Stock_id
                     AND sd.Invert = 'N'
                     AND (@Outlet_id IS NULL OR sd.Outlet_id = @Outlet_id)), 0)
            AS Available_Qty,

            -- STOCK BALANCE AMT
            ISNULL((SELECT SUM(sb.Amount) FROM STOCK_BALANCE sb
                     WHERE sb.Stock_id = @Stock_id
                     AND (@Outlet_id IS NULL OR sb.Outlet_id = @Outlet_id)), 0)
            +
            -- GS_INWARD AMT
            ISNULL((SELECT SUM(gs.Amount) FROM GS_INWARD gs
                     WHERE gs.Stock_id = @Stock_id), 0)
            +
            -- DISTRIBUTION AMT
            ISNULL((SELECT SUM(sd.Amount) FROM stock_distribution sd
                     WHERE sd.Stock_id = @Stock_id
                     AND sd.Invert = 'N'
                     AND (@Outlet_id IS NULL OR sd.Outlet_id = @Outlet_id)), 0)
            AS Available_Amt";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@Stock_id", Stock_id);
                cmd.Parameters.AddWithValue("@Outlet_id", (object)Outlet_id ?? DBNull.Value);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                decimal Available_Qty = dt.Rows.Count > 0 ? Convert.ToDecimal(dt.Rows[0]["Available_Qty"]) : 0;
                decimal Available_Amt = dt.Rows.Count > 0 ? Convert.ToDecimal(dt.Rows[0]["Available_Amt"]) : 0;

                db.Disconnect();

                var result = new
                {
                    Stock_id = Stock_id,
                    Outlet_id = Outlet_id,
                    Available_Qty = Available_Qty,
                    Available_Amt = Available_Amt
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/SaleTransaction")]
        public HttpResponseMessage GetSale()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from SALE WHERE Status = 1");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpGet]
        [Route("api/SaleTransaction/Details")]
        public HttpResponseMessage GetSaleDetails(int Sale_id)
        {
            try
            {
                db.Connect();

                string query = @"
        SELECT 
            s.Sale_id,
            s.Sale_date,
            s.Final_amt,
            s.Narr,
            d.Details_id,
            d.Stock_id,
            d.Quantity,
            d.MRP,
            d.Rate,
            d.Amount
        FROM SALE s
        INNER JOIN SALE_DETAILS d 
            ON s.Sale_id = d.Sale_Rtn_id
        WHERE s.Sale_id = @Sale_id
        AND s.Status = 1";

                SqlCommand cmd = new SqlCommand(query, db.cn);
                cmd.Parameters.AddWithValue("@Sale_id", Sale_id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, dt);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //public List<BazaSetting> GetBazarSetting()
        //{
        //    try
        //    {
        //        var result = db.GetTable("select * from VIEW_BAZAR_SETTING");
        //        var list = new List<BazaSetting>();
        //        foreach (DataRow row in result.Rows)
        //        {
        //            list.Add(new BazaSetting
        //            {
        //                Pur_id = Convert.ToDecimal(row["Pur_id"]),
        //                Round_Off_id = Convert.ToDecimal(row["Round_Off_id"]),
        //                Hamali_id = Convert.ToDecimal(row["Hamali_id"]),
        //                Commi_id = Convert.ToDecimal(row["Commi_id"]),
        //                Transport_id = Convert.ToDecimal(row["Transport_id"]),
        //                Ma_ses_id = Convert.ToDecimal(row["Ma_ses_id"]),
        //                Tcs_id = Convert.ToDecimal(row["Tcs_id"]),
        //                Net_Disc_id = Convert.ToDecimal(row["Net_Disc_id"]),
        //                Transfer_id = Convert.ToDecimal(row["Transfer_id"])
        //            });
        //        }
        //        return list;
        //    }
        //    catch
        //    {
        //        return new List<BazaSetting>();
        //    }
        //}

        [HttpPost]
        [Route("api/SaleTransaction")]
        public HttpResponseMessage PostSaleTransaction([FromBody] SaleTransactionModel request)
        {
            try
            {
                db.Connect();

                SqlCommand cmdSetting = new SqlCommand("SELECT L_id FROM Bazar_Settg WHERE Purpose = 'Sale account'", db.cn);
                object saleL = cmdSetting.ExecuteScalar();
                if (saleL == null || saleL == DBNull.Value)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sale account not found in Bazar Setting");
                }
                int Sale_L_id = Convert.ToInt32(saleL);


                if (!db.IsValidUser(request.User))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");
                }

                if (request == null)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sale data required");
                }

                if (request.Items == null || request.Items.Count == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sale items required");
                }

                int Sale_id = 0;

                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        //  MAIN SALE INSERT 
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_Sale", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        // SALE
                        cmd.Parameters.AddWithValue("@Outlet_id", request.Outlet_id);
                        cmd.Parameters.AddWithValue("@Sale_date", request.Sale_date.Date);
                        cmd.Parameters.AddWithValue("@Counter_id", request.Counter_id);
                        cmd.Parameters.AddWithValue("@Pavati_no", request.Pavati_no);
                        cmd.Parameters.AddWithValue("@Emp_id", request.Emp_id);
                        cmd.Parameters.AddWithValue("@State_id", request.State_id);
                        cmd.Parameters.AddWithValue("@Card_no", (object)request.Card_no ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("@Total_quantity", request.Total_quantity);
                        cmd.Parameters.AddWithValue("@Total_Rate_amt", request.Total_Rate_amt);
                        cmd.Parameters.AddWithValue("@Total_disc", request.Total_disc);
                        cmd.Parameters.AddWithValue("@Total_SGST", request.Total_SGST);
                        cmd.Parameters.AddWithValue("@Total_CGST", request.Total_CGST);
                        cmd.Parameters.AddWithValue("@Total_IGST", request.Total_IGST);
                        cmd.Parameters.AddWithValue("@Round_off", request.Round_off);
                        cmd.Parameters.AddWithValue("@Final_amt", request.Final_amt);
                        cmd.Parameters.AddWithValue("@Receive_cash", request.Receive_cash);
                        cmd.Parameters.AddWithValue("@Return_cash", request.Return_cash);
                        cmd.Parameters.AddWithValue("@UPI_AMT", request.UPI_AMT);
                        cmd.Parameters.AddWithValue("@Taxable_amt", request.Receive_cash);

                        cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                        cmd.Parameters.AddWithValue("@Sale_CashTrans", request.Sale_CashTrans);
                        cmd.Parameters.AddWithValue("@Narr", request.Narr);
                        cmd.Parameters.AddWithValue("@User", request.User);
                        cmd.Parameters.AddWithValue("@Status", request.Status);
                        cmd.Parameters.AddWithValue("@txt", 1);

                        // TRANS
                        cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                        cmd.Parameters.AddWithValue("@Trans_type_id", 2);
                        cmd.Parameters.AddWithValue("@trans_code", request.trans_code);
                        cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);

                        // TRANS DETAILS
                        //int ledger = 0;

                        //if (request.CashTrans == 'C')
                        //{
                        //    SqlCommand cmdCash = new SqlCommand(
                        //        "SELECT L_id FROM Bazar_Settg WHERE Purpose='Cash submit'",
                        //        db.cn,
                        //        transaction);

                        //    ledger = Convert.ToInt32(cmdCash.ExecuteScalar());
                        //}
                        //else if (request.CashTrans == 'T')
                        //{
                        //    SqlCommand cmdTransfer = new SqlCommand(
                        //        "SELECT L_id FROM Bazar_Settg WHERE Purpose='Transfer account'",
                        //        db.cn,
                        //        transaction);

                        //    ledger = Convert.ToInt32(cmdTransfer.ExecuteScalar());
                        //}

                        //cmd.Parameters.AddWithValue("@Sale_L_id", ledger);
                        cmd.Parameters.AddWithValue("@Sale_L_id", Sale_L_id);
                
                        cmd.Parameters.AddWithValue("@CGST_id", request.CGST_id);
                        cmd.Parameters.AddWithValue("@SGST_id", request.SGST_id);
                        cmd.Parameters.AddWithValue("@IGST_id", request.IGST_id);

                        cmd.Parameters.AddWithValue("@Roundoff_id", request.Roundoff_id);
                        cmd.Parameters.AddWithValue("@Transfer_id", 11);
                        cmd.Parameters.AddWithValue("@Cash_return_id_cr", request.Cash_return_id_cr);
                        cmd.Parameters.AddWithValue("@Cash_return_id_dr", request.Cash_return_id_dr);

                        // POINT
                        cmd.Parameters.AddWithValue("@Cr_point", request.Cr_point);
                        cmd.Parameters.AddWithValue("@Dr_point", request.Dr_point);
                        cmd.Parameters.AddWithValue("@Bal", request.Bal);
                        cmd.Parameters.AddWithValue("@Point_amt", request.Point_amt);
                        cmd.Parameters.AddWithValue("@Redeem_id", request.Redeem_id);

                        // OUTPUT
                        SqlParameter outParam = new SqlParameter("@Sale_id", SqlDbType.Int);
                        outParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outParam);

                        cmd.ExecuteNonQuery();

                        if (outParam.Value != DBNull.Value)
                        {
                            Sale_id = Convert.ToInt32(outParam.Value);
                        }
                        else
                        {
                            throw new Exception("Sale_id not returned from SP");
                        }
                        if (Sale_id <= 0)
                            throw new Exception("Sale insert failed");

                        //  DETAILS INSERT 
                        foreach (var item in request.Items)
                        {
                            SqlCommand cmdDetails = new SqlCommand("Sp_Bazar_Sale_Details", db.cn, transaction);
                            cmdDetails.CommandType = CommandType.StoredProcedure;

                            cmdDetails.Parameters.AddWithValue("@Sale_Rtn_id", Sale_id);
                            cmdDetails.Parameters.AddWithValue("@Stock_id", item.Stock_id);
                            cmdDetails.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmdDetails.Parameters.AddWithValue("@MRP", item.MRP);
                            cmdDetails.Parameters.AddWithValue("@Disc", item.Disc);
                            cmdDetails.Parameters.AddWithValue("@Rate", item.Rate);
                            cmdDetails.Parameters.AddWithValue("@Amount", item.Amount);
                            cmdDetails.Parameters.AddWithValue("@Taxable_amt", item.Taxable_amt);

                            cmdDetails.Parameters.AddWithValue("@CGST_per", item.CGST_per);
                            cmdDetails.Parameters.AddWithValue("@SGST_per", item.SGST_per);
                            cmdDetails.Parameters.AddWithValue("@IGST_per", item.IGST_per);

                            cmdDetails.Parameters.AddWithValue("@CGST_amt", item.CGST_amt);
                            cmdDetails.Parameters.AddWithValue("@SGST_amt", item.SGST_amt);
                            cmdDetails.Parameters.AddWithValue("@IGST_amt", item.IGST_amt);

                            cmdDetails.Parameters.AddWithValue("@Mode", item.Mode);

                            cmdDetails.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        db.Disconnect();

                        return Request.CreateResponse(HttpStatusCode.OK, "Sale Inserted Successfully");
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

        [HttpPut]
        [Route("api/SaleTransaction")]
        public HttpResponseMessage PutSaleTransaction([FromBody] SaleTransactionModel request)
        {
            try
            {
                db.Connect();

                SqlCommand cmdSetting = new SqlCommand("SELECT L_id FROM Bazar_Settg WHERE Purpose = 'Sale account'", db.cn);
                object saleL = cmdSetting.ExecuteScalar();
                if (saleL == null || saleL == DBNull.Value)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sale account not found in Bazar Setting");
                }
                int Sale_L_id = Convert.ToInt32(saleL);

                if (!db.IsAdmin(request.User))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");
                }

                if (request == null)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sale data required");
                }

                if (request.Items == null || request.Items.Count == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sale items required");
                }

                int Sale_id = 0;

                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        //  UPDATE SALE 
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_Sale", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        // SALE UPDATE
                        cmd.Parameters.AddWithValue("@update_Sale_id", request.Sale_id);
                        cmd.Parameters.AddWithValue("@Outlet_id", request.Outlet_id);
                        cmd.Parameters.AddWithValue("@Sale_date", request.Sale_date.Date);
                        cmd.Parameters.AddWithValue("@Counter_id", request.Counter_id);
                        cmd.Parameters.AddWithValue("@Pavati_no", request.Pavati_no);
                        cmd.Parameters.AddWithValue("@Emp_id", request.Emp_id);
                        cmd.Parameters.AddWithValue("@State_id", request.State_id);
                        cmd.Parameters.AddWithValue("@Card_no", (object)request.Card_no ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("@Total_quantity", request.Total_quantity);
                        cmd.Parameters.AddWithValue("@Total_Rate_amt", request.Total_Rate_amt);
                        cmd.Parameters.AddWithValue("@Total_disc", request.Total_disc);
                        cmd.Parameters.AddWithValue("@Total_SGST", request.Total_SGST);
                        cmd.Parameters.AddWithValue("@Total_CGST", request.Total_CGST);
                        cmd.Parameters.AddWithValue("@Total_IGST", request.Total_IGST);
                        cmd.Parameters.AddWithValue("@Round_off", request.Round_off);
                        cmd.Parameters.AddWithValue("@Final_amt", request.Final_amt);
                        cmd.Parameters.AddWithValue("@Receive_cash", request.Receive_cash);
                        cmd.Parameters.AddWithValue("@Return_cash", request.Return_cash);
                        cmd.Parameters.AddWithValue("@UPI_AMT", request.UPI_AMT);
                        cmd.Parameters.AddWithValue("@Taxable_amt", request.Receive_cash);

                        cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                        cmd.Parameters.AddWithValue("@Sale_CashTrans", request.Sale_CashTrans);
                        cmd.Parameters.AddWithValue("@Narr", request.Narr);
                        cmd.Parameters.AddWithValue("@User", request.User);
                        cmd.Parameters.AddWithValue("@Status", request.Status);
                        cmd.Parameters.AddWithValue("@txt", 2); // UPDATE 

                        // TRANS UPDATE
                        cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                        cmd.Parameters.AddWithValue("@Trans_type_id", 2);
                        cmd.Parameters.AddWithValue("@trans_code", request.trans_code);
                        cmd.Parameters.AddWithValue("@update_trans_id", request.Trans_id);
                        cmd.Parameters.AddWithValue("@Modify_reason", request.Modify_reason);
                        cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);

                        // TRANS DETAILS
                        int ledger = Sale_L_id;

                        if (request.CashTrans == 'T')
                        {
                            SqlCommand cmdTransfer = new SqlCommand("SELECT L_id FROM Bazar_Settg WHERE Purpose='Transfer account'",
                                                     db.cn, transaction);

                            ledger = Convert.ToInt32(cmdTransfer.ExecuteScalar());
                        }
                        cmd.Parameters.AddWithValue("@Sale_L_id", Sale_L_id);
                        cmd.Parameters.AddWithValue("@CGST_id", request.CGST_id);
                        cmd.Parameters.AddWithValue("@SGST_id", request.SGST_id);
                        cmd.Parameters.AddWithValue("@IGST_id", request.IGST_id);

                        cmd.Parameters.AddWithValue("@Roundoff_id", request.Roundoff_id);
                        cmd.Parameters.AddWithValue("@Transfer_id", 11);
                        cmd.Parameters.AddWithValue("@Cash_return_id_cr", request.Cash_return_id_cr);
                        cmd.Parameters.AddWithValue("@Cash_return_id_dr", request.Cash_return_id_dr);

                        // POINT
                        cmd.Parameters.AddWithValue("@Cr_point", request.Cr_point);
                        cmd.Parameters.AddWithValue("@Dr_point", request.Dr_point);
                        cmd.Parameters.AddWithValue("@Bal", request.Bal);
                        cmd.Parameters.AddWithValue("@Point_amt", request.Point_amt);
                        cmd.Parameters.AddWithValue("@Redeem_id", request.Redeem_id);

                        // OUTPUT
                        SqlParameter outParam = new SqlParameter("@Sale_id", SqlDbType.Int);
                        outParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outParam);

                        cmd.ExecuteNonQuery();

                        if (outParam.Value != DBNull.Value)
                        {
                            Sale_id = Convert.ToInt32(outParam.Value);
                        }
                        else
                        {
                            throw new Exception("Sale_id not returned from SP");
                        }

                        if (Sale_id <= 0)
                            throw new Exception("Sale update failed");

                        // DELETE OLD DETAILS
                        SqlCommand cmdDelete = new SqlCommand(
                            "DELETE FROM Sale_details WHERE Sale_Rtn_id = @Sale_id",
                            db.cn,
                            transaction
                        );
                        cmdDelete.Parameters.AddWithValue("@Sale_id", Sale_id);
                        cmdDelete.ExecuteNonQuery();

                        //  INSERT NEW DETAILS
                        foreach (var item in request.Items)
                        {
                            SqlCommand cmdDetails = new SqlCommand("Sp_Bazar_Sale_Details", db.cn, transaction);
                            cmdDetails.CommandType = CommandType.StoredProcedure;

                            cmdDetails.Parameters.AddWithValue("@Sale_Rtn_id", Sale_id);
                            cmdDetails.Parameters.AddWithValue("@Stock_id", item.Stock_id);
                            cmdDetails.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmdDetails.Parameters.AddWithValue("@MRP", item.MRP);
                            cmdDetails.Parameters.AddWithValue("@Disc", item.Disc);
                            cmdDetails.Parameters.AddWithValue("@Rate", item.Rate);
                            cmdDetails.Parameters.AddWithValue("@Amount", item.Amount);
                            cmdDetails.Parameters.AddWithValue("@Taxable_amt", item.Taxable_amt);

                            cmdDetails.Parameters.AddWithValue("@CGST_per", item.CGST_per);
                            cmdDetails.Parameters.AddWithValue("@SGST_per", item.SGST_per);
                            cmdDetails.Parameters.AddWithValue("@IGST_per", item.IGST_per);

                            cmdDetails.Parameters.AddWithValue("@CGST_amt", item.CGST_amt);
                            cmdDetails.Parameters.AddWithValue("@SGST_amt", item.SGST_amt);
                            cmdDetails.Parameters.AddWithValue("@IGST_amt", item.IGST_amt);

                            cmdDetails.Parameters.AddWithValue("@Mode", item.Mode);

                            cmdDetails.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        db.Disconnect();

                        return Request.CreateResponse(HttpStatusCode.OK, "Sale Updated Successfully");
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


        [HttpPost]
        [Route("api/DeleteSale")]
        public HttpResponseMessage DeleteSale([FromBody] SaleTransactionModel request)
        {
            try
            {
                db.Connect();

                //if (!db.IsAdmin(request.User))
                //{
                //    db.Disconnect();
                //    return Request.CreateResponse(HttpStatusCode.BadRequest, "Only admin can delete");
                //}

                if (request == null || request.Sale_id <= 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Sale_id required");
                }

                int trans_id = 0;

                // Trans_id fetch from DB
                var dt = db.GetTable("SELECT TOP 1 Trans_id FROM TRANS_DETAILS WHERE Master_id = " + request.Sale_id);

                if (dt.Rows.Count > 0)
                {
                    trans_id = Convert.ToInt32(dt.Rows[0]["Trans_id"]);
                }
                else
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Trans_id not found for this Sale");
                }

                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_Sale_dlt", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Sale_id", request.Sale_id);
                        cmd.Parameters.AddWithValue("@Trans_id", trans_id);
                        cmd.Parameters.AddWithValue("@Reason", request.Reason ?? "Deleted by Admin");

                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        db.Disconnect();

                        return Request.CreateResponse(HttpStatusCode.OK, "Sale Deleted Successfully ");
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
