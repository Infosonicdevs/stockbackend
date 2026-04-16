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

        [Route("api/Saleretun")]
        public HttpResponseMessage GetSale()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from SALE_RETURN where Status = 1 ");
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

                if (!db.IsValidUser(request.User))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid user");

                if (request == null || request.DETAILS.Count == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Data");

                int Return_id = 0;

                using (SqlTransaction transaction = db.cn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_Sale_Return", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        // MAIN
                        cmd.Parameters.AddWithValue("@Return_date", request.Return_date);
                        cmd.Parameters.AddWithValue("@Sale_id", request.Sale_id);
                        cmd.Parameters.AddWithValue("@Total_quantity", request.Total_quantity);
                        cmd.Parameters.AddWithValue("@Total_Disc", request.Total_Disc);
                        cmd.Parameters.AddWithValue("@Total_amt", request.Total_amt);
                        cmd.Parameters.AddWithValue("@Round_off", request.Round_off);
                        cmd.Parameters.AddWithValue("@Roundoff_id", request.Roundoff_id);
                        cmd.Parameters.AddWithValue("@Bill_amt", request.Bill_amt);
                        cmd.Parameters.AddWithValue("@Sale_l_id", request.Sale_l_id);

                        cmd.Parameters.AddWithValue("@Total_CGST", request.Total_CGST);
                        cmd.Parameters.AddWithValue("@Total_SGST", request.Total_SGST);
                        cmd.Parameters.AddWithValue("@Total_IGST", request.Total_IGST);
                        cmd.Parameters.AddWithValue("@Total_Taxable", request.Total_Taxable);

                        cmd.Parameters.AddWithValue("@User", request.User);
                        cmd.Parameters.AddWithValue("@txt", 1);

                        // TRANS
                        cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                        cmd.Parameters.AddWithValue("@Trans_type_id", request.Trans_type_id);
                        cmd.Parameters.AddWithValue("@trans_code", request.trans_code);

                        // TRANS DETAILS
                        cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                        cmd.Parameters.AddWithValue("@Status", request.Status);
                        cmd.Parameters.AddWithValue("@L_id", request.L_id);
                        cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);
                        cmd.Parameters.AddWithValue("@Card_no", request.Card_no ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Narr", request.Narr ?? "Sale Return");

                        cmd.Parameters.AddWithValue("@CGST_id", request.CGST_id);
                        cmd.Parameters.AddWithValue("@SGST_id", request.SGST_id);
                        cmd.Parameters.AddWithValue("@IGST_id", request.IGST_id);

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
                            SqlCommand cmdDet = new SqlCommand("Sp_Bazar_Sale_Details", db.cn, transaction);
                            cmdDet.CommandType = CommandType.StoredProcedure;

                            cmdDet.Parameters.AddWithValue("@Sale_Rtn_id", Return_id);
                            cmdDet.Parameters.AddWithValue("@Stock_id", item.Stock_id);
                            cmdDet.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmdDet.Parameters.AddWithValue("@MRP", item.MRP);
                            cmdDet.Parameters.AddWithValue("@Disc", item.Disc);
                            cmdDet.Parameters.AddWithValue("@Rate", item.Rate);
                            cmdDet.Parameters.AddWithValue("@Amount", item.Amount);
                            cmdDet.Parameters.AddWithValue("@Taxable_amt", item.Taxable_amt);

                            cmdDet.Parameters.AddWithValue("@CGST_per", item.CGST_per);
                            cmdDet.Parameters.AddWithValue("@SGST_per", item.SGST_per);
                            cmdDet.Parameters.AddWithValue("@IGST_per", item.IGST_per);

                            cmdDet.Parameters.AddWithValue("@CGST_amt", item.CGST_amt);
                            cmdDet.Parameters.AddWithValue("@SGST_amt", item.SGST_amt);
                            cmdDet.Parameters.AddWithValue("@IGST_amt", item.IGST_amt);

                            cmdDet.Parameters.AddWithValue("@Mode", item.Mode);

                            cmdDet.ExecuteNonQuery();
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

                if (!db.IsAdmin(request.User))
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
                        SqlCommand cmd = new SqlCommand("Sp_Bazar_Sale_Return", db.cn, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;

                        //  MODE
                        cmd.Parameters.AddWithValue("@txt", 2);

                        // IDS
                        cmd.Parameters.AddWithValue("@update_return_id", request.update_return_id);
                        cmd.Parameters.AddWithValue("@update_trans_id", request.update_trans_id);

                        // MAIN
                        cmd.Parameters.AddWithValue("@Return_date", request.Return_date);
                        cmd.Parameters.AddWithValue("@Sale_id", request.Sale_id);
                        cmd.Parameters.AddWithValue("@Total_quantity", request.Total_quantity);
                        cmd.Parameters.AddWithValue("@Total_Disc", request.Total_Disc);
                        cmd.Parameters.AddWithValue("@Total_amt", request.Total_amt);
                        cmd.Parameters.AddWithValue("@Round_off", request.Round_off);
                        cmd.Parameters.AddWithValue("@Roundoff_id", request.Roundoff_id);
                        cmd.Parameters.AddWithValue("@Bill_amt", request.Bill_amt);
                        cmd.Parameters.AddWithValue("@Sale_l_id", request.Sale_l_id);

                        cmd.Parameters.AddWithValue("@Total_CGST", request.Total_CGST);
                        cmd.Parameters.AddWithValue("@Total_SGST", request.Total_SGST);
                        cmd.Parameters.AddWithValue("@Total_IGST", request.Total_IGST);
                        cmd.Parameters.AddWithValue("@Total_Taxable", request.Total_Taxable);

                        cmd.Parameters.AddWithValue("@User", request.User);

                        // TRANS
                        cmd.Parameters.AddWithValue("@Year_id", request.Year_id);
                        cmd.Parameters.AddWithValue("@Trans_type_id", request.Trans_type_id);
                        cmd.Parameters.AddWithValue("@trans_code", request.trans_code);
                        cmd.Parameters.AddWithValue("@Modify_reason", request.Modify_reason ?? (object)DBNull.Value);

                        // TRANS DETAILS
                        cmd.Parameters.AddWithValue("@CashTrans", request.CashTrans);
                        cmd.Parameters.AddWithValue("@Status", request.Status);
                        cmd.Parameters.AddWithValue("@L_id", request.L_id);
                        cmd.Parameters.AddWithValue("@Cust_id", request.Cust_id);
                        cmd.Parameters.AddWithValue("@Card_no", request.Card_no ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Narr", request.Narr ?? "Sale Return");

                        cmd.Parameters.AddWithValue("@CGST_id", request.CGST_id);
                        cmd.Parameters.AddWithValue("@SGST_id", request.SGST_id);
                        cmd.Parameters.AddWithValue("@IGST_id", request.IGST_id);

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
                            "DELETE FROM Sale_details WHERE Sale_Rtn_id = @Return_id",
                            db.cn,
                            transaction
                        );
                        cmdDelete.Parameters.AddWithValue("@Return_id", Return_id);
                        cmdDelete.ExecuteNonQuery();

                        // INSERT NEW DETAILS
                        foreach (var item in request.DETAILS)
                        {
                            SqlCommand cmdDetail = new SqlCommand("Sp_Bazar_Sale_Details", db.cn, transaction);
                            cmdDetail.CommandType = CommandType.StoredProcedure;

                            cmdDetail.Parameters.AddWithValue("@Sale_Rtn_id", Return_id);
                            cmdDetail.Parameters.AddWithValue("@Stock_id", item.Stock_id);
                            cmdDetail.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmdDetail.Parameters.AddWithValue("@MRP", item.MRP);
                            cmdDetail.Parameters.AddWithValue("@Disc", item.Disc);
                            cmdDetail.Parameters.AddWithValue("@Rate", item.Rate);
                            cmdDetail.Parameters.AddWithValue("@Amount", item.Amount);
                            cmdDetail.Parameters.AddWithValue("@Taxable_amt", item.Taxable_amt);

                            cmdDetail.Parameters.AddWithValue("@CGST_per", item.CGST_per);
                            cmdDetail.Parameters.AddWithValue("@SGST_per", item.SGST_per);
                            cmdDetail.Parameters.AddWithValue("@IGST_per", item.IGST_per);

                            cmdDetail.Parameters.AddWithValue("@CGST_amt", item.CGST_amt);
                            cmdDetail.Parameters.AddWithValue("@SGST_amt", item.SGST_amt);
                            cmdDetail.Parameters.AddWithValue("@IGST_amt", item.IGST_amt);

                            cmdDetail.Parameters.AddWithValue("@Mode", item.Mode);

                            cmdDetail.ExecuteNonQuery();
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
                int Trans_id = request.Trans_id;
                string Reason = request.Reason;
                string User = request.User;

                if (!db.IsAdmin(User))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Only Admin Can Delete");
                }

                if (Return_id == 0 || Trans_id == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid IDs");
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
