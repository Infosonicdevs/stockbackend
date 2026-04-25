using Stock_Backend.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace Stock_Backend.Controllers
{

    public class UserController : ApiController
    {
        DbClass db = new DbClass();

        [HttpGet]
        [Route("api/getUsers")]
        public HttpResponseMessage GetUsers()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select USER_LOGIN.User_id, USER_LOGIN.Emp_id, USER_LOGIN.User_name, USER_LOGIN.Role_id, USER_LOGIN.Log_in, USER_LOGIN.Status from USER_LOGIN");
                db.Disconnect();

                if (result.Rows.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No Users Found!");
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("api/getUserRole")]
        public HttpResponseMessage getUserRole([FromBody] User_Login request)
        {
            try
            {
                db.Connect();
                var result = db.GetTable("SELECT USER_LOGIN.Emp_id, ROLE_MASTER.Role FROM USER_LOGIN INNER JOIN ROLE_MASTER ON ROLE_MASTER.Role_id = USER_LOGIN.Role_id WHERE USER_LOGIN.User_name = '" + request.User_name + "'AND USER_LOGIN.Password = '" + request.Password + "'AND Log_in = 0 AND Status = 1");
                db.Disconnect();
                if (result.Rows.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, data = result });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = "Invalid Credentials!", data = new DataTable() });
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/loginUser")]
        public HttpResponseMessage LoginUser([FromBody] User_Login request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.User_name) || string.IsNullOrEmpty(request.Password))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        success = false,
                        message = "Username and Password are required"
                    });
                }

                db.Connect();

                string query = @"SELECT 
	                                UL.User_id,
	                                UL.Emp_id,
	                                UL.User_name,
	                                UL.Role_id,
	                                UL.Log_in,
	                                UL.Status,
	                                RM.Role,
	                                EI.Outlet_id,
	                                (SELECT Outlet_name FROM OUTLET where Outlet_id = EI.Outlet_id) as Outlet_name
                                FROM USER_LOGIN UL
                                INNER JOIN ROLE_MASTER RM ON RM.Role_id = UL.Role_id
                                INNER JOIN EMPLOYEE_INFO EI ON EI.Emp_id = UL.Emp_id
                                WHERE 
                                    UL.User_name = @UserName
                                    AND UL.Password = @Password
                                    AND UL.Log_in = 0
                                    AND UL.Status = 1";

                var parameters = new[]
                {
                    new SqlParameter("@UserName", request.User_name),
                    new SqlParameter("@Password", request.Password)
                };

                var result = db.GetTable(query, parameters);
                db.Disconnect();

                if (result.Rows.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = true,
                        data = result
                    });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new
                    {
                        success = false,
                        message = "Invalid Credentials"
                    });
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [HttpGet]
        [Route("api/getBranches")]
        public HttpResponseMessage GetBranches()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("SELECT Outlet_id, Outlet_name, Is_main_branch FROM OUTLET");
                db.Disconnect();
                if (result.Rows.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, data = result });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { success = true, message = "Invalid Credentials!", data = new DataTable() });
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        [Route("api/verifySystemUser")]
        public HttpResponseMessage verifySystemUser([FromBody] System_User_Login request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        success = false,
                        message = "Username and Password are required"
                    });
                }

                db.Connect();

                string query = @"SELECT 
                            UL.User_id,
                            UL.Emp_id,
                            UL.User_name,
                            UL.Role_id,
                            UL.Log_in,
                            UL.Status,
                            RM.Role,
                            EI.Outlet_id,
                            (SELECT Outlet_name FROM OUTLET where Outlet_id = EI.Outlet_id) as Outlet_name
                        FROM USER_LOGIN UL
                        INNER JOIN ROLE_MASTER RM ON RM.Role_id = UL.Role_id
                        INNER JOIN EMPLOYEE_INFO EI ON EI.Emp_id = UL.Emp_id
                        WHERE 
                            UL.User_name = @UserName
                            AND UL.Password = @Password
                            AND RM.Role_id = 1";

                var parameters = new[]
                {
            new SqlParameter("@UserName", request.Username),
            new SqlParameter("@Password", request.Password)
        };

                var userResult = db.GetTable(query, parameters);

                if (userResult.Rows.Count > 0)
                {
                    DateTime dateInDateTime;

                    if (!DateTime.TryParseExact(
                            request.Date,
                            "dd-MM-yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out dateInDateTime))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid date format");
                    }

                    int outlet_id = Convert.ToInt32(userResult.Rows[0]["Outlet_id"]);

                    // Previous day close check
                    string checkQuery = @"
SELECT TOP 1 CAST(Date AS DATE) AS PendingDate 
FROM TEMP_TBL 
WHERE Vibhag_id = @Outlet_id 
AND Status = 1
AND CAST(Date AS DATE) < @Date
ORDER BY Date DESC";

                    SqlCommand checkCmd = new SqlCommand(checkQuery, db.cn);
                    checkCmd.Parameters.AddWithValue("@Outlet_id", outlet_id);
                    checkCmd.Parameters.AddWithValue("@Date", dateInDateTime);

                    DataTable dtCheck = new DataTable();
                    new SqlDataAdapter(checkCmd).Fill(dtCheck);

                    if (dtCheck.Rows.Count > 0)
                    {
                        string pendingDate = Convert.ToDateTime(dtCheck.Rows[0]["PendingDate"]).ToString("dd-MM-yyyy");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new
                        {
                            success = false,
                            message = pendingDate + " is Not Closed. Please close the day first!"
                        });
                    }

                    // Duplicate check
                    string dupCheckQuery = @"
