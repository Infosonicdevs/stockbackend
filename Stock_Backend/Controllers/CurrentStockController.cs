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
    public class CurrentStockController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/GetMainBranchCurrentStock")]
        public HttpResponseMessage GetMainBranchCurrentStock()
        {
            try
            {
                db.Connect();

                DataTable dt = new DataTable();

                using (SqlCommand cmd = new SqlCommand("SP_Get_MainBranch_CurrentStock", db.cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true,
                    data = dt
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
            finally
            {
                db.Disconnect();
            }
        }

        [Route("api/GetOutletCurrentStock")]
        public HttpResponseMessage GetOutletCurrentStock(int outlet_id)
        {
            try
            {
                db.Connect();

                DataTable dt = new DataTable();

                using (SqlCommand cmd = new SqlCommand("SP_Get_Outlet_CurrentStock", db.cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Parameter
                    cmd.Parameters.AddWithValue("@Outlet_id", outlet_id);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = true,
                    data = dt
                });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
            finally
            {
                db.Disconnect();
            }
        }
    }
}