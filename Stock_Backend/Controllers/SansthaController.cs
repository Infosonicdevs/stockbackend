using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Stock_Backend.Controllers
{
    public class SansthaController : ApiController
    {
        DbClass db = new DbClass();
        //
        //get Sanstha
        [Route("api/Sanstha")]
        public HttpResponseMessage GetSanstha()
        {
            try
            {
                db.Connect();
                var result = db.GetTable("select * from SANSTHA_INFO");
                db.Disconnect();

                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        //get Sanstha Logo
        [Route("api/SansthaLogo")]
        public HttpResponseMessage GetSansthaLogo()
        {
            try
            {
                db.Connect();
                string result = Convert.ToString( db.ExecuteScalar("select Top(1) Logo from SANSTHA_INFO"));
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        //Save Sanstha
        [HttpPost]
        [Route("api/Sanstha")]
        public HttpResponseMessage SaveSanstha()
        {
            try
            {
               
                    HttpFileCollection UploadedFiles = HttpContext.Current.Request.Files;
                    NameValueCollection value = HttpContext.Current.Request.Form;
                    

                    string Sanstha_name = Convert.ToString(value["Sanstha_name"]);
                    string Reg_no = Convert.ToString(value["Reg_no"]);
                    DateTime Reg_date = Convert.ToDateTime(value["Reg_date"]);
                    string Address = Convert.ToString(value["Address"]);
                    int State_id = Convert.ToInt32(value["State_id"]);
                    int Dist_id = Convert.ToInt32(value["Dist_id"]);
                    int Tal_id = Convert.ToInt32(value["Tal_id"]);
                    int City_id = Convert.ToInt32(value["City_id"]);
                    string Tag_line = Convert.ToString(value["Tag_line"]);
                    string Website = Convert.ToString(value["Website"]);
                    string Logo = Convert.ToString(value["Logo"]);
                    char Is_logo = Convert.ToChar(value["Is_logo"]);
                    string GST_no = Convert.ToString(value["GST_no"]);
                    string Short_name = Convert.ToString(value["Short_name"]);


                    db.Connect();
                    int count = db.Execute(@"insert into SANSTHA_INFO (Sanstha_name,Reg_no,Reg_date,Address,State_id,Dist_id,Tal_id,City_id,Tag_line,Website,Is_logo,GST_no,Short_name) values 
                                                                    ('" + Sanstha_name + "','" + Reg_no + "','" + Reg_date.ToString("MM/dd/yyyy") + "','" + Address + "'," +State_id + "," + Dist_id + "," + Tal_id + "," + City_id + ",'" +Tag_line + "','" + Website + "','" + Is_logo + "','" +GST_no + "','" + Short_name + "')");

                    if (count > 0)
                    {
                        string UploadPath = HttpContext.Current.Server.MapPath("/Content/Upload/Logo/");
                        string filename =  "Logo"+ ".jpg";  // file name will be same for all folder

                        if (!Directory.Exists(UploadPath))
                        {
                            Directory.CreateDirectory(UploadPath); //Create directory if it doesn't exist
                        }

                        //for photo
                        HttpPostedFile image = UploadedFiles["Photo"];
                        if (image != null && image.ContentLength > 0)
                        {
                            image.SaveAs(UploadPath + filename);

                            if (File.Exists(UploadPath + filename))
                            {
                                db.Execute("update Top(1) Sanstha_info set Logo='"+filename+"'");
                            }
                        }
                        db.Disconnect();
                        return Request.CreateResponse(HttpStatusCode.OK, "Record Saved!");
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Something Went Wrong!");
              
            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        //Update Sanstha
        [HttpPut]
        [Route("api/Sanstha")]
        public HttpResponseMessage EditSanstha()
        {
            try
            {

                HttpFileCollection UploadedFiles = HttpContext.Current.Request.Files;
                NameValueCollection value = HttpContext.Current.Request.Form;


                string Sanstha_name = Convert.ToString(value["Sanstha_name"]);
                string Reg_no = Convert.ToString(value["Reg_no"]);
                DateTime Reg_date = Convert.ToDateTime(value["Reg_date"]);
                string Address = Convert.ToString(value["Address"]);
                int State_id = Convert.ToInt32(value["State_id"]);
                int Dist_id = Convert.ToInt32(value["Dist_id"]);
                int Tal_id = Convert.ToInt32(value["Tal_id"]);
                int City_id = Convert.ToInt32(value["City_id"]);
                string Tag_line = Convert.ToString(value["Tag_line"]);
                string Website = Convert.ToString(value["Website"]);
                string Logo = Convert.ToString(value["Logo"]);
                char Is_logo = Convert.ToChar(value["Is_logo"]);
                string GST_no = Convert.ToString(value["GST_no"]);
                string Short_name = Convert.ToString(value["Short_name"]);


                db.Connect();
                int count = db.Execute(@"Update Top(1) SANSTHA_INFO set Sanstha_name='" + Sanstha_name + "',Reg_no='" + Reg_no + "',Reg_date='" + Reg_date.ToString("MM/dd/yyyy") + "',Address='" + Address + "',State_id=" + State_id + ",Dist_id=" + Dist_id + ",Tal_id=" + Tal_id + ",City_id=" + City_id + ",Tag_line='" + Tag_line + "',Website='" + Website + "',Is_logo='" + Is_logo + "',GST_no='" + GST_no + "',Short_name='" + Short_name + "'");

                if (count > 0)
                {
                    string UploadPath = HttpContext.Current.Server.MapPath("/Content/Upload/Logo/");
                    string filename = "Logo" + ".jpg";  // file name will be same for all folder

                    if (!Directory.Exists(UploadPath))
                    {
                        Directory.CreateDirectory(UploadPath); //Create directory if it doesn't exist
                    }

                    //for photo
                    HttpPostedFile image = UploadedFiles["Photo"];
                    if (image != null && image.ContentLength > 0)
                    {
                        image.SaveAs(UploadPath + filename);

                        if (File.Exists(UploadPath + filename))
                        {
                            db.Execute("UPDATE TOP(1) Sanstha_info SET Logo='" + filename + "'");
                        }
                    }
                    db.Disconnect();
                    return Request.CreateResponse(HttpStatusCode.OK, "Record Saved!");
                }
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Something Went Wrong!");

            }
            catch (Exception ex)
            {
                db.Disconnect();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
