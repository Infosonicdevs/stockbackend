using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using Stock_Backend;
using System.Web;

namespace Stock_Backend.Controllers
{
    public class LedgerController : ApiController
    {
        DbClass db = new DbClass();
      

        #region Ledger
        //
        //get ledger
        [Route("api/Ledger")]
        public HttpResponseMessage GetLedger()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER order by Ledger_name");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpGet]
        [Route("api/LedgerType")]
        public HttpResponseMessage GetLedgerType()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from LEDGER_TYPE order by Ledger_Type_id");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        //get ledger by id
        [Route("api/Ledger")]   //"api/Ledger?L_id=1"
        public HttpResponseMessage GetLedger(int L_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER where Ledger_id=" + L_id);
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }

        //get ledger by No

        [HttpGet]
        [Route("api/LedgerByNo")]   //"api/Ledger?L_id=1"
        public HttpResponseMessage GetLedgerByNo(int L_no)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER where Ledger_no=" + L_no);
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }

        //Update ledger
        [Route("api/Ledger")]
        public HttpResponseMessage PutLedger([FromBody] LEDGER ledger)
        {
            try
            {
                db.Connect();               
                if (db.IsAdmin(ledger.Modified_by))
                {
                    
                    DataTable dt1 = db.GetTable("Select * from ledger where Ledger_name ='" + ledger.Ledger_name + "' and Ledger_id!="+ledger.Ledger_id+"");
                    if (dt1.Rows.Count == 0)
                    {
                        if (!db.IsExists("Select Ledger_no from ledger where Ledger_no =" + ledger.Ledger_no + " and Ledger_id!=" + ledger.Ledger_id + ""))
                        {
                            db.Execute("update LEDGER set Ledger_no =" + ledger.Ledger_no + ", Ledger_name ='" + ledger.Ledger_name + "', Ledger_name_RL =N'" + ledger.Ledger_name_RL + "', Ledger_group_id =" + ledger.Ledger_group_id + ", Ledger_subgroup_id =" + ledger.Ledger_subgroup_id + ", Ledger_type =" + ledger.Ledger_type + ", Is_personal =" + ledger.Is_personal + ", Cust_type_id =" + ledger.Cust_type_id + ", Accountable =" + ledger.Accountable + ", Status =" + ledger.Status + ", Modified_by ='" + ledger.Modified_by + "', Modified_date ='" + DateTime.Now.ToString("MM-dd-yyyy") + "' where Ledger_id= " + ledger.Ledger_id + "");
                            db.Disconnect();
                            return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                        }
                        else
                        {
                            db.Disconnect();
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Ledger_no already Exists");
                        }
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already Exists");        
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }

        }

        //insert ledger
        [Route("api/Ledger")]
        public HttpResponseMessage PostLedger([FromBody] LEDGER ledger)
        {
            try
            {

                db.Connect();               
                if (db.IsValidUser(ledger.Created_by))
                {

                    DataTable dt1 = db.GetTable("Select * from ledger where Ledger_name ='" + ledger.Ledger_name + "'");
                    if (dt1.Rows.Count == 0)
                    {
                        if (!db.IsExists("Select Ledger_no from ledger where Ledger_no =" + ledger.Ledger_no))
                        {
                            db.Execute("insert into ledger(Ledger_no, Ledger_name , Ledger_name_RL , Ledger_group_id, Ledger_subgroup_id , Ledger_type , Is_personal , Cust_type_id , Accountable , Status , Created_by , Created_date) values (" + ledger.Ledger_no + ",'" + ledger.Ledger_name + "',N'" + ledger.Ledger_name_RL + "'," + ledger.Ledger_group_id + "," + ledger.Ledger_subgroup_id + "," + ledger.Ledger_type + "," + ledger.Is_personal + "," + ledger.Cust_type_id + "," + ledger.Accountable + "," + ledger.Status + ",'" + ledger.Created_by + "','" + DateTime.Now.ToString("MM-dd-yyyy") + "') ");
                            db.Disconnect();
                            return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                        }
                        else
                        {
                            db.Disconnect();
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Ledger_no already Exists");
                        }
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already Exists");
                }

                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");
                
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }

        }

        //delete ledger
        [HttpPost]
        [Route("api/DelLedger")]
        public HttpResponseMessage DeleteLedger([FromBody] LEDGER ledger)
        {
            try
            {
                db.Connect();               
                if(db.IsAdmin(ledger.Modified_by))
                {
                    if (db.IsExists("select * from TRANS_DETAILS where L_id = " + ledger.Ledger_id + ""))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Ledger has Transaction,Can't Delete!");              
                    }
                    if (db.IsExists("select * from OPENING_BAL where L_id = " + ledger.Ledger_id + ""))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Ledger has Balance,Can't Delete!");
                    }
                    else
                    {
                        db.Execute("delete from ledger where Ledger_id = " + ledger.Ledger_id + "");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record deleted");
                    }
                }               
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }

        }

        //get by ledger name
        [Route("api/Ledger")]   
        public HttpResponseMessage GetLedger(string name)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER where Ledger_name='" + name +"'");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }

        [Route("api/MaxLedgerNo")]
        public int GetMAxLedgerNo()
        {
            try
            {
                db.Connect();
                var result = Convert.ToInt32( db.ExecuteScalar("Select Coalesce(Max(Ledger_no),0) from VIEW_LEDGER"));
                db.Disconnect();

                return result;

            }
            catch (Exception )
            {

                return -1;

            }
        }

        #endregion


        #region Ledger Group

        [Route("api/LedgerGroup")]
        public HttpResponseMessage GetLedgerGroup()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER_GROUP order by L_group_name");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }


        //get ledgergroup by id
        [Route("api/LedgerGroup")]   //"api/Ledger?L_id=1"
        public HttpResponseMessage GetLedgerGroup(int LG_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER_GROUP where L_group_id=" + LG_id);
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }


        //Update LedgerGroup
        [Route("api/LedgerGroup")]
        public HttpResponseMessage PutLedgerGroup([FromBody] LEDGER_GROUP l_group)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(l_group.User_name))
                {
                    DataTable dt1 = db.GetTable("Select * from LEDGER_GROUP where L_group_name ='" + l_group.L_group_name + "' and L_group_id!=" + l_group.L_group_id + "");
                    if (dt1.Rows.Count == 0)
                    {
                        db.Execute("update LEDGER_GROUP set L_group_name ='" + l_group.L_group_name + "', L_group_name_RL =N'" + l_group.L_group_name_RL + "', Patrak_id =" + l_group.Patrak_id + ", crdr_id = " + l_group.crdr_id + ", Seqno = " + l_group.Seqno + ",Code = '" + l_group.Code + "' where L_group_id = " + l_group.L_group_id + "");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already Exists");
                }
   
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }

        }


        //insert LedgerGroup
        [Route("api/LedgerGroup")]
        public HttpResponseMessage PostLedgerGroup([FromBody] LEDGER_GROUP l_group)
        {
            try
            {
                db.Connect();             
                if (db.IsValidUser(l_group.User_name))
                {

                    if (db.GetTable("Select * from LEDGER_GROUP where L_group_name ='" + l_group.L_group_name + "'").Rows.Count == 0)
                    {
                        db.Execute("insert into LEDGER_GROUP(L_group_name, L_group_name_RL, Patrak_id, crdr_id, Seqno, Code) values('" + l_group.L_group_name + "',N'" + l_group.L_group_name_RL + "'," + l_group.Patrak_id + "," + l_group.crdr_id + "," + l_group.Seqno + ",'" + l_group.Code + "')");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already Exists");
                }

                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }

        }


        //delete LedgerGroup
        [HttpPost]
        [Route("api/DelLedgerGroup")]
        public HttpResponseMessage DelLedgerGroup([FromBody] LEDGER_GROUP l_group)
        {
            try
            {
                db.Connect();
                
                if (db.IsAdmin(l_group.User_name))
                {
                    var Ledger_id = l_group.L_group_id;

                     if (db.GetTable("Select * from Ledger where Ledger_group_id=" + Ledger_id + "").Rows.Count>0 ||
                        db.GetTable("Select * from LEDGER_SUBGROUP where Ledger_group_id=" + Ledger_id + "").Rows.Count > 0
                        )
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "This record can not be delteted..!");
                    }
                    else
                    {
                        db.Execute("delete from LEDGER_GROUP where L_group_id = " + l_group.L_group_id + "");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted");
                    }
                  
                }
  
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }

        }


        [Route("api/LedgerGroup")]   
        public HttpResponseMessage GetLedgerGroup(string name)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER_GROUP where L_group_name='" + name + "'");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }
        }



        // api/LedgerGroup?patrak=1&CrDr=1
        [Route("api/LedgerGroup")]
        public HttpResponseMessage GetLedgerGroup(int patrak, int CrDr)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER_GROUP where patrak_id=" + patrak + " and CrDr_id=" + CrDr + " order by L_group_name");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }


        #endregion


        #region Ledger Sub Group

        //LEDGER SUB_GROUP API

        //get ledgersubgroup
        [Route("api/LedgerSubGroup")]
        public HttpResponseMessage GetLedgerSubGroup()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER_SUBGROUP order by Ledger_subgroup_name");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }


        //get ledgersubgroup by id
        [Route("api/LedgerSubGroup")]
        public HttpResponseMessage GetLedgerSubGroup(int id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER_SUBGROUP where Ledger_subgroup_id = " + id+"");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        [HttpGet]
        [Route("api/LedgerSubGroupByGroup")]
        public HttpResponseMessage GetLedgerSubGroupByGroup(int id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER_SUBGROUP where Ledger_group_id = " + id + "");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }


        //Update LedgerSubGroup
        [Route("api/LedgerSubGroup")]
        public HttpResponseMessage PutLedgerSubGroup([FromBody] LEDGER_SUBGROUP ls_group)
        {
            try
            {
                db.Connect();
                
                if (db.IsAdmin(ls_group.User_name))
                {
                    DataTable dt1 = db.GetTable("Select * from LEDGER_SUBGROUP where Ledger_subgroup_name ='" + ls_group.Ledger_subgroup_name + "' and Ledger_subgroup_id!=" + ls_group.Ledger_subgroup_id + "");
                    if (dt1.Rows.Count == 0)
                    {
                        db.Execute("Update LEDGER_SUBGROUP set Ledger_subgroup_name ='"+ ls_group.Ledger_subgroup_name + "', Ledger_subgroup_name_RL =N'"+ ls_group.Ledger_subgroup_name_RL + "', Ledger_group_id ="+ ls_group.Ledger_group_id + ", Seqno ="+ ls_group.Seqno + ", Code ='"+ ls_group.Code + "' where Ledger_subgroup_id ="+ ls_group.Ledger_subgroup_id + "");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already Exists");
                }
                
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }

        }

        //insert LedgerSubGroup
        [Route("api/LedgerSubGroup")]
        public HttpResponseMessage PostLedgerSubGroup([FromBody] LEDGER_SUBGROUP ls_group)
        {
            try
            {
                db.Connect();
              
                if (db.IsValidUser(ls_group.User_name))
                {

                    DataTable dt1 = db.GetTable("Select * from LEDGER_SUBGROUP where Ledger_subgroup_name ='" + ls_group.Ledger_subgroup_name + "'");
                    if (dt1.Rows.Count == 0)
                    {
                        db.Execute("insert into LEDGER_SUBGROUP(Ledger_subgroup_name, Ledger_subgroup_name_RL, Ledger_group_id, Seqno, Code) values ('" + ls_group.Ledger_subgroup_name + "',N'" + ls_group.Ledger_subgroup_name_RL + "'," + ls_group.Ledger_group_id + "," + ls_group.Seqno + ",'" + ls_group.Code + "')");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already Exists");
                }

                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }

        }


        //delete LedgerSubGroup
        [HttpPost]
        [Route("api/DelLedgerSubGroup")]
        public HttpResponseMessage DelLedgerSubGroup([FromBody] LEDGER_SUBGROUP ls_group)
        {
            try
            {
                db.Connect();
                
                if (db.IsAdmin(ls_group.User_name))
                {
                    var Ledger_subgroup_id = ls_group.Ledger_subgroup_id;

                    if (db.GetTable("Select * from Ledger where Ledger_subgroup_id="+Ledger_subgroup_id+"").Rows.Count == 0)
                    {

                        db.Execute("delete from LEDGER_SUBGROUP where Ledger_subgroup_id = " + Ledger_subgroup_id + "");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Deleted");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "This record can not be delteted..!");
                    }
                }
        
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);

            }

        }


        [Route("api/LedgerSubGroup")]
        public HttpResponseMessage GetLedgerSubGroup(string name)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER_SUBGROUP where Ledger_subgroup_name = '" + name + "'");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        #endregion



        //Get Personal Ledgers
        [Route("api/PersonalLedger")]
        public HttpResponseMessage GetpersonalLedger()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from view_ledger where is_personal=1");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //Get Non Personal Ledgers
        [Route("api/NonPersonalLedger")]
        public HttpResponseMessage GetnotpersonalLedger()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from view_ledger where is_personal=0");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //Get Ledger Open balance List
        [Route("api/LedgerOpnBalList")]
        public HttpResponseMessage GetLedgerOpnBalList()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_LEDGER where ( crdr_id = 1 and Ledger_group_seqno not in(1,3) and Patrak_id = 1) or ( crdr_id=2 and Ledger_group_seqno not in (3,4,5) and Patrak_id = 1)");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        //for deadstock Ledger
        // patrak_id=1 for DS khate
        // patrak_id=2 for Zij khate
        [Route("api/DStockLedger")]
        public HttpResponseMessage GetDStockLedger(int patrak_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_LEDGER where LEDGER_TYPE=9 and Patrak_id="+patrak_id+" and crdr_id=2 order by Ledger_name");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        //for gst ledger
        [Route("api/GSTLedger")]
        public HttpResponseMessage GetGstLedger()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_LEDGER where Patrak_id=1 order by Ledger_name");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #region Ledger Setting
        [Route("api/LedgerSetting")]
        public HttpResponseMessage GetLedgerSetting()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from VIEW_LEDGER_SETTING order by No");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //insert LedgerSetting
        [Route("api/LedgerSetting")]
        public HttpResponseMessage PostLedgerSetting([FromBody]LEDGER_SETTING l_setting)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(l_setting.User_name))
                {
                    db.Execute("Insert into LEDGER_SETTING(No, Purpose, Ledger_id) values((SELECT ISNULL(MAX(No), 0) + 1 from LEDGER_SETTING), '" + l_setting.Purpose + "', " + l_setting.Ledger_id + ")");
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
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

        //update LedgerSetting
        [Route("api/LedgerSetting")]
        public HttpResponseMessage PutLedgerSetting([FromBody] List<LEDGER_SETTING> l_settings_list)
        {
            try
            {
                db.Connect();
                if (db.IsAdmin(l_settings_list.First().User_name))
                {
                    foreach (var l_setting in l_settings_list)
                    {
                        db.Execute("Update LEDGER_SETTING set ledger_id = " + l_setting.Ledger_id + " where No = " + l_setting.No);
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                }
                else
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User");
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //delete LedgerSetting
        [HttpPost]
        [Route("api/DelLedgerSetting")]
        public HttpResponseMessage DelLedgerSetting([FromBody]LEDGER_SETTING l_setting)
        {
            try
            {
                db.Connect();
                db.Execute("delete from LEDGER_SETTING where No = " + l_setting.No);
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, "Record deleted");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion

        #region Patrak Master
        [Route("api/PatrakMaster")]
        public HttpResponseMessage GetPatrak()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select Patrak_id, Patrak, Patrak_RL from PATRAK_MASTER");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion

        #region CR_DR
        [Route("api/CR_DR")]
        public HttpResponseMessage GetCR_DR()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select CrDr_id, CrDr from CR_DR");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion

        #region Personal Ledger Type
        [Route("api/PersonalLedType")]
        public HttpResponseMessage GetPersonalLedType()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select cust_type_id,Cust_type_name from PERSONAL_L_TYPE");
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion

        [Route("api/CheckPersonalLedger")]
        public HttpResponseMessage GetCheckPersonalLedger(int L_id)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select Ledger_id, Ledger_name, Cust_type_id, Is_personal from VIEW_LEDGER where Ledger_id = " + L_id);
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
