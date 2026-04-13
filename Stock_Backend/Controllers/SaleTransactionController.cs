using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class SaleTransactionController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/SaleTransaction")]
        public HttpResponseMessage GetSale()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_SALE");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


    }
}
