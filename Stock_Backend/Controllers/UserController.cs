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
                if (request.Username == "admin" && request.Password == "admin@123")
                {
                    DateTime dateInDateTime;

                    if (!DateTime.TryParseExact(
                            request.Date,
                            "dd-MM-yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out dateInDateTime))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid date format");
                    }

                    db.Connect();
                    var result = db.ExecuteScalar(
                            "INSERT INTO ASSIGN_COUNTER(Counter_id, Emp_id, Login_date, Status, Opn_bal, Closing_bal, Login_time) " +
                            "VALUES(1, " + request.Emp_id + ", '" + dateInDateTime.ToString("yyyy-MM-dd") + "', 1, 0.00, 0.00, '" + DateTime.Now.ToString("HH:mm") + "'); " +
                            "SELECT TOP 1 Login_date FROM ASSIGN_COUNTER ORDER BY Id DESC;"
                    );
                    //db.Execute("INSERT INTO TEMP_TBL(Date, Status) VALUES('" + request.Date + "', 1);");
                    db.Disconnect();
                    if (result != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, data = result });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, new { success = false, data = request });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = "Invalid Credentials!", data = new DataTable() });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/getLatestSystemDateSelectedByAdmin")]
        public HttpResponseMessage GetLatestSystemDateSelectedByAdmin()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("SELECT TOP 1 Login_date FROM ASSIGN_COUNTER ORDER BY Id DESC");
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
                    if (!db.IsExists("select * from User_Login where User_name='"+user.User_name+"'"))
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
                    if (!db.IsExists("select * from User_Login where User_name='"+user.User_name+"' and User_id!="+user.User_id+""))
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
                    db.Execute("delete from USER_LOGIN where User_id="+user.User_id+"");
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
