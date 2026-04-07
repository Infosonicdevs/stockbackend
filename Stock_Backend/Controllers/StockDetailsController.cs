using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.UI.WebControls;

namespace Stock_Backend.Controllers
{
    public class StockDetailsController : ApiController
    {
        DbClass db = new DbClass();


        [Route("api/StockDetail")]
        [HttpGet]
        public HttpResponseMessage GetStockDetail()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_STOCK_DETAILS");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("api/MaxStockNo")]
        [HttpGet]
        public HttpResponseMessage GetMaxStockNo()
        {
            try
            {
                db.Connect();
                var maxStockNo = db.ExecuteScalar("SELECT COALESCE(MAX(Stock_no),0)+1 FROM VIEW_STOCK_DETAILS");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, maxStockNo);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("api/StockDetail")]
        [HttpGet]
        public HttpResponseMessage GetStockDetail(int Stock_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_STOCK_DETAILS where Stock_id = " + Stock_id);
                if (result.Rows.Count == 0)
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.NotFound, $"Stock detail with Id - {Stock_id} not found");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/StockDetail")]
        [HttpPost]
        public HttpResponseMessage PostStockDetail([FromBody] STOCK_DETAILS stock_details)
        {
            try
            {
                db.Connect();

                if (db.IsValidUser(stock_details.Created_by))
                {
                    if (db.IsExists("select * from STOCK_DETAILS where Barcode = '" + stock_details.Barcode + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Barcode already exists");
                    }
                    else if (db.IsExists("select * from STOCK_DETAILS where Stock_name = '" + stock_details.Stock_name + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Stock name already exists");
                    }
                    else
                    {
                        var purchase_details = db.GetTable(
                           "select CGST_per, SGST_per, IGST_per from GST_SLAB where Heading = '" + stock_details.Purchase_Heading + "'"
                        );

                        var sale_details = db.GetTable(
                            "select CGST_per, SGST_per, IGST_per from GST_SLAB where Heading = '" + stock_details.Sale_Heading + "'"
                        );

                        SqlCommand cmd = new SqlCommand("Sp_Stock_Details", db.cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Vibhag_id", 0);
                        cmd.Parameters.AddWithValue("@Barcode", stock_details.Barcode);
                        cmd.Parameters.AddWithValue("@Stock_name", stock_details.Stock_name);
                        cmd.Parameters.AddWithValue("@HSN_no", stock_details.HSN_no);
                        cmd.Parameters.AddWithValue("@Group_id", stock_details.Group_id);
                        cmd.Parameters.AddWithValue("@Subgroup_id", stock_details.Subgroup_id);
                        cmd.Parameters.AddWithValue("@Weight", stock_details.Weight);
                        cmd.Parameters.AddWithValue("@Unit_id", stock_details.Unit_id);
                        cmd.Parameters.AddWithValue("@Include_GST", stock_details.Include_GST);
                        cmd.Parameters.AddWithValue("@slab_id", stock_details.Slab_id);
                        cmd.Parameters.AddWithValue("@SGST_per", Convert.ToDecimal(sale_details.Rows[0]["SGST_per"]));
                        cmd.Parameters.AddWithValue("@CGST_per", Convert.ToDecimal(sale_details.Rows[0]["CGST_per"]));
                        cmd.Parameters.AddWithValue("@IGST_per", Convert.ToDecimal(sale_details.Rows[0]["IGST_per"]));
                        cmd.Parameters.AddWithValue("@Pur_slab_id", stock_details.Pur_slab_id);                                    
                        cmd.Parameters.AddWithValue("@Pur_SGST_per", Convert.ToDecimal(purchase_details.Rows[0]["SGST_per"]));
                        cmd.Parameters.AddWithValue("@Pur_CGST_per", Convert.ToDecimal(purchase_details.Rows[0]["CGST_per"]));
                        cmd.Parameters.AddWithValue("@Pur_IGST_per", Convert.ToDecimal(purchase_details.Rows[0]["IGST_per"]));
                        cmd.Parameters.AddWithValue("@Is_offer", stock_details.Is_offer);
                        cmd.Parameters.AddWithValue("@Offer_qty", stock_details.Offer_qty);
                        cmd.Parameters.AddWithValue("@MRP", stock_details.MRP);
                        cmd.Parameters.AddWithValue("@Discount", stock_details.Discount);
                        cmd.Parameters.AddWithValue("@Rate", stock_details.Rate);
                        cmd.Parameters.AddWithValue("@Change_date", stock_details.Change_date.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@On_form", stock_details.On_form);
                        cmd.Parameters.AddWithValue("@Is_stock", stock_details.Is_stock);
                        cmd.Parameters.AddWithValue("@B_G", '0');
                        cmd.Parameters.AddWithValue("@txt", 1);

                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
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

        [Route("api/StockDetail")]
        [HttpPut]
        public HttpResponseMessage PutStockDetail([FromBody] STOCK_DETAILS stock_details)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(stock_details.Modified_by))
                {
                    if (db.IsExists("select * from STOCK_DETAILS where Barcode = '" + stock_details.Barcode + "' and Stock_id != " + stock_details.Stock_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Barcode already exists");
                    }
                    else if (db.IsExists("select * from STOCK_DETAILS where Stock_no = " + stock_details.Stock_no + " and Stock_id != " + stock_details.Stock_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Stock no already exists");
                    }
                    else if (db.IsExists("select * from STOCK_DETAILS where Stock_name = '" + stock_details.Stock_name + "' and Stock_id !=" + stock_details.Stock_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Stock name already exists");
                    }
                    else
                    {
                                                                                                        
                        var purchase_details = db.GetTable(
                           "select CGST_per, SGST_per, IGST_per from GST_SLAB where Heading = '" + stock_details.Purchase_Heading + "'"
                        );

                        var sale_details = db.GetTable(
                            "select CGST_per, SGST_per, IGST_per from GST_SLAB where Heading = '" + stock_details.Sale_Heading + "'"
                        );

                        SqlCommand cmd = new SqlCommand("Sp_Stock_Details", db.cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Stock_id", stock_details.Stock_id);
                        cmd.Parameters.AddWithValue("@Vibhag_id", 0);
                        cmd.Parameters.AddWithValue("@Barcode", stock_details.Barcode);
                        cmd.Parameters.AddWithValue("@Stock_name", stock_details.Stock_name);
                        cmd.Parameters.AddWithValue("@HSN_no", stock_details.HSN_no);
                        cmd.Parameters.AddWithValue("@Group_id", stock_details.Group_id);
                        cmd.Parameters.AddWithValue("@Subgroup_id", stock_details.Subgroup_id);
                        cmd.Parameters.AddWithValue("@Weight", stock_details.Weight);
                        cmd.Parameters.AddWithValue("@Unit_id", stock_details.Unit_id);
                        cmd.Parameters.AddWithValue("@Include_GST", stock_details.Include_GST);
                        cmd.Parameters.AddWithValue("@slab_id", stock_details.Slab_id);
                        cmd.Parameters.AddWithValue("@SGST_per", Convert.ToDecimal(sale_details.Rows[0]["SGST_per"]));
                        cmd.Parameters.AddWithValue("@CGST_per", Convert.ToDecimal(sale_details.Rows[0]["CGST_per"]));
                        cmd.Parameters.AddWithValue("@IGST_per", Convert.ToDecimal(sale_details.Rows[0]["IGST_per"]));
                        cmd.Parameters.AddWithValue("@Pur_slab_id", stock_details.Pur_slab_id);     
                        cmd.Parameters.AddWithValue("@Pur_SGST_per", Convert.ToDecimal(purchase_details.Rows[0]["SGST_per"]));
                        cmd.Parameters.AddWithValue("@Pur_CGST_per", Convert.ToDecimal(purchase_details.Rows[0]["CGST_per"]));
                        cmd.Parameters.AddWithValue("@Pur_IGST_per", Convert.ToDecimal(purchase_details.Rows[0]["IGST_per"]));
                        cmd.Parameters.AddWithValue("@Is_offer", stock_details.Is_offer);
                        cmd.Parameters.AddWithValue("@Offer_qty", stock_details.Offer_qty);
                        cmd.Parameters.AddWithValue("@MRP", stock_details.MRP);
                        cmd.Parameters.AddWithValue("@Discount", stock_details.Discount);
                        cmd.Parameters.AddWithValue("@Rate", stock_details.Rate);
                        cmd.Parameters.AddWithValue("@Change_date", stock_details.Change_date.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@On_form", stock_details.On_form);
                        cmd.Parameters.AddWithValue("@Up_Stock_no", stock_details.Stock_no);
                        cmd.Parameters.AddWithValue("@Is_stock", stock_details.Is_stock);
                        cmd.Parameters.AddWithValue("@B_G", '0');
                        cmd.Parameters.AddWithValue("@txt", 2);

                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
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

        [Route("api/DelStockDetail")]
        [HttpPost]
        public HttpResponseMessage DeleteStockDetail([FromBody] STOCK_DETAILS stock_details)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(stock_details.Modified_by))
                {
                    SqlCommand cmd = new SqlCommand("Sp_Stock_Details_dlt", db.cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Stock_id", stock_details.Stock_id);
                    cmd.ExecuteNonQuery();

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
    }
}