SELECT COUNT(*) 
FROM TEMP_TBL T
WHERE 
    T.Vibhag_id = @Outlet_id
    AND CAST(T.Date AS DATE) = @Date
    AND T.Status = 1
    AND EXISTS (
        SELECT 1 
        FROM ASSIGN_COUNTER A
        WHERE 
            CAST(A.Login_date AS DATE) = @Date
            AND ISNULL(A.Is_closed,0) = 0
            AND A.Log_out_time IS NULL
    )";

                    SqlCommand dupCheckCmd = new SqlCommand(dupCheckQuery, db.cn);
                    dupCheckCmd.Parameters.AddWithValue("@Outlet_id", outlet_id);
                    dupCheckCmd.Parameters.AddWithValue("@Date", dateInDateTime);

                    int alreadyExists = Convert.ToInt32(dupCheckCmd.ExecuteScalar());

                    if (alreadyExists > 0)
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new
                        {
                            success = false,
                            message = "Already logged in for this date!"
                        });
                    }

                    //  TEMP_TBL insert
                    string checkTemp = @"
SELECT COUNT(*) FROM TEMP_TBL
WHERE Vibhag_id = @Outlet_id
AND CAST(Date AS DATE) = @Date";

                    SqlCommand checkCmd2 = new SqlCommand(checkTemp, db.cn);
                    checkCmd2.Parameters.AddWithValue("@Outlet_id", outlet_id);
                    checkCmd2.Parameters.AddWithValue("@Date", dateInDateTime);

                    int exists = Convert.ToInt32(checkCmd2.ExecuteScalar());

                    if (exists > 0)
                    {
                        //  UPDATE existing row
                        string updateTemp = @"
  UPDATE TEMP_TBL
SET Status = 1
WHERE Vibhag_id = @Outlet_id
AND CAST(Date AS DATE) = @Date";

                        SqlCommand cmdUpdate = new SqlCommand(updateTemp, db.cn);
                        cmdUpdate.Parameters.AddWithValue("@Outlet_id", outlet_id);
                        cmdUpdate.Parameters.AddWithValue("@Date", dateInDateTime);
                        cmdUpdate.ExecuteNonQuery();
                    }
                    else
                    {
                        //  INSERT new row
                        SqlCommand cmdTemp = new SqlCommand("Sp_Temp_Tbl", db.cn);
                        cmdTemp.CommandType = CommandType.StoredProcedure;
                        cmdTemp.Parameters.AddWithValue("@Vibhag_id", outlet_id);
                        cmdTemp.Parameters.AddWithValue("@Date", dateInDateTime);
                        cmdTemp.Parameters.AddWithValue("@Status", "1");
                        cmdTemp.ExecuteNonQuery();
                    }

                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = true,
                        data = dateInDateTime.ToString("yyyy-MM-dd")
                    });
                }
                else
                {
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new
                    {
                        success = false,
                        message = "Invalid Credentials! Check if Admin Role!"
                    });
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }



        [HttpGet]
        [Route("api/getLatestSystemDateSelectedByAdmin")]
        public HttpResponseMessage GetLatestSystemDateSelectedByAdmin()
        {
            try
            {
                db.Connect();
                var result = db.GetTable(@"
SELECT TOP 1 CAST(Date AS DATE) AS Login_date 
FROM TEMP_TBL 
WHERE Status = 1
ORDER BY Date DESC");
                db.Disconnect();
                if (result.Rows.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, data = result });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { success = false, message = "System Date Selected By Admin Not Found", data = new DataTable() });
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = ex.Message });
            }
        }


        #region User CRUD
        [HttpGet]
        [Route("api/UserLogin")]
        public HttpResponseMessage GetUserLogin()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from VIEW_USER_LOGIN");
                db.Disconnect();

                if (result.Rows.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No Users Found!");
                }
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("api/UserLogin")]
        public HttpResponseMessage SaveUserLogin([FromBody] User_Login user)
        {
            try
            {
                db.Connect();
                if (db.IsValidUser(user.Created_by))
                {
                    if (!db.IsExists("select * from User_Login where User_name='" + user.User_name + "'"))
                    {
                        SqlCommand cmd = new SqlCommand("Sp_User_Login", db.cn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Emp_id", user.Emp_id);
                        cmd.Parameters.AddWithValue("@User_name", user.User_name);
                        cmd.Parameters.AddWithValue("@Password", user.Password);
                        cmd.Parameters.AddWithValue("@Role_id", user.Role_id);
                        cmd.Parameters.AddWithValue("@Log_in", user.Log_in);
                        cmd.Parameters.AddWithValue("@Status", user.Status);
                        cmd.Parameters.AddWithValue("@text", 1);

                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Username already Exists!");
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

        [HttpPut]
        [Route("api/UserLogin")]
        public HttpResponseMessage PutUserLogin([FromBody] User_Login user)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(user.Created_by))
                {
                    if (!db.IsExists("select * from User_Login where User_name='" + user.User_name + "' and User_id!=" + user.User_id + ""))
                    {
                        SqlCommand cmd = new SqlCommand("Sp_User_Login", db.cn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@User_id", user.User_id);
                        cmd.Parameters.AddWithValue("@Emp_id", user.Emp_id);
                        cmd.Parameters.AddWithValue("@User_name", user.User_name);
                        cmd.Parameters.AddWithValue("@Password", user.Password);
                        cmd.Parameters.AddWithValue("@Role_id", user.Role_id);
                        cmd.Parameters.AddWithValue("@Log_in", user.Log_in);
                        cmd.Parameters.AddWithValue("@Status", user.Status);
                        cmd.Parameters.AddWithValue("@text", 2);

                        cmd.ExecuteNonQuery();
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Username already Exists!");

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


        [HttpPost]
        [Route("api/DelUserLogin")]
        public HttpResponseMessage deleteUserLogin([FromBody] User_Login user)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(user.Created_by))
                {
                    db.Execute("delete from USER_LOGIN where User_id=" + user.User_id + "");
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record deleted");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User!");
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NotFound, ex.Message);

            }

        }
        #endregion
    }
}
