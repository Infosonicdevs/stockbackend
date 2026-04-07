using Finex_api.Models;
using Stock_Backend.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class RoleController : ApiController
    {
        DbClass db = new DbClass();


        [Route("api/Role")]
        public HttpResponseMessage GetRole()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("Select * from ROLE_MASTER order by Role");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);
            }

        }


        [Route("api/Role")]   
        public HttpResponseMessage GetRole(int R_id)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from ROLE_MASTER where Role_id=" + R_id);
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }


        [Route("api/Role")]
        public HttpResponseMessage PutRole([FromBody] Role_Master role)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(role.User_name))
                {
                    if (db.IsExists("Select * from ROLE_MASTER where Role = '" + role.Role + "' and Role_id != " + role.Role_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Role already exists");
                    }
                    else
                    {
                        db.Execute("update ROLE_MASTER set Role ='" + role.Role + "' where Role_id= " + role.Role_id + "");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Updated");
                    }
                }

                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid User ");

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }


        [Route("api/Role")]
        public HttpResponseMessage PostRole([FromBody] Role_Master role)
        {
            try
            {

                db.Connect();
                if (db.IsValidUser(role.User_name))
                {
                    if (db.IsExists("Select * from ROLE_MASTER where Role = '" + role.Role + "'"))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Role already exists");
                    }
                    else
                    {
                        db.Execute("insert into ROLE_MASTER(Role) values('" + role.Role + "')");
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Inserted");
                    }
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


        [HttpPost]
        [Route("api/DelRole")]
        public HttpResponseMessage deleteRole([FromBody] Role_Master role)
        {
            try
            {
                db.Connect();

                if (db.IsAdmin(role.User_name))
                {
                    if(db.IsExists("select * from USER_LOGIN where Role_id = " + role.Role_id))
                    {
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Role is used in Login,Can't Delete!");
                    }
                    db.Execute("delete from ROLE_MASTER where Role_id = " + role.Role_id + "");
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


        [Route("api/Role")]
        public HttpResponseMessage GetRole(string name)
        {
            try
            {

                db.Connect();
                var result = db.GetTable("Select * from ROLE_MASTER where Role='" + name +"'");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.NoContent, ex.Message);

            }
        }
    }
}
