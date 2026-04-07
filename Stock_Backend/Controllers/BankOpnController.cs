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
    public class BankOpnController : ApiController
    {
        DbClass db = new DbClass();

        [Route("api/BankOpening")]
        public HttpResponseMessage GetBankOpeing()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from Bank_Opening");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }


        [Route("api/BankOpening")]
        public HttpResponseMessage PostBankOpening([FromBody] BankOpening bankopening)
        {
            try
            {
                db.Connect();
                if (!db.IsValidUser(bankopening.User_name))
                { 
                    db.Disconnect();
                 return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");
                }

                if (bankopening.Amount <= 0)
                {
                    db.Disconnect();
                 return Request.CreateResponse(HttpStatusCode.BadRequest, "Amount must be greater than 0"); 
                }

                if (db.IsExists("SELECT * FROM BANK_OPENING WHERE L_id = " + bankopening.L_id))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Ledger ID already exists!");
                }

                SqlCommand cmd = new SqlCommand("Sp_Bank_Opening", db.cn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@L_id", bankopening.L_id);
                cmd.Parameters.AddWithValue("@Amount", bankopening.Amount);
                cmd.Parameters.AddWithValue("@Date", bankopening.Date.ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@txt", 1);

                cmd.ExecuteNonQuery();
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
            }
            catch (Exception ex) { db.Disconnect(); return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        [Route("api/BankOpening")]
        public HttpResponseMessage PutBankOpening([FromBody] BankOpening bankopening)
        {
            try
            {
                db.Connect();
                if (!db.IsAdmin(bankopening.User_name)) 
                { 
                    db.Disconnect(); 
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");
                }
                if (bankopening.Amount <= 0)
                {
                    db.Disconnect(); 
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Amount must be greater than 0"); 
                }

                if (db.IsExists("SELECT * FROM BANK_OPENING WHERE L_id = " + bankopening.L_id + " AND Id != " + bankopening.Id))
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Ledger ID already exists for another record!");
                }

                SqlCommand cmd = new SqlCommand("Sp_Bank_Opening", db.cn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Up_id", bankopening.Id);
                cmd.Parameters.AddWithValue("@L_id", bankopening.L_id);
                cmd.Parameters.AddWithValue("@Amount", bankopening.Amount);
                cmd.Parameters.AddWithValue("@Date", bankopening.Date.ToString("MM/dd/yyyy"));
                cmd.Parameters.AddWithValue("@txt", 2);

                cmd.ExecuteNonQuery();
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
            }
            catch (Exception ex) { db.Disconnect(); return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message); }
        }

        [HttpPost]
        [Route("api/DelBankOpening")]
        public HttpResponseMessage DeleteBankOpening([FromBody] BankOpening bankopening)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(bankopening.User_name))
                {
                    db.Execute("DELETE FROM BANK_OPENING WHERE Id = " + bankopening.Id);

                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted");
                }

                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }
    }
    }





