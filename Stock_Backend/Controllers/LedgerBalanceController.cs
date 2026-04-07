using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class LedgerBalanceController : ApiController
    {
        DbClass db = new DbClass();

        #region Ledger Balance

        //get LedgerBalance
        [Route("api/LedgerBalance")]
        public HttpResponseMessage GetLedgerBalance()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_OPENING_BAL");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception ex) 
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //get LedgerBalance by id
        [Route("api/LedgerBalance")]
        public HttpResponseMessage GetLedgerBalance(int Opn_bal_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_OPENING_BAL where Opn_bal_id = " + Opn_bal_id);
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //insert LedgerBalance
        [Route("api/LedgerBalance")]
        public HttpResponseMessage PostLedgerBalance([FromBody]LedgerBalance ledgerBalance)
        {
            try
            {
                db.Connect();             
                if(db.IsValidUser(ledgerBalance.User_name))
                {
                    if (ledgerBalance.Amt <= 0)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Amount should be greater than 0");
                    }
                    else if (db.IsExists("SELECT * FROM OPENING_BAL WHERE L_id = " + ledgerBalance.L_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Ledger balance already exists");
                    }
                    else
                    {
                        db.Execute("insert into OPENING_BAL(L_id, Amt, Outlet_id) values(" + ledgerBalance.L_id + ", " + ledgerBalance.Amt + ", "+ ledgerBalance.Outlet_id + ")");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        

        //update LedgerBalance
        [Route("api/LedgerBalance")]
        public HttpResponseMessage PutLedgerBalance([FromBody]LedgerBalance ledgerBalance)
        {
            try
            {
                db.Connect();
                if(db.IsAdmin(ledgerBalance.User_name))
                {
                    if (ledgerBalance.Amt <= 0)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Amount should be greater than 0");
                    }
                    else if (db.IsExists("SELECT * FROM OPENING_BAL WHERE L_id = " + ledgerBalance.L_id + " and Opn_bal_id != " + ledgerBalance.Opn_bal_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Ledger balance already exists");
                    }
                    else
                    {
                        db.Execute("update OPENING_BAL set L_id = " + ledgerBalance.L_id + ", Amt = " + ledgerBalance.Amt + ", Outlet_id = " + ledgerBalance.Outlet_id + " where Opn_bal_id = " + ledgerBalance.Opn_bal_id);
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");
            }
            catch(Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //delete LedgerBalance
        [HttpPost]
        [Route("api/DelLedgerBalance")]
        public HttpResponseMessage DeleteLedgerBalance([FromBody]LedgerBalance ledgerBalance)
        {
            try
            {
                db.Connect();
                if(db.IsAdmin(ledgerBalance.User_name))
                {
                    db.Execute("delete from OPENING_BAL where Opn_bal_id = " + ledgerBalance.Opn_bal_id);
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, "Invalid User");
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
