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
    public class CustomerController : ApiController
    {
        DbClass db = new DbClass();

        #region Customer

        [Route("api/Customer")]
        public HttpResponseMessage GetCustomer()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_CUSTOMER");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Customer")]
        public HttpResponseMessage GetCustomer(int Cust_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_CUSTOMER where Cust_id = " + Cust_id);
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/Customer")]
        public HttpResponseMessage PostCustomer([FromBody] CUSTOMER customer)
        {
            try
            {
                db.Connect();
                if(db.IsValidUser(customer.Created_by))
                {
                    if(db.IsExists("Select * from CUSTOMER where First_name = '" + customer.First_name + "' and Middle_name = '" + customer.Middle_name +"' and Last_name = '" + customer.Last_name + "' and Append = '" + customer.Append + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer name already exists");
                    }
                    else if (db.IsExists("Select * from CUSTOMER where Cust_no = " + customer.Cust_no))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer no already exists");
                    }
                    else if(db.IsExists("Select * from CUSTOMER where Card_no = '" + customer.Card_no + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer card no already exists");
                    }
                    else if (db.IsExists("Select * from CUSTOMER where Phone_no = '" + customer.Phone_no + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer phone no already exists");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Customer", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Cust_no", customer.Cust_no);
                        cmd.Parameters.AddWithValue("@Card_no", customer.Card_no);
                        cmd.Parameters.AddWithValue("@Prefix_id", customer.Prefix_id);
                        cmd.Parameters.AddWithValue("@First_name", customer.First_name);
                        cmd.Parameters.AddWithValue("@Middle_name", customer.Middle_name);
                        cmd.Parameters.AddWithValue("@Last_name", customer.Last_name);
                        cmd.Parameters.AddWithValue("@Append", customer.Append);
                        cmd.Parameters.AddWithValue("@Address", customer.Address);
                        cmd.Parameters.AddWithValue("@City_id", customer.City_id);
                        cmd.Parameters.AddWithValue("@Taluka_id", customer.Taluka_id);
                        cmd.Parameters.AddWithValue("@District_id", customer.District_id);
                        cmd.Parameters.AddWithValue("@State_id", customer.State_id);
                        cmd.Parameters.AddWithValue("@Gender", customer.Gender);
                        cmd.Parameters.AddWithValue("@Cust_type", customer.Cust_type);
                        cmd.Parameters.AddWithValue("@Matdar_prakar", customer.Matdar_prakar);
                        cmd.Parameters.AddWithValue("@DOB", customer.DOB);
                        cmd.Parameters.AddWithValue("@Phone_no", customer.Phone_no);
                        cmd.Parameters.AddWithValue("@Acc_start_date", customer.Acc_start_date);
                        if(customer.Status == '0')
                        {
                            cmd.Parameters.AddWithValue("@Acc_end_date", customer.Acc_end_date);
                            cmd.Parameters.AddWithValue("@Reason", customer.Reason);
                        }
                        cmd.Parameters.AddWithValue("@Status", customer.Status);
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

        [Route("api/Customer")]
        public HttpResponseMessage PutCustomer([FromBody]CUSTOMER customer)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(customer.Modified_by))
                {
                    if (db.IsExists("Select * from CUSTOMER where First_name = '" + customer.First_name + "' and Middle_name = '" + customer.Middle_name + "' and Last_name = '" + customer.Last_name + "' and Append = '" + customer.Append + "' and Cust_id !=" + customer.Cust_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer name already exists");
                    }
                    else if (db.IsExists("Select * from CUSTOMER where Cust_no = " + customer.Cust_no + " and Cust_id !=" + customer.Cust_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer no already exists");
                    }
                    else if (db.IsExists("Select * from CUSTOMER where Card_no = '" + customer.Card_no + "' and Cust_id !=" + customer.Cust_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer card no already exists");
                    }
                    else if (db.IsExists("Select * from CUSTOMER where Phone_no = '" + customer.Phone_no + "' and Cust_id !=" + customer.Cust_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Customer phone no already exists");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("Sp_Customer", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Cust_id", customer.Cust_id);
                        cmd.Parameters.AddWithValue("@Cust_no", customer.Cust_no);
                        cmd.Parameters.AddWithValue("@Card_no", customer.Card_no);
                        cmd.Parameters.AddWithValue("@Prefix_id", customer.Prefix_id);
                        cmd.Parameters.AddWithValue("@First_name", customer.First_name);
                        cmd.Parameters.AddWithValue("@Middle_name", customer.Middle_name);
                        cmd.Parameters.AddWithValue("@Last_name", customer.Last_name);
                        cmd.Parameters.AddWithValue("@Append", customer.Append);
                        cmd.Parameters.AddWithValue("@Address", customer.Address);
                        cmd.Parameters.AddWithValue("@City_id", customer.City_id);
                        cmd.Parameters.AddWithValue("@Taluka_id", customer.Taluka_id);
                        cmd.Parameters.AddWithValue("@District_id", customer.District_id);
                        cmd.Parameters.AddWithValue("@State_id", customer.State_id);
                        cmd.Parameters.AddWithValue("@Gender", customer.Gender);
                        cmd.Parameters.AddWithValue("@Cust_type", customer.Cust_type);
                        cmd.Parameters.AddWithValue("@Matdar_prakar", customer.Matdar_prakar);
                        cmd.Parameters.AddWithValue("@DOB", customer.DOB);
                        cmd.Parameters.AddWithValue("@Phone_no", customer.Phone_no);
                        cmd.Parameters.AddWithValue("@Acc_start_date", customer.Acc_start_date);
                        if (customer.Status == '0')
                        {
                            cmd.Parameters.AddWithValue("@Acc_end_date", customer.Acc_end_date);
                            cmd.Parameters.AddWithValue("@Reason", customer.Reason);
                        }
                        cmd.Parameters.AddWithValue("@Status", customer.Status);
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

        [Route("api/DelCustomer")]
        public HttpResponseMessage DeleteCustomer([FromBody]CUSTOMER customer)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(customer.Modified_by))
                {
                    if(db.IsExists("select * from VIEW_EMPLOYEE_INFO where Cust_Id = " + customer.Cust_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Can't delete, customer used in employee");
                    }
                    SqlCommand cmd = new SqlCommand("sp_Customer_dlt", db.cn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Cust_id", customer.Cust_id);
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
                return Request.CreateResponse(HttpStatusCode.NotFound, ex.Message);

            }
        }

        [Route("api/MaxCustNo")]
        public HttpResponseMessage GetMaxCustomerNo()
        {
            try
            {
                db.Connect();
                var result = Convert.ToInt32(db.ExecuteScalar("SELECT COALESCE(MAX(Cust_no), 0) + 1 FROM CUSTOMER"));
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("api/CustomerByAccountNo")]
        public HttpResponseMessage GetCustomerByAccountNo(int Cust_no)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_CUSTOMER where Cust_no = " +  Cust_no);
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
